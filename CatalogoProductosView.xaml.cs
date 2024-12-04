using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient; // Asegúrate de incluir esta línea

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

                using (var connection = conexion.ObtenerConexion()) // Asegúrate de obtener la conexión aquí
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        List<Producto> productos = new List<Producto>(); // Especificar el tipo

                        while (reader.Read())
                        {
                            Producto producto = new Producto // Especificar el tipo
                            {
                                Id = reader.GetInt32("Id"),
                                Nombre = reader.GetString("Nombre"),
                                Precio = reader.GetDouble("Precio"),
                                Iva = reader.GetDouble("Iva"),
                                PrecioTotal = reader.GetDouble("PrecioTotal"),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                                Categoria = reader.IsDBNull(reader.GetOrdinal("Categoria")) ? null : reader.GetString("Categoria"),
                                Stock = reader.GetInt32("Stock"),
                                Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString("Imagen"),
                                FechaCreacion = reader.GetDateTime("FechaCreacion"),
                                Descuento = reader.GetDouble("Descuento"),
                                EAN = reader.IsDBNull(reader.GetOrdinal("EAN")) ? null : reader.GetString("EAN") // Asegúrate de que esto es correcto
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
                // Modificar la consulta para incluir búsqueda por Id
                string sql = "SELECT * FROM productos WHERE Nombre LIKE @query OR Id LIKE @idQuery";

                using (var connection = conexion.ObtenerConexion()) // Asegúrate de obtener la conexión aquí
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@query", "%" + query + "%");
                    command.Parameters.AddWithValue("@idQuery", "%" + query + "%"); // Añadir parámetro para Id

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        List<Producto> productos = new List<Producto>(); // Especificar el tipo

                        while (reader.Read())
                        {
                            Producto producto = new Producto // Especificar el tipo
                            {
                                Id = reader.GetInt32("Id"),
                                Nombre = reader.GetString("Nombre"),
                                Precio = reader.GetDouble("Precio"),
                                Iva = reader.GetDouble("Iva"),
                                PrecioTotal = reader.GetDouble("PrecioTotal"),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                                Categoria = reader.IsDBNull(reader.GetOrdinal("Categoria")) ? null : reader.GetString("Categoria"),
                                Stock = reader.GetInt32("Stock"),
                                Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString("Imagen"),
                                FechaCreacion = reader.GetDateTime("FechaCreacion"),
                                Descuento = reader.GetDouble("Descuento"),
                                EAN = reader.IsDBNull(reader.GetOrdinal("EAN")) ? null : reader.GetString("EAN") // Asegúrate de que esto es correcto
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
            // Implementa la lógica para agregar un nuevo producto aquí
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
