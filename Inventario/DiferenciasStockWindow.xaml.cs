using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace TFG.Inventario
{
    public partial class DiferenciasStockWindow : Window
    {
        private List<Producto> productosConDiferencia;

        public DiferenciasStockWindow(List<Producto> productosConDiferencia)
        {
            InitializeComponent();
            this.productosConDiferencia = productosConDiferencia;
            DiferenciasDataGrid.ItemsSource = productosConDiferencia;
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PonerRecuentoACeroButton_Click(object sender, RoutedEventArgs e)
        {
            if (DiferenciasDataGrid.SelectedItem is Producto selectedProducto)
            {
                using (var conexion = new Conexion())
                {
                    try
                    {
                        conexion.AbrirConexion();

                        // Obtener el ID del producto seleccionado
                        string queryId = "SELECT Id FROM productos WHERE EAN = @EAN";
                        int productoId;

                        using (var commandId = new MySqlCommand(queryId, conexion.ObtenerConexion()))
                        {
                            commandId.Parameters.AddWithValue("@EAN", selectedProducto.EAN);
                            productoId = Convert.ToInt32(commandId.ExecuteScalar());
                        }

                        // Actualizar todos los registros de inventario relacionados con este producto
                        string queryInventario = @"
                    UPDATE inventario 
                    SET Recuento = 0 
                    WHERE ProductoId = @ProductoId";

                        using (var command = new MySqlCommand(queryInventario, conexion.ObtenerConexion()))
                        {
                            command.Parameters.AddWithValue("@ProductoId", productoId);
                            int rowsAffected = command.ExecuteNonQuery();

                            MessageBox.Show($"Se ha actualizado el recuento del producto '{selectedProducto.Nombre}' a 0 en todas las ubicaciones.",
                                "Actualización Completada", MessageBoxButton.OK, MessageBoxImage.Information);
                            this.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al actualizar los recuentos: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un producto en la lista.",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ActualizarStockButton_Click(object sender, RoutedEventArgs e)
        {
            if (DiferenciasDataGrid.SelectedItem is Producto selectedProducto)
            {
                using (var conexion = new Conexion())
                {
                    try
                    {
                        conexion.AbrirConexion();

                        // Obtener el ID del producto seleccionado
                        string queryId = "SELECT Id FROM productos WHERE EAN = @EAN";
                        int productoId;

                        using (var commandId = new MySqlCommand(queryId, conexion.ObtenerConexion()))
                        {
                            commandId.Parameters.AddWithValue("@EAN", selectedProducto.EAN);
                            productoId = Convert.ToInt32(commandId.ExecuteScalar());
                        }

                        // Obtener la suma de los recuentos de inventario para este producto
                        string queryInventarioSum = @"
                    SELECT SUM(Recuento) AS TotalRecuento
                    FROM inventario
                    WHERE ProductoId = @ProductoId";

                        int totalRecuento;
                        using (var commandInventarioSum = new MySqlCommand(queryInventarioSum, conexion.ObtenerConexion()))
                        {
                            commandInventarioSum.Parameters.AddWithValue("@ProductoId", productoId);
                            totalRecuento = Convert.ToInt32(commandInventarioSum.ExecuteScalar());
                        }

                        // Actualizar el Stock en la tabla productos
                        string queryProductos = @"
                    UPDATE productos
                    SET Stock = @TotalRecuento
                    WHERE Id = @ProductoId";

                        using (var commandProductos = new MySqlCommand(queryProductos, conexion.ObtenerConexion()))
                        {
                            commandProductos.Parameters.AddWithValue("@TotalRecuento", totalRecuento);
                            commandProductos.Parameters.AddWithValue("@ProductoId", productoId);
                            commandProductos.ExecuteNonQuery();

                            MessageBox.Show($"Se ha actualizado el Stock del producto '{selectedProducto.Nombre}' a {totalRecuento}.",
                                "Actualización Completada", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al actualizar el Stock: {ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Debe seleccionar un producto en la lista.",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
