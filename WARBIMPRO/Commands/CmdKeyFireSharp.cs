using Autodesk.Revit.Attributes;
using WARBIMPRO.Utils;
using WARBIMPRO.Views;
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
