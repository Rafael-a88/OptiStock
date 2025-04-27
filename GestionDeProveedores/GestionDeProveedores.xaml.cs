using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using TFG.Trabajadores;

namespace TFG.GestionDeProveedores
{
    public partial class GestionDeProveedores : UserControl
    {
        private List<Proveedor> proveedores;
        private const string TEXTO_BUSQUEDA_PREDETERMINADO = "Introduce el proveedor por ID, Nombre o Ciudad";

        public GestionDeProveedores()
        {
            InitializeComponent();
            CargarProveedores();
            ProveedoresListView.ItemsSource = proveedores;
        }

        private void CargarProveedores()
        {
            proveedores = new List<Proveedor>();
            try
            {
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    string query = "SELECT * FROM Proveedores";
                    using (var comando = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        using (var reader = comando.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                proveedores.Add(new Proveedor
                                {
                                    ID = Convert.ToInt32(reader["Id"]),
                                    Nombre = reader["Nombre"].ToString(),
                                    Contacto = reader["Contacto"].ToString(),
                                    Telefono = reader["Telefono"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Direccion = reader["Direccion"].ToString(),
                                    Ciudad = reader["Ciudad"].ToString(),
                                    Provincia = reader["Provincia"].ToString(),
                                    CodigoPostal = reader["CodigoPostal"].ToString(),
                                    Pais = reader["Pais"].ToString(),
                                    TipoProveedor = reader["TipoProveedor"].ToString(),
                                    Notas = reader["Notas"].ToString(),
                                    SitioWeb = reader["SitioWeb"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar proveedores: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            // Limpiar el cuadro de búsqueda
            BuscarTextBox.Text = TEXTO_BUSQUEDA_PREDETERMINADO;
            BuscarTextBox.Foreground = System.Windows.Media.Brushes.Gray;

            // Volver a cargar todos los proveedores
            CargarProveedores();
            ProveedoresListView.ItemsSource = null;
            ProveedoresListView.ItemsSource = proveedores;
        }

        private void AgregarProveedorButton_Click(object sender, RoutedEventArgs e)
        {
            // Crear una instancia del UserControl AgregarProveedor
            AgregarProveedor agregarProveedorWindow = new AgregarProveedor();

            // Obtener el control padre donde se cambiará el contenido
            ContentControl contenidoPrincipal = this.Parent as ContentControl;

            if (contenidoPrincipal != null)
            {
                // Cambiar la visibilidad del control actual
                this.Visibility = Visibility.Collapsed;
                // Establecer el nuevo UserControl como contenido
                contenidoPrincipal.Content = agregarProveedorWindow;
            }
        }


        private void ModificarProveedorButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProveedoresListView.SelectedItem is Proveedor proveedorSeleccionado)
            {
                ModificarProveedores modificarProveedoresWindow = new ModificarProveedores(proveedorSeleccionado);
                ContentControl contenidoPrincipal = this.Parent as ContentControl;
                if (contenidoPrincipal != null)
                {
                    this.Visibility = Visibility.Collapsed;
                    contenidoPrincipal.Content = modificarProveedoresWindow;
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un proveedor para modificar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EliminarProveedorButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProveedoresListView.SelectedItem is Proveedor proveedorSeleccionado)
            {
                var resultado = MessageBox.Show(
                    $"¿Está seguro que desea eliminar el proveedor {proveedorSeleccionado.Nombre}?",
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
                            string query = "DELETE FROM Proveedores WHERE Id = @Id";
                            using (var comando = new MySqlCommand(query, conexion.ObtenerConexion()))
                            {
                                comando.Parameters.AddWithValue("@Id", proveedorSeleccionado.ID);
                                int filasAfectadas = comando.ExecuteNonQuery();

                                if (filasAfectadas > 0)
                                {
                                    proveedores.Remove(proveedorSeleccionado);
                                    ProveedoresListView.Items.Refresh();
                                    MessageBox.Show("Proveedor eliminado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    MessageBox.Show("No se pudo eliminar el proveedor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar proveedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un proveedor para eliminar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            string terminoBusqueda = BuscarTextBox.Text.ToLower();

            // Si el término de búsqueda es el texto predeterminado, no realizar búsqueda
            if (terminoBusqueda == TEXTO_BUSQUEDA_PREDETERMINADO.ToLower())
            {
                terminoBusqueda = "";
            }

            var resultados = proveedores.FindAll(p =>
                p.Nombre.ToLower().Contains(terminoBusqueda) ||
                p.ID.ToString() == terminoBusqueda ||
                p.Contacto.ToLower().Contains(terminoBusqueda)
            );
            ProveedoresListView.ItemsSource = resultados;
        }
    }
}
