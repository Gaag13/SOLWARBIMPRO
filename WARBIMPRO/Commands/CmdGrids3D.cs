using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using JetBrains.Annotations;
using Nice3point.Revit.Toolkit.External;
using System.Linq;

namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CmdGrids3D : ExternalCommand
    {
        public override void Execute()
        {
            var view = ActiveView;
            
            var visibleGrids = new FilteredElementCollector(Document, view.Id)
                .OfClass(typeof(Grid))
                .Cast<Grid>()
                .ToList();

            if (!visibleGrids.Any()) return;
            
            Grid firstGrid = visibleGrids.First();
            DatumExtentType currentMode = visibleGrids.First().GetDatumExtentTypeInView(DatumEnds.End0, view);
           
            DatumExtentType newMode = (currentMode == DatumExtentType.Model)
                ? DatumExtentType.ViewSpecific
                : DatumExtentType.Model;
            
            using (var trans = new Transaction(Document, "Toggle Grids 2D/3D"))
            {
                trans.Start();

                foreach (var grid in visibleGrids)
                {
                    // Cambiamos ambos extremos (Burbuja y extremo opuesto) al nuevo modo
                    grid.SetDatumExtentType(DatumEnds.End0,view, newMode);
                    grid.SetDatumExtentType(DatumEnds.End1,view,newMode);
                }

                trans.Commit();
            }

            TaskDialog.Show("Grids 3D Toggle", $"Se han actualizado {visibleGrids.Count} rejillas al modo {(newMode == DatumExtentType.Model ? "3D" : "2D")}.");
        }
    }
}