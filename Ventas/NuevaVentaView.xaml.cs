using iTextSharp.text.pdf;
using iTextSharp.text;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TFG.SeguimientoPedidos;

namespace TFG
{
    public partial class NuevaVentaView : UserControl
    {
        private int TicketContador = 1;
        private int FacturaContador = 1;
        private List<DVenta> productosEnVenta = new List<DVenta>();

        // Constructor
        public NuevaVentaView()
        {
            InitializeComponent(); // Asegúrate de que NuevaVenta.xaml esté correctamente vinculado
            CodigoProductoTextBox.KeyDown += CodigoProductoTextBox_KeyDown;
        }

        private void CodigoProductoTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
                // Validar que se ingrese el código del producto
                if (string.IsNullOrWhiteSpace(CodigoProductoTextBox.Text))
                {
                    MessageBox.Show("Por favor, ingrese el código del producto.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Intentar convertir el código de producto a long
                if (!long.TryParse(CodigoProductoTextBox.Text, out long ean))
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

                // Verificar si el producto ya existe en la lista
                var productoExistente = productosEnVenta.FirstOrDefault(p => p.ProductoId == ean);
                if (productoExistente != null)
                {
                    // Si existe, incrementar la cantidad
                    productoExistente.Cantidad += 1;
                }
                else
                {
                    // Si no existe, crear un nuevo producto
                    var nuevoProducto = new DVenta(ean, nombreProducto, precioTotal, 1);
                    productosEnVenta.Add(nuevoProducto);
                }

                // Actualizar el DataGrid
                ProductosDataGrid.ItemsSource = null; // Esto es necesario para refrescar la UI
                ProductosDataGrid.ItemsSource = productosEnVenta;

                // Actualizar el total
                ActualizarTotal();

                // Limpiar el campo de entrada
                CodigoProductoTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar el producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Metodos para poner o quitar el placeholder del dni
        private void DniClienteTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Ocultar el texto de marcador de posición cuando el usuario hace clic en el TextBox
            DniClienteTextBlockPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void DniClienteTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Mostrar el texto de marcador de posición si el TextBox está vacío
            if (string.IsNullOrWhiteSpace(DniClienteTextBox.Text))
            {
                DniClienteTextBlockPlaceholder.Visibility = Visibility.Visible;
            }
        }





        // Evento para agregar un producto
        private void AgregarProducto_Click(object sender, RoutedEventArgs e)
        {
            BuscarProducto();
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

                // Obtener los datos necesarios
                string dniCliente = DniClienteTextBox.Text;
                double total = double.Parse(TotalTextBlock.Text.Replace("€", ""));
                DateTime fechaCompra = DateTime.Now;
                string nombreEmpresa = "OptiStock S.L.";

                // Generar la lista de celdas para la tabla de productos
                List<PdfPCell> productCells = new List<PdfPCell>();
                foreach (var producto in productosEnVenta)
                {
                    productCells.Add(new PdfPCell(new Phrase(producto.ProductoId.ToString())));
                    productCells.Add(new PdfPCell(new Phrase(producto.Cantidad.ToString())));
                    productCells.Add(new PdfPCell(new Phrase(producto.ValorUnitario.ToString("C2"))));
                    productCells.Add(new PdfPCell(new Phrase(producto.Subtotal.ToString("C2"))));
                }


                // Verificar si se seleccionó generar un ticket o una factura
                if (TicketRadioButton.IsChecked == true)
                {
                    // Generar el PDF del ticket
                    GenerarPDFTicket(dniCliente, productosEnVenta, total, fechaCompra, nombreEmpresa);
                }
                else if (FacturaRadioButton.IsChecked == true)
                {
                    // Generar el PDF de la factura
                    GenerarPDFFactura(dniCliente, productosEnVenta, total, fechaCompra, nombreEmpresa);
                }
                else
                {
                    MessageBox.Show("Debe seleccionar si desea generar un ticket o una factura.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }


                // Limpiar la venta
                productosEnVenta.Clear();
                ProductosDataGrid.ItemsSource = null;
                ActualizarTotal();

                MessageBox.Show("Venta procesada correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al procesar la venta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerarPDFTicket(string dniCliente, List<DVenta> productosEnVenta, double totalCompra, DateTime fechaCompra, string nombreEmpresa)
        {
            // Generar el identificador único del ticket
            string ticketId = $"TS{fechaCompra.ToString("yyyyMMdd")}{TicketContador.ToString("D4")}";
            TicketContador++;

            using (var document = new iTextSharp.text.Document())
            {
                var outputFile = $"{ticketId}.pdf";
                PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));

                document.Open();

                // Agregar el nombre de la empresa en negrita
                var empresaPhrase = new Phrase($"{nombreEmpresa}", new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD));
                var empresaParagraph = new Paragraph(empresaPhrase);
                empresaParagraph.Alignment = Element.ALIGN_CENTER;
                document.Add(empresaParagraph);

                // Agregar un espacio de 2 líneas
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));

                // Agregar el DNI del cliente
                var dniParagraph = new Paragraph($"DNI Cliente: {dniCliente}");
                dniParagraph.Alignment = Element.ALIGN_LEFT;
                document.Add(dniParagraph);

                // Agregar el número de ticket
                var ticketParagraph = new Paragraph($"Nº de Ticket: {ticketId}");
                ticketParagraph.Alignment = Element.ALIGN_LEFT;
                document.Add(ticketParagraph);

                // Agregar la fecha de compra
                var fechaParagraph = new Paragraph($"Fecha: {fechaCompra.ToString("dd/MM/yyyy HH:mm:ss")}");
                fechaParagraph.Alignment = Element.ALIGN_LEFT;
                document.Add(fechaParagraph);

                // Agregar un espacio de 1 línea
                document.Add(new Paragraph(" "));

                // Agregar el listado de productos
                var productTable = new PdfPTable(5);
                productTable.WidthPercentage = 100;
                productTable.SetWidths(new float[] { 100f, 200f, 50f, 100f, 100f });

                // Agregar las celdas con fondo gris claro
                PdfPCell cell;
                cell = new PdfPCell(new Phrase("EAN", new Font(Font.FontFamily.HELVETICA, 10)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                cell = new PdfPCell(new Phrase("Producto", new Font(Font.FontFamily.HELVETICA, 10)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                cell = new PdfPCell(new Phrase("Cantidad", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                cell = new PdfPCell(new Phrase("Precio", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                cell = new PdfPCell(new Phrase("Total", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();

                    foreach (var producto in productosEnVenta)
                    {
                        // Obtener el nombre del producto desde la base de datos
                        string nombreProducto;
                        var command = new MySqlCommand("SELECT Nombre FROM Productos WHERE EAN = @ean", conexion.ObtenerConexion());
                        command.Parameters.AddWithValue("@ean", producto.ProductoId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                nombreProducto = reader["Nombre"].ToString();
                            }
                            else
                            {
                                nombreProducto = "Producto no encontrado";
                            }
                        }

                        productTable.AddCell(new Phrase(producto.ProductoId.ToString(), new Font(Font.FontFamily.HELVETICA, 10)));
                        productTable.AddCell(new Phrase(nombreProducto, new Font(Font.FontFamily.HELVETICA, 10)));
                        productTable.AddCell(new Phrase(producto.Cantidad.ToString(), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                        productTable.AddCell(new Phrase(producto.ValorUnitario.ToString("C2"), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                        productTable.AddCell(new Phrase(producto.Subtotal.ToString("C2"), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                    }

                    conexion.CerrarConexion();
                }

                document.Add(productTable);

                // Agregar el total de la compra
                var totalParagraph = new Paragraph($"Total: {totalCompra.ToString("C2")}");
                totalParagraph.Alignment = Element.ALIGN_RIGHT;
                document.Add(totalParagraph);

                document.Close();

                // Abrir el archivo PDF generado
                System.Diagnostics.Process.Start(outputFile);
            }
        }


        private void GenerarPDFFactura(string dniCliente, List<DVenta> productosEnVenta, double totalCompra, DateTime fechaCompra, string nombreEmpresa)
        {
            // Generar el identificador único de la factura
            string facturaId = $"FS{fechaCompra.ToString("yyyyMMdd")}{FacturaContador.ToString("D4")}";
            FacturaContador++;

            using (var document = new iTextSharp.text.Document())
            {
                var outputFile = $"{facturaId}.pdf";
                PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));

                document.Open();

                // Agregar el nombre de la empresa en negrita
                var empresaPhrase = new Phrase($"{nombreEmpresa}", new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD));
                var empresaParagraph = new Paragraph(empresaPhrase);
                empresaParagraph.Alignment = Element.ALIGN_CENTER;
                document.Add(empresaParagraph);

                // Agregar un espacio de 2 líneas
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph(" "));

                // Agregar el DNI del cliente
                var dniParagraph = new Paragraph($"DNI Cliente: {dniCliente}");
                dniParagraph.Alignment = Element.ALIGN_LEFT;
                document.Add(dniParagraph);

                // Agregar el número de factura
                var facturaParagraph = new Paragraph($"Nº de Factura: {facturaId}");
                facturaParagraph.Alignment = Element.ALIGN_LEFT;
                document.Add(facturaParagraph);

                // Agregar la fecha de compra
                var fechaParagraph = new Paragraph($"Fecha: {fechaCompra.ToString("dd/MM/yyyy HH:mm:ss")}");
                fechaParagraph.Alignment = Element.ALIGN_LEFT;
                document.Add(fechaParagraph);

                // Agregar un espacio de 1 línea
                document.Add(new Paragraph(" "));

                // Agregar el listado de productos
                var productTable = new PdfPTable(6);
                productTable.WidthPercentage = 100;
                productTable.SetWidths(new float[] { 100f, 190f, 60f, 100f, 100f, 100f });

                // Agregar las celdas con fondo gris claro
                PdfPCell cell;
                cell = new PdfPCell(new Phrase("EAN", new Font(Font.FontFamily.HELVETICA, 10)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                cell = new PdfPCell(new Phrase("Producto", new Font(Font.FontFamily.HELVETICA, 10)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                cell = new PdfPCell(new Phrase("Cantidad", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                cell = new PdfPCell(new Phrase("Precio Sin IVA", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                cell = new PdfPCell(new Phrase("Precio", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                cell = new PdfPCell(new Phrase("Total", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                cell.BackgroundColor = new BaseColor(220, 220, 220);
                productTable.AddCell(cell);

                double totalSinIVA = 0;

                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();

                    foreach (var producto in productosEnVenta)
                    {
                        // Obtener el nombre y el precio sin IVA del producto desde la base de datos
                        string nombreProducto;
                        double precioSinIVA;
                        var command = new MySqlCommand("SELECT Nombre, Precio FROM Productos WHERE EAN = @ean", conexion.ObtenerConexion());
                        command.Parameters.AddWithValue("@ean", producto.ProductoId);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                nombreProducto = reader["Nombre"].ToString();
                                precioSinIVA = Convert.ToDouble(reader["Precio"]);
                            }
                            else
                            {
                                nombreProducto = "Producto no encontrado";
                                precioSinIVA = 0;
                            }
                        }

                        productTable.AddCell(new Phrase(producto.ProductoId.ToString(), new Font(Font.FontFamily.HELVETICA, 10)));
                        productTable.AddCell(new Phrase(nombreProducto, new Font(Font.FontFamily.HELVETICA, 10)));
                        productTable.AddCell(new Phrase(producto.Cantidad.ToString(), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                        productTable.AddCell(new Phrase(precioSinIVA.ToString("C2"), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                        productTable.AddCell(new Phrase(producto.ValorUnitario.ToString("C2"), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                        productTable.AddCell(new Phrase(producto.Subtotal.ToString("C2"), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));

                        totalSinIVA += precioSinIVA * producto.Cantidad;
                    }

                    conexion.CerrarConexion();
                }

                document.Add(productTable);

                // Agregar el total de la compra sin IVA
                var totalSinIVAParagraph = new Paragraph($"Total Sin IVA: {totalSinIVA.ToString("C2")}");
                totalSinIVAParagraph.Alignment = Element.ALIGN_RIGHT;
                document.Add(totalSinIVAParagraph);

                // Agregar el total de la compra con IVA
                var totalParagraph = new Paragraph($"Total: {totalCompra.ToString("C2")}");
                totalParagraph.Alignment = Element.ALIGN_RIGHT;
                document.Add(totalParagraph);

                document.Close();

                // Abrir el archivo PDF generado
                System.Diagnostics.Process.Start(outputFile);
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
}
