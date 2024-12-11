using MySqlConnector;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace TFG
{
    public partial class CatalogoProductosView : UserControl
    {
        private string currentSort = "";
        private bool isAscending = true;
        private ICollectionView view;

        public CatalogoProductosView()
        {
            InitializeComponent();
            CargarProductos();
        }

        private void CargarProductos()
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string sql = @"
                    SELECT p.*, c.Nombre AS NombreCategoria 
                    FROM productos p
                    LEFT JOIN categorias c ON p.CategoriaId = c.Id";

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
                                Marca = reader.GetString("Marca"),
                                Modelo = reader.GetString("Modelo"),
                                Precio = reader.GetDouble("Precio"),
                                Iva = reader.GetDouble("Iva"),
                                PrecioTotal = reader.GetDouble("PrecioTotal"),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                                CategoriaNombre = reader.IsDBNull(reader.GetOrdinal("NombreCategoria")) ? null : reader.GetString("NombreCategoria"),
                                Stock = reader.GetInt32("Stock"),
                                Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString("Imagen"),
                                FechaCreacion = reader.GetDateTime("FechaCreacion"),
                                Descuento = reader.GetDouble("Descuento"),
                                EAN = reader.IsDBNull(reader.GetOrdinal("EAN")) ? null : reader.GetString("EAN")
                            };
                            productos.Add(producto);
                        }

                        ProductosListView.ItemsSource = productos;
                        view = CollectionViewSource.GetDefaultView(ProductosListView.ItemsSource);
                    }
                }
            }
        }

        private void Header_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock header)
            {
                string propertyName = header.Tag.ToString();

                if (currentSort == propertyName)
                {
                    isAscending = !isAscending;
                }
                else
                {
                    currentSort = propertyName;
                    isAscending = true;
                }

                view = CollectionViewSource.GetDefaultView(ProductosListView.ItemsSource);
                view.SortDescriptions.Clear();

                // Forzar la conversión a string para la categoría
                if (propertyName == "CategoriaNombre")
                {
                    view.SortDescriptions.Add(new SortDescription(propertyName, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                }
                else
                {
                    view.SortDescriptions.Add(new SortDescription(propertyName, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                }

                var gridView = ProductosListView.View as GridView;
                if (gridView != null)
                {
                    foreach (GridViewColumn column in gridView.Columns)
                    {
                        if (column.Header is TextBlock headerText)
                        {
                            if (headerText.Tag.ToString() == propertyName)
                            {
                                headerText.Text = $"{headerText.Tag} {(isAscending ? "▲" : "▼")}";
                            }
                            else
                            {
                                headerText.Text = headerText.Tag.ToString();
                            }
                        }
                    }
                }
            }
        }


        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BuscarTextBox.Text == "Introduce el producto por ID, Nombre, Marca o Categoria")
            {
                BuscarTextBox.Text = "";
                BuscarTextBox.Foreground = Brushes.Black;
            }
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BuscarTextBox.Text))
            {
                BuscarTextBox.Text = "Introduce el producto por ID, Nombre, Marca o Categoria";
                BuscarTextBox.Foreground = Brushes.Gray;
            }
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            string query = BuscarTextBox.Text == "Introduce el producto por ID, Nombre, Marca o Categoria" ? "" : BuscarTextBox.Text;

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string sql = @"
            SELECT p.*, c.Nombre AS NombreCategoria 
            FROM productos p
            LEFT JOIN categorias c ON p.CategoriaId = c.Id
            WHERE p.Nombre LIKE @query 
               OR CAST(p.Id AS CHAR) LIKE @idQuery 
               OR p.Marca LIKE @marcaQuery 
               OR c.Nombre LIKE @categoriaQuery";

                using (var connection = conexion.ObtenerConexion())
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@query", "%" + query + "%");
                    command.Parameters.AddWithValue("@idQuery", "%" + query + "%");
                    command.Parameters.AddWithValue("@marcaQuery", "%" + query + "%");
                    command.Parameters.AddWithValue("@categoriaQuery", "%" + query + "%");

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        List<Producto> productos = new List<Producto>();

                        while (reader.Read())
                        {
                            Producto producto = new Producto
                            {
                                Id = reader.GetInt32("Id"),
                                Nombre = reader.GetString("Nombre"),
                                Marca = reader.GetString("Marca"),
                                Modelo = reader.GetString("Modelo"),
                                Precio = reader.GetDouble("Precio"),
                                Iva = reader.GetDouble("Iva"),
                                PrecioTotal = reader.GetDouble("PrecioTotal"),
                                Descripcion = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion"),
                                CategoriaId = reader.GetInt32("CategoriaId"),
                                CategoriaNombre = reader.IsDBNull(reader.GetOrdinal("NombreCategoria")) ? null : reader.GetString("NombreCategoria"),
                                Stock = reader.GetInt32("Stock"),
                                Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString("Imagen"),
                                FechaCreacion = reader.GetDateTime("FechaCreacion"),
                                Descuento = reader.GetDouble("Descuento"),
                                EAN = reader.IsDBNull(reader.GetOrdinal("EAN")) ? null : reader.GetString("EAN")
                            };
                            productos.Add(producto);
                        }

                        ProductosListView.ItemsSource = productos;
                        view = CollectionViewSource.GetDefaultView(ProductosListView.ItemsSource);
                    }
                }
            }
        }


        private void ExportarButton_Click(object sender, RoutedEventArgs e)
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string sql = "SELECT * FROM productos";

                using (var connection = conexion.ObtenerConexion())
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    using (var package = new ExcelPackage())
                    {
                        var worksheet = package.Workbook.Worksheets.Add("Productos");

                        // Agregar encabezados
                        worksheet.Cells[1, 1].Value = "ID";
                        worksheet.Cells[1, 2].Value = "Nombre";
                        worksheet.Cells[1, 3].Value = "Marca";
                        worksheet.Cells[1, 4].Value = "Modelo";
                        worksheet.Cells[1, 5].Value = "Precio";
                        worksheet.Cells[1, 6].Value = "IVA";
                        worksheet.Cells[1, 7].Value = "Precio Total";
                        worksheet.Cells[1, 8].Value = "Descripción";
                        worksheet.Cells[1, 9].Value = "Categoría ID";
                        worksheet.Cells[1, 10].Value = "Stock";
                        worksheet.Cells[1, 11].Value = "Imagen";
                        worksheet.Cells[1, 12].Value = "Fecha Creación";
                        worksheet.Cells[1, 13].Value = "Descuento";
                        worksheet.Cells[1, 14].Value = "EAN";

                        int row = 2;
                        while (reader.Read())
                        {
                            worksheet.Cells[row, 1].Value = reader.GetInt32("Id");
                            worksheet.Cells[row, 2].Value = reader.GetString("Nombre");
                            worksheet.Cells[row, 3].Value = reader.GetString("Marca");
                            worksheet.Cells[row, 4].Value = reader.GetString("Modelo");
                            worksheet.Cells[row, 5].Value = reader.GetDouble("Precio");
                            worksheet.Cells[row, 6].Value = reader.GetDouble("Iva");
                            worksheet.Cells[row, 7].Value = reader.GetDouble("PrecioTotal");
                            worksheet.Cells[row, 8].Value = reader.IsDBNull(reader.GetOrdinal("Descripcion")) ? null : reader.GetString("Descripcion");
                            worksheet.Cells[row, 9].Value = reader.GetInt32("CategoriaId");
                            worksheet.Cells[row, 10].Value = reader.GetInt32("Stock");
                            worksheet.Cells[row, 11].Value = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString("Imagen");
                            worksheet.Cells[row, 12].Value = reader.GetDateTime("FechaCreacion");
                            worksheet.Cells[row, 13].Value = reader.GetDouble("Descuento");
                            worksheet.Cells[row, 14].Value = reader.IsDBNull(reader.GetOrdinal("EAN")) ? null : reader.GetString("EAN");
                            row++;
                        }

                        // Guardar el archivo en el escritorio
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string filePath = System.IO.Path.Combine(desktopPath, "Catalogo Productos.xlsx");

                        // Guardar el archivo
                        System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
                        package.SaveAs(fi);
                        MessageBox.Show("Exportación a Excel completada.");

                        // Abrir el archivo Excel
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = fi.FullName,
                            UseShellExecute = true // Importante para abrir el archivo
                        });
                    }
                }
            }
        }


        private void AgregarProductoButton_Click(object sender, RoutedEventArgs e)
        {
            AgregarProductoView agregarProductoView = new AgregarProductoView();
            Window mainWindow = Window.GetWindow(this);
            if (mainWindow is Principal principal)
            {
                principal.ContenidoPrincipal.Content = agregarProductoView;
            }
            else
            {
                MessageBox.Show("No se pudo obtener la ventana principal.");
            }
        }

        private void ModificarProductoButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductosListView.SelectedItem is Producto productoSeleccionado)
            {
                ModificarProductoView modificarProductoView = new ModificarProductoView(productoSeleccionado);
                Window mainWindow = Window.GetWindow(this);
                if (mainWindow is Principal principal)
                {
                    principal.ContenidoPrincipal.Content = modificarProductoView;
                }
                else
                {
                    MessageBox.Show("No se pudo obtener la ventana principal.");
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un producto para modificar.");
            }
        }

        private void EliminarProductoButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductosListView.SelectedItem is Producto productoSeleccionado)
            {
                string mensaje = $"¿Desea eliminar el producto:\n\n" +
                                 $"ID: {productoSeleccionado.Id}\n" +
                                 $"Nombre: {productoSeleccionado.Nombre}\n" +
                                 $"Precio: {productoSeleccionado.Precio:C}\n" +
                                 $"Descripción: {productoSeleccionado.Descripcion}\n";

                MessageBoxResult resultado = MessageBox.Show(mensaje, "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    using (var conexion = new Conexion())
                    {
                        conexion.AbrirConexion();
                        string sql = "DELETE FROM productos WHERE Id = @id";

                        using (var connection = conexion.ObtenerConexion())
                        using (MySqlCommand command = new MySqlCommand(sql, connection))
                        {
                            command.Parameters.AddWithValue("@id", productoSeleccionado.Id);
                            int filasAfectadas = command.ExecuteNonQuery();

                            if (filasAfectadas > 0)
                            {
                                MessageBox.Show("Producto eliminado exitosamente.");
                                CargarProductos();
                            }
                            else
                            {
                                MessageBox.Show("No se pudo eliminar el producto.");
                            }
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un producto para eliminar.");
            }
        }

        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            CargarProductos();
        }

        private void ProductosListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ProductosListView.SelectedItem is Producto productoSeleccionado)
            {
                DetallesProductoView detallesView = new DetallesProductoView(productoSeleccionado);
                detallesView.ShowDialog(); // Mostrar la ventana como un diálogo
            }
        }

    }
}