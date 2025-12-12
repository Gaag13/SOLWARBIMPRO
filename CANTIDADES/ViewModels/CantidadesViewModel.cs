using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WARBIMPRO.Models;
using WARBIMPRO.Utils;
using System.Windows.Markup;
using System.Windows;
using Autodesk.Revit.DB;
using System.Xml.Linq;
using WARBIMPRO.Views;

namespace WARBIMPRO.ViewModels
{
    public class CantidadesViewModel: ObservableObject
    {
        private UIDocument _uidoc;
        public List<Data> ContenidoTabla {  get; set; }
        public Dictionary<string,List<string>> TiposElementosNivel {  get; set; }

        private List<DosificacionConcreto> _dosificaciones;

        public CantidadesViewModel(UIDocument uidoc)
        {
            _uidoc = uidoc;
            CargarDosificaciones();
            CargarDatos();
            
        }
        private void CargarDosificaciones()
        {
            _dosificaciones = new List<DosificacionConcreto>
            {
                new DosificacionConcreto { WARBIMPRO = "1 - 2 - 2", ResistenciaKgCm2 = 280, ResistenciaPSI = 4000, ResistenciaMpa = 27, Cemento = 420, Arena = 0.67, Grava = 0.67, Agua = 190 },
                new DosificacionConcreto { WARBIMPRO = "1 - 2 - 2,5", ResistenciaKgCm2 = 240, ResistenciaPSI = 3555, ResistenciaMpa = 24, Cemento = 380, Arena = 0.60, Grava = 0.76, Agua = 180 },
                new DosificacionConcreto { WARBIMPRO = "1 - 2 - 3", ResistenciaKgCm2 = 226, ResistenciaPSI = 3224, ResistenciaMpa = 22, Cemento = 350, Arena = 0.55, Grava = 0.84, Agua = 170 },
                new DosificacionConcreto { WARBIMPRO = "1 - 2 - 3,5", ResistenciaKgCm2 = 210, ResistenciaPSI = 3000, ResistenciaMpa = 20, Cemento = 320, Arena = 0.52, Grava = 0.90, Agua = 170 },
                new DosificacionConcreto { WARBIMPRO = "1 - 2 - 4,3", ResistenciaKgCm2 = 200, ResistenciaPSI = 2850, ResistenciaMpa = 19, Cemento = 300, Arena = 0.48, Grava = 0.95, Agua = 158 },
                new DosificacionConcreto { WARBIMPRO = "1 - 2,5 - 4", ResistenciaKgCm2 = 189, ResistenciaPSI = 2700, ResistenciaMpa = 18, Cemento = 280, Arena = 0.55, Grava = 0.89, Agua = 158 },
                new DosificacionConcreto { WARBIMPRO = "1 - 3 - 3", ResistenciaKgCm2 = 168, ResistenciaPSI = 2400, ResistenciaMpa = 16, Cemento = 300, Arena = 0.72, Grava = 0.72, Agua = 158 },
                new DosificacionConcreto { WARBIMPRO = "1 - 3 - 4", ResistenciaKgCm2 = 159, ResistenciaPSI = 2275, ResistenciaMpa = 15, Cemento = 260, Arena = 0.63, Grava = 0.83, Agua = 163 },
                new DosificacionConcreto { WARBIMPRO = "1 - 3 - 5", ResistenciaKgCm2 = 140, ResistenciaPSI = 2000, ResistenciaMpa = 14, Cemento = 230, Arena = 0.55, Grava = 0.92, Agua = 148 },
                new DosificacionConcreto { WARBIMPRO = "1 - 3 - 6", ResistenciaKgCm2 = 119, ResistenciaPSI = 1700, ResistenciaMpa = 12, Cemento = 110, Arena = 0.50, Grava = 0.90, Agua = 143 },
                new DosificacionConcreto { WARBIMPRO = "1 - 4 - 7", ResistenciaKgCm2 = 109, ResistenciaPSI = 1560, ResistenciaMpa = 11, Cemento = 175, Arena = 0.55, Grava = 0.98, Agua = 133 },
                new DosificacionConcreto { WARBIMPRO = "1 - 4 - 8", ResistenciaKgCm2 = 99, ResistenciaPSI = 1420, ResistenciaMpa = 10, Cemento = 160, Arena = 0.55, Grava = 0.90, Agua = 125 },

            };
        }

