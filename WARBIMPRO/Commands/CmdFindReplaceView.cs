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
            var vm = new FindReplaceViewModel(Document);
            var window = new ViewFindReplaceWindow(vm)
            {
                DataContext = vm
            };

            window.ShowDialog();
        }
    }
}
