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
            DockablePaneProvider.Register(Application, new Guid("0525d7a0-5b14-462b-aa81-1198eb12b387"), "Family Browser")
                .SetConfiguration(data => {

                    var provider = new DockPanelProvider();
                    provider.SetupDockablePane(data);

                });
        }

        private void CreateRibbon()
        {
            var panelLogin = Application.CreatePanel("Login", "WARBIMPRO");

            panelLogin.AddPushButton<CmdKeyFireSharp>("Login")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/User20x20_dark.png")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/User20x20_light.png");

            var panel = Application.CreatePanel("Cuantificacióm", "WARBIMPRO");

            panel.AddPushButton<CmdCantidades>("QUANTITIES")
                .SetAvailabilityController<AvailabilityButton>()
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Cantidades20x20_light.png")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Cantidades20x20_dark.png");

            var panelFamilys = Application.CreatePanel("Familys", "WARBIMPRO");

            panelFamilys.AddPushButton<CmdLoadFamilys>("Load Family")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/load20x20_light.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/load20x20_light.png");

            panelFamilys.AddSeparator();

            panelFamilys.AddPushButton<CmdFamilyBrowser>("FamilyBrowser")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/lista20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/lista20x20_light.png");

            panelFamilys.AddSeparator();

            panelFamilys.AddPushButton<CmdFindReplaceView>("FinReplceViews")
               .SetAvailabilityController<AvailabilityButton>()
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/find20x20_dark.png")
               .SetLargeImage("/WARBIMPRO;component/Resources/Icons/find20x20_light.png");
        }
    }
}