using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Autodesk.Revit.UI;
using WARBIMPRO.Commands;
using WARBIMPRO.Models;
using WARBIMPRO.Utils;
using FireSharp.Interfaces;
using FireSharp.Response;

namespace WARBIMPRO.Views
{
    /// <summary>
    /// Lógica de interacción para Logeo.xaml
    /// </summary>
    public partial class Logeo : Window
    {
        private FirebaseAuthService _auth;

        public Logeo(FirebaseAuthService authService)
        {
            InitializeComponent();
            _auth = authService;

        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            string email = txtLoginUsername.Text.Trim();
            string password = txtLoginPassword.Password.Trim();

            DataFirebase usuario = await _auth.IniciarSesionUsuarioAsync(email, password);

            if (usuario != null)
            {
                MessageBox.Show("Logueado Exitoso", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                SesionManager guardarEstadoDeSesion = new SesionManager();
                guardarEstadoDeSesion.GuardarEstadoDeSesion(email);
                AvailabilityButton.IsEnabled = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Error al iniciar sesión: Usuario o contraseña inválidos", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        private void ButtonBaseR_OnClick(object sender, RoutedEventArgs e)
        {
            var authService = new FirebaseAuthService();
            Register registerWindow = new Register(authService);
            registerWindow.ShowDialog();
            this.Close();
        }
        private void Logeo_OnLoaded(object sender, RoutedEventArgs e)
        {

            SesionManager estadoLogin = new SesionManager();
            var usuarioId = estadoLogin.ObtenerUsuarioActual();

            if (!string.IsNullOrEmpty(usuarioId))
            {
                //ValidarLicencia(usuarioId);
            }
            else
            {
                MessageBox.Show("No se encontró un usuario logueado.");
            }

            if (estadoLogin.EstaLogueado())
            {
                AvailabilityButton.IsEnabled = true;
                MessageBox.Show("Bienvenido de nuevo. Ya estas logueado");
                this.Close();
            }
            else
            {
                MessageBox.Show("No estas logueado");
            }

        }
    }
}
