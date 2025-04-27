using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
using System.Collections.ObjectModel;
using OfficeOpenXml;

namespace TFG.ControlDeStock
{
    public partial class ControlDeStock : UserControl
    {
        private string currentSort = ""; // Variable para controlar la columna actual de ordenamiento
        private bool isAscending = true;
        private ICollectionView view; // Vista para gestionar el ordenamiento

        public ControlDeStock()
        {
            InitializeComponent();
            this.DataContext = this;
            CargarProductos();
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
            string searchTerm = BuscarTextBox.Text.Trim();
            string query = @"
            SELECT p.*, c.Nombre AS CategoriaNombre
            FROM productos p
            JOIN categorias c ON p.CategoriaId = c.Id
            WHERE CAST(p.Id AS CHAR) LIKE @searchTerm OR p.Nombre LIKE @searchTerm OR p.Marca LIKE @searchTerm OR c.Nombre LIKE @searchTerm;";

            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    using (MySqlCommand command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");

                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            List<Producto> productos = new List<Producto>();

                            while (reader.Read())
                            {
                                Producto producto = new Producto
                                {
                                    EAN = reader["EAN"].ToString(),
                                    Nombre = reader["Nombre"].ToString(),
                                    Stock = reader["Stock"] != DBNull.Value ? Convert.ToInt32(reader["Stock"]) : 0,
                                    CantidadMaxima = reader["CantidadMaxima"] != DBNull.Value ? Convert.ToInt32(reader["CantidadMaxima"]) : 0,
                                    CantidadMinima = reader["CantidadMinima"] != DBNull.Value ? Convert.ToInt32(reader["CantidadMinima"]) : 0,
                                    Marca = reader["Marca"] != DBNull.Value ? reader["Marca"].ToString() : string.Empty
                                };

                                productos.Add(producto);
                            }

                            ControlDeStockListView.ItemsSource = productos;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            CargarProductos();
        }

        private void CargarProductos()
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string query = "SELECT * FROM productos";

                using (MySqlCommand command = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    try
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            List<Producto> productos = new List<Producto>();

                            while (reader.Read())
                            {
                                Producto producto = new Producto
                                {
                                    EAN = reader["EAN"].ToString(),
                                    Nombre = reader["Nombre"].ToString(),
                                    Stock = reader["Stock"] != DBNull.Value ? Convert.ToInt32(reader["Stock"]) : 0,
                                    CantidadMaxima = reader["CantidadMaxima"] != DBNull.Value ? Convert.ToInt32(reader["CantidadMaxima"]) : 0,
                                    CantidadMinima = reader["CantidadMinima"] != DBNull.Value ? Convert.ToInt32(reader["CantidadMinima"]) : 0,
                                    Marca = reader["Marca"] != DBNull.Value ? reader["Marca"].ToString() : string.Empty
                                };

                                productos.Add(producto);
                            }

                            ControlDeStockListView.ItemsSource = productos;

                            // Configurar la vista para ordenamiento
                            view = CollectionViewSource.GetDefaultView(ControlDeStockListView.ItemsSource);
                            view.SortDescriptions.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        ControlDeStockListView.ItemsSource = null; // Limpiar la lista en caso de error
                        MessageBox.Show("Error al cargar productos: " + ex.Message);
                    }
                }
            }
        }

        private void ControlDeStockListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var mousePos = Mouse.GetPosition(ControlDeStockListView);
            var item = ControlDeStockListView.InputHitTest(mousePos) as ListViewItem;

            if (item == null)
            {
                e.Handled = true;
            }
        }

        private void MandarOrdenesButton_Click(object sender, RoutedEventArgs e)
        {
            var productoSeleccionado = ControlDeStockListView.SelectedItem as Producto;

            if (productoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione un producto.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Verificar si el producto ya está en las órdenes de compra
            using (var conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    string queryVerificar = @"
                    SELECT COUNT(*) 
                    FROM DetallesOrdenDeCompra d 
                    INNER JOIN OrdenesDeCompra o ON d.OrdenDeCompraId = o.Id 
                    WHERE d.EAN = @ean AND o.Estado != 'Entregado'";

                    using (var cmdVerificar = new MySqlCommand(queryVerificar, conexion.ObtenerConexion()))
                    {
                        cmdVerificar.Parameters.AddWithValue("@ean", productoSeleccionado.EAN);
                        int count = Convert.ToInt32(cmdVerificar.ExecuteScalar());

                        if (count > 0)
                        {
                            var result = MessageBox.Show("Este artículo ya está añadido a órdenes de compra, ¿desea mandarlo de todos modos?",
                                                          "Confirmación",
                                                          MessageBoxButton.YesNo,
                                                          MessageBoxImage.Question);
                            if (result == MessageBoxResult.No)
                            {
                                return;
                            }
                        }
                    }

                    // Proceder a añadir el producto a la orden de compra
                    var connection = conexion.ObtenerConexion();
                    string queryProducto = @"SELECT Precio, Marca FROM productos WHERE EAN = @ean";
                    decimal precioVenta = 0;
                    string proveedor = "";

                    using (var cmdProducto = new MySqlCommand(queryProducto, connection))
                    {
                        cmdProducto.Parameters.AddWithValue("@ean", productoSeleccionado.EAN);
                        using (var reader = cmdProducto.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                precioVenta = Convert.ToDecimal(reader["Precio"]);
                                proveedor = reader["Marca"].ToString();
                            }
                        }
                    }

                    decimal precioCompra = Math.Round(precioVenta * 0.8m, 2);

                    string queryOrdenExistente = @"
                    SELECT Id 
                    FROM OrdenesDeCompra 
                    WHERE Proveedor = @proveedor 
                    AND Estado != 'Entregado'
                    ORDER BY FechaApertura DESC 
                    LIMIT 1";

                    int ordenId;
                    using (var cmdOrden = new MySqlCommand(queryOrdenExistente, connection))
                    {
                        cmdOrden.Parameters.AddWithValue("@proveedor", proveedor);
                        var result = cmdOrden.ExecuteScalar();

                        if (result == null)
                        {
                            string insertOrden = @"
                            INSERT INTO OrdenesDeCompra (NumeroOrden, Proveedor, Estado) 
                            VALUES (@numeroOrden, @proveedor, 'Pendiente');
                            SELECT LAST_INSERT_ID();";

                            using (var cmdNuevaOrden = new MySqlCommand(insertOrden, connection))
                            {
                                string numeroOrden = $"OC-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8)}";
                                cmdNuevaOrden.Parameters.AddWithValue("@numeroOrden", numeroOrden);
                                cmdNuevaOrden.Parameters.AddWithValue("@proveedor", proveedor);
                                ordenId = Convert.ToInt32(cmdNuevaOrden.ExecuteScalar());
                            }
                        }
                        else
                        {
                            ordenId = Convert.ToInt32(result);
                        }
                    }

                    string insertDetalle = @"
                    INSERT INTO DetallesOrdenDeCompra 
                    (OrdenDeCompraId, ProductoId, EAN, Cantidad, PrecioUnitario)
                    VALUES 
                    (@ordenId, (SELECT Id FROM productos WHERE EAN = @ean), @ean, 1, @precioUnitario)";

                    using (var cmdDetalle = new MySqlCommand(insertDetalle, connection))
                    {
                        cmdDetalle.Parameters.AddWithValue("@ordenId", ordenId);
                        cmdDetalle.Parameters.AddWithValue("@ean", productoSeleccionado.EAN);
                        cmdDetalle.Parameters.AddWithValue("@precioUnitario", precioCompra);

                        cmdDetalle.ExecuteNonQuery();
                    }

                    MessageBox.Show($"Producto añadido correctamente a la orden de compra #{ordenId}", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al procesar la orden: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

                        worksheet.Cells[1, 1].Value = "EAN";
                        worksheet.Cells[1, 2].Value = "Nombre";
                        worksheet.Cells[1, 3].Value = "Stock";
                        worksheet.Cells[1, 4].Value = "Cantidad Maxima";
                        worksheet.Cells[1, 5].Value = "Cantidad Minima";
                        worksheet.Cells[1, 6].Value = "Proveedor";

                        int row = 2;
                        while (reader.Read())
                        {
                            worksheet.Cells[row, 1].Value = reader.IsDBNull(reader.GetOrdinal("EAN")) ? null : reader.GetString("EAN");
                            worksheet.Cells[row, 2].Value = reader.IsDBNull(reader.GetOrdinal("Nombre")) ? null : reader.GetString("Nombre");
                            worksheet.Cells[row, 3].Value = reader.IsDBNull(reader.GetOrdinal("Stock")) ? 0 : reader.GetInt32("Stock");
                            worksheet.Cells[row, 4].Value = reader.IsDBNull(reader.GetOrdinal("CantidadMaxima")) ? 0 : reader.GetInt32("CantidadMaxima");
                            worksheet.Cells[row, 5].Value = reader.IsDBNull(reader.GetOrdinal("CantidadMinima")) ? 0 : reader.GetInt32("CantidadMinima");
                            worksheet.Cells[row, 6].Value = reader.IsDBNull(reader.GetOrdinal("Marca")) ? null : reader.GetString("Marca");

                            row++;
                        }

                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string filePath = System.IO.Path.Combine(desktopPath, "Control de Stock.xlsx");

                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }

                        System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
                        package.SaveAs(fi);
                        MessageBox.Show("Exportación a Excel completada.");

                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = fi.FullName,
                            UseShellExecute = true
                        });
                    }
                }
            }
        }

        private void ModificarMaximasyMinimasButton_Click(object sender, RoutedEventArgs e)
        {
            if (ControlDeStockListView.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un elemento de la lista.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedItem = ControlDeStockListView.SelectedItem as Producto;
            if (selectedItem != null)
            {
                Producto producto = new Producto
                {
                    Nombre = selectedItem.Nombre,
                    EAN = selectedItem.EAN,
                    Stock = selectedItem.Stock,
                    Marca = selectedItem.Marca,
                    CantidadMaxima = selectedItem.CantidadMaxima,
                    CantidadMinima = selectedItem.CantidadMinima
                };

                ModificarMaximasyMinimas modificarMaximasyMinimasView = new ModificarMaximasyMinimas();
                modificarMaximasyMinimasView.SetDataContext(producto);

                Window mainWindow = Window.GetWindow(this);
                if (mainWindow is Principal principal)
                {
                    principal.ContenidoPrincipal.Content = modificarMaximasyMinimasView;
                }
                else
                {
                    MessageBox.Show("No se pudo obtener la ventana principal.");
                }
            }
        }

        private void MostrarPosiblesPedidosButton_Click(object sender, RoutedEventArgs e)
        {
            MostrarPosiblesPedidos();
        }

        private void MostrarPosiblesPedidos()
        {
            using (Conexion conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string query = @"
                SELECT * FROM productos
                WHERE Stock <= CantidadMinima";

                using (MySqlCommand command = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    try
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            List<Producto> productos = new List<Producto>();

                            while (reader.Read())
                            {
                                Producto producto = new Producto
                                {
                                    EAN = reader["EAN"].ToString(),
                                    Nombre = reader["Nombre"].ToString(),
                                    Stock = reader["Stock"] != DBNull.Value ? Convert.ToInt32(reader["Stock"]) : 0,
                                    CantidadMaxima = reader["CantidadMaxima"] != DBNull.Value ? Convert.ToInt32(reader["CantidadMaxima"]) : 0,
                                    CantidadMinima = reader["CantidadMinima"] != DBNull.Value ? Convert.ToInt32(reader["CantidadMinima"]) : 0,
                                    Marca = reader["Marca"] != DBNull.Value ? reader["Marca"].ToString() : string.Empty
                                };

                                productos.Add(producto);
                            }

                            ControlDeStockListView.ItemsSource = productos;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al cargar productos: " + ex.Message);
                    }
                }
            }
        }
        private void Header_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock header)
            {
                string propertyName = header.Tag.ToString();

                if (string.IsNullOrEmpty(propertyName))
                {
                    return; // Salir si no hay propiedad asociada al encabezado
                }

                // Invertir la dirección de ordenación si se hace clic en la misma columna
                if (currentSort == propertyName)
                {
                    isAscending = !isAscending;
                }
                else
                {
                    currentSort = propertyName;
                    isAscending = true;
                }

                if (ControlDeStockListView.ItemsSource == null || ControlDeStockListView.Items.Count == 0)
                {
                    MessageBox.Show("No hay datos para ordenar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                view = CollectionViewSource.GetDefaultView(ControlDeStockListView.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(propertyName, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));

                var gridView = ControlDeStockListView.View as GridView;
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


    }
}
