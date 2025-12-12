using System.Windows.Controls;
using Autodesk.Revit.UI;


namespace WARBIMPRO.DockablePanes
{
    public  class DockPanelProvider: IDockablePaneProvider
    {
        public static ContentControl Host { get; } = new ContentControl();
 
        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = Host;
            data.InitialState = new DockablePaneState
            {
                DockPosition = DockPosition.Left
            };
        }
    }
}
