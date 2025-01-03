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
using static iTextSharp.text.pdf.AcroFields;
using System.util.zlib;

namespace TFG
{
    public partial class NuevaVentaView : UserControl
    {
       
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

        // Metodo para ver si un usuario esta registrado en la empresa
        private (bool esValido, string nombreCompleto) ValidarYObtenerNombreCliente(string dni)
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                var command = new MySqlCommand("SELECT NombreCompleto FROM Clientes WHERE Dni = @dni", conexion.ObtenerConexion());
                command.Parameters.AddWithValue("@dni", dni);

                var nombreCompleto = command.ExecuteScalar()?.ToString();

                // Retornar tupla: (esValido, nombreCompleto)
                return (nombreCompleto != null, nombreCompleto ?? "Nombre no encontrado");
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

                // Obtener el DNI del cliente
                string dniCliente = DniClienteTextBox.Text;

                // Validar el DNI del cliente y obtener el nombre completo
                var (esValido, nombreCompletoCliente) = ValidarYObtenerNombreCliente(dniCliente);
                if (!esValido)
                {
                    MessageBox.Show("Este usuario no está dado de alta, pase por administración.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                double total = double.Parse(TotalTextBlock.Text.Replace("€", ""));
                DateTime fechaCompra = DateTime.Now;
                string nombreEmpresa = "OptiStock S.L.";

                // Generar el número de documento
                string numeroDocumento = GenerarNumeroDocumento(fechaCompra, TicketRadioButton.IsChecked == true);

                // Insertar la venta en la tabla Ventas
                int ventaId;
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    var command = new MySqlCommand("INSERT INTO Ventas (DniCliente, NumeroDocumento, Total) VALUES (@dniCliente, @numeroDocumento, @total); SELECT LAST_INSERT_ID();", conexion.ObtenerConexion());
                    command.Parameters.AddWithValue("@dniCliente", dniCliente);
                    command.Parameters.AddWithValue("@numeroDocumento", numeroDocumento);
                    command.Parameters.AddWithValue("@total", total);

                    ventaId = Convert.ToInt32(command.ExecuteScalar()); // Obtener el ID de la venta insertada
                }

                // Insertar en HistorialVentas
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    foreach (var producto in productosEnVenta)
                    {
                        var command = new MySqlCommand("INSERT INTO HistorialVentas (NumeroDocumento, Producto, Cantidad) VALUES (@numeroDocumento, @productoId, @cantidad);", conexion.ObtenerConexion());
                        command.Parameters.AddWithValue("@numeroDocumento", numeroDocumento);
                        command.Parameters.AddWithValue("@productoId", producto.ProductoId);
                        command.Parameters.AddWithValue("@cantidad", producto.Cantidad);
                        command.ExecuteNonQuery();
                    }
                    conexion.CerrarConexion();
                }

                RegularizarStock(productosEnVenta);

                // Generar el PDF del ticket o factura
                if (TicketRadioButton.IsChecked == true)
                {
                    GenerarPDFTicket(dniCliente, productosEnVenta, total, fechaCompra, nombreEmpresa, numeroDocumento); // Pasar numeroDocumento
                }
                else if (FacturaRadioButton.IsChecked == true)
                {
                    GenerarPDFFactura(dniCliente, nombreCompletoCliente, productosEnVenta, total, fechaCompra, nombreEmpresa, numeroDocumento); // Pasar numeroDocumento
                }

                // Limpiar la venta
                productosEnVenta.Clear();
                ProductosDataGrid.ItemsSource = null;
                ActualizarTotal();

                MessageBox.Show("Venta procesada correctamente.", "Información", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al buscar el producto: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string GenerarNumeroDocumento(DateTime fechaCompra, bool esTicket)
        {
            string prefijo = esTicket ? "TS" : "FS";
            string fechaFormateada = fechaCompra.ToString("yyyyMMdd");

            // Consultar el último número de documento
            string query = $"SELECT NumeroDocumento FROM Ventas WHERE NumeroDocumento LIKE '{prefijo}{fechaFormateada}%' ORDER BY NumeroDocumento DESC LIMIT 1";

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                var command = new MySqlCommand(query, conexion.ObtenerConexion());

                // Obtener el último número de documento
                var ultimoNumeroDocumento = command.ExecuteScalar()?.ToString();
                int nuevoNumero;

                if (ultimoNumeroDocumento != null)
                {
                    // Extraer las últimas 4 cifras y convertir a entero
                    string numeroStr = ultimoNumeroDocumento.Substring(ultimoNumeroDocumento.Length - 4);
                    nuevoNumero = int.Parse(numeroStr) + 1;
                }
                else
                {
                    nuevoNumero = 1; // Si no hay documentos, comienza desde 1
                }

                return $"{prefijo}{fechaFormateada}{nuevoNumero:D4}"; // Formatear como 4 dígitos
            }
        }




        private void GenerarPDFTicket(string dniCliente, List<DVenta> productosEnVenta, double totalCompra, DateTime fechaCompra, string nombreEmpresa, string ticketId)
        {
            using (var document = new iTextSharp.text.Document())
            {
                var outputFile = $"{ticketId}.pdf";
                var writer = PdfWriter.GetInstance(document, new FileStream(outputFile, FileMode.Create));

                // Agregar el manejador de eventos para el pie de página
                writer.PageEvent = new ManejadorPieDePagina("Para devolución de un producto, debera presentar este ticket.");

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



        public class ManejadorPieDePagina : PdfPageEventHelper
        {
            private string _texto;

            public ManejadorPieDePagina(string texto)
            {
                _texto = texto;
            }

            public override void OnEndPage(PdfWriter writer, Document document)
            {
                BaseFont fuenteBase = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                PdfContentByte contenidoPdf = writer.DirectContent;

                contenidoPdf.BeginText();
                contenidoPdf.SetFontAndSize(fuenteBase, 10);
                contenidoPdf.ShowTextAligned(PdfContentByte.ALIGN_CENTER,
                    _texto,
                    document.PageSize.Width/2,
                    document.BottomMargin +10,
                    0);
                contenidoPdf.EndText();
            }
        }

        private void GenerarPDFFactura(string dniCliente, string nombreCompletoCliente, List<DVenta> productosEnVenta, double totalCompra, DateTime fechaCompra, string nombreEmpresa, string idFactura)
        {
            using (var documento = new iTextSharp.text.Document())
            {
                var archivoSalida = $"{idFactura}.pdf";
                var escritor = PdfWriter.GetInstance(documento, new FileStream(archivoSalida, FileMode.Create));

                // Agregar el manejador de eventos para el pie de página
                escritor.PageEvent = new ManejadorPieDePagina("Para devolución de un producto, debera presentar esta factura.");

                documento.Open();

                // Agregar el nombre de la empresa en negrita
                var frasaEmpresa = new Phrase($"{nombreEmpresa}", new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD));
                var parrafoEmpresa = new Paragraph(frasaEmpresa);
                parrafoEmpresa.Alignment = Element.ALIGN_CENTER;
                documento.Add(parrafoEmpresa);

                // Agregar un espacio de 2 líneas
                documento.Add(new Paragraph(" "));
                documento.Add(new Paragraph(" "));

                // Agregar el DNI del cliente
                var parrafoDni = new Paragraph($"DNI Cliente: {dniCliente}");
                parrafoDni.Alignment = Element.ALIGN_LEFT;
                documento.Add(parrafoDni);

                // Agregar el nombre completo del cliente
                var parrafoNombreCompleto = new Paragraph($"Nombre: {nombreCompletoCliente}");
                parrafoNombreCompleto.Alignment = Element.ALIGN_LEFT;
                documento.Add(parrafoNombreCompleto);

                // Agregar el número de factura
                var parrafoFactura = new Paragraph($"Nº de Factura: {idFactura}");
                parrafoFactura.Alignment = Element.ALIGN_LEFT;
                documento.Add(parrafoFactura);

                // Agregar la fecha de compra
                var parrafoFecha = new Paragraph($"Fecha: {fechaCompra.ToString("dd/MM/yyyy HH:mm:ss")}");
                parrafoFecha.Alignment = Element.ALIGN_LEFT;
                documento.Add(parrafoFecha);

                // Agregar un espacio de 1 línea
                documento.Add(new Paragraph(" "));

                // Agregar el listado de productos
                var tablaProductos = new PdfPTable(6);
                tablaProductos.WidthPercentage = 100;
                tablaProductos.SetWidths(new float[] { 100f, 190f, 60f, 100f, 100f, 100f });

                // Agregar las celdas con fondo gris claro
                PdfPCell celda;
                celda = new PdfPCell(new Phrase("EAN", new Font(Font.FontFamily.HELVETICA, 10)));
                celda.BackgroundColor = new BaseColor(220, 220, 220);
                tablaProductos.AddCell(celda);

                celda = new PdfPCell(new Phrase("Producto", new Font(Font.FontFamily.HELVETICA, 10)));
                celda.BackgroundColor = new BaseColor(220, 220, 220);
                tablaProductos.AddCell(celda);

                celda = new PdfPCell(new Phrase("Cantidad", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                celda.BackgroundColor = new BaseColor(220, 220, 220);
                tablaProductos.AddCell(celda);

                celda = new PdfPCell(new Phrase("Precio Sin IVA", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                celda.BackgroundColor = new BaseColor(220, 220, 220);
                tablaProductos.AddCell(celda);

                celda = new PdfPCell(new Phrase("Precio", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                celda.BackgroundColor = new BaseColor(220, 220, 220);
                tablaProductos.AddCell(celda);

                celda = new PdfPCell(new Phrase("Total", new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                celda.BackgroundColor = new BaseColor(220, 220, 220);
                tablaProductos.AddCell(celda);

                double totalSinIVA = 0;

                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();

                    foreach (var producto in productosEnVenta)
                    {
                        // Obtener el nombre y el precio sin IVA del producto desde la base de datos
                        string nombreProducto;
                        double precioSinIVA;
                        var comando = new MySqlCommand("SELECT Nombre, Precio FROM Productos WHERE EAN = @ean", conexion.ObtenerConexion());
                        comando.Parameters.AddWithValue("@ean", producto.ProductoId);

                        using (var lector = comando.ExecuteReader())
                        {
                            if (lector.Read())
                            {
                                nombreProducto = lector["Nombre"].ToString();
                                precioSinIVA = Convert.ToDouble(lector["Precio"]);
                            }
                            else
                            {
                                nombreProducto = "Producto no encontrado";
                                precioSinIVA = 0;
                            }
                        }

                        tablaProductos.AddCell(new Phrase(producto.ProductoId.ToString(), new Font(Font.FontFamily.HELVETICA, 10)));
                        tablaProductos.AddCell(new Phrase(nombreProducto, new Font(Font.FontFamily.HELVETICA, 10)));
                        tablaProductos.AddCell(new Phrase(producto.Cantidad.ToString(), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                        tablaProductos.AddCell(new Phrase(precioSinIVA.ToString("C2"), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                        tablaProductos.AddCell(new Phrase(producto.ValorUnitario.ToString("C2"), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));
                        tablaProductos.AddCell(new Phrase(producto.Subtotal.ToString("C2"), new Font(Font.FontFamily.HELVETICA, 10, Font.NORMAL)));

                        totalSinIVA += precioSinIVA * producto.Cantidad;
                    }

                    conexion.CerrarConexion();
                }

                documento.Add(tablaProductos);

                // Agregar el total de la compra sin IVA
                var parrafoTotalSinIVA = new Paragraph($"Total Sin IVA: {totalSinIVA.ToString("C2")}");
                parrafoTotalSinIVA.Alignment = Element.ALIGN_RIGHT;
                documento.Add(parrafoTotalSinIVA);

                // Agregar el total de la compra con IVA
                var parrafoTotal = new Paragraph($"Total: {totalCompra.ToString("C2")}");
                parrafoTotal.Alignment = Element.ALIGN_RIGHT;
                documento.Add(parrafoTotal);

                documento.Close();

                // Abrir el archivo PDF generado
                System.Diagnostics.Process.Start(archivoSalida);
            }
        }

        private void RegularizarStock(List<DVenta> productosEnVenta)
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();

                foreach (var producto in productosEnVenta)
                {
                    var comando = new MySqlCommand("UPDATE productos SET Stock = Stock - @cantidad WHERE EAN = @ean", conexion.ObtenerConexion());
                    comando.Parameters.AddWithValue("@cantidad", producto.Cantidad);
                    comando.Parameters.AddWithValue("@ean", producto.ProductoId);

                    // Ejecutar el comando
                    comando.ExecuteNonQuery();
                }

                conexion.CerrarConexion();
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
