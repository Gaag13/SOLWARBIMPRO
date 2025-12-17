using System.Windows;
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
