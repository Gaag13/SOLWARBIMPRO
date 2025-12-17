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
            var uidoc= UiDocument;
            var uiapp= UiApplication;

            var viewModel = new SettingsViewModel(uidoc);
            var window = new ViewSettings(viewModel)
            {
                DataContext = viewModel,                
            };

            window.ShowDialog();   

            
        }
    }
}
