using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WARBIMPRO.Utils
{
    public static class RevitUtils
    {
        public static List<Element> SelectElementMetrados(Document doc)
        {
            // Filtrar muros estructurales
            var wallCollector = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .WherePasses(new ElementParameterFilter(
                    new FilterIntegerRule(
                        new ParameterValueProvider(new ElementId(BuiltInParameter.WALL_STRUCTURAL_SIGNIFICANT)),
                        new FilterNumericEquals(),
                        1)))
                .ToList();

            // Filtrar pisos
            var floorCollector = new FilteredElementCollector(doc).OfClass(typeof(Floor)).ToList();

            // Filtrar instancias de familias
            var familyInstanceCollector = new FilteredElementCollector(doc).OfClass(typeof(FamilyInstance)).ToList();

            // Combinar resultados
            List<Element> result = new List<Element>();
            result.AddRange(wallCollector);
            result.AddRange(floorCollector);
            result.AddRange(familyInstanceCollector);

            return result;
        }
        public static void MostrarElementosEnTaskDialog(List<Element> elementos)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Elementos seleccionados:");

            foreach (var element in elementos)
            {
                string elementName = element.Name;
                string elementId = element.Id.ToString();

                sb.AppendLine($"Elemento: {elementName}, ID: {elementId}");
            }
            TaskDialog.Show("Metrados - Elementos Seleccionados", sb.ToString());
        }
        public static void MostrarElementosStringEnTaskDialog(List<string> elementos)
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Elementos seleccionados:");

            foreach (var element in elementos)
            {

                sb.AppendLine(element);
            }
            TaskDialog.Show("Elementos ", sb.ToString());
        }
    }
}
