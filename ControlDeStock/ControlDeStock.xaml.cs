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
       
        private string currentSort;
        private bool isAscending = true;
        public ControlDeStock()
        {
            InitializeComponent();
            this.DataContext = this;
            CargarProductos();
           
           
        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Código para manejar el evento GotFocus
            if (BuscarTextBox.Text == "Introduce el producto por ID, Nombre, Marca o Categoria")
            {
                BuscarTextBox.Text = "";
                BuscarTextBox.Foreground = Brushes.Black;
            }
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Código para manejar el evento LostFocus
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
        WHERE p.Id = @searchTerm OR p.Nombre LIKE @searchTerm OR p.Marca LIKE @searchTerm OR c.Nombre LIKE @searchTerm;";

            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion(); // Abrir la conexión
                    using (MySqlCommand command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");

                        // Ejecutar la consulta y llenar el DataTable
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            ControlDeStockListView.ItemsSource = dataTable.DefaultView; // Asignar el resultado al ListView

                            // Cambiar el color de fondo de cada ListViewItem
                            foreach (DataRowView rowView in dataTable.DefaultView)
                            {
                                var listViewItem = (ListViewItem)ControlDeStockListView.ItemContainerGenerator.ContainerFromItem(rowView);
                                if (listViewItem != null)
                                {
                                    // Obtener los valores necesarios
                                    var stock = Convert.ToInt32(rowView["Stock"]);
                                    var cantidadMinima = Convert.ToInt32(rowView["CantidadMinima"]);

                                    // Comprobar si el stock es menor que la cantidad mínima
                                    if (stock < cantidadMinima)
                                    {
                                        listViewItem.Background = new SolidColorBrush(Colors.Red); // Cambiar a rojo
                                    }
                                    else
                                    {
                                        listViewItem.Background = new SolidColorBrush(Colors.White); // Color normal
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    conexion.CerrarConexion(); // Cerrar la conexión
                }
            }
        }



        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            BuscarButton_Click(sender, e);
        }

        private void CargarProductos()
        {
            using (Conexion conexion = new Conexion())
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

                                // No necesitas asignar IsOutOfStock aquí, se calculará automáticamente
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

        private void ControlDeStockListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Obtiene la posición del cursor
            var mousePos = Mouse.GetPosition(ControlDeStockListView);
            var item = ControlDeStockListView.InputHitTest(mousePos) as ListViewItem;

            
            if (item == null)
            {
                e.Handled = true; // Evita que se muestre el menú si no se hace clic en un elemento
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

            using (var conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    var connection = conexion.ObtenerConexion();

                    // 1. Primero obtenemos el precio y la marca (proveedor) del producto
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

                    // Calculamos el precio de compra (80% del precio de venta como ejemplo)
                    decimal precioCompra = Math.Round(precioVenta * 0.8m, 2);

                    // 2. Buscamos si existe una orden abierta para este proveedor
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

                        if (result == null) // No existe orden abierta
                        {
                            // 3. Creamos una nueva orden
                            string insertOrden = @"
                        INSERT INTO OrdenesDeCompra (NumeroOrden, Proveedor, Estado) 
                        VALUES (@numeroOrden, @proveedor, 'Pendiente');
                        SELECT LAST_INSERT_ID();";

                            using (var cmdNuevaOrden = new MySqlCommand(insertOrden, connection))
                            {
                                string numeroOrden = $"OC-{DateTime.Now:yyyyMMdd}-{new Random().Next(1000, 9999)}";
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

                    // 4. Insertamos el detalle de la orden
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
                finally
                {
                    conexion.CerrarConexion();
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

                        // Guardar el archivo en el escritorio
                        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                        string filePath = System.IO.Path.Combine(desktopPath, "Control de Stock.xlsx");

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


        private void ModificarMaximasyMinimasButton_Click(object sender, RoutedEventArgs e)
        {
            if (ControlDeStockListView.SelectedItem == null)
            {
                MessageBox.Show("Debe seleccionar un elemento de la lista.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Cambia este cast a Producto
            var selectedItem = ControlDeStockListView.SelectedItem as Producto;
            if (selectedItem != null)
            {
                // Crea un nuevo objeto Producto basado en el seleccionado
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



        private void Header_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock header)
            {
                string propertyName = header.Tag.ToString();

                // Cambiar el estado de ordenamiento
                if (currentSort == propertyName)
                {
                    isAscending = !isAscending; // Cambia de ascendente a descendente
                }
                else
                {
                    currentSort = propertyName; // Actualiza la propiedad actual
                    isAscending = true; // Por defecto, ascendente
                }

                // Obtener la vista de colección
                var view = CollectionViewSource.GetDefaultView(ControlDeStockListView.ItemsSource);
                view.SortDescriptions.Clear(); // Limpiar descripciones de ordenamiento

                // Añadir la nueva descripción de ordenamiento
                view.SortDescriptions.Add(new SortDescription(propertyName, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));

                // Actualizar las cabeceras para mostrar la dirección del orden
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
