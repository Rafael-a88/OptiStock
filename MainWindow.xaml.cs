using System;
using System.Windows;
using System.Windows.Media;
using OfficeOpenXml;
using System.Diagnostics;


namespace TFG
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Establecer el contexto de licencia para EPPlus
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; 
        }

        private void UsuarioTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (UsuarioTextBox.Text == "Introduce tu correo electronico")
            {
                UsuarioTextBox.Text = "";
                UsuarioTextBox.Foreground = Brushes.Black;
            }
        }

        private void UsuarioTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(UsuarioTextBox.Text))
            {
                UsuarioTextBox.Text = "Introduce tu correo electronico";
                UsuarioTextBox.Foreground = Brushes.Gray;
            }
        }

        private void IniciarSesionButton_Click(object sender, RoutedEventArgs e)
        {
            string usuario = UsuarioTextBox.Text;
            string contrasena = ContrasenaPasswordBox.Password;

            // Condición corregida
            if (
                (usuario == "auxiliaesquinarafael@gmail.com" ||
                 usuario == "antonio92guerrerogarcia@gmail.com" ||
                 usuario == "a")
                && (contrasena == "admin1234" || contrasena == "a")
            )
            {
                // Crear una nueva instancia de la ventana Principal
                Principal principalWindow = new Principal();
                principalWindow.Show(); // Mostrar la nueva ventana
                this.Close(); // Cerrar la ventana actual (MainWindow)
            }
            else
            {
                MessageBox.Show("Credenciales incorrectas. Inténtalo de nuevo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
