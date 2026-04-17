using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Nice3point.Revit.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Xml.Linq;
using WARBIMPRO.Models;

namespace WARBIMPRO.Services
{
    public class FiltroElementosService
    {
        private readonly UIApplication _uiApp;
        private readonly Document _doc;
        public FiltroElementosService(UIApplication uiApp)
        {
            _uiApp = uiApp;
            _doc = uiApp.ActiveUIDocument.Document;
        }

#if REVIT2024_OR_GREATER
        // ─── Cargar niveles del modelo ───────────────────────────────────────
        public List<LevelItem> GetLevels()
        {
            return new FilteredElementCollector(_doc)
            .OfClass(typeof(Level))
            .Cast<Level>()
            .OrderBy(l => l.Elevation)
            .Select(l => new LevelItem
            {
                Id = l.Id.Value.ToString(),
                Name = l.Name,
                Elevation = UnitUtils.ConvertFromInternalUnits(
            l.Elevation, UnitTypeId.Meters)
            })
            .ToList();
        }
#else
        // ─── Cargar niveles del modelo ───────────────────────────────────────
        public List<LevelItem> GetLevels()
        {
            return new FilteredElementCollector(_doc)
                .OfClass(typeof(Level))
                .Cast<Level>()
                .OrderBy(l => l.Elevation)
                .Select(l => new LevelItem
                {
                    Id = l.Id.IntegerValue.ToString(),
                    Name = l.Name,
                    Elevation = UnitUtils.ConvertFromInternalUnits(
                        l.Elevation, UnitTypeId.Meters)
                })
                .ToList();
        }
#endif
        // ─── Categorías disponibles ──────────────────────────────────────────
        public List<CategoryItem> GetAvailableCategories()
        {
            return new List<CategoryItem>
              {
                // ESTRUCTURA
              new CategoryItem { Name = "Muros", Icon = "🧱", TapGroup = "Estructura", BuiltInCategory = BuiltInCategory.OST_Walls, SupportTypes = false },
              new CategoryItem { Name = "Muros Comp.", Icon = "▓", TapGroup = "Estructura", BuiltInCategory = BuiltInCategory.OST_StackedWalls, SupportTypes = false },
              new CategoryItem { Name = "Columnas", Icon = "🏛", TapGroup = "Estructura", BuiltInCategory = BuiltInCategory.OST_StructuralColumns, SupportTypes = true },
              new CategoryItem { Name = "Vigas", Icon = "━", TapGroup = "Estructura", BuiltInCategory = BuiltInCategory.OST_StructuralFraming, SupportTypes = true },
              new CategoryItem { Name = "Losas", Icon = "⬜", TapGroup = "Estructura", BuiltInCategory = BuiltInCategory.OST_Floors, SupportTypes = true },
              new CategoryItem { Name = "Fundaciones", Icon = "🔷", TapGroup = "Estructura", BuiltInCategory = BuiltInCategory.OST_StructuralFoundation,SupportTypes = true },
              new CategoryItem { Name = "Refuerzo", Icon = "⬡", TapGroup = "Estructura", BuiltInCategory = BuiltInCategory.OST_Rebar, SupportTypes = true },
                // ARQUITECTURA
                
              new CategoryItem { Name = "Ventanas", Icon = "🪟", TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Windows, SupportTypes = true },
              new CategoryItem { Name = "Escaleras", Icon = "🪜", TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Stairs, SupportTypes = false },
              new CategoryItem { Name = "Techos", Icon = "🏠", TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Roofs, SupportTypes = true },
              new CategoryItem { Name = "Mobiliario", Icon = "🛋", TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Furniture, SupportTypes = true },
              new CategoryItem { Name = "Rampas", Icon = "📐", TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Ramps, SupportTypes = false },
              //MEP
              new CategoryItem { Name = "Ductos", Icon = "💨", TapGroup = "MEP", BuiltInCategory = BuiltInCategory.OST_DuctCurves, SupportTypes = true },
              new CategoryItem { Name = "Tuberías", Icon = "🔵", TapGroup = "MEP", BuiltInCategory = BuiltInCategory.OST_PipeCurves, SupportTypes = true },
              new CategoryItem { Name = "Accesorios\nTuberia", Icon = "⚡", TapGroup = "MEP",BuiltInCategory = BuiltInCategory.OST_PipeFitting, SupportTypes = true },
              new CategoryItem { Name = "Bandejas", Icon = "📦", TapGroup = "MEP", BuiltInCategory = BuiltInCategory.OST_CableTray, SupportTypes = true },
              new CategoryItem { Name = "Eq. Mecánico",Icon = "⚙", TapGroup = "MEP", BuiltInCategory = BuiltInCategory.OST_MechanicalEquipment, SupportTypes = true },
              new CategoryItem { Name = "Luminarias", Icon = "💡", TapGroup = "MEP", BuiltInCategory = BuiltInCategory.OST_LightingFixtures, SupportTypes = true },
              new CategoryItem { Name = "Plomeria", Icon = "⚔", TapGroup = "MEP", BuiltInCategory = BuiltInCategory.OST_PlumbingFixtures, SupportTypes = true },
              };
        }
        // ─── Obtener tipos de familia para una categoría ─────────────────────
        public List<TypeItem> GetTypesForCategory(BuiltInCategory category)
        {
            var typeIds = new FilteredElementCollector(_doc)
            .OfCategory(category)
            .WhereElementIsNotElementType()
            .ToElements()
            .Select(e => e.GetTypeId())
            .Where(id => id != null && id != ElementId.InvalidElementId)
            .Distinct()
            .ToList();
            var result = new List<TypeItem>();
            foreach (var typeId in typeIds)
            {
                var typeElem = _doc.GetElement(typeId);
                if (typeElem == null) continue;
                var count = new FilteredElementCollector(_doc)
                .OfCategory(category)
                .WhereElementIsNotElementType()
                .Count(e => e.GetTypeId() == typeId);
                result.Add(new TypeItem
                {
                    Name = typeElem.Name,
                    TypeId = typeId,
                    ElementCount = count
                });
            }
            return result.OrderBy(t => t.Name).ToList();
        }
        // ─── Helper: obtiene el LevelId según el tipo de elemento ────────────
        private ElementId GetElementLevelId(Element element)
        {
            if (element.LevelId != null && element.LevelId != ElementId.InvalidElementId)
                return element.LevelId;
            var refLevel = element.get_Parameter(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);
            if (refLevel?.AsElementId() is ElementId rid && rid != ElementId.InvalidElementId)
                return rid;
            var schedLevel = element.get_Parameter(BuiltInParameter.SCHEDULE_LEVEL_PARAM);
            if (schedLevel?.AsElementId() is ElementId sid && sid != ElementId.InvalidElementId)
                return sid;
            var baseLevel = element.get_Parameter(BuiltInParameter.FAMILY_BASE_LEVEL_PARAM);
            if (baseLevel?.AsElementId() is ElementId bid && bid != ElementId.InvalidElementId)
                return bid;
            return ElementId.InvalidElementId;
        }
        // ─── Obtener elementos según filtro ──────────────────────────────────
        public List<ElementId> GetFilteredElementIds(
          IEnumerable<LevelItem> selectedLevels,
          IEnumerable<CategoryItem> selectedCategories,
          bool allModel,
          IEnumerable<ElementId> selectedTypeIds = null)
        {
#if !REVIT2024_OR_GREATER
            var result   = new List<ElementId>();
            var levelIds = selectedLevels
                .Select(l => new ElementId(int.Parse(l.Id)))
                .ToHashSet();
#else
            var result = new List<ElementId>();
            var levelIds = selectedLevels
            .Select(l => new ElementId(long.Parse(l.Id)))
            .ToHashSet();
#endif
            var typeIdSet = selectedTypeIds?.ToHashSet();
            bool filterByType = typeIdSet != null && typeIdSet.Any();

            foreach (var cat in selectedCategories)
            {
                var collector = new FilteredElementCollector(_doc)
                .OfCategory(cat.BuiltInCategory)
                .WhereElementIsNotElementType();
                IEnumerable<Element> elements = collector;
                // Filtro por nivel
                if (!allModel && levelIds.Any())
                    elements = elements.Where(e => levelIds.Contains(GetElementLevelId(e)));
                // Filtro por tipo (opcional)
                if (filterByType)
                    elements = elements.Where(e => typeIdSet.Contains(e.GetTypeId()));
                result.AddRange(elements.Select(e => e.Id));
            }
            return result.Distinct().ToList();
        }
        // ─── Aplicar color ───────────────────────────────────────────────────
        public void ApplyColor(
          List<ElementId> elementIds,
          System.Windows.Media.Color wpfColor,
          int opacityPercent)
        {
            if (!elementIds.Any()) return;
            var view = _uiApp.ActiveUIDocument.ActiveView;
            var revitColor = new Autodesk.Revit.DB.Color(wpfColor.R, wpfColor.G, wpfColor.B);
            int transparency = 100 - opacityPercent;
            ElementId solidPatternId = GetSolidFillPatternId();
            var ogs = new OverrideGraphicSettings();
            ogs.SetSurfaceForegroundPatternColor(revitColor);
            ogs.SetSurfaceForegroundPatternId(solidPatternId);
            ogs.SetSurfaceForegroundPatternVisible(true);
            ogs.SetSurfaceBackgroundPatternColor(revitColor);
            ogs.SetSurfaceBackgroundPatternId(solidPatternId);
            ogs.SetSurfaceBackgroundPatternVisible(true);

            ogs.SetCutForegroundPatternColor(revitColor);
            ogs.SetCutForegroundPatternId(solidPatternId);
            ogs.SetCutForegroundPatternVisible(true);

            ogs.SetCutBackgroundPatternColor(revitColor);
            ogs.SetCutBackgroundPatternId(solidPatternId);
            ogs.SetCutBackgroundPatternVisible(true);

            ogs.SetProjectionLineColor(revitColor);
            ogs.SetCutLineColor(revitColor);
            ogs.SetSurfaceTransparency(transparency);

            using (var tx = new Transaction(_doc, "Aplicar Color - FiltroElementos"))
            {
                tx.Start();
                foreach (var id in elementIds)
                    view.SetElementOverrides(id, ogs);
                tx.Commit();
            }
        }
        private ElementId GetSolidFillPatternId()
        {
            var solidPattern = new FilteredElementCollector(_doc)
            .OfClass(typeof(FillPatternElement))
            .Cast<FillPatternElement>()
            .FirstOrDefault(fp => fp.GetFillPattern().IsSolidFill);

            return solidPattern?.Id ?? ElementId.InvalidElementId;
        }
        // ─── Aislar elementos ────────────────────────────────────────────────
        public void IsolateElements(List<ElementId> elementIds)
        {
            if (!elementIds.Any()) return;
            var view = _uiApp.ActiveUIDocument.ActiveView;
            using (var tx = new Transaction(_doc, "Aislar Elementos - FiltroElementos"))
            {
                tx.Start();
                view.IsolateElementsTemporary(elementIds);
                tx.Commit();
            }
        }
        // ─── Resetear overrides ──────────────────────────────────────────────
        public void ResetOverrides()
        {
            var uidoc = _uiApp.ActiveUIDocument;
            var view = uidoc.ActiveView;
            using (var tx = new Transaction(_doc, "Reset Overrides - FiltroElementos"))
            {
                tx.Start();
                if (view.IsInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate))
                    view.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);
                var collector = new FilteredElementCollector(_doc, view.Id)
                .WhereElementIsNotElementType();
                var emptyOgs = new OverrideGraphicSettings();
                foreach (var el in collector)
                    view.SetElementOverrides(el.Id, emptyOgs);
                if (view.ViewTemplateId != ElementId.InvalidElementId)
                {
                    var template = _doc.GetElement(view.ViewTemplateId) as View;
                    if (template == null)
                    {
                        var filters = template.GetFilters();
                        foreach (var fId in filters)
                        {
                            template.RemoveFilter(fId);
                        }
                    }
                }
                else
                {
                    // Si la vista no tiene template, también limpiar filtros de vista
                    var filters = view.GetFilters();
                    foreach (var fId in filters)
                    {
                        view.RemoveFilter(fId);
                    }
                }
                tx.Commit();
            }
        }
        // ─── Contar elementos por categoría ─────────────────────────────────
        public int CountElements(BuiltInCategory category, ElementId levelId = null)
        {
            var collector = new FilteredElementCollector(_doc)
            .OfCategory(category)
            .WhereElementIsNotElementType();
            if (levelId != null)
                return collector.Count(e => e.LevelId == levelId);
            return collector.Count();
        }

        public List<string> CreateViewFilters(
          IEnumerable<CategoryItem> selectedCategories,
          IEnumerable<LevelItem> selectedLevels,
          IEnumerable<TypeItem> selectedTypes,
          bool allModel,
          System.Windows.Media.Color wpfColor,
          int opacityPercent)
        {
            var view = _uiApp.ActiveUIDocument.ActiveView;
            var revitColor = new Autodesk.Revit.DB.Color(wpfColor.R, wpfColor.G, wpfColor.B);
            int transparency = 100 - opacityPercent;
            var solidId = GetSolidFillPatternId();

            var cats = selectedCategories.ToList();
            var levels = selectedLevels.ToList();
            var types = selectedTypes?.ToList() ?? new List<TypeItem>();
            bool hasTypes = types.Any();
            bool hasLevels = !allModel && levels.Any();

            // Preparar las listas de nombres para el mensaje
            string catNames = string.Join(", ", cats.Select(c => c.Name));
            string typeNames = hasTypes ? string.Join(", ", types.Select(t => t.Name)) : "Ninguno";
            string levelNames = hasLevels ? string.Join(", ", levels.Select(l => l.Name)) : (allModel ? "Todo el modelo" : "Ninguno");

            // Construir el mensaje principal
            string mainContent = $"Categorías ({cats.Count}): {catNames}\n" +
                $"Tipos ({types.Count}): {typeNames}\n" +
                $"Niveles ({levels.Count}): {levelNames}\n" +
                $"Modo: {(allModel ? "Todo el Modelo" : "Por Nivel")}";

            // Configurar y mostrar el TaskDialog
            TaskDialog mainDialog = new TaskDialog("Debug - WARBIMPRO")
            {
                MainInstruction = "Resumen de Selección para Filtros",
                MainContent = mainContent,
                CommonButtons = TaskDialogCommonButtons.Ok,
                DefaultButton = TaskDialogResult.Ok,
                MainIcon = TaskDialogIcon.TaskDialogIconInformation
            };
            mainDialog.Show();

            // Construir OverrideGraphicSettings del color
            var ogs = BuildOgs(revitColor, solidId, transparency);
            var createdNames = new List<string>();

            using var tx = new Transaction(_doc, "Crear Filtros Vista - WARBIMPRO");
            tx.Start();

            if (hasTypes)
            {
                int totalCreados = 0;
                int totalFallidos = 0;
                StringBuilder reporte = new StringBuilder();

                foreach (var cat in cats)
                {
                    Category category = Category.GetCategory(_doc, cat.BuiltInCategory);
                    if (category == null) continue;

                    var catIds = new List<ElementId> { category.Id };

                    // 🔥 Parámetro correcto por categoría (ya lo tienes bien)
                    ElementId levelParamId = GetFilterableLevelParameter(cat.BuiltInCategory);

                    foreach (var type in types)
                    {
                        if (type == null) continue;

                        // 🔹 REGLA TIPO (string)
                        var typeProvider = new ParameterValueProvider(
                            new ElementId(BuiltInParameter.ALL_MODEL_TYPE_NAME));

                        var typeRule = new FilterStringRule(
                            typeProvider,
                            new FilterStringEquals(),
                            type.Name);

                        var typeFilter = new ElementParameterFilter(typeRule);

                        foreach (var level in levels)
                        {
                            if (level == null) continue;

                            ElementId lvlId = GetIdFromString(level.Id);
                            if (lvlId == ElementId.InvalidElementId) continue;

                            var filterName = SanitizeName($"WBP_{cat.Name}_{type.Name}_{level.Name}");

                            try
                            {
                                // 🔹 REGLA NIVEL (ElementId)
                                var levelProvider = new ParameterValueProvider(levelParamId);

                                var levelRule = new FilterElementIdRule(
                                    levelProvider,
                                    new FilterNumericEquals(),
                                    lvlId);

                                var levelFilter = new ElementParameterFilter(levelRule);

                                // 🔥 AQUÍ ESTÁ LA MAGIA (lo que te faltaba)
                                var andFilter = new LogicalAndFilter(typeFilter, levelFilter);

                                var pfe = GetOrCreateFilter(filterName, catIds, andFilter);

                                if (pfe != null)
                                {
                                    ApplyFilterToView(view, pfe, ogs);
                                    createdNames.Add(filterName);
                                    reporte.AppendLine($"✓ {filterName}");
                                    totalCreados++;
                                }
                            }
                            catch (Exception ex)
                            {
                                totalFallidos++;
                                reporte.AppendLine($"✗ {filterName}: {ex.Message}");
                            }
                        }
                    }
                }

                TaskDialog.Show("Resultado",
                    $"Filtros creados: {totalCreados}\n" +
                    $"Fallidos: {totalFallidos}\n\n" +
                    $"Detalles:\n{reporte.ToString()}");
            }
            else
            {
                // Sin tipos seleccionados → un filtro por categoría + nivel
                foreach (var cat in cats)
                {
                    Category category = Category.GetCategory(_doc, cat.BuiltInCategory);
                    if (category == null) continue;
                    var catIds = new List<ElementId> { category.Id };

                    if (hasLevels)
                    {
                        // ⭐ Obtener parámetro específico para esta categoría
                        ElementId levelParamId = GetFilterableLevelParameter( cat.BuiltInCategory);

                        if (levelParamId != null)
                        {
                            foreach (var level in levels)
                            {
                                var lvlId = GetIdFromString(level.Id);
                                if (lvlId == ElementId.InvalidElementId) continue;

                                var levelName = _doc.GetElement(lvlId)?.Name ?? level.Name;
                                var filterName = SanitizeName($"WBP_{cat.Name}_{levelName}");

                                var rule = ParameterFilterRuleFactory.CreateEqualsRule(levelParamId, lvlId);
                                var pfe = GetOrCreateFilter(filterName, catIds,
                                    new ElementParameterFilter(rule));

                                ApplyFilterToView(view, pfe, ogs);
                                createdNames.Add(filterName);
                            }
                        }
                        else
                        {
                            // Categoría no soporta filtro por nivel
                            TaskDialog.Show("Advertencia",
                                $"La categoría {cat.Name} no soporta filtros por nivel en Revit");
                        }
                    }
                    else if (allModel)
                    {
                        // Solo categoría, sin reglas de parámetro (todo el modelo)
                        var filterName = SanitizeName($"WBP_{cat.Name}_Todo");
                        var pfe = GetOrCreateFilterNoRule(filterName, catIds);
                        ApplyFilterToView(view, pfe, ogs);
                        createdNames.Add(filterName);
                    }
                }
            }

            tx.Commit();
            return createdNames;
        }
        /// <summary>
        /// Obtiene el BuiltInParameter de nivel correcto y filtrable para una categoría específica.
        /// Este método usa un mapeo categoría-específico en lugar de una prioridad genérica.
        /// </summary>
       
        /// <summary>
        /// Método fallback: detecta dinámicamente el parámetro de nivel filtrable
        /// cuando el mapeo estático no funciona.
        /// </summary>
       

        private ElementId GetIdFromString(string id)
        {
#if REVIT2024_OR_GREATER
            return new ElementId(long.Parse(id));
#else
    return new ElementId(int.Parse(id));
#endif
        }
        private ElementId GetFilterableLevelParameter(BuiltInCategory category)
        {
            switch (category)
            {
                case BuiltInCategory.OST_StructuralColumns:
                    return new ElementId(BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM);

                case BuiltInCategory.OST_StructuralFraming:
                    return new ElementId(BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM);

                case BuiltInCategory.OST_StructuralFoundation:
                    return new ElementId(BuiltInParameter.LEVEL_PARAM);

                case BuiltInCategory.OST_Walls:
                    return new ElementId(BuiltInParameter.WALL_BASE_CONSTRAINT);

                case BuiltInCategory.OST_Floors:
                case BuiltInCategory.OST_Roofs:
                case BuiltInCategory.OST_Ceilings:
                    return new ElementId(BuiltInParameter.SCHEDULE_LEVEL_PARAM);

                case BuiltInCategory.OST_Doors:
                case BuiltInCategory.OST_Windows:
                case BuiltInCategory.OST_Furniture:
                    return new ElementId(BuiltInParameter.FAMILY_LEVEL_PARAM);

                case BuiltInCategory.OST_DuctCurves:
                case BuiltInCategory.OST_PipeCurves:
                case BuiltInCategory.OST_CableTray:
                    return new ElementId(BuiltInParameter.RBS_START_LEVEL_PARAM);

                default:
                    // 🔥 fallback seguro
                    return new ElementId(BuiltInParameter.LEVEL_PARAM);
            }
        }
        public string CreateViewTemplate(string templateName)
        {
            var view = _uiApp.ActiveUIDocument.ActiveView;
            var safeName = SanitizeName(templateName);
            using var tx = new Transaction(_doc, "Crear ViewTemplate - WARBIMPRO");
            tx.Start();
            // Duplicar la vista
            var newViewId = view.Duplicate(ViewDuplicateOption.Duplicate);
            var newView = _doc.GetElement(newViewId) as View;
            // Asegurarse de que el nombre sea único
            string finalName = safeName;
            int suffix = 1;
            while (new FilteredElementCollector(_doc)
            .OfClass(typeof(View))
            .Cast<View>()
            .Any(v => v.IsTemplate && v.Name == finalName))
            {
                finalName = $"{safeName}_{suffix++}";
            }
            // ✅ Así se crea un ViewTemplate en Revit API
            var templateId = view.CreateViewTemplate();
            var template = _doc.GetElement(templateId.Id) as View;
            template.Name = finalName;
            newView.ViewTemplateId = template.Id;
            tx.Commit();
            return finalName;
        }
        // ─── Helpers privados ────────────────────────────────────────────────
        private OverrideGraphicSettings BuildOgs(
            Autodesk.Revit.DB.Color color, ElementId solidId, int transparency)
        {
            var ogs = new OverrideGraphicSettings();
            ogs.SetSurfaceForegroundPatternColor(color); ogs.SetSurfaceForegroundPatternId(solidId); ogs.SetSurfaceForegroundPatternVisible(true);
            ogs.SetSurfaceBackgroundPatternColor(color); ogs.SetSurfaceBackgroundPatternId(solidId); ogs.SetSurfaceBackgroundPatternVisible(true);
            ogs.SetCutForegroundPatternColor(color); ogs.SetCutForegroundPatternId(solidId); ogs.SetCutForegroundPatternVisible(true);
            ogs.SetCutBackgroundPatternColor(color); ogs.SetCutBackgroundPatternId(solidId); ogs.SetCutBackgroundPatternVisible(true);
            ogs.SetProjectionLineColor(color);
            ogs.SetCutLineColor(color);
            ogs.SetSurfaceTransparency(transparency);
            return ogs;
        }
        private ElementFilter BuildLogicalAnd(List<FilterRule> rules)
        {
            // Convertimos cada regla en un ElementParameterFilter individual
            List<ElementFilter> filters = rules
              .Select(r => new ElementParameterFilter(r))
              .Cast<ElementFilter>()
              .ToList();
            if (filters.Count == 1) return filters[0];
            // Esto crea el contenedor AND que viste en la documentación
            return new LogicalAndFilter(filters);
        }
        private ParameterFilterElement GetOrCreateFilter(
        string name, List<ElementId> catIds, ElementFilter filter)
        {
            var existing = new FilteredElementCollector(_doc)
            .OfClass(typeof(ParameterFilterElement))
            .Cast<ParameterFilterElement>()
            .FirstOrDefault(f => f.Name == name);
            if (existing != null)
            {
                existing.SetElementFilter(filter);
                return existing;
            }
            return ParameterFilterElement.Create(_doc, name, catIds, filter);
        }
        private ParameterFilterElement GetOrCreateFilterNoRule(
        string name, List<ElementId> catIds)
        {
            var existing = new FilteredElementCollector(_doc)
            .OfClass(typeof(ParameterFilterElement))
            .Cast<ParameterFilterElement>()
            .FirstOrDefault(f => f.Name == name);
            if (existing != null) return existing;
            return ParameterFilterElement.Create(_doc, name, catIds);
        }
        private static void ApplyFilterToView(
        View view, ParameterFilterElement pfe, OverrideGraphicSettings ogs)
        {
            if (!view.GetFilters().Contains(pfe.Id))
                view.AddFilter(pfe.Id);
            view.SetFilterOverrides(pfe.Id, ogs);
            view.SetFilterVisibility(pfe.Id, true);
        }
        private static string SanitizeName(string name)
        {
            // Revit no permite estos caracteres en nombres de filtro
            var invalid = new[] { '\\', ':', '{', '}', '[', ']', '|', ';', '<', '>', '?', '`', '~' };
            foreach (var c in invalid)
                name = name.Replace(c, '_');
            return name.Length > 100 ? name.Substring(0, 100) : name;
        }
    }
}
