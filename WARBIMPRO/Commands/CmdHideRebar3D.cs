using Nice3point.Revit.Toolkit.External;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CmdHideRebar3D : ExternalCommand
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

            int contador = 0;

            using (var tx = new Transaction(Document, "Ocultar Refuerzo 3D"))
            {
                tx.Start();

                foreach (Element elem in rebars)
                {
                    if (elem is not Rebar rebar) continue;
                    try { rebar.SetUnobscuredInView(activeView, false); contador++; }
                    catch { }
                }

                foreach (Element elem in rebarSystems)
                {
                    if (elem is not RebarInSystem rebarSys) continue;
                    try { rebarSys.SetUnobscuredInView(activeView, false); contador++; }
                    catch { }
                }

                foreach (Element elem in areaReinf)
                {
                    if (elem is not AreaReinforcement area) continue;
                    try { area.SetUnobscuredInView(activeView, false); contador++; }
                    catch { }
                }

                foreach (Element elem in pathReinf)
                {
                    if (elem is not PathReinforcement path) continue;
                    try { path.SetUnobscuredInView(activeView, false); contador++; }
                    catch { }
                }

                tx.Commit();
            }

            TaskDialog.Show(
                "Refuerzo 3D ✗",
                $"Se desactivó la vista sólida para {contador} elemento(s)\n" +
                $"en la vista: \"{activeView.Name}\""
            );
        }
    }
}