using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TFG.Ubicaciones;

namespace TFG.Inventario
{
    public partial class Inventario : UserControl
    {
        public List<Producto> Productos { get; set; }
        public List<Ubicacion> Ubicaciones { get; set; }

        public Inventario()
        {
            InitializeComponent();
            CargarUbicaciones(); // Primero cargamos las ubicaciones
        }

        private void CargarProductos(Ubicacion ubicacion)
        {
            Productos = new List<Producto>();

            using (var conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    string query = @"
                SELECT p.EAN, p.Nombre, p.Stock, c.Nombre AS CategoriaNombre, i.Recuento, i.UbicacionRecuento, u.Nombre AS UbicacionNombre
                FROM productos p
                LEFT JOIN inventario i ON p.Id = i.ProductoId
                LEFT JOIN ubicaciones u ON i.UbicacionId = u.Id
                LEFT JOIN categorias c ON p.CategoriaId = c.Id
                WHERE i.UbicacionId = @UbicacionId";

                    using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@UbicacionId", ubicacion.Id);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var producto = new Producto
                                {
                                    EAN = reader.GetString("EAN"),
                                    Nombre = reader.GetString("Nombre"),
                                    CategoriaNombre = reader.IsDBNull(reader.GetOrdinal("CategoriaNombre")) ? null : reader.GetString("CategoriaNombre"),
                                    Stock = reader.IsDBNull(reader.GetOrdinal("Stock")) ? 0 : reader.GetInt32("Stock"),
                                    Recuento = reader.IsDBNull(reader.GetOrdinal("Recuento")) ? 0 : reader.GetInt32("Recuento"),
                                    UbicacionRecuento = reader.IsDBNull(reader.GetOrdinal("UbicacionNombre")) ? null : reader.GetString("UbicacionNombre")
                                };
                                Productos.Add(producto);
                            }
                        }
                    }

                    InventarioDataGrid.ItemsSource = Productos; // Actualiza el DataGrid
                    UbicacionTextBlock.Text = ubicacion.Nombre;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar los productos: " + ex.Message);
                }
            }
        }

        private void CargarUbicaciones()
        {
            Ubicaciones = new List<Ubicacion>();

            using (var conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    string query = "SELECT Id, Nombre FROM ubicaciones";

                    using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Ubicaciones.Add(new Ubicacion(reader.GetString("Nombre"))
                                {
                                    Id = reader.GetInt32("Id"),
                                    Nombre = reader.GetString("Nombre")
                                });
                            }
                        }
                    }

                    UbicacionComboBox.ItemsSource = Ubicaciones.OrderBy(u => u.Nombre).ToList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar las ubicaciones: " + ex.Message);
                }
            }
        }

        private void BuscarProductoPorEAN(string ean)
        {
            using (var conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    string query = @"
                SELECT p.EAN, p.Nombre, p.Stock, c.Nombre AS CategoriaNombre, 
                       SUM(i.Recuento) AS RecuentoTotal, 
                       GROUP_CONCAT(i.UbicacionRecuento SEPARATOR ', ') AS UbicacionesRecuento
                FROM productos p
                LEFT JOIN inventario i ON p.Id = i.ProductoId
                JOIN categorias c ON p.CategoriaId = c.Id
                WHERE p.EAN = @EAN
                GROUP BY p.EAN, p.Nombre, p.Stock, c.Nombre";

                    using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@EAN", ean);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var producto = new Producto
                                {
                                    EAN = reader.GetString("EAN"),
                                    Nombre = reader.GetString("Nombre"),
                                    CategoriaNombre = reader.GetString("CategoriaNombre"),
                                    Stock = reader.GetInt32("Stock"),
                                    Recuento = reader.IsDBNull(reader.GetOrdinal("RecuentoTotal")) ? 0 : reader.GetInt32("RecuentoTotal"),
                                    UbicacionRecuento = reader.IsDBNull(reader.GetOrdinal("UbicacionesRecuento")) ? null : reader.GetString("UbicacionesRecuento")
                                };

                                // Comprobar si el producto ya está en la lista de productos
                                var existingProducto = Productos.FirstOrDefault(p => p.EAN == producto.EAN);
                                if (existingProducto != null)
                                {
                                    // Actualizar el recuento y la ubicación del recuento existente
                                    existingProducto.Recuento = producto.Recuento;
                                    existingProducto.UbicacionRecuento = producto.UbicacionRecuento;
                                }
                                else
                                {
                                    Productos.Add(producto);
                                }

                                InventarioDataGrid.ItemsSource = null;
                                InventarioDataGrid.ItemsSource = Productos;

                                MessageBox.Show($"El producto con EAN {ean} tiene un recuento acumulado de {producto.Recuento} en las ubicaciones: {producto.UbicacionRecuento}.");
                            }
                            else
                            {
                                MessageBox.Show($"No se encontró ningún producto con el EAN {ean}.");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al buscar el producto: " + ex.Message);
                }
            }
        }

        private void EANTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) // Detecta si se presionó la tecla Enter
            {
                string ean = EANTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(ean))
                {
                    BuscarProductoPorEAN(ean);
                }
                else
                {
                    MessageBox.Show("Por favor, introduce un código EAN válido.");
                }
            }
        }

        private void UbicacionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UbicacionComboBox.SelectedItem is Ubicacion ubicacion)
            {
                CargarProductos(ubicacion);
            }
        }

        private void ActualizarInventario_Click(object sender, RoutedEventArgs e)
        {
            if (UbicacionComboBox.SelectedItem is Ubicacion ubicacion && !string.IsNullOrWhiteSpace(CantidadTextBox.Text))
            {
                if (!int.TryParse(CantidadTextBox.Text, out int cantidad))
                {
                    MessageBox.Show("Por favor, introduce un valor numérico válido en el campo de cantidad.", "Cantidad Inválida", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var conexion = new Conexion())
                {
                    try
                    {
                        conexion.AbrirConexion();

                        // Obtener el Id del producto seleccionado
                        if (InventarioDataGrid.SelectedItem is Producto producto)
                        {
                            int productoId = GetProductoIdFromEAN(producto.EAN);

                            // Verificar si ya existe un registro para este producto en esta ubicación
                            string queryVerificar = @"
                        SELECT Recuento FROM inventario
                        WHERE ProductoId = @ProductoId AND UbicacionId = @UbicacionId";

                            using (var commandVerificar = new MySqlCommand(queryVerificar, conexion.ObtenerConexion()))
                            {
                                commandVerificar.Parameters.AddWithValue("@ProductoId", productoId);
                                commandVerificar.Parameters.AddWithValue("@UbicacionId", ubicacion.Id);

                                object result = commandVerificar.ExecuteScalar();

                                if (result != null)
                                {
                                    // Si existe, actualizar el recuento sumando/restando la cantidad ingresada
                                    int recuentoActual = Convert.ToInt32(result);
                                    int nuevoRecuento = recuentoActual + cantidad;

                                    if (nuevoRecuento < 0)
                                    {
                                        MessageBox.Show("El recuento no puede ser negativo. Verifica la cantidad ingresada.", "Error de Recuento", MessageBoxButton.OK, MessageBoxImage.Warning);
                                        return;
                                    }

                                    string queryActualizar = @"
                                UPDATE inventario
                                SET Recuento = @NuevoRecuento
                                WHERE ProductoId = @ProductoId AND UbicacionId = @UbicacionId";

                                    using (var commandActualizar = new MySqlCommand(queryActualizar, conexion.ObtenerConexion()))
                                    {
                                        commandActualizar.Parameters.AddWithValue("@NuevoRecuento", nuevoRecuento);
                                        commandActualizar.Parameters.AddWithValue("@ProductoId", productoId);
                                        commandActualizar.Parameters.AddWithValue("@UbicacionId", ubicacion.Id);
                                        commandActualizar.ExecuteNonQuery();
                                    }

                                    MessageBox.Show($"Recuento actualizado correctamente. Nuevo recuento: {nuevoRecuento}.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    MessageBox.Show("El producto no tiene un registro previo en esta ubicación. Usa el botón 'Agregar a Ubicación' para asociarlo.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                                }
                            }

                            // Recargar datos del DataGrid
                            CargarProductos(ubicacion);
                        }
                        else
                        {
                            MessageBox.Show("Selecciona un producto de la lista antes de actualizar el inventario.", "Producto no seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al actualizar el inventario: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecciona una ubicación válida y proporciona una cantidad numérica.", "Datos Inválidos", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private int GetProductoIdFromEAN(string ean)
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string query = "SELECT Id FROM productos WHERE EAN = @EAN";

                using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    command.Parameters.AddWithValue("@EAN", ean);
                    object result = command.ExecuteScalar();

                    if (result != null && int.TryParse(result.ToString(), out int productoId))
                    {
                        return productoId;
                    }
                    else
                    {
                        throw new Exception($"No se encontró ningún producto con el EAN {ean}.");
                    }
                }
            }
        }


        private int GetProductoIdFromSelectedItem()
        {
            if (InventarioDataGrid.SelectedItem is Producto producto)
            {
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    string queryObtenerId = "SELECT Id FROM productos WHERE EAN = @EAN";

                    using (var command = new MySqlCommand(queryObtenerId, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@EAN", producto.EAN);
                        var result = command.ExecuteScalar();
                        if (result != null && int.TryParse(result.ToString(), out int productoId))
                        {
                            return productoId;
                        }
                        else
                        {
                            MessageBox.Show($"No se encontró ningún producto con el EAN {producto.EAN}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return -1; // o manejar el error de otra manera
                        }
                    }
                }
            }
            else
            {
                MostrarMensajeSeleccionProducto();
                return -1;
            }
        }

        private void AgregarAUbicacion_Click(object sender, RoutedEventArgs e)
        {
            AgregarProductoAUbicacion();
        }

        private void AgregarProductoAUbicacion()
        {
            try
            {
                if (UbicacionComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Por favor, selecciona una ubicación antes de agregar el producto al inventario.", "Seleccionar Ubicación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (UbicacionComboBox.SelectedItem is Ubicacion ubicacion)
                {
                    if (InventarioDataGrid.SelectedItem is Producto producto)
                    {
                        using (var conexion = new Conexion())
                        {
                            conexion.AbrirConexion();

                            // Verificar si ya existe un registro para este producto en esta ubicación
                            string queryVerificar = @"
                        SELECT COUNT(*) FROM inventario
                        WHERE ProductoId = (SELECT Id FROM productos WHERE EAN = @EAN) AND UbicacionId = @UbicacionId";

                            using (var commandVerificar = new MySqlCommand(queryVerificar, conexion.ObtenerConexion()))
                            {
                                commandVerificar.Parameters.AddWithValue("@EAN", producto.EAN);
                                commandVerificar.Parameters.AddWithValue("@UbicacionId", ubicacion.Id);

                                int existe = Convert.ToInt32(commandVerificar.ExecuteScalar() ?? 0);

                                if (existe > 0)
                                {
                                    MessageBox.Show($"El producto '{producto.Nombre}' ya está en la ubicación '{ubicacion.Nombre}'.", "Producto Existente", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                                else
                                {
                                    // Insertar el producto en la ubicación seleccionada con recuento inicial de 0
                                    string queryInsertar = @"
                                INSERT INTO inventario (ProductoId, UbicacionId, Recuento, UbicacionRecuento)
                                VALUES ((SELECT Id FROM productos WHERE EAN = @EAN), @UbicacionId, 0, @UbicacionRecuento)";

                                    using (var command = new MySqlCommand(queryInsertar, conexion.ObtenerConexion()))
                                    {
                                        command.Parameters.AddWithValue("@EAN", producto.EAN);
                                        command.Parameters.AddWithValue("@UbicacionId", ubicacion.Id);
                                        command.Parameters.AddWithValue("@UbicacionRecuento", ubicacion.Nombre);

                                        int rowsAffected = command.ExecuteNonQuery();
                                        if (rowsAffected > 0)
                                        {
                                            MessageBox.Show($"El producto '{producto.Nombre}' se ha agregado a la ubicación '{ubicacion.Nombre}' con recuento inicial de 0.", "Producto Agregado", MessageBoxButton.OK, MessageBoxImage.Information);
                                            CargarProductos(ubicacion); // Recargar productos para mostrar la actualización
                                        }
                                        else
                                        {
                                            MessageBox.Show("No se pudo agregar el producto. Verifica los datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        MostrarMensajeSeleccionProducto();
                    }
                }
                else
                {
                    MessageBox.Show("Selecciona una ubicación válida.", "Ubicación no seleccionada", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (MySqlException sqlEx)
            {
                MessageBox.Show($"Error de base de datos: {sqlEx.Message}", "Error SQL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Se produjo un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MostrarMensajeSeleccionProducto()
        {
            MessageBox.Show("Por favor, selecciona el producto a ubicar o inventariar.", "Producto no seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void ConfirmarStockProducto_Click(object sender, RoutedEventArgs e)
        {
            if (InventarioDataGrid.SelectedItem is Producto producto)
            {
                using (var conexion = new Conexion())
                {
                    try
                    {
                        conexion.AbrirConexion();

                        // Actualizar el campo Stock en la tabla productos
                        string queryActualizar = "UPDATE productos SET Stock = @Recuento WHERE EAN = @EAN";
                        using (var commandActualizar = new MySqlCommand(queryActualizar, conexion.ObtenerConexion()))
                        {
                            commandActualizar.Parameters.AddWithValue("@Recuento", producto.Recuento);
                            commandActualizar.Parameters.AddWithValue("@EAN", producto.EAN);
                            int rowsAffected = commandActualizar.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show($"El stock del producto '{producto.Nombre}' con EAN {producto.EAN} se ha actualizado a {producto.Recuento}.", "Stock Actualizado", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show($"No se pudo actualizar el stock del producto '{producto.Nombre}' con EAN {producto.EAN}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    catch (MySqlException sqlEx)
                    {
                        MessageBox.Show($"Error de base de datos: {sqlEx.Message}", "Error SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Se produjo un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selecciona un producto de la lista para confirmar su stock.", "Producto no seleccionado", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void RevisarDiferenciasStock_Click(object sender, RoutedEventArgs e)
        {
            List<Producto> productosConDiferencia = new List<Producto>();

            using (var conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();

                    // Obtener los productos con diferencia entre Stock y Recuento
                    string queryDiferencias = @"
                SELECT p.EAN, p.Nombre, p.Stock, 
                       SUM(IFNULL(i.Recuento, 0)) AS RecuentoTotal,
                       (SUM(IFNULL(i.Recuento, 0)) - p.Stock) AS Diferencia
                FROM productos p
                LEFT JOIN inventario i ON p.Id = i.ProductoId
                GROUP BY p.EAN, p.Nombre, p.Stock
                HAVING p.Stock <> SUM(IFNULL(i.Recuento, 0))
                ORDER BY ABS(SUM(IFNULL(i.Recuento, 0)) - p.Stock) DESC";

                    using (var commandDiferencias = new MySqlCommand(queryDiferencias, conexion.ObtenerConexion()))
                    {
                        using (var reader = commandDiferencias.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var producto = new Producto
                                {
                                    EAN = reader.GetString("EAN"),
                                    Nombre = reader.GetString("Nombre"),
                                    Stock = reader.GetInt32("Stock"),
                                    Recuento = reader.GetInt32("RecuentoTotal"),
                                    Diferencia = reader.GetInt32("Diferencia")
                                };
                                productosConDiferencia.Add(producto);
                            }
                        }
                    }

                    if (productosConDiferencia.Count > 0)
                    {
                        // Mostrar la ventana con los productos con diferencias
                        var diferenciasWindow = new DiferenciasStockWindow(productosConDiferencia);
                        diferenciasWindow.ShowDialog();
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron diferencias entre el stock y el recuento de los productos.",
                                        "Sin Diferencias", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al revisar las diferencias de stock: {ex.Message}",
                                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void NuevoInventarioButton_Click(object sender, RoutedEventArgs e)
        {
            using (var conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();

                    // Actualizar el campo Recuento y Cantidad a 0 en la tabla inventario
                    string queryActualizar = "UPDATE inventario SET Recuento = 0";
                    using (var commandActualizar = new MySqlCommand(queryActualizar, conexion.ObtenerConexion()))
                    {
                        int rowsAffected = commandActualizar.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Se ha realizado un nuevo inventario. Los campos Recuento en la tabla inventario se han puesto a 0.", "Nuevo Inventario", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        else
                        {
                            MessageBox.Show("No se pudo realizar el nuevo inventario. Verifica la conexión a la base de datos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    CargarUbicaciones();
                }
                catch (MySqlException sqlEx)
                {
                    MessageBox.Show($"Error de base de datos: {sqlEx.Message}", "Error SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Se produjo un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void EliminarProductoDeUbicacion(object sender, RoutedEventArgs e)
        {
            // Verifica si se ha seleccionado una ubicación y un producto en la interfaz
            if (UbicacionComboBox.SelectedItem is Ubicacion ubicacion && InventarioDataGrid.SelectedItem is Producto producto)
            {
                using (var conexion = new Conexion())
                {
                    try
                    {
                        conexion.AbrirConexion();

                        // Obtener el Id del producto a partir del EAN
                        int productoId = GetProductoIdFromSelectedItem();

                        // Eliminar el registro de la tabla inventario
                        string queryEliminar = "DELETE FROM inventario WHERE ProductoId = @ProductoId AND UbicacionId = @UbicacionId";
                        using (var commandEliminar = new MySqlCommand(queryEliminar, conexion.ObtenerConexion()))
                        {
                            // Agregar los parámetros a la consulta SQL
                            commandEliminar.Parameters.AddWithValue("@ProductoId", productoId);
                            commandEliminar.Parameters.AddWithValue("@UbicacionId", ubicacion.Id);

                            // Ejecutar la consulta SQL y obtener la cantidad de filas afectadas
                            int rowsAffected = commandEliminar.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                // Actualizar la ubicación del recuento del producto en la lista local
                                producto.UbicacionRecuento = producto.UbicacionRecuento.Replace($"{ubicacion.Nombre}, ", "")
                                                                            .Replace($", {ubicacion.Nombre}", "")
                                                                            .Replace(ubicacion.Nombre, "");

                                // Actualizar la fuente de datos del DataGrid
                                InventarioDataGrid.ItemsSource = null;
                                InventarioDataGrid.ItemsSource = Productos;

                                // Mostrar un mensaje de éxito
                                MessageBox.Show($"El producto {producto.Nombre} se ha eliminado de la ubicación {ubicacion.Nombre}.");
                            }
                            else
                            {
                                // Mostrar un mensaje de error
                                MessageBox.Show("No se pudo eliminar el producto de la ubicación. Verifica los datos.");
                            }
                        }
                    }
                    catch (MySqlException sqlEx)
                    {
                        // Mostrar un mensaje de error de base de datos
                        MessageBox.Show($"Error de base de datos: {sqlEx.Message}", "Error SQL", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    catch (Exception ex)
                    {
                        // Mostrar un mensaje de error general
                        MessageBox.Show($"Se produjo un error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                // Mostrar un mensaje de error si no se ha seleccionado un producto o una ubicación válida
                MessageBox.Show("Selecciona un producto de la lista y una ubicación válida.");
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            CantidadTextBox.Clear();
            UbicacionComboBox.SelectedIndex = -1;
            UltimaActualizacionTextBlock.Text = "Sin Inventariar";
            UbicacionTextBlock.Text = string.Empty;
        }
    }
}



