using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WARBIMPRO.Utils;
using WARBIMPRO.ViewModels;
using WARBIMPRO.Views;

namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]

    public class CmdTranferViewTemplate: ExternalCommand
    {
        public override void Execute()
        {
            var viewModel = new TransferViewTemplateViewModel(Application);
            var view = new TranferTemplatesView(viewModel)
            {
                DataContext = viewModel
            };

            view.ShowDialog();
        }
    }
    
}
