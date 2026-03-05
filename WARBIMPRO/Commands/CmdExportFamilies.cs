using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using Microsoft.Win32;
using Nice3point.Revit.Toolkit.External;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WARBIMPRO.Views;

namespace WARBIMPRO.Commands
{
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CmdExportFamilies : ExternalCommand
    {
        public override void Execute()
        {
            var uidoc = UiDocument;
            var doc = uidoc.Document;

            ViewExportFamilies view = new ViewExportFamilies(doc);
            view.ShowDialog();


        }
    }
}
