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

            if (!IsValidView(activeView))
            {
                TaskDialog.Show("Refuerzo 3D", "Esta vista no soporta modificación de refuerzo.");
                return;
            }

            var rebars = new FilteredElementCollector(Document, activeView.Id)
                .OfClass(typeof(Rebar));

            var rebarSystems = new FilteredElementCollector(Document, activeView.Id)
                .OfClass(typeof(RebarInSystem));

            var areaReinf = new FilteredElementCollector(Document, activeView.Id)
                .OfClass(typeof(AreaReinforcement));

            var pathReinf = new FilteredElementCollector(Document, activeView.Id)
                .OfClass(typeof(PathReinforcement));

            int contador = 0;

            using (var tx = new Transaction(Document, "Mostrar Refuerzo 3D"))
            {
                tx.Start();

                try
                {
                    activeView.DetailLevel = ViewDetailLevel.Fine;
                }
                catch { }

                try
                {
                    var rebarCat = Document.Settings.Categories
                        .get_Item(BuiltInCategory.OST_Rebar);

                    if (rebarCat != null)
                        activeView.SetCategoryHidden(rebarCat.Id, false);
                }
                catch { }

                foreach (Rebar rebar in rebars)
                {
                    try
                    {
                        rebar.SetUnobscuredInView(activeView, true);
                        contador++;
                    }
                    catch { }
                }

                foreach (RebarInSystem rebarSys in rebarSystems)
                {
                    try
                    {
                        rebarSys.SetUnobscuredInView(activeView, true);
                        contador++;
                    }
                    catch { }
                }

                foreach (AreaReinforcement area in areaReinf)
                {
                    try
                    {
                        area.SetUnobscuredInView(activeView, true);
                        contador++;
                    }
                    catch { }
                }

                foreach (PathReinforcement path in pathReinf)
                {
                    try
                    {
                        path.SetUnobscuredInView(activeView, true);
                        contador++;
                    }
                    catch { }
                }

                tx.Commit();
            }

            TaskDialog.Show(
                "Refuerzo 3D ✓",
                $"Se activó la visualización sólida para {contador} elemento(s)\n" +
                $"en la vista: \"{activeView.Name}\""
            );
        }

        private bool IsValidView(View view)
        {
            return view.ViewType == ViewType.ThreeD ||
                   view.ViewType == ViewType.EngineeringPlan ||
                   view.ViewType == ViewType.FloorPlan ||
                   view.ViewType == ViewType.Elevation ||
                   view.ViewType == ViewType.Section ||
                   view.ViewType == ViewType.Detail;
        }
    }
}