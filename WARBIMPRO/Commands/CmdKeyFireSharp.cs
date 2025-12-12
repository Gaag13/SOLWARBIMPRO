using Autodesk.Revit.Attributes;
using Autodesk.Revit.UI;
using WARBIMPRO.Models;
using WARBIMPRO.Utils;
using WARBIMPRO.Views;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Newtonsoft.Json;
using Nice3point.Revit.Toolkit.External;



namespace WARBIMPRO.Commands
{
   
    [UsedImplicitly]
    [Transaction(TransactionMode.Manual)]
    public class CmdKeyFireSharp : ExternalCommand 
    {
        public override void Execute()
        {
            var authService = new FirebaseAuthService();
            Logeo loginWindow = new Logeo(authService);
            loginWindow.Show();
        }
    }
}
