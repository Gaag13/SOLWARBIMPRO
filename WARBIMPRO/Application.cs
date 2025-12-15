using Nice3point.Revit.Toolkit.Decorators;
using Nice3point.Revit.Toolkit.External;
using WARBIMPRO.Commands;
using WARBIMPRO.Models;
using WARBIMPRO.DockablePanes;

namespace WARBIMPRO
{
    /// <summary>
    ///     Application entry point
    /// </summary>
    [UsedImplicitly]
    public class Application : ExternalApplication
    {
        public override void OnStartup()
        {
            CreateRibbon();
            CreateRibbon();
            DockablePaneProvider.Register(Application, new Guid("0525d7a0-5b14-462b-aa81-1198eb12b387"), "Family Browser")
                .SetConfiguration(data => {

                    var provider = new DockPanelProvider();
                    provider.SetupDockablePane(data);

                });
        }

        private void CreateRibbon()
        {
            var panel = Application.CreatePanel("Cuantificacióm", "WARBIMPRO");

            panel.AddPushButton<CmdKeyFireSharp>("Login")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/User16x16.png");

            panel.AddPushButton<CmdCantidades>("QUANTITIES")
                .SetAvailabilityController<AvailabilityButton>()
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Contar16x16.png");

            panel.AddPushButton<CmdLoadFamilys>("Load Family")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Contar16x16.png");

            panel.AddPushButton<CmdFamilyBrowser>("Load Family")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Contar16x16.png");

            panel.AddPushButton<CmdFindReplaceView>("Load Family")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Contar16x16.png");
        }
    }
}