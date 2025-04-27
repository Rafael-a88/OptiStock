using System;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace TFG.GestionDeClientes
{
    public partial class AgregarCliente : UserControl
    {
        public AgregarCliente()
        {
            InitializeComponent();
        }

        private void AñadirCliente_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos())
            {
                return;
            }

            try
            {
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();

                    string query = @"INSERT INTO Clientes 
                        (NombreCompleto, FechaNacimiento, DNI, Telefono, Email, Direccion, Ciudad) 
                        VALUES 
                        (@NombreCompleto, @FechaNacimiento, @DNI, @Telefono, @Email, @Direccion, @Ciudad)";

                    using (var comando = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@NombreCompleto", NombreCompletoTextBox.Text);
                        comando.Parameters.AddWithValue("@FechaNacimiento", FechaNacimientoDatePicker.SelectedDate);
                        comando.Parameters.AddWithValue("@DNI", DNITextBox.Text);
                        comando.Parameters.AddWithValue("@Telefono", TelefonoTextBox.Text);
                        comando.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                        comando.Parameters.AddWithValue("@Direccion", DireccionTextBox.Text);
                        comando.Parameters.AddWithValue("@Ciudad", CiudadTextBox.Text);

                        int resultado = comando.ExecuteNonQuery();

                        if (resultado > 0)
                        {
                            MessageBox.Show("Cliente añadido exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            LimpiarCampos();
                        }
                        else
                        {
                            MessageBox.Show("No se pudo añadir el cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al añadir cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(NombreCompletoTextBox.Text))
            {
                MessageBox.Show("El nombre completo es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DNITextBox.Text))
            {
                MessageBox.Show("El DNI es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private void LimpiarCampos()
        {
            NombreCompletoTextBox.Clear();
            DNITextBox.Clear();
            TelefonoTextBox.Clear();
            EmailTextBox.Clear();
            DireccionTextBox.Clear();
            CiudadTextBox.Clear();
            FechaNacimientoDatePicker.SelectedDate = null;
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            ContentControl contenidoPrincipal = this.Parent as ContentControl;
            if (contenidoPrincipal != null)
            {
                GestionDeClientes gestionClientes = new GestionDeClientes();
                contenidoPrincipal.Content = gestionClientes;
            }
        }
    }
}
