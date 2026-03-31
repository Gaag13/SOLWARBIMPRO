using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using WARBIMPRO.Models;

namespace WARBIMPRO.Services
{


    public class FamilyBuilderService
    {
        private readonly UIApplication _uiApp;
        private readonly Document _projectDoc;

        public FamilyBuilderService(UIApplication uiApp, Document projectDoc)
        {
            _uiApp = uiApp;
            _projectDoc = projectDoc;
        }

        public void Build(FamiliaJson fam)
        {
            string tpl = BuscarTemplate()
                ?? throw new Exception("No se encontró Metric Generic Model.rft");

            Document famDoc = _uiApp.Application.NewFamilyDocument(tpl);

            using (var tx = new Transaction(famDoc, "WARBIMPRO — Parámetros"))
            {
                tx.Start();
                DefinirParametros(famDoc.FamilyManager, fam);
                tx.Commit();
            }

            using (var tx = new Transaction(famDoc, "WARBIMPRO — Geometría"))
            {
                tx.Start();
                CrearGeometria(famDoc, fam);
                tx.Commit();
            }

            string rfa = Path.Combine(Path.GetTempPath(),
                Sanitizar(fam.nombre_familia) + ".rfa");

            famDoc.SaveAs(rfa, new SaveAsOptions { OverwriteExistingFile = true });

            Family? loaded = null;
            using (var tx = new Transaction(_projectDoc, "WARBIMPRO — Cargar familia"))
            {
                tx.Start();
                _projectDoc.LoadFamily(rfa, out loaded);
                tx.Commit();
            }

            famDoc.Close(false);

            if (loaded is null) return;

            using (var tx = new Transaction(_projectDoc, "WARBIMPRO — Colocar instancia"))
            {
                tx.Start();
                ColocarInstancia(_projectDoc, loaded);
                tx.Commit();
            }
        }

        // ── Parámetros ────────────────────────────────────────
        private void DefinirParametros(FamilyManager fm, FamiliaJson fam)
        {
            Registrar(fm, fam.parametros_tipo, true);
            Registrar(fm, fam.parametros_instancia, false);
        }

        private void Registrar(FamilyManager fm, List<Parametro> lista, bool esTipo)
        {
            foreach (var par in lista)
            {
                bool existe = fm.Parameters.Cast<FamilyParameter>()
                                .Any(p => p.Definition.Name == par.nombre);
                if (existe) continue;

                fm.AddParameter(par.nombre, ResolverGrupo(par.grupo),
                                ResolverSpec(par.tipo), esTipo);

                if (par.valor_defecto is null) continue;

                FamilyParameter? fp = fm.Parameters.Cast<FamilyParameter>()
                    .FirstOrDefault(p => p.Definition.Name == par.nombre);
                if (fp is null) continue;

                try
                {
                    switch (par.tipo.ToLower())
                    {
                        case "length": fm.Set(fp, MmToFt(Convert.ToDouble(par.valor_defecto))); break;
                        case "integer": fm.Set(fp, Convert.ToInt32(par.valor_defecto)); break;
                        case "yesno": fm.Set(fp, Convert.ToInt32(par.valor_defecto)); break;
                        case "text": fm.Set(fp, par.valor_defecto.ToString()!); break;
                    }
                }
                catch { /* ignorar si no se puede asignar valor */ }
            }
        }

        // ── Geometría (DirectShape) ───────────────────────────
        private void CrearGeometria(Document famDoc, FamiliaJson fam)
        {
            double halfW = MmToFt(fam.dimensiones_generales.ancho_total / 2.0);
            var catId = new ElementId(BuiltInCategory.OST_GenericModel);

            foreach (var comp in fam.componentes)
            {
                if (!comp.visible) continue;

                double ox = MmToFt(comp.origen.x) - halfW;
                double oy = MmToFt(comp.origen.y);
                double oz = MmToFt(comp.origen.z);
                double sx = MmToFt(comp.dimensiones.ancho);
                double sy = MmToFt(comp.dimensiones.profundidad);
                double sz = MmToFt(comp.dimensiones.alto);

                if (sx < 1e-6 || sy < 1e-6 || sz < 1e-6) continue;

                try
                {
                    var loop = new CurveLoop();
                    var pts = new[] {
                        new XYZ(0,0,0), new XYZ(sx,0,0),
                        new XYZ(sx,sy,0), new XYZ(0,sy,0), new XYZ(0,0,0)
                    };
                    for (int i = 0; i < 4; i++)
                        loop.Append(Line.CreateBound(pts[i], pts[i + 1]));

                    Solid local = GeometryCreationUtilities
                        .CreateExtrusionGeometry(new[] { loop }, XYZ.BasisZ, sz);

                    Solid world = SolidUtils.CreateTransformed(
                        local, Transform.CreateTranslation(new XYZ(ox, oy, oz)));

                    DirectShape ds = DirectShape.CreateElement(famDoc, catId);
                    ds.SetShape(new GeometryObject[] { world });
                    ds.get_Parameter(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS)
                      ?.Set(comp.nombre);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"[WARBIMPRO.FamilyBuilder] {comp.nombre}: {ex.Message}");
                }
            }
        }

        // ── Colocar instancia ─────────────────────────────────
        private static void ColocarInstancia(Document doc, Family family)
        {
            var symId = family.GetFamilySymbolIds().FirstOrDefault();
            if (symId is null) return;

            if (doc.GetElement(symId) is not FamilySymbol sym) return;
            if (!sym.IsActive) { sym.Activate(); doc.Regenerate(); }

            Level? level = new FilteredElementCollector(doc)
                .OfClass(typeof(Level)).Cast<Level>()
                .OrderBy(l => l.Elevation).FirstOrDefault();
            if (level is null) return;

            doc.Create.NewFamilyInstance(
                new XYZ(0, 0, level.Elevation), sym, level,
                Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
        }

        // ── Utilidades ────────────────────────────────────────
        private static double MmToFt(double mm)
            => UnitUtils.ConvertToInternalUnits(mm, UnitTypeId.Millimeters);

        private static ForgeTypeId ResolverSpec(string t) => t.ToLower() switch
        {
            "length" => SpecTypeId.Length,
            "material" => SpecTypeId.Reference.Material,
            "yesno" => SpecTypeId.Boolean.YesNo,
            "integer" => SpecTypeId.Int.Integer,
            _ => SpecTypeId.String.Text
        };

        private static ForgeTypeId ResolverGrupo(string g) => g.ToLower() switch
        {
            "geometry" => GroupTypeId.Geometry,
            "materials" => GroupTypeId.Materials,
            "construction" => GroupTypeId.Construction,
            _ => GroupTypeId.IdentityData
        };

        private static string Sanitizar(string n)
            => string.IsNullOrEmpty(n) ? "WARBIMPRO_Familia"
               : string.Concat(n.Split(Path.GetInvalidFileNameChars())).Replace(" ", "_");

        private static string? BuscarTemplate() => new[]
        {
                 @"C:\ProgramData\Autodesk\RVT 2022\Family Templates\English\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2022\Family Templates\Spanish\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2023\Family Templates\Spanish\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2023\Family Templates\English\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\Spanish\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2024\Family Templates\English\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2025\Family Templates\English\Metric Generic Model.rft",
                @"C:\ProgramData\Autodesk\RVT 2025\Family Templates\Spanish\Metric Generic Model.rft",
        }.FirstOrDefault(File.Exists);
    }
}
