using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using WARBIMPRO.ViewModels;
using WARBIMPRO.Views;

namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CmdBimFamilyCreator : ExternalCommand
    {
        public override void Execute()
        {
            var vm = new BimFamilyCreatorViewModel(Application);
            var view = new BimFamilyCreatorView(vm);
            view.ShowDialog();
        }
    }
}