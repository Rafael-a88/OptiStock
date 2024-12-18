using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TFG
{
    public partial class NuevaVentaView : UserControl
    {
        // Lista para almacenar los productos en la venta
        private List<DetalleVenta> productosEnVenta = new List<DetalleVenta>();

        // Constructor
        public NuevaVentaView()
        {
            InitializeComponent(); // Asegúrate de que NuevaVenta.xaml esté correctamente vinculado
        }

        // Evento para agregar un producto
        private void AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar que se ingresen datos
                if (string.IsNullOrWhiteSpace(CodigoProductoTextBox.Text) || string.IsNullOrWhiteSpace(CantidadTextBox.Text))
                {
                    MessageBox.Show("Por favor, ingrese el código y la cantidad del producto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Simular la búsqueda del producto (esto normalmente viene de una base de datos)
                int codigoProducto = int.Parse(CodigoProductoTextBox.Text);
                string nombreProducto = $"Producto {codigoProducto}";
                double precioUnitario = 10.0; // Precio fijo para el ejemplo
                int cantidad = int.Parse(CantidadTextBox.Text);

                // Crear un nuevo producto y agregarlo a la lista
                var nuevoProducto = new DetalleVenta(codigoProducto, nombreProducto, precioUnitario, cantidad);
                productosEnVenta.Add(nuevoProducto);

                // Actualizar el DataGrid
                ProductosDataGrid.ItemsSource = null; // Esto es necesario para refrescar la UI
                ProductosDataGrid.ItemsSource = productosEnVenta;

                // Actualizar el total
                ActualizarTotal();

                // Limpiar los campos de entrada
                CodigoProductoTextBox.Clear();
                CantidadTextBox.Clear();

                MessageBox.Show("Producto agregado correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al agregar el producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento para pagar la venta
        private void PagarVenta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!productosEnVenta.Any())
                {
                    MessageBox.Show("No hay productos en la venta.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Simular el procesamiento de la venta (normalmente esto se guarda en la base de datos)
                double totalVenta = productosEnVenta.Sum(p => p.Subtotal);
                MessageBox.Show($"Venta procesada correctamente. Total: {totalVenta:C2}", "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar la venta
                productosEnVenta.Clear();
                ProductosDataGrid.ItemsSource = null;
                ActualizarTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la venta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Evento para cancelar la venta
        private void CancelarVenta_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("¿Está seguro de que desea cancelar la venta?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    // Limpiar los datos de la venta
                    productosEnVenta.Clear();
                    ProductosDataGrid.ItemsSource = null;
                    ActualizarTotal();

                    MessageBox.Show("Venta cancelada correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cancelar la venta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método para actualizar el total de la venta
        private void ActualizarTotal()
        {
            double total = productosEnVenta.Sum(p => p.Subtotal);
            TotalTextBlock.Text = total.ToString("C2");
        }
    }

    // Clase auxiliar para los detalles de la venta
    public class DetalleVenta
    {
        public int ProductoId { get; set; }
        public string ProductoNombre { get; set; }
        public double PrecioUnitario { get; set; }
        public int Cantidad { get; set; }
        public double Subtotal => PrecioUnitario * Cantidad; // Propiedad calculada

        public DetalleVenta(int productoId, string productoNombre, double precioUnitario, int cantidad)
        {
            ProductoId = productoId;
            ProductoNombre = productoNombre;
            PrecioUnitario = precioUnitario;
            Cantidad = cantidad;
        }
    }
}
