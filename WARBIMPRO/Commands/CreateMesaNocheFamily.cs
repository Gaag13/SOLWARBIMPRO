// =============================================================================
// REVIT API 2024 — SISTEMA PARAMÉTRICO UNIVERSAL DESDE JSON
// =============================================================================
// ROOT CAUSE DEL BUG GEOMÉTRICO:
//   NewExtrusion(solid, profile, sketchPlane, height) tiene este comportamiento:
//   - El parámetro 'height' es la distancia de extrusión desde el plano
//   - Pero el plano de sketch solo define la ORIENTACIÓN, no la posición Z real
//   - El perfil de curvas tiene sus propias coordenadas Z=0 porque Line.CreateBound
//     ignora la Z del SketchPlane — siempre proyecta al plano Z=0
//   - Resultado: TODOS los paneles se crean en Z=0, superpuestos
//
// SOLUCIÓN CORRECTA:
//   Usar GeometryCreationUtilities.CreateExtrusionGeometry() para crear un Solid
//   y luego insertarlo como DirectShape en el documento de familia.
//   DirectShape acepta cualquier Solid con posición 3D absoluta correcta.
//
//   Flujo:
//   1. Crear perfil 2D del panel (rectángulo) en plano XY
//   2. Crear vector de extrusión en Z con la altura correcta
//   3. GeometryCreationUtilities.CreateExtrusionGeometry → Solid 3D
//   4. Transformar el Solid a su posición final con Transform.CreateTranslation
//   5. DirectShape.CreateElement + ds.SetShape(solid)
//
// APIs 2024:
//   SpecTypeId / GroupTypeId / UnitTypeId (sin ParameterType ni BuiltInParameterGroup)
//   fm.Parameters.Cast<FamilyParameter>().FirstOrDefault(...)
// =============================================================================

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Newtonsoft.Json; // NuGet: Install-Package Newtonsoft.Json

namespace WARBIMPRO
{
    // =========================================================================
    // MODELOS DE DATOS DEL JSON
    // =========================================================================
    public class FamiliaJson
    {
        public string tipo_familia { get; set; }
        public string nombre_familia { get; set; }
        public string fabricante { get; set; }
        public DimensionesGenerales dimensiones_generales { get; set; }
        public List<Componente> componentes { get; set; }
        public List<Parametro> parametros_tipo { get; set; }
        public List<Parametro> parametros_instancia { get; set; }
    }

    public class DimensionesGenerales
    {
        public double ancho_total { get; set; } // mm
        public double alto_total { get; set; } // mm
        public double profundidad_total { get; set; } // mm
    }

    /// <summary>
    /// Sistema de coordenadas del JSON (relativo al mueble):
    ///   X: 0 = borde izquierdo,  +X = hacia la derecha
    ///   Y: 0 = frente del mueble, +Y = hacia el fondo
    ///   Z: 0 = base del mueble,   +Z = hacia arriba
    /// Las dimensiones son: ancho(X), profundidad(Y), alto(Z)
    /// </summary>
    public class Componente
    {
        public string nombre { get; set; }
        public string tipo { get; set; }
        public Punto3D origen { get; set; } // mm
        public Dim3D dimensiones { get; set; } // mm
        public string material_param { get; set; }
        public bool visible { get; set; } = true;
    }

    public class Punto3D { public double x { get; set; } public double y { get; set; } public double z { get; set; } }
    public class Dim3D { public double ancho { get; set; } public double profundidad { get; set; } public double alto { get; set; } }

    public class Parametro
    {
        public string nombre { get; set; }
        public string tipo { get; set; } // Length|Material|YesNo|Text|Integer
        public string grupo { get; set; } // Geometry|Materials|IdentityData|Construction
        public object valor_defecto { get; set; }
    }