        public Dictionary<string, double> EstimarMateriales(double volumen, string resistencia)
        {
            var dosificacion = _dosificaciones.FirstOrDefault(d => d.ResistenciaPSI.ToString() == resistencia);
            if (dosificacion == null) return null;          

            return new Dictionary<string, double>
            {
                { "Cemento", volumen* Math.Round((double)dosificacion.Cemento, 3) },
                { "Arena", volumen * dosificacion.Arena },
                { "Grava", volumen * dosificacion.Grava },
                { "Agua", volumen * dosificacion.Agua }
            };
           
        }
        public Dictionary<string, double> CalcularCantidadesMaterial()
        {
            var cantidadesTotales = new Dictionary<string, double>
            {
                { "Cemento", 0 },
                { "Arena", 0 },
                { "Grava", 0 },
                { "Agua", 0 }
            };

            foreach (var data in ContenidoTabla)
            {
                double volumen = double.Parse(data.VOLUMEN);
                var materiales = EstimarMateriales(volumen, "280"); // Ejemplo: resistencia de 280 kg/cm2

                cantidadesTotales["Cemento"] += materiales["Cemento"];
                cantidadesTotales["Arena"] += materiales["Arena"];
                cantidadesTotales["Grava"] += materiales["Grava"];
                cantidadesTotales["Agua"] += materiales["Agua"];
            }

            return cantidadesTotales;
        }
        private void CargarDatos()
        {
            var doc = _uidoc.Document;
            List<Element> list = RevitUtils.SelectElementMetrados(doc);
           
            var builtInParameterFloor = BuiltInParameter.SCHEDULE_LEVEL_PARAM;
            var builtInParameterWall = BuiltInParameter.WALL_BASE_CONSTRAINT;
            var builtInParameterColumn = BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM;
            var builtInParameterVigas = BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM;

#if REVIT2024_OR_GREATER

            List<string> nivelElementos = new List<string>();
            foreach (var element in list)
            {
                if ((BuiltInCategory)(int)element.Category.Id.Value == BuiltInCategory.OST_StructuralFoundation ||
                    (BuiltInCategory)(int)element.Category.Id.Value == BuiltInCategory.OST_Floors)
                {
                    nivelElementos.Add(element.get_Parameter(builtInParameterFloor).AsValueString());
                }
                else if ((BuiltInCategory)(int)element.Category.Id.Value == BuiltInCategory.OST_Walls)
                {
                    nivelElementos.Add(element.get_Parameter(builtInParameterWall).AsValueString());
                }
                else if ((BuiltInCategory)(int)element.Category.Id.Value== BuiltInCategory.OST_StructuralColumns)
                {
                    nivelElementos.Add(element.get_Parameter(builtInParameterColumn).AsValueString());
                }
                else if ((BuiltInCategory)(int)element.Category.Id.Value == BuiltInCategory.OST_StructuralFraming)
                {
                    nivelElementos.Add(element.get_Parameter(builtInParameterVigas).AsValueString());
                }
            }

            var nivelesUnicos = nivelElementos.Distinct().ToList();
            //RevitUtils.MostrarElementosStringEnTaskDialog(nivelElementos);
            TiposElementosNivel = new Dictionary<string, List<string>>();

            foreach (var nivel in nivelesUnicos)
            {
                List<string> existCategory = new List<string>();
                foreach (var elemento in list)
                {
                    if ((BuiltInCategory)(int)elemento.Category.Id.Value == BuiltInCategory.OST_StructuralFoundation ||
                        (BuiltInCategory)(int)elemento.Category.Id.Value == BuiltInCategory.OST_Floors)
                    {
                        if (elemento.get_Parameter(builtInParameterFloor).AsValueString() == nivel)
                        {
                            existCategory.Add(elemento.Category.Name);
                        }
                    }
                    else if ((BuiltInCategory)(int)elemento.Category.Id.Value == BuiltInCategory.OST_Walls)
                    {
                        if (elemento.get_Parameter(builtInParameterWall).AsValueString() == nivel)
                        {
                            existCategory.Add(elemento.Category.Name);
                        }
                    }
                    else if ((BuiltInCategory)(int)elemento.Category.Id.Value == BuiltInCategory.OST_StructuralColumns)
                    {
                        if (elemento.get_Parameter(builtInParameterColumn).AsValueString() == nivel)
                        {
                            existCategory.Add(elemento.Category.Name);
                        }
                    }
                    else if ((BuiltInCategory)(int)elemento.Category.Id.Value == BuiltInCategory.OST_StructuralFraming)
                    {
                        if (elemento.get_Parameter(builtInParameterVigas).AsValueString() == nivel)
                        {
                            existCategory.Add(elemento.Category.Name);
                        }
                    }
                }
                TiposElementosNivel.Add(nivel, existCategory.Distinct().ToList());
            }

#else
            List<string> nivelElementos = new List<string>();
            foreach (var element in list)
            {
                if ((BuiltInCategory)element.Category.Id.IntegerValue == BuiltInCategory.OST_StructuralFoundation ||
                    (BuiltInCategory)element.Category.Id.IntegerValue == BuiltInCategory.OST_Floors)
                {
                    nivelElementos.Add(element.get_Parameter(builtInParameterFloor).AsValueString());
                }
                else if ((BuiltInCategory)element.Category.Id.IntegerValue == BuiltInCategory.OST_Walls)
                {
                    nivelElementos.Add(element.get_Parameter(builtInParameterWall).AsValueString());
                }
                else if ((BuiltInCategory)element.Category.Id.IntegerValue == BuiltInCategory.OST_StructuralColumns)
                {
                    nivelElementos.Add(element.get_Parameter(builtInParameterColumn).AsValueString());
                }
                else if ((BuiltInCategory)element.Category.Id.IntegerValue == BuiltInCategory.OST_StructuralFraming)
                {
                    nivelElementos.Add(element.get_Parameter(builtInParameterVigas).AsValueString());
                }
            }

            var nivelesUnicos = nivelElementos.Distinct().ToList();
            TiposElementosNivel = new Dictionary<string, List<string>>();

            foreach (var nivel in nivelesUnicos)
            {
                List<string> existCategory = new List<string>();
                foreach (var elemento in list)
                {
                    if ((BuiltInCategory)elemento.Category.Id.IntegerValue == BuiltInCategory.OST_StructuralFoundation ||
                        (BuiltInCategory)elemento.Category.Id.IntegerValue == BuiltInCategory.OST_Floors)
                    {
                        if (elemento.get_Parameter(builtInParameterFloor).AsValueString() == nivel)
                        {
                            existCategory.Add(elemento.Category.Name);
                        }
                    }
                    else if ((BuiltInCategory)elemento.Category.Id.IntegerValue == BuiltInCategory.OST_Walls)
                    {
                        if (elemento.get_Parameter(builtInParameterWall).AsValueString() == nivel)
                        {
                            existCategory.Add(elemento.Category.Name);
                        }
                    }
                    else if ((BuiltInCategory)elemento.Category.Id.IntegerValue == BuiltInCategory.OST_StructuralColumns)
                    {
                        if (elemento.get_Parameter(builtInParameterColumn).AsValueString() == nivel)
                        {
                            existCategory.Add(elemento.Category.Name);
                        }
                    }
                    else if ((BuiltInCategory)elemento.Category.Id.IntegerValue == BuiltInCategory.OST_StructuralFraming)
                    {
                        if (elemento.get_Parameter(builtInParameterVigas).AsValueString() == nivel)
                        {
                            existCategory.Add(elemento.Category.Name);
                        }
                    }
                }
                TiposElementosNivel.Add(nivel, existCategory.Distinct().ToList());
            }
#endif
            var interfaz = new ViewResistencia();
            interfaz.ShowDialog();
            
            if(string.IsNullOrEmpty(interfaz.Resistecia ) || string.IsNullOrEmpty(interfaz.Cemento))
            {
                MessageBox.Show("Por favor, seleccione una resistencia y un tipo de cemento.");
                return;
            }

            ContenidoTabla = new List<Data>();
            for (int i = 0; i < nivelElementos.Count; i++)
            {
                if (list[i].Category.Name == "Title Blocks")
                {
                    continue;
                }
                double volumen = Tools.Feet3_to_m3(list[i].get_Parameter(BuiltInParameter.HOST_VOLUME_COMPUTED).AsDouble());
                var materiales = EstimarMateriales(volumen, interfaz.Resistecia); 

                ContenidoTabla.Add(new Data
                {
                    ID = list[i].Id.ToString(),
                    NIVEL = nivelElementos[i],
                    CATEGORIA = list[i].Category.Name,
                    AREA = Tools.Feet2_to_m2(list[i].get_Parameter(BuiltInParameter.HOST_AREA_COMPUTED).AsDouble()).ToString("F3"),
                    VOLUMEN = volumen.ToString("F3"),
                    Cemento = Math.Round(materiales["Cemento"]/ double.Parse(interfaz.Cemento), 3),
                    Arena = Math.Round(materiales["Arena"], 3),
                    Grava = Math.Round(materiales["Grava"], 3),
                    Agua = Math.Round(materiales["Agua"], 3)
                });
            }
        }

#if !REVIT2024_OR_GREATER
        public void SeleccionarElementosRevit(List<string> ids)
        {
            ICollection<ElementId> coleccion = _uidoc.Selection.GetElementIds();
            coleccion.Clear();
            foreach (string id in ids)
            {
                ElementId idSeleccionado = new ElementId(int.Parse(id));
                coleccion.Add(idSeleccionado);
            }
            _uidoc.Selection.SetElementIds(coleccion);
        }

#else
        public void SeleccionarElementosRevit(List<string> ids)
        {
            ICollection<ElementId> coleccion = _uidoc.Selection.GetElementIds();
            coleccion.Clear();
            foreach (string id in ids)
            {
                ElementId idSeleccionado = new ElementId(long.Parse(id));
                coleccion.Add(idSeleccionado);
            }
            _uidoc.Selection.SetElementIds(coleccion);
        }

#endif
    }
}
