using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WARBIMPRO.DockablePanes;
using WARBIMPRO.ViewModels;
using WARBIMPRO.Views;

namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CmdFamilyBrowser : ExternalCommand
    {
        public override void Execute()
        {
            var uidoc = UiDocument;
            var uiapp = UiApplication;

            var vm= new FamilyBrowserViewModel(uidoc);
            var view = new ViewFamilyBrowser(vm);
           
            DockPanelProvider.Host.Content = view;

            var paneId = new DockablePaneId(new Guid("0525d7a0-5b14-462b-aa81-1198eb12b387"));
            var pane = uiapp.GetDockablePane(paneId);

            if (!pane.IsShown())
                pane.Show();
            else
                pane.Hide();

        }
    }    
}
