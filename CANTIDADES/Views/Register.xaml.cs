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
using System.Windows.Shapes;
using System.Configuration;
using System.Data.SqlClient;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using WARBIMPRO.Commands;
using WARBIMPRO.Models;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp;
using Microsoft.Office.Interop.Excel;
using WARBIMPRO.Utils;

namespace WARBIMPRO.Views
{
    /// <summary>
    /// Lógica de interacción para Register.xaml
    /// </summary>
    public partial class Register : System.Windows.Window
    {
        private FirebaseAuthService _auth;

        public Register(FirebaseAuthService authService)
        {
            InitializeComponent();
            _auth = authService;
        }


        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            string contraseña = txtPassword.Password;

            var data = new
            {
                Name = txtNombreUsuario.Text,
                Email = txtEmail.Text,
                Country = txtPais.Text,
                Password = contraseña
            };

            var auth = new FirebaseAuthService();
            string respuesta = await auth.RegistrarUsuarioAsync(data);

            MessageBox.Show("Registro exitoso: ");

            //// Volver al login
            //Logeo loginWindow = new Logeo(_auth);
            //loginWindow.Show();
            this.Close();
        }

    }
}