    // =========================================================================
    // COMANDO PRINCIPAL
    // =========================================================================
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CreateFamilyFromJson : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData,
                              ref string message, ElementSet elements)
        {
            var uiApp = commandData.Application;
            var doc = uiApp.ActiveUIDocument.Document;

            try
            {
                // 1. Abrir JSON
                string jsonPath = AbrirDialogoJson();
                if (string.IsNullOrEmpty(jsonPath)) return Result.Cancelled;

                var fam = JsonConvert.DeserializeObject<FamiliaJson>(
                    File.ReadAllText(jsonPath));

                // 2. Template de familia
                string tplPath = BuscarTemplate();
                if (string.IsNullOrEmpty(tplPath))
                {
                    TaskDialog.Show("Error", "No se encontró Metric Generic Model.rft");
                    return Result.Failed;
                }

                Document famDoc = uiApp.Application.NewFamilyDocument(tplPath);

                // 3. Parámetros de familia
                using (var tx = new Transaction(famDoc, "Parámetros"))
                {
                    tx.Start();
                    DefinirParametros(famDoc.FamilyManager, fam);
                    tx.Commit();
                }

                // 4. Geometría con DirectShape (método correcto para posición 3D)
                using (var tx = new Transaction(famDoc, "Geometría"))
                {
                    tx.Start();
                    CrearGeometriaConDirectShape(famDoc, fam);
                    tx.Commit();
                }

                // 5. Guardar .rfa
                string rfaPath = Path.Combine(
                    Path.GetDirectoryName(jsonPath),
                    SanitizarNombre(fam.nombre_familia) + ".rfa");

                famDoc.SaveAs(rfaPath, new SaveAsOptions { OverwriteExistingFile = true });

                // 6. Cargar en proyecto
                Family loadedFam = null;
                using (var tx = new Transaction(doc, "Cargar familia"))
                {
                    tx.Start();
                    doc.LoadFamily(rfaPath, out loadedFam);
                    tx.Commit();
                }
                famDoc.Close(false);

                // 7. Colocar instancia
                if (loadedFam != null)
                {
                    using (var tx = new Transaction(doc, "Colocar instancia"))
                    {
                        tx.Start();
                        ColocarInstancia(doc, loadedFam);
                        tx.Commit();
                    }
                }

                TaskDialog.Show("✅ Listo",
                    $"Familia '{fam.nombre_familia}' creada.\nRFA: {rfaPath}");

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                TaskDialog.Show("Error", ex.Message + "\n\n" + ex.StackTrace);
                return Result.Failed;
            }
        }

        // =====================================================================
        // PARÁMETROS
        // =====================================================================
        private void DefinirParametros(FamilyManager fm, FamiliaJson fam)
        {
            RegistrarLista(fm, fam.parametros_tipo, esTipo: true);
            RegistrarLista(fm, fam.parametros_instancia, esTipo: false);
        }

        private void RegistrarLista(FamilyManager fm, List<Parametro> lista, bool esTipo)
        {
            if (lista == null) return;
            foreach (var par in lista)
            {
                bool existe = fm.Parameters
                    .Cast<FamilyParameter>()
                    .Any(fp => fp.Definition.Name == par.nombre);
                if (existe) continue;

                fm.AddParameter(par.nombre,
                    ResolverGrupo(par.grupo),
                    ResolverSpec(par.tipo),
                    esTipo);

                AsignarValorDefecto(fm, par);
            }
        }

        private void AsignarValorDefecto(FamilyManager fm, Parametro par)
        {
            if (par.valor_defecto == null) return;
            FamilyParameter fp = fm.Parameters
                .Cast<FamilyParameter>()
                .FirstOrDefault(x => x.Definition.Name == par.nombre);
            if (fp == null) return;

            try
            {
                switch (par.tipo?.ToLower())
                {
                    case "length": fm.Set(fp, MmToFt(Convert.ToDouble(par.valor_defecto))); break;
                    case "integer": fm.Set(fp, Convert.ToInt32(par.valor_defecto)); break;
                    case "yesno": fm.Set(fp, Convert.ToInt32(par.valor_defecto)); break;
                    case "text": fm.Set(fp, par.valor_defecto.ToString()); break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ValorDefecto] {par.nombre}: {ex.Message}");
            }
        }

        // =====================================================================
        // GEOMETRÍA CORRECTA CON DirectShape
        // =====================================================================
        // Por qué DirectShape y no NewExtrusion:
        //
        // NewExtrusion en documentos de FAMILIA tiene una limitación crítica:
        // el método NewExtrusion(bool, CurveArrArray, SketchPlane, double) toma
        // el 'double' como la altura de extrusión desde el plano, PERO el perfil
        // de curvas DEBE estar en el plano del sketch — no puede tener coordenadas
        // fuera de ese plano. Si las curvas tienen Z≠0 y el plano está en Z=oz,
        // Revit las proyecta al plano y pierde la posición. Por eso todos los
        // paneles terminaban en Z=0.
        //
        // DirectShape es un contenedor de geometría BRep arbitraria. Acepta
        // cualquier Solid creado con GeometryCreationUtilities, el cual sí
        // respeta las coordenadas 3D absolutas.
        // =====================================================================
        private void CrearGeometriaConDirectShape(Document famDoc, FamiliaJson fam)
        {
            // El template centra la familia en el origen.
            // Mapeamos el JSON así:
            //   JSON X (0..ancho)       → Revit X (-ancho/2 .. +ancho/2)  [centrado]
            //   JSON Y (0..profundidad) → Revit Y (0 .. +profundidad)      [frente=0]
            //   JSON Z (0..alto)        → Revit Z (0 .. +alto)             [base=0]
            double halfW = MmToFt(fam.dimensiones_generales.ancho_total / 2.0);

            // Categoría para DirectShape en documento de familia
            // Usamos GenericModel que es compatible con el template genérico
            var catId = new ElementId(BuiltInCategory.OST_GenericModel);

            foreach (var comp in fam.componentes)
            {
                if (!comp.visible) continue;
                if (comp.dimensiones == null || comp.origen == null) continue;

                double ox = MmToFt(comp.origen.x) - halfW;
                double oy = MmToFt(comp.origen.y);
                double oz = MmToFt(comp.origen.z);

                double sx = MmToFt(comp.dimensiones.ancho);
                double sy = MmToFt(comp.dimensiones.profundidad);
                double sz = MmToFt(comp.dimensiones.alto);

                if (sx < 1e-6 || sy < 1e-6 || sz < 1e-6) continue;

                try
                {
                    // Crear caja sólida con GeometryCreationUtilities
                    // El perfil se dibuja en Z=0, la extrusión va de 0 a sz
                    // Luego se traslada a la posición final (ox, oy, oz)
                    Solid solid = CrearCajaSolida(sx, sy, sz, ox, oy, oz);
                    if (solid == null) continue;

                    // Insertar como DirectShape en el documento de familia
                    DirectShape ds = DirectShape.CreateElement(famDoc, catId);
                    ds.SetShape(new GeometryObject[] { solid });
                    ds.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
                      ?.Set(comp.nombre);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[DirectShape] {comp.nombre}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Crea un Solid box de dimensiones (sx, sy, sz) posicionado en (ox, oy, oz).
        /// Método: perfil en Z=0, extrusión en +Z, luego traslación al origen correcto.
        /// </summary>
        private Solid CrearCajaSolida(double sx, double sy, double sz,
                                      double ox, double oy, double oz)
        {
            // Perfil rectangular en plano Z=0 (coordenadas locales 0..sx, 0..sy)
            var pts = new List<XYZ>
            {
                new XYZ(0,  0,  0),
                new XYZ(sx, 0,  0),
                new XYZ(sx, sy, 0),
                new XYZ(0,  sy, 0),
                new XYZ(0,  0,  0)  // cerrar loop
            };

            var loop = new CurveLoop();
            for (int i = 0; i < pts.Count - 1; i++)
                loop.Append(Line.CreateBound(pts[i], pts[i + 1]));

            var loops = new List<CurveLoop> { loop };
            var extDir = XYZ.BasisZ;  // extruir en +Z

            // Crear sólido en origen local (0,0,0)..(sx,sy,sz)
            Solid localSolid = GeometryCreationUtilities
                .CreateExtrusionGeometry(loops, extDir, sz);

            // Trasladar al origen final (ox, oy, oz)
            var translation = Transform.CreateTranslation(new XYZ(ox, oy, oz));
            Solid worldSolid = SolidUtils.CreateTransformed(localSolid, translation);

            return worldSolid;
        }

        // =====================================================================
        // COLOCAR INSTANCIA EN PROYECTO
        // =====================================================================
        private void ColocarInstancia(Document projectDoc, Family family)
        {
            var symId = family.GetFamilySymbolIds().FirstOrDefault();
            if (symId == null) return;

            var symbol = projectDoc.GetElement(symId) as FamilySymbol;
            if (symbol == null) return;

            if (!symbol.IsActive)
            {
                symbol.Activate();
                projectDoc.Regenerate();
            }

            Level level = new FilteredElementCollector(projectDoc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .FirstOrDefault();

            if (level == null) return;

            projectDoc.Create.NewFamilyInstance(
                new XYZ(0, 0, level.Elevation),
                symbol, level,
                Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        // =====================================================================
        // UTILIDADES
        // =====================================================================
        private double MmToFt(double mm)
            => UnitUtils.ConvertToInternalUnits(mm, UnitTypeId.Millimeters);

        private ForgeTypeId ResolverSpec(string tipo) => (tipo?.ToLower()) switch
        {
            "length" => SpecTypeId.Length,
            "material" => SpecTypeId.Reference.Material,
            "yesno" => SpecTypeId.Boolean.YesNo,
            "integer" => SpecTypeId.Int.Integer,
            _ => SpecTypeId.String.Text
        };

        private ForgeTypeId ResolverGrupo(string grupo) => (grupo?.ToLower()) switch
        {
            "geometry" => GroupTypeId.Geometry,
            "materials" => GroupTypeId.Materials,
            "construction" => GroupTypeId.Construction,
            _ => GroupTypeId.IdentityData
        };

        private string SanitizarNombre(string n)
            => string.IsNullOrEmpty(n) ? "Familia_BIM"
               : string.Concat(n.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

        private string AbrirDialogoJson()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar JSON de familia BIM",
                Filter = "JSON (*.json)|*.json|Todos (*.*)|*.*",
                InitialDirectory = Environment.GetFolderPath(
                    Environment.SpecialFolder.Desktop)
            };
            return dlg.ShowDialog() == true ? dlg.FileName : null;
        }

        private string BuscarTemplate()
        {
            var candidatos = new[]
            {
                @"C:\ProgramData\Autodesk\RVT 2022\Family Templates\English\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2022\Family Templates\Spanish\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2023\Family Templates\Spanish\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2023\Family Templates\English\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\Spanish\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\English\Metric Generic Model.rft",                
                @"C:\ProgramData\Autodesk\RVT 2025\Family Templates\English\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2025\Family Templates\Spanish\Metric Generic Model.rft",
               
            };
            return candidatos.FirstOrDefault(File.Exists);
        }
    }
}