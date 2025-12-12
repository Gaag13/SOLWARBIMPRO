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
