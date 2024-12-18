using System;
using System.Windows;
using System.Windows.Controls;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using MySqlConnector;

namespace TFG.Nominas
{
    public partial class Nominas : UserControl
    {
        public Nominas()
        {
            InitializeComponent();
            CargarNombresTrabajadores();
        }

        private void CargarNombresTrabajadores()
        {
            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    string query = "SELECT NombreCompleto FROM Trabajadores";
                    using (var cmd = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                NombreNomina.Items.Add(reader.GetString(0));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al cargar los trabajadores: " + ex.Message);
                }
            }
        }

        private void GenerarNomina_click(object sender, RoutedEventArgs e)
        {
            // Obtener los valores seleccionados
            string nombreSeleccionado = NombreNomina.SelectedItem as string;
            ComboBoxItem mesSeleccionado = MesNomina.SelectedItem as ComboBoxItem;
            ComboBoxItem añoSeleccionado = AñoNomina.SelectedItem as ComboBoxItem;

            // Validar que todos los campos estén seleccionados
            if (string.IsNullOrEmpty(nombreSeleccionado) || mesSeleccionado == null || añoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione todos los campos necesarios.");
                return;
            }

            string mes = mesSeleccionado.Content.ToString();
            int año = int.Parse(añoSeleccionado.Content.ToString());

            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();

                    // Obtener datos del trabajador
                    var trabajador = ObtenerDatosTrabajador(nombreSeleccionado, mes, año, conexion);
                    if (trabajador == null)
                    {
                        MessageBox.Show("No se encontraron los datos del trabajador.");
                        return;
                    }

                    // Generar el PDF
                    string rutaPDF = GenerarPDFNomina(trabajador);

                    // Mostrar mensaje de éxito y abrir el PDF
                    MessageBox.Show("Nómina generada correctamente.");
                    System.Diagnostics.Process.Start(rutaPDF);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al generar la nómina: " + ex.Message);
                }
            }
        }

        private Trabajadores ObtenerDatosTrabajador(string nombreCompleto, string mes, int año, Conexion conexion)
        {
            string query = @"
                SELECT 
                    t.Id,
                    t.NombreCompleto,
                    t.DNI,
                    t.FechaNacimiento,
                    t.Telefono,
                    t.Email,
                    t.Direccion,
                    t.FechaContratacion,
                    t.Salario,
                    t.NumeroSeguridadSocial,
                    t.CategoriaProfesional,
                    n.SalarioBruto,
                    n.Deducciones,
                    n.SalarioNeto
                FROM Trabajadores t
                LEFT JOIN Nomina n ON t.Id = n.TrabajadorId
                WHERE t.NombreCompleto = @nombre
                AND n.Mes = @mes
                AND n.Anio = @año";

            using (var cmd = new MySqlCommand(query, conexion.ObtenerConexion()))
            {
                cmd.Parameters.AddWithValue("@nombre", nombreCompleto);
                cmd.Parameters.AddWithValue("@mes", mes);
                cmd.Parameters.AddWithValue("@año", año);

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Trabajadores
                        {
                            Id = reader.GetInt32("Id"),
                            NombreCompleto = reader.GetString("NombreCompleto"),
                            DNI = reader.GetString("DNI"),
                            FechaNacimiento = reader.GetDateTime("FechaNacimiento"),
                            Telefono = reader.GetString("Telefono"),
                            Email = reader.GetString("Email"),
                            Direccion = reader.GetString("Direccion"),
                            FechaContratacion = reader.GetDateTime("FechaContratacion"),
                            Salario = reader.GetDecimal("Salario"),
                            SalarioBruto = reader.GetDecimal("SalarioBruto"),
                            Deducciones = reader.GetDecimal("Deducciones"),
                            SalarioNeto = reader.GetDecimal("SalarioNeto"),
                            NumeroSeguridadSocial = reader.GetString("NumeroSeguridadSocial"),
                            CategoriaProfesional = reader.GetInt32("CategoriaProfesional"),
                            Mes = mes,
                            Año = año
                        };
                    }
                }
            }
            return null;
        }

        public string GenerarPDFNomina(Trabajadores trabajador)
        {
            string rutaPDF = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $"Nomina_{trabajador.NombreCompleto}_{trabajador.Mes}_{trabajador.Año}.pdf"
            );

            using (FileStream fs = new FileStream(rutaPDF, FileMode.Create))
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter writer = PdfWriter.GetInstance(document, fs);
                document.Open();

                // Definir fuentes
                BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Font titleFont = new Font(bf, 14, Font.BOLD);
                Font headerFont = new Font(bf, 12, Font.BOLD);
                Font normalFont = new Font(bf, 10, Font.NORMAL);

                // Logo y encabezado
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 100;
                headerTable.SetWidths(new float[] { 1f, 2f });

                // Logo
                if (File.Exists("C:\\Users\\rafae\\source\\repos\\TFG\\Imagenes\\OptiStockLogo.png"))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance("C:\\Users\\rafae\\source\\repos\\TFG\\Imagenes\\OptiStockLogo.png");
                    logo.ScaleToFit(130f, 80f);
                    PdfPCell logoCell = new PdfPCell(logo);
                    logoCell.Border = Rectangle.NO_BORDER;
                    headerTable.AddCell(logoCell);
                }

                // Datos de la empresa
                PdfPCell empresaCell = new PdfPCell();
                empresaCell.Border = Rectangle.NO_BORDER;
                empresaCell.AddElement(new Paragraph("OptiStock S.L.", headerFont));
                empresaCell.AddElement(new Paragraph("CIF: B12345678", normalFont));
                empresaCell.AddElement(new Paragraph("Dirección: Calle Maestro Tejera, 123", normalFont));
                headerTable.AddCell(empresaCell);
                document.Add(headerTable);

                // Título de la nómina
                Paragraph titulo = new Paragraph("RECIBO DE SALARIOS", titleFont);
                titulo.Alignment = Element.ALIGN_CENTER;
                titulo.SpacingBefore = 20f;
                titulo.SpacingAfter = 20f;
                document.Add(titulo);

                // Período de liquidación
                PdfPTable periodoTable = new PdfPTable(1);
                periodoTable.WidthPercentage = 100;
                PdfPCell periodoCell = new PdfPCell(new Phrase($"Período de liquidación: {trabajador.Mes} {trabajador.Año}", headerFont));
                periodoCell.BackgroundColor = new BaseColor(220, 220, 220);
                periodoCell.Padding = 8f;
                periodoTable.AddCell(periodoCell);
                document.Add(periodoTable);

                // Datos del trabajador
                PdfPTable datosTable = new PdfPTable(2);
                datosTable.WidthPercentage = 100;
                datosTable.SpacingBefore = 20f;

                AddDatosTrabajador(datosTable, "Trabajador:", trabajador.NombreCompleto);
                AddDatosTrabajador(datosTable, "DNI:", trabajador.DNI);
                AddDatosTrabajador(datosTable, "Nº Afiliación SS:", trabajador.NumeroSeguridadSocial); 
                AddDatosTrabajador(datosTable, "Categoría: Grupo:", trabajador.CategoriaProfesional.ToString());
                document.Add(datosTable);

                // Tabla de devengos
                document.Add(new Paragraph("\nI. DEVENGOS", headerFont));
                PdfPTable devengosTable = new PdfPTable(2);
                devengosTable.WidthPercentage = 100;
                devengosTable.SetWidths(new float[] { 3f, 1f });

                // Encabezados de devengos
                PdfPCell conceptoHeader = new PdfPCell(new Phrase("Concepto", headerFont));
                PdfPCell importeHeader = new PdfPCell(new Phrase("Importe", headerFont));
                conceptoHeader.BackgroundColor = new BaseColor(220, 220, 220);
                importeHeader.BackgroundColor = new BaseColor(220, 220, 220);
                devengosTable.AddCell(conceptoHeader);
                devengosTable.AddCell(importeHeader);

                // Añadir conceptos de devengos
                AddDevengo(devengosTable, "Salario Base", trabajador.Salario.ToString("C"));
                AddDevengo(devengosTable, "Complementos", "0,00 €");

                // Total devengos
                PdfPCell totalDevengosLabel = new PdfPCell(new Phrase("TOTAL DEVENGADO", headerFont));
                PdfPCell totalDevengosValor = new PdfPCell(new Phrase(trabajador.SalarioBruto.ToString("C"), headerFont));
                totalDevengosLabel.BackgroundColor = new BaseColor(220, 220, 220);
                totalDevengosValor.BackgroundColor = new BaseColor(220, 220, 220);
                devengosTable.AddCell(totalDevengosLabel);
                devengosTable.AddCell(totalDevengosValor);
                document.Add(devengosTable);

                // Tabla de deducciones
                document.Add(new Paragraph("\nII. DEDUCCIONES", headerFont));
                PdfPTable deduccionesTable = new PdfPTable(2);
                deduccionesTable.WidthPercentage = 100;
                deduccionesTable.SetWidths(new float[] { 3f, 1f });

                // Encabezados de deducciones
                deduccionesTable.AddCell(conceptoHeader);
                deduccionesTable.AddCell(importeHeader);

                // Añadir deducciones
                decimal contingenciasComunes = trabajador.SalarioBruto * 0.047m; // 4.7%
                decimal desempleo = trabajador.SalarioBruto * 0.0155m; // 1.55%
                decimal irpf = trabajador.Deducciones - contingenciasComunes - desempleo;

                AddDeduccion(deduccionesTable, "Contingencias comunes (4.7%)", contingenciasComunes.ToString("C"));
                AddDeduccion(deduccionesTable, "Desempleo (1.55%)", desempleo.ToString("C"));
                AddDeduccion(deduccionesTable, "IRPF", irpf.ToString("C"));

                // Total deducciones
                PdfPCell totalDeduccionesLabel = new PdfPCell(new Phrase("TOTAL DEDUCCIONES", headerFont));
                PdfPCell totalDeduccionesValor = new PdfPCell(new Phrase(trabajador.Deducciones.ToString("C"), headerFont));
                totalDeduccionesLabel.BackgroundColor = new BaseColor(220, 220, 220);
                totalDeduccionesValor.BackgroundColor = new BaseColor(220, 220, 220);
                deduccionesTable.AddCell(totalDeduccionesLabel);
                deduccionesTable.AddCell(totalDeduccionesValor);
                document.Add(deduccionesTable);

                // Líquido a percibir
                PdfPTable liquidoTable = new PdfPTable(2);
                liquidoTable.WidthPercentage = 100;
                liquidoTable.SetWidths(new float[] { 3f, 1f });
                liquidoTable.SpacingBefore = 20f;

                PdfPCell liquidoLabel = new PdfPCell(new Phrase("LÍQUIDO A PERCIBIR", headerFont));
                PdfPCell liquidoValor = new PdfPCell(new Phrase(trabajador.SalarioNeto.ToString("C"), headerFont));
                liquidoLabel.BackgroundColor = new BaseColor(200, 200, 200);
                liquidoValor.BackgroundColor = new BaseColor(200, 200, 200);
                liquidoTable.AddCell(liquidoLabel);
                liquidoTable.AddCell(liquidoValor);
                document.Add(liquidoTable);

                // Fecha y firma
                document.Add(new Paragraph($"\nFecha: {DateTime.Now:dd/MM/yyyy}", normalFont));
                document.Add(new Paragraph("\n\n"));

                PdfPTable firmasTable = new PdfPTable(2);
                firmasTable.WidthPercentage = 100;
                firmasTable.AddCell(new PdfPCell(new Phrase("Firma de la empresa")) { Border = Rectangle.ALIGN_TOP });
                firmasTable.AddCell(new PdfPCell(new Phrase("Firma del trabajador")) { Border = Rectangle.ALIGN_TOP });
                document.Add(firmasTable);


                document.Close();
            }
            return rutaPDF;
        }

        private void AddDatosTrabajador(PdfPTable table, string label, string valor)
        {
            PdfPCell labelCell = new PdfPCell(new Phrase(label, new Font(BaseFont.CreateFont(), 10, Font.BOLD)));
            PdfPCell valorCell = new PdfPCell(new Phrase(valor, new Font(BaseFont.CreateFont(), 10)));
            labelCell.Border = Rectangle.NO_BORDER;
            valorCell.Border = Rectangle.NO_BORDER;
            table.AddCell(labelCell);
            table.AddCell(valorCell);

        }

        private void AddDevengo(PdfPTable table, string concepto, string importe)
        {
            PdfPCell conceptoCell = new PdfPCell(new Phrase(concepto, new Font(BaseFont.CreateFont(), 10)));
            PdfPCell importeCell = new PdfPCell(new Phrase(importe, new Font(BaseFont.CreateFont(), 10)));
            table.AddCell(conceptoCell);
            table.AddCell(importeCell);
        }

        private void AddDeduccion(PdfPTable table, string concepto, string importe)
        {
            AddDevengo(table, concepto, importe);
        }

    }
}
