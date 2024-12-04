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


namespace TFG
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
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

            if ((usuario == "auxiliaesquinarafael@gmail.com" || usuario == "antonio92guerrerogarcia@gmail.com")
                && contrasena == "admin1234")
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