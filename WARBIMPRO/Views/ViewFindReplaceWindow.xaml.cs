using System.Windows;
using System.Windows.Input;
using WARBIMPRO.ViewModels;

namespace WARBIMPRO.Views
{
    public partial class ViewFindReplaceWindow : Window
    {
        public ViewFindReplaceWindow(FindReplaceViewModel viewModel)
        {
            InitializeComponent();

            // Asignar el ViewModel al DataContext
            DataContext = viewModel;
        }
       

    }
}