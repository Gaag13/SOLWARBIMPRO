using System.IO;
using System.Windows;
using System.Windows.Input;
using WARBIMPRO.ShellExtension.Models;

namespace WARBIMPRO.ShellExtension.Views
{
    public partial class RevitInfoWindow : Window
    {
        public RevitInfoWindow(RevitFileInfo info)
        {
            InitializeComponent();
            PopulateUI(info);
        }

        private void PopulateUI(RevitFileInfo info)
        {
            TxtFileName.Text = info.FileName;
            TxtFilePath.Text = info.FilePath;
            TxtRevitVersion.Text = info.RevitVersion;
            TxtFileSize.Text = info.FileSize;
            TxtLastModified.Text = info.LastModified;

            // Fecha de creación desde el sistema de archivos
            try
            {
                var created = File.GetCreationTime(info.FilePath);
                TxtCreated.Text = created.ToString("dd/MM/yyyy HH:mm");
            }
            catch
            {
                TxtCreated.Text = "—";
            }

            if (info.IsWorkshared)
                BadgeWorkshared.Visibility = Visibility.Visible;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1) DragMove();
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }
}