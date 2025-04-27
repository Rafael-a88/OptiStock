using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace TFG.GestionDeClientes
{
    public partial class GestionDeClientes : UserControl
    {
        private List<Cliente> clientes;
        private const string TEXTO_BUSQUEDA_PREDETERMINADO = "Introduce el cliente por ID, Nombre o DNI";

        public GestionDeClientes()
        {
            InitializeComponent();
            CargarClientes();
            ClientesListView.ItemsSource = clientes;
        }

        private void CargarClientes()
        {
            clientes = new List<Cliente>();
            try
            {
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    string query = "SELECT * FROM Clientes";
                    using (var comando = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        using (var reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                clientes.Add(new Cliente
                                {
                                    ID = Convert.ToInt32(reader["Id"]),
                                    NombreCompleto = reader["NombreCompleto"].ToString(),
                                    DNI = reader["DNI"].ToString(),
                                    Telefono = reader["Telefono"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Direccion = reader["Direccion"].ToString(),
                                    Ciudad = reader["Ciudad"].ToString(),
                                    FechaNacimiento = Convert.ToDateTime(reader["FechaNacimiento"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BuscarTextBox.Text == TEXTO_BUSQUEDA_PREDETERMINADO)
            {
                BuscarTextBox.Text = "";
                BuscarTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BuscarTextBox.Text))
            {
                BuscarTextBox.Text = TEXTO_BUSQUEDA_PREDETERMINADO;
                BuscarTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            BuscarTextBox.Text = TEXTO_BUSQUEDA_PREDETERMINADO;
            BuscarTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            CargarClientes();
            ClientesListView.ItemsSource = null;
            ClientesListView.ItemsSource = clientes;
        }

        private void AgregarClienteButton_Click(object sender, RoutedEventArgs e)
        {
            AgregarCliente agregarClienteWindow = new AgregarCliente();
            ContentControl contenidoPrincipal = this.Parent as ContentControl;
            if (contenidoPrincipal != null)
            {
                this.Visibility = Visibility.Collapsed;
                contenidoPrincipal.Content = agregarClienteWindow;
            }
        }

        private void ModificarClienteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientesListView.SelectedItem is Cliente clienteSeleccionado)
            {
                ModificarCliente modificarClienteWindow = new ModificarCliente(clienteSeleccionado);
                ContentControl contenidoPrincipal = this.Parent as ContentControl;
                if (contenidoPrincipal != null)
                {
                    this.Visibility = Visibility.Collapsed;
                    contenidoPrincipal.Content = modificarClienteWindow;
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un cliente para modificar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EliminarClienteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ClientesListView.SelectedItem is Cliente clienteSeleccionado)
            {
                var resultado = MessageBox.Show(
                    $"¿Está seguro que desea eliminar el cliente {clienteSeleccionado.NombreCompleto}?",
                    "Confirmar Eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                );

                if (resultado == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (var conexion = new Conexion())
                        {
                            conexion.AbrirConexion();
                            string query = "DELETE FROM Clientes WHERE Id = @Id";
                            using (var comando = new MySqlCommand(query, conexion.ObtenerConexion()))
                            {
                                comando.Parameters.AddWithValue("@Id", clienteSeleccionado.ID);
                                int filasAfectadas = comando.ExecuteNonQuery();

                                if (filasAfectadas > 0)
                                {
                                    clientes.Remove(clienteSeleccionado);
                                    ClientesListView.Items.Refresh();
                                    MessageBox.Show("Cliente eliminado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    MessageBox.Show("No se pudo eliminar el cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un cliente para eliminar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            string terminoBusqueda = BuscarTextBox.Text.ToLower();

            if (terminoBusqueda == TEXTO_BUSQUEDA_PREDETERMINADO.ToLower())
            {
                terminoBusqueda = "";
            }

            var resultados = clientes.FindAll(c =>
                c.NombreCompleto.ToLower().Contains(terminoBusqueda) ||
                c.ID.ToString() == terminoBusqueda ||
                c.DNI.ToLower().Contains(terminoBusqueda)
            );
            ClientesListView.ItemsSource = resultados;
        }
    }
}
