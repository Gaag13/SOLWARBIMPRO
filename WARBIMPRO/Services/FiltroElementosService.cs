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
                new CategoryItem { Name = "Muros",       Icon = "🧱", GroupName = "Estructura",   BuiltInCategory = BuiltInCategory.OST_Walls } ,
                new CategoryItem { Name = "Muros",       Icon = "🧱", GroupName = "Estructura",   BuiltInCategory = BuiltInCategory.OST_StackedWalls },
                new CategoryItem { Name = "Columnas",    Icon = "🏛",  GroupName = "Estructura",   BuiltInCategory = BuiltInCategory.OST_StructuralColumns },
                new CategoryItem { Name = "Vigas",       Icon = "━",  GroupName = "Estructura",   BuiltInCategory = BuiltInCategory.OST_StructuralFraming },
                new CategoryItem { Name = "Losas",       Icon = "⬜", GroupName = "Estructura",   BuiltInCategory = BuiltInCategory.OST_Floors },
                new CategoryItem { Name = "Fundaciones", Icon = "🔷", GroupName = "Estructura",   BuiltInCategory = BuiltInCategory.OST_StructuralFoundation },
                // ARQUITECTURA
                new CategoryItem { Name = "Puertas",     Icon = "🚪", GroupName = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Doors },
                new CategoryItem { Name = "Ventanas",    Icon = "🪟", GroupName = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Windows },
                new CategoryItem { Name = "Escaleras",   Icon = "🪜", GroupName = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Stairs },
                new CategoryItem { Name = "Techos",      Icon = "🏠", GroupName = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Roofs },
                new CategoryItem { Name = "Mobiliario",  Icon = "🛋",  GroupName = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Furniture },
                new CategoryItem { Name = "Rampas",      Icon = "📐", GroupName = "Arquitectura", BuiltInCategory = BuiltInCategory.OST_Ramps },
            };
        }



        // ─── Obtener elementos según filtro ─────────────────────────────────
        public List<ElementId> GetFilteredElementIds(
            IEnumerable<LevelItem> selectedLevels,
            IEnumerable<CategoryItem> selectedCategories,
            bool allModel)
        {
#if !REVIT2024_OR_GREATER
         var result = new List<ElementId>();
                    var levelIds = selectedLevels
                        .Select(l => new ElementId(int.Parse(l.Id)))
                        .ToList();
#else
         var result = new List<ElementId>();
                            var levelIds = selectedLevels
                                .Select(l => new ElementId(long.Parse(l.Id)))
                                .ToList();
#endif



            foreach (var cat in selectedCategories)
            {
                var collector = new FilteredElementCollector(_doc)
                    .OfCategory(cat.BuiltInCategory)
                    .WhereElementIsNotElementType();

                IEnumerable<Element> elements = collector;

                if (!allModel && levelIds.Any())
                {
                    elements = collector.Where(e =>
                        e.LevelId != null &&
                        levelIds.Contains(e.LevelId));
                }

                result.AddRange(elements.Select(e => e.Id));
            }

            return result.Distinct().ToList();
        }




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

            // ── Superficie ────────────────────────────────────────────────
            ogs.SetSurfaceForegroundPatternColor(revitColor);
            ogs.SetSurfaceForegroundPatternId(solidPatternId);
            ogs.SetSurfaceForegroundPatternVisible(true);

            ogs.SetSurfaceBackgroundPatternColor(revitColor);
            ogs.SetSurfaceBackgroundPatternId(solidPatternId);
            ogs.SetSurfaceBackgroundPatternVisible(true);

            // ── Corte (cuando el elemento se corta en planta/sección) ─────
            ogs.SetCutForegroundPatternColor(revitColor);
            ogs.SetCutForegroundPatternId(solidPatternId);
            ogs.SetCutForegroundPatternVisible(true);

            ogs.SetCutBackgroundPatternColor(revitColor);
            ogs.SetCutBackgroundPatternId(solidPatternId);
            ogs.SetCutBackgroundPatternVisible(true);

            // ── Líneas ────────────────────────────────────────────────────
            ogs.SetProjectionLineColor(revitColor);
            ogs.SetCutLineColor(revitColor);

            // ── Transparencia ─────────────────────────────────────────────
            ogs.SetSurfaceTransparency(transparency);

            using (var tx = new Transaction(_doc, "Aplicar Color - FiltroElementos"))
            {
                tx.Start();
                foreach (var id in elementIds)
                    view.SetElementOverrides(id, ogs);
                tx.Commit();
            }
        }

        // Busca el patrón sólido que Revit trae por defecto
        private ElementId GetSolidFillPatternId()
        {
            var solidPattern = new FilteredElementCollector(_doc)
                .OfClass(typeof(FillPatternElement))
                .Cast<FillPatternElement>()
                .FirstOrDefault(fp => fp.GetFillPattern().IsSolidFill);

            // Si no encuentra ninguno (raro pero posible), retorna InvalidElementId
            // y Revit usará el patrón por defecto
            return solidPattern?.Id ?? ElementId.InvalidElementId;
        }

        // ─── Aislar elementos en vista activa ────────────────────────────────
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

        // ─── Resetear overrides de la vista ─────────────────────────────────
        public void ResetOverrides()
        {
            var uidoc = _uiApp.ActiveUIDocument;
            var view = uidoc.ActiveView;

            using (var tx = new Transaction(_doc, "Reset Overrides - FiltroElementos"))
            {
                tx.Start();

                // Quitar aislamiento si está activo
                if (view.IsInTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate))
                    view.DisableTemporaryViewMode(TemporaryViewMode.TemporaryHideIsolate);

                // Quitar overrides a todos los elementos de la vista
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