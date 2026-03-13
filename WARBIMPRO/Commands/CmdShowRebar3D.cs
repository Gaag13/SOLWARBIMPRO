using Nice3point.Revit.Toolkit.External;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CmdShowRebar3D : ExternalCommand
    {
        public override void Execute()
        {
            var activeView = Document.ActiveView;

            if (activeView == null)
            {
                TaskDialog.Show("Error", "No hay una vista activa.");
                return;
            }

            if (activeView.ViewType != ViewType.ThreeD)
            {
                TaskDialog.Show("Refuerzo 3D",
                    "Esta función solo aplica a vistas 3D.\n" +
                    "Por favor activa una vista 3D e intenta de nuevo.");
                return;
            }

            var rebars = new FilteredElementCollector(Document).OfClass(typeof(Rebar)).ToElements();
            var rebarSystems = new FilteredElementCollector(Document).OfClass(typeof(RebarInSystem)).ToElements();
            var areaReinf = new FilteredElementCollector(Document).OfClass(typeof(AreaReinforcement)).ToElements();
            var pathReinf = new FilteredElementCollector(Document).OfClass(typeof(PathReinforcement)).ToElements();

            if (rebars.Count == 0 && rebarSystems.Count == 0 &&
                areaReinf.Count == 0 && pathReinf.Count == 0)
            {
                TaskDialog.Show("Refuerzo 3D",
                    "No se encontró ningún elemento de refuerzo en el modelo.");
                return;
            }

            int contador = 0;

            using (var tx = new Transaction(Document, "Mostrar Refuerzo 3D"))
            {
                tx.Start();

                // Categoría Structural Rebar visible en la vista
                var rebarCat = Document.Settings.Categories
                    .get_Item(BuiltInCategory.OST_Rebar);
                if (rebarCat != null)
                {
                    try { activeView.SetCategoryHidden(rebarCat.Id, false); }
                    catch { }
                }

                // Detail Level = Fine — activa renderizado sólido
                try { activeView.DetailLevel = ViewDetailLevel.Fine; }
                catch { }

                foreach (Element elem in rebars)
                {
                    if (elem is not Rebar rebar) continue;
                    try { rebar.SetUnobscuredInView(activeView, true); contador++; }
                    catch { }
                }

                foreach (Element elem in rebarSystems)
                {
                    if (elem is not RebarInSystem rebarSys) continue;
                    try { rebarSys.SetUnobscuredInView(activeView, true); contador++; }
                    catch { }
                }

                foreach (Element elem in areaReinf)
                {
                    if (elem is not AreaReinforcement area) continue;
                    try { area.SetUnobscuredInView(activeView, true); contador++; }
                    catch { }
                }

                foreach (Element elem in pathReinf)
                {
                    if (elem is not PathReinforcement path) continue;
                    try { path.SetUnobscuredInView(activeView, true); contador++; }
                    catch { }
                }

                tx.Commit();
            }

            TaskDialog.Show(
                "Refuerzo 3D ✓",
                $"Se procesaron {contador} elemento(s) de refuerzo\n" +
                $"en la vista: \"{activeView.Name}\"\n\n" +
                $"• Rebar individual ✓\n" +
                $"• Rebar en sistema ✓\n" +
                $"• Area Reinforcement ✓\n" +
                $"• Path Reinforcement ✓"
            );
        }
    }
}