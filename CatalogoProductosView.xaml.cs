using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient;
using TFG;

namespace TFG
{
    public partial class CatalogoProductosView : UserControl
    {
        public CatalogoProductosView()
        {
            InitializeComponent();
            CargarProductos(); // Cargar productos al inicializar el control
        }

        private void CargarProductos()
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string sql = "SELECT * FROM productos";

                using (var connection = conexion.ObtenerConexion())
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        List<Producto> productos = new List<Producto>();

                        while (reader.Read())
                        {
                            Producto producto = new Producto
                            {
                                Id = reader.GetInt32("Id"),
                                Nombre = reader.GetString("Nombre"),
                                Precio = reader.GetDouble("Precio"),
                                Iva = reader.GetDouble("Iva"),
                                PrecioTotal = reader.GetDouble("PrecioTotal"),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                                CategoriaId = reader.GetInt32("CategoriaId"),
                                Stock = reader.GetInt32("Stock"),
                                Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString("Imagen"),
                                FechaCreacion = reader.GetDateTime("FechaCreacion"),
                                Descuento = reader.GetDouble("Descuento"),
                                EAN = reader.IsDBNull(reader.GetOrdinal("EAN")) ? null : reader.GetString("EAN")
                            };
                            productos.Add(producto);
                        }

                        ProductosListView.ItemsSource = productos;
                    }
                }
            }
        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BuscarTextBox.Text == "Introduce el producto por ID o por Nombre")
            {
                BuscarTextBox.Text = "";
                BuscarTextBox.Foreground = Brushes.Black; // Cambia el color del texto a negro
            }
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BuscarTextBox.Text))
            {
                BuscarTextBox.Text = "Introduce el producto por ID o por Nombre";
                BuscarTextBox.Foreground = Brushes.Gray; // Cambia el color del texto a gris
            }
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            string query = BuscarTextBox.Text == "Buscar producto..." ? "" : BuscarTextBox.Text;

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string sql = "SELECT * FROM productos WHERE Nombre LIKE @query OR CAST(Id AS CHAR) LIKE @idQuery";

                using (var connection = conexion.ObtenerConexion())
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@query", "%" + query + "%");
                    command.Parameters.AddWithValue("@idQuery", "%" + query + "%");

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        List<Producto> productos = new List<Producto>();

                        while (reader.Read())
                        {
                            Producto producto = new Producto
                            {
                                Id = reader.GetInt32("Id"),
                                Nombre = reader.GetString("Nombre"),
                                Precio = reader.GetDouble("Precio"),
                                Iva = reader.GetDouble("Iva"),
                                PrecioTotal = reader.GetDouble("PrecioTotal"),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                                CategoriaId = reader.GetInt32("CategoriaId"),
                                Stock = reader.GetInt32("Stock"),
                                Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString("Imagen"),
                                FechaCreacion = reader.GetDateTime("FechaCreacion"),
                                Descuento = reader.GetDouble("Descuento"),
                                EAN = reader.IsDBNull(reader.GetOrdinal("EAN")) ? null : reader.GetString("EAN")
                            };
                            productos.Add(producto);
                        }

                        ProductosListView.ItemsSource = productos;
                    }
                }
            }
        }

        private void AgregarProductoButton_Click(object sender, RoutedEventArgs e)
        {
            // Crear una instancia de la vista AgregarProductoView
            AgregarProductoView agregarProductoView = new AgregarProductoView();

            // Obtener la ventana principal o el contenedor donde se mostrará la nueva vista
            Window mainWindow = Window.GetWindow(this);
            if (mainWindow is Principal principal) // Asegúrate de que 'Principal' es el nombre correcto de tu ventana principal
            {
                // Reemplazar el contenido del ContentControl en la ventana principal
                principal.ContenidoPrincipal.Content = agregarProductoView;
            }
            else
            {
                MessageBox.Show("No se pudo obtener la ventana principal.");
            }
        }



        private void ModificarProductoButton_Click(object sender, RoutedEventArgs e)
        {
            // Implementa la lógica para modificar un producto aquí
        }

        private void EliminarProductoButton_Click(object sender, RoutedEventArgs e)
        {
            // Implementa la lógica para eliminar un producto aquí
        }

        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            CargarProductos(); // Llama al método que carga los productos
        }
    }
}
