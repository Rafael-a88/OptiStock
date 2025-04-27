using System;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace TFG.GestionDeClientes
{
    public partial class ModificarCliente : UserControl
    {
        private int clienteIdActual = -1;

        public ModificarCliente(Cliente cliente)
        {
            InitializeComponent();
            CargarDatosCliente(cliente);
        }

        private void CargarDatosCliente(Cliente cliente)
        {
            clienteIdActual = cliente.ID;
            NombreCompletoTextBox.Text = cliente.NombreCompleto;
            DNITextBox.Text = cliente.DNI;
            TelefonoTextBox.Text = cliente.Telefono;
            EmailTextBox.Text = cliente.Email;
            DireccionTextBox.Text = cliente.Direccion;
            CiudadTextBox.Text = cliente.Ciudad;
            FechaNacimientoDatePicker.SelectedDate = cliente.FechaNacimiento;
        }

        private void ModificarCliente_Click(object sender, RoutedEventArgs e)
        {
            if (clienteIdActual == -1)
            {
                MessageBox.Show("Primero debe seleccionar un cliente.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();

                    string query = @"UPDATE Clientes 
                        SET NombreCompleto = @NombreCompleto,
                            FechaNacimiento = @FechaNacimiento, 
                            DNI = @DNI, 
                            Telefono = @Telefono, 
                            Email = @Email, 
                            Direccion = @Direccion, 
                            Ciudad = @Ciudad
                        WHERE Id = @Id";

                    using (var comando = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@Id", clienteIdActual);
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
                            MessageBox.Show("Cliente modificado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                            ContentControl contenidoPrincipal = this.Parent as ContentControl;
                            if (contenidoPrincipal != null)
                            {
                                GestionDeClientes gestionClientes = new GestionDeClientes();
                                contenidoPrincipal.Content = gestionClientes;
                            }
                        }
                        else
                        {
                            MessageBox.Show("No se pudo modificar el cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al modificar cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
