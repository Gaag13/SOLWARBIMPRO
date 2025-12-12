using WARBIMPRO.Commands;
using WARBIMPRO.Models;
using Nice3point.Revit.Toolkit.External;

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
        }

        private void CreateRibbon()
        {
            var panel = Application.CreatePanel("Cuantificacióm", "WARBIMPRO");

            panel.AddPushButton<CmdKeyFireSharp>("Login")
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/User16x16.png");

            panel.AddPushButton<CmdCantidades>("QUANTITIES")
                .SetAvailabilityController<AvailabilityButton>()
                .SetLargeImage("/WARBIMPRO;component/Resources/Icons/Contar16x16.png");
        }
    }
}