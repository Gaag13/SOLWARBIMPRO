using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using WARBIMPRO.ViewModels;
using WARBIMPRO.Views;

namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CmdFindReplaceView : ExternalCommand
    {
        public override void Execute()
        {
            var doc = Application.ActiveUIDocument.Document;

            var vm = new FindReplaceViewModel(doc);
            var window = new ViewFindReplaceWindow(vm)
            {
                DataContext = vm
            };

            window.ShowDialog();
        }
    }
}
