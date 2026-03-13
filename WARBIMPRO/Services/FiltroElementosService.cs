using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
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
                new CategoryItem { Name = "Muros",       Icon = "🧱", TapGroup = "Estructura",   BuiltInCategory = BuiltInCategory.OST_Walls,               SupportTypes = false },
                new CategoryItem { Name = "Muros Comp.", Icon = "🧱", TapGroup = "Estructura",   BuiltInCategory = BuiltInCategory.OST_StackedWalls,        SupportTypes = false },
                new CategoryItem { Name = "Columnas",    Icon = "🏛",  TapGroup = "Estructura",   BuiltInCategory = BuiltInCategory.OST_StructuralColumns,   SupportTypes = true  },
                new CategoryItem { Name = "Vigas",       Icon = "━",  TapGroup = "Estructura",   BuiltInCategory = BuiltInCategory.OST_StructuralFraming,   SupportTypes = true  },
                new CategoryItem { Name = "Losas",       Icon = "⬜", TapGroup = "Estructura",   BuiltInCategory = BuiltInCategory.OST_Floors,              SupportTypes = true  },
                new CategoryItem { Name = "Fundaciones", Icon = "🔷", TapGroup = "Estructura",   BuiltInCategory = BuiltInCategory.OST_StructuralFoundation,SupportTypes = true  },
                new CategoryItem { Name = "Refuerzo",    Icon = "⬡",  TapGroup = "Estructura",   BuiltInCategory = BuiltInCategory.OST_Rebar,               SupportTypes = true  },
                // ARQUITECTURA
                new CategoryItem { Name = "Puertas",     Icon = "🚪", TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Doors,               SupportTypes = true  },
                new CategoryItem { Name = "Ventanas",    Icon = "🪟", TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Windows,             SupportTypes = true  },
                new CategoryItem { Name = "Escaleras",   Icon = "🪜", TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Stairs,              SupportTypes = false },
                new CategoryItem { Name = "Techos",      Icon = "🏠", TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Roofs,               SupportTypes = true  },
                new CategoryItem { Name = "Mobiliario",  Icon = "🛋",  TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Furniture,           SupportTypes = true  },
                new CategoryItem { Name = "Rampas",      Icon = "📐", TapGroup = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Ramps,               SupportTypes = false },
                 //MEP
                new CategoryItem { Name = "Ductos",      Icon = "💨", TapGroup = "MEP",          BuiltInCategory = BuiltInCategory.OST_DuctCurves,          SupportTypes = true  },
                new CategoryItem { Name = "Tuberías",    Icon = "🔵", TapGroup = "MEP",          BuiltInCategory = BuiltInCategory.OST_PipeCurves,          SupportTypes = true  },
                new CategoryItem { Name = "Bandejas",    Icon = "📦", TapGroup = "MEP",          BuiltInCategory = BuiltInCategory.OST_CableTray,           SupportTypes = true  },
                new CategoryItem { Name = "Eq. Mecánico",Icon = "⚙",  TapGroup = "MEP",          BuiltInCategory = BuiltInCategory.OST_MechanicalEquipment, SupportTypes = true  },
                new CategoryItem { Name = "Luminarias",  Icon = "💡", TapGroup = "MEP",          BuiltInCategory = BuiltInCategory.OST_LightingFixtures,    SupportTypes = true  },
                new CategoryItem { Name = "Plomeria",    Icon = "⚔", TapGroup = "MEP",          BuiltInCategory = BuiltInCategory.OST_PlumbingFixtures,    SupportTypes = true  },
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
            var result   = new List<ElementId>();
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
    }
}
