using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    public sealed partial class WARBIMPROView
    {
        public WARBIMPROView(WARBIMPROViewModel viewModel)
        {
            DataContext = viewModel;
            InitializeComponent();
        }
    }
}