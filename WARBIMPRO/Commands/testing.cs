using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WARBIMPRO.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class TestFiltro: IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var doc = commandData.Application.ActiveUIDocument.Document;
            var view = doc.ActiveView;

            using (var tx = new Transaction(doc, "Filtro Nivel 2"))
            {
                tx.Start();

                // 🔹 Categoría: Columnas
                var cat = Category.GetCategory(doc, BuiltInCategory.OST_StructuralColumns);
                var catIds = new List<ElementId> { cat.Id };

                // 🔹 Obtener NIVEL 2 (real del modelo)
                var level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .FirstOrDefault(l => l.Name.Contains("Nivel 2"));

                if (level == null)
                {
                    TaskDialog.Show("Error", "No se encontró Nivel 2");
                    return Result.Failed;
                }

                // 🔹 Obtener una columna REAL para debug
                var sample = new FilteredElementCollector(doc)
                    .OfCategory(BuiltInCategory.OST_StructuralColumns)
                    .WhereElementIsNotElementType()
                    .FirstElement();

                if (sample == null)
                {
                    TaskDialog.Show("Error", "No hay columnas en el modelo");
                    return Result.Failed;
                }

                // 🔥 TEST DE PARÁMETROS (CLAVE)
                bool p1 = ParameterFilterUtilities.IsParameterApplicable(sample, new ElementId(BuiltInParameter.LEVEL_PARAM));
                bool p2 = ParameterFilterUtilities.IsParameterApplicable(sample, new ElementId(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM));
                bool p3 = ParameterFilterUtilities.IsParameterApplicable(sample, new ElementId(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM));

                TaskDialog.Show("DEBUG PARAMS",
                    $"LEVEL_PARAM: {p1}\n" +
                    $"INSTANCE_REFERENCE_LEVEL_PARAM: {p2}\n" +
                    $"FAMILY_BASE_LEVEL_PARAM: {p3}");

                // 🔹 Regla tipo (OK)
                var typeRule = ParameterFilterRuleFactory.CreateEqualsRule(
                    new ElementId(BuiltInParameter.SYMBOL_NAME_PARAM),
                    "WBP_C1(0.3X0.3)m");

                // 🔥 USA SOLO EL QUE DÉ TRUE ARRIBA
                var levelRule = ParameterFilterRuleFactory.CreateEqualsRule(
                    new ElementId(BuiltInParameter.LEVEL_PARAM), // 👈 ESTE ES EL QUE VAMOS A PROBAR
                    level.Id);

                var rules = new List<FilterRule> { typeRule, levelRule };

                var elementFilter = new ElementParameterFilter(rules);

                var filter = ParameterFilterElement.Create(
                    doc,
                    "TEST_NIVEL_2",
                    catIds,
                    elementFilter);

                view.AddFilter(filter.Id);

                tx.Commit();
            }

            return Result.Succeeded;
        }
    }
}

