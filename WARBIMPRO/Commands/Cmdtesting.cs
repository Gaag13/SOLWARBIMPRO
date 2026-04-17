
using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WARBIMPRO.Commands
{
    [Transaction(TransactionMode.Manual)]

    public class Cmdtesting : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var vista = doc.ActiveView;
            using (Transaction trans = new Transaction(doc, "Crear Filtro con Reglas"))
            {
                trans.Start();

                try
                {
                    // 1. Definir las categorías que aplicarán al filtro
                    // En tu caso parece ser Structural Columns (columnas estructurales)
                    List<ElementId> categorias = new List<ElementId>();
                    categorias.Add(new ElementId(BuiltInCategory.OST_StructuralColumns));
                    // Si necesitas otras categorías, agrégalas aquí

                    // 2. REGLA 1: Type Name equals "WBP_C1(0.3X0.3)m"
                    // Para el nombre del tipo, usamos ALL_MODEL_TYPE_NAME o ELEM_TYPE_PARAM
                    ElementId typeNameParamId = new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME);
                    ParameterValueProvider typeNameProvider = new ParameterValueProvider(typeNameParamId);
                    FilterStringRuleEvaluator equalsEvaluator = new FilterStringEquals();
                    string typeNameValue = "WBP_C1(0.3X0.3)m";

                    // En Revit 2022+ no se usa el parámetro caseSensitive
                    // Si usas Revit 2021 o anterior, descomenta la línea de abajo
                    // FilterStringRule typeNameRule = new FilterStringRule(typeNameProvider, equalsEvaluator, typeNameValue, false);

                    // Para Revit 2022+:
                    FilterStringRule typeNameRule = new FilterStringRule(typeNameProvider, equalsEvaluator, typeNameValue);

                    // 3. REGLA 2: Base Level equals "Nivel 1-EST"
                    // Primero necesitamos obtener el ElementId del nivel
                    Level nivelBase = ObtenerNivelPorNombre(doc, "Nivel 1-EST");

                    if (nivelBase == null)
                    {
                        TaskDialog.Show("Error", "No se encontró el nivel 'Nivel 1-EST'");
                        trans.RollBack();
                        return Result.Failed;
                    }

                    // Para Base Level, el BuiltInParameter depende del tipo de elemento
                    // Para columnas estructurales, usualmente es FAMILY_BASE_LEVEL_PARAM o INSTANCE_REFERENCE_LEVEL_PARAM
                    // Vamos a intentar con FAMILY_BASE_LEVEL_PARAM primero
                    ElementId baseLevelParamId = new ElementId(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM);
                    ParameterValueProvider baseLevelProvider = new ParameterValueProvider(baseLevelParamId);
                    FilterNumericRuleEvaluator equalsIdEvaluator = new FilterNumericEquals();
                    ElementId levelId = nivelBase.Id;

                    FilterElementIdRule baseLevelRule = new FilterElementIdRule(baseLevelProvider, equalsIdEvaluator, levelId);

                    // 4. COMBINAR LAS REGLAS CON AND
                    // Para Revit 2020+, debemos usar ElementFilter en lugar de List<FilterRule>

                    // Opción A: Crear un ElementParameterFilter para cada regla y combinar con LogicalAndFilter
                    ElementParameterFilter filter1 = new ElementParameterFilter(typeNameRule);
                    ElementParameterFilter filter2 = new ElementParameterFilter(baseLevelRule);

                    List<ElementFilter> filters = new List<ElementFilter>();
                    filters.Add(filter1);
                    filters.Add(filter2);

                    LogicalAndFilter andFilter = new LogicalAndFilter(filters);

                    // 5. CREAR EL PARAMETERFILTERELEMENT
                    ParameterFilterElement filterElement = ParameterFilterElement.Create(
                        doc,
                        "Filtro WBP Niveal 1-EST",  // Nombre del filtro
                        categorias,
                        andFilter
                    );

                    // 6. APLICAR EL FILTRO A LA VISTA (OPCIONAL)
                    if (vista != null)
                    {
                        vista.AddFilter(filterElement.Id);
                        vista.SetFilterVisibility(filterElement.Id, true);

                        // Opcional: Configurar override gráfico
                        OverrideGraphicSettings overrides = new OverrideGraphicSettings();
                        // Ejemplo: Cambiar color de líneas a rojo
                        // overrides.SetProjectionLineColor(new Color(255, 0, 0));
                        vista.SetFilterOverrides(filterElement.Id, overrides);
                    }

                    trans.Commit();
                    TaskDialog.Show("Éxito", "Filtro creado correctamente");
                }
                catch (System.Exception ex)
                {
                    trans.RollBack();
                    TaskDialog.Show("Error", "Error al crear filtro: " + ex.Message);
                }
            }

           

            return Result.Succeeded;
        }

        /// <summary>
        /// Método auxiliar para obtener un nivel por su nombre
        /// </summary>
        private Level ObtenerNivelPorNombre(Document doc, string nombreNivel)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Level));

            foreach (Level level in collector)
            {
                if (level.Name == nombreNivel)
                {
                    return level;
                }
            }

            return null;
        }
    }




} 


