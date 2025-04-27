using System;
using System.Windows;
using System.Windows.Media;
using OfficeOpenXml;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;


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

            try
            {
                // Primero intentar obtener el departamento para verificar la conexión
                DepartamentoManager departamentoManager = new DepartamentoManager();
                string departamento = departamentoManager.ObtenerDepartamento(usuario, contrasena);

                if (departamento != null)
                {
                    // Usuario encontrado en la base de datos
                    MessageBox.Show($"Bienvenido. Usted pertenece al departamento de: {departamento}", "Inicio de sesión exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                    Principal principalWindow = new Principal(departamento);
                    principalWindow.Show();
                    this.Close();
                }
                else if ((usuario == "a") && (contrasena == "a"))
                {
                    // Credenciales hardcodeadas
                    string departamentoPredeterminado = "Gerencia";
                    MessageBox.Show($"Bienvenido. Usted pertenece al departamento de Departamento: {departamentoPredeterminado}",
                                  "Inicio de sesión exitoso", MessageBoxButton.OK, MessageBoxImage.Information);

                    Principal principalWindow = new Principal(departamentoPredeterminado);
                    principalWindow.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Credenciales incorrectas. Inténtalo de nuevo.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al conectar con la base de datos: {ex.Message}", "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
