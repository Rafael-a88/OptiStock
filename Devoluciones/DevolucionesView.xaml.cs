using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using iTextSharp.text.pdf;
using iTextSharp.text;
using MySqlConnector;

namespace TFG
{
    public partial class DevolucionesView : UserControl
    {
        private List<DVenta> productosEnDevolucion = new List<DVenta>();

        // Constructor
        public DevolucionesView()
        {
            InitializeComponent();
            CodigoProductoDevolucionTextBox.KeyDown += CodigoProductoDevolucionTextBox_KeyDown;
        }

        private void CodigoProductoDevolucionTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BuscarProducto();
            }
        }

        private void BuscarProducto()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(CodigoProductoDevolucionTextBox.Text))
                {
                    MessageBox.Show("Por favor, ingrese el código del producto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!long.TryParse(CodigoProductoDevolucionTextBox.Text, out long ean))
                {
                    MessageBox.Show("El código de producto no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string nombreProducto = string.Empty;
                double precioTotal = 0;

                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    var command = new MySqlCommand("SELECT Nombre, PrecioTotal FROM Productos WHERE EAN = @ean", conexion.ObtenerConexion());
                    command.Parameters.AddWithValue("@ean", ean);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            nombreProducto = reader["Nombre"].ToString();
                            precioTotal = Convert.ToDouble(reader["PrecioTotal"]);
                        }
                        else
                        {
                            MessageBox.Show("Producto no encontrado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }

                var nuevoProducto = new DVenta(ean, nombreProducto, precioTotal, 1);
                productosEnDevolucion.Add(nuevoProducto);

                // Actualizar el DataGrid
                ProductosDevolucionDataGrid.ItemsSource = null; // Refrescar la UI
                ProductosDevolucionDataGrid.ItemsSource = productosEnDevolucion;

                // Actualizar el total
                ActualizarTotal();

                // Limpiar el campo de entrada
                CodigoProductoDevolucionTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar el producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegistrarDevolucion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!productosEnDevolucion.Any())
                {
                    MessageBox.Show("No hay productos en la devolución.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string dniCliente = DniClienteDevolucionTextBox.Text;
                string motivoDevolucion = (MotivoDevolucionComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(); // Obtener el motivo

                // Validar que se haya seleccionado un motivo
                if (string.IsNullOrWhiteSpace(motivoDevolucion))
                {
                    MessageBox.Show("Por favor, seleccione un motivo de devolución.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Generar el número de documento
                string numeroDocumento = GenerarNumeroDocumento(DateTime.Now, true);

                // Insertar la devolución en la base de datos
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();

                    // Verificar si el NumeroDocumento ya existe en Ventas
                    var verificarVentaCommand = new MySqlCommand("SELECT COUNT(*) FROM Ventas WHERE NumeroDocumento = @numeroDocumento;", conexion.ObtenerConexion());
                    verificarVentaCommand.Parameters.AddWithValue("@numeroDocumento", numeroDocumento);
                    var existeVenta = Convert.ToInt32(verificarVentaCommand.ExecuteScalar()) > 0;

                    // Si no existe, registrar una nueva venta ficticia para cumplir con la clave foránea
                    if (!existeVenta)
                    {
                        var registrarVentaCommand = new MySqlCommand("INSERT INTO Ventas (DniCliente, NumeroDocumento, Total) VALUES (@dniCliente, @numeroDocumento, @total);", conexion.ObtenerConexion());
                        registrarVentaCommand.Parameters.AddWithValue("@dniCliente", dniCliente);
                        registrarVentaCommand.Parameters.AddWithValue("@numeroDocumento", numeroDocumento);
                        registrarVentaCommand.Parameters.AddWithValue("@total", productosEnDevolucion.Sum(p => p.Subtotal));
                        registrarVentaCommand.ExecuteNonQuery();
                    }

                    // Registrar devolución
                    var command = new MySqlCommand("INSERT INTO Devoluciones (DniCliente, NumeroDocumento, Motivo) VALUES (@dniCliente, @numeroDocumento, @motivo);", conexion.ObtenerConexion());
                    command.Parameters.AddWithValue("@dniCliente", dniCliente);
                    command.Parameters.AddWithValue("@numeroDocumento", numeroDocumento);
                    command.Parameters.AddWithValue("@motivo", motivoDevolucion);
                    command.ExecuteNonQuery();

                    // Actualizar el stock del producto
                    foreach (var producto in productosEnDevolucion)
                    {
                        var updateStockCommand = new MySqlCommand("UPDATE Productos SET Stock = Stock + @cantidad WHERE EAN = @ean;", conexion.ObtenerConexion());
                        updateStockCommand.Parameters.AddWithValue("@cantidad", producto.Cantidad);
                        updateStockCommand.Parameters.AddWithValue("@ean", producto.ProductoId);
                        updateStockCommand.ExecuteNonQuery();
                    }

                    // Registrar movimiento en la tabla Movimientos
                    foreach (var producto in productosEnDevolucion)
                    {
                        var movimientoCommand = new MySqlCommand("INSERT INTO Movimientos (ProductoId, TipoMovimiento, Cantidad, FechaMovimiento) VALUES (@productoId, 'Devolución', @cantidad, NOW());", conexion.ObtenerConexion());
                        movimientoCommand.Parameters.AddWithValue("@productoId", producto.ProductoId);
                        movimientoCommand.Parameters.AddWithValue("@cantidad", producto.Cantidad);
                        movimientoCommand.ExecuteNonQuery();
                    }

                    // Registrar el detalle en el historial de ventas
                    foreach (var producto in productosEnDevolucion)
                    {
                        var historialCommand = new MySqlCommand("INSERT INTO HistorialVentas (NumeroDocumento, Producto, Cantidad) VALUES (@numeroDocumento, @productoId, @cantidad);", conexion.ObtenerConexion());
                        historialCommand.Parameters.AddWithValue("@numeroDocumento", numeroDocumento);
                        historialCommand.Parameters.AddWithValue("@productoId", producto.ProductoId);
                        historialCommand.Parameters.AddWithValue("@cantidad", producto.Cantidad);
                        historialCommand.ExecuteNonQuery();
                    }
                }

                // Generar PDF de la devolución
                GenerarPDFDevolucion(dniCliente, productosEnDevolucion, numeroDocumento, motivoDevolucion);

                // Limpiar la devolución
                productosEnDevolucion.Clear();
                ProductosDevolucionDataGrid.ItemsSource = null;
                ActualizarTotal();

                MessageBox.Show("Devolución procesada correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar la devolución: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GenerarNumeroDocumento(DateTime fechaCompra, bool esDevolucion)
        {
            string prefijo = esDevolucion ? "DV" : "DV";
            string fechaFormateada = fechaCompra.ToString("yyyyMMdd");

            string query = $"SELECT NumeroDocumento FROM Devoluciones WHERE NumeroDocumento LIKE '{prefijo}{fechaFormateada}%' ORDER BY NumeroDocumento DESC LIMIT 1";

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                var command = new MySqlCommand(query, conexion.ObtenerConexion());

                var ultimoNumeroDocumento = command.ExecuteScalar()?.ToString();
                int nuevoNumero;

                if (ultimoNumeroDocumento != null)
                {
                    string numeroStr = ultimoNumeroDocumento.Substring(ultimoNumeroDocumento.Length - 4);
                    nuevoNumero = int.Parse(numeroStr) + 1;
                }
                else
                {
                    nuevoNumero = 1;
                }

                return $"{prefijo}{fechaFormateada}{nuevoNumero:D4}";
            }
        }

        private void GenerarPDFDevolucion(string dniCliente, List<DVenta> productosEnDevolucion, string idDevolucion, string motivoDevolucion)
        {
            try
            {
                using (var document = new iTextSharp.text.Document())
                {
                    // Ruta donde se guardará el PDF
                    var outputFile = $"{idDevolucion}.pdf";
                    var writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));
                    document.Open();

                    // Encabezado
                    var empresaPhrase = new Phrase("OptiStock S.L.", new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD));
                    var empresaParagraph = new Paragraph(empresaPhrase) { Alignment = Element.ALIGN_CENTER };
                    document.Add(empresaParagraph);
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph($"DNI Cliente: {dniCliente}"));
                    document.Add(new Paragraph($"Nº de Devolución: {idDevolucion}"));
                    document.Add(new Paragraph($"Motivo de Devolución: {motivoDevolucion}"));
                    document.Add(new Paragraph($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}"));
                    document.Add(new Paragraph(" "));

                    // Tabla de productos
                    var productTable = new PdfPTable(5);
                    productTable.WidthPercentage = 100;
                    productTable.SetWidths(new float[] { 100f, 200f, 70f, 70f, 70f });

                    // Encabezados de la tabla
                    productTable.AddCell(CrearCelda("EAN", true));
                    productTable.AddCell(CrearCelda("Producto", true));
                    productTable.AddCell(CrearCelda("Cantidad", true));
                    productTable.AddCell(CrearCelda("Precio", true));
                    productTable.AddCell(CrearCelda("Total", true));

                    // Detalles de los productos
                    foreach (var producto in productosEnDevolucion)
                    {
                        productTable.AddCell(CrearCelda(producto.ProductoId.ToString()));
                        productTable.AddCell(CrearCelda(producto.Nombre));
                        productTable.AddCell(CrearCelda(producto.Cantidad.ToString()));
                        productTable.AddCell(CrearCelda((-producto.ValorUnitario).ToString("C2"))); // Precio negativo por devolución
                        productTable.AddCell(CrearCelda((-producto.Subtotal).ToString("C2")));    // Total negativo por devolución
                    }

                    document.Add(productTable);

                    // Total de la devolución
                    document.Add(new Paragraph($"Total: {productosEnDevolucion.Sum(p => p.Subtotal) * -1:C2}"));
                    document.Close();

                    // Abrir el archivo PDF automáticamente
                    System.Diagnostics.Process.Start(outputFile);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al generar el PDF: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private PdfPCell CrearCelda(string contenido, bool esEncabezado = false)
        {
            var frase = new Phrase(contenido, new Font(Font.FontFamily.HELVETICA, esEncabezado ? 14 : 12));
            var celda = new PdfPCell(frase)
            {
                HorizontalAlignment = Element.ALIGN_CENTER,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5
            };

            if (esEncabezado)
            {
                celda.BackgroundColor = new BaseColor(220, 220, 220); // Color de fondo para encabezados
            }

            return celda;
        }

        private void CancelarDevolucion_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show("¿Está seguro de que desea cancelar la devolución?", "Confirmación", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    productosEnDevolucion.Clear();
                    ProductosDevolucionDataGrid.ItemsSource = null;
                    ActualizarTotal();

                    MessageBox.Show("Devolución cancelada correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cancelar la devolución: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ActualizarTotal()
        {
            double total = productosEnDevolucion.Sum(p => p.Subtotal) * -1;
            TotalDevolucionTextBlock.Text = total.ToString("C2");
        }
    }
}
