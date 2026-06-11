using Autodesk.Revit.Attributes;
using Nice3point.Revit.Toolkit.External;
using WARBIMPRO.ViewModels;
using WARBIMPRO.Views;

namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]

    public class CmdLoadFamilys : ExternalCommand
    { 
        public override void Execute()
        {
            var uidoc= Application.ActiveUIDocument;
            var uiapp= Application;

            var viewModel = new LoadFamiliesViewModel(uidoc);
            var window = new ViewLoadFamilies(viewModel)
            {
                DataContext = viewModel,                
            };

            window.ShowDialog();   

            
        }
    }
}
