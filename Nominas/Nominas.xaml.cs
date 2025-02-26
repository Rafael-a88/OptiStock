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
            if (string.IsNullOrEmpty(nombreSeleccionado) || mesSeleccionado == null ||
                añoSeleccionado == null || string.IsNullOrEmpty(IrpfTextBox.Text))
            {
                MessageBox.Show("Por favor, seleccione todos los campos necesarios, incluyendo el IRPF.");
                return;
            }

            string mes = mesSeleccionado.Content.ToString();
            int año = int.Parse(añoSeleccionado.Content.ToString());

            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();

                    // Verificar si ya existe una nómina para este mes y año
                    var nominaExistente = ObtenerDatosTrabajador(nombreSeleccionado, mes, año, conexion);
                    if (nominaExistente != null)
                    {
                        MessageBox.Show($"Ya existe una nómina para {nombreSeleccionado} en {mes} de {año}");
                        string rutaPDF = GenerarPDFNomina(nominaExistente);
                        System.Diagnostics.Process.Start(rutaPDF);
                        return;
                    }

                    // Si no existe, generar nueva nómina
                    var nuevaNomina = GenerarNuevaNomina(nombreSeleccionado, mes, año, conexion);
                    if (nuevaNomina != null)
                    {
                        string rutaPDF = GenerarPDFNomina(nuevaNomina);
                        System.Diagnostics.Process.Start(rutaPDF);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error al procesar la nómina: " + ex.Message);
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
                            NumeroSeguridadSocial = reader.GetString("NumeroSeguridadSocial"),
                            CategoriaProfesional = reader.GetInt32("CategoriaProfesional"),
                            SalarioBruto = reader.GetDecimal("SalarioBruto"),
                            Deducciones = reader.GetDecimal("Deducciones"),
                            SalarioNeto = reader.GetDecimal("SalarioNeto"),
                            Mes = mes,
                            Año = año
                        };
                    }
                }
            }
            return null;
        }

        private Trabajadores GenerarNuevaNomina(string nombreCompleto, string mes, int año, Conexion conexion)
        {
            // Obtener el porcentaje de IRPF del TextBox (asegúrate de que existe un TextBox llamado IrpfTextBox)
            decimal porcentajeIRPF;
            if (!decimal.TryParse(IrpfTextBox.Text, out porcentajeIRPF))
            {
                MessageBox.Show("Por favor, ingrese un porcentaje de IRPF válido.");
                return null;
            }

            string queryNominaExistente = @"
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
        ORDER BY n.Anio DESC, n.Mes DESC
        LIMIT 1";

            decimal salarioBruto = 0;
            int trabajadorId = 0;
            Trabajadores trabajador = null;

            using (var cmdConsulta = new MySqlCommand(queryNominaExistente, conexion.ObtenerConexion()))
            {
                cmdConsulta.Parameters.AddWithValue("@nombre", nombreCompleto);

                using (var reader = cmdConsulta.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        trabajadorId = reader.GetInt32("Id");
                        salarioBruto = reader.IsDBNull(reader.GetOrdinal("SalarioBruto")) ?
                                      reader.GetDecimal("Salario") : reader.GetDecimal("SalarioBruto");

                        // Calcular deducciones
                        decimal contingenciasComunes = salarioBruto * 0.047m; // 4.7%
                        decimal desempleo = salarioBruto * 0.0155m; // 1.55%
                        decimal irpf = salarioBruto * (porcentajeIRPF / 100m);
                        decimal totalDeducciones = contingenciasComunes + desempleo + irpf;
                        decimal salarioNeto = salarioBruto - totalDeducciones;

                        trabajador = new Trabajadores
                        {
                            Id = trabajadorId,
                            NombreCompleto = reader.GetString("NombreCompleto"),
                            DNI = reader.GetString("DNI"),
                            FechaNacimiento = reader.GetDateTime("FechaNacimiento"),
                            Telefono = reader.GetString("Telefono"),
                            Email = reader.GetString("Email"),
                            Direccion = reader.GetString("Direccion"),
                            FechaContratacion = reader.GetDateTime("FechaContratacion"),
                            Salario = reader.GetDecimal("Salario"),
                            NumeroSeguridadSocial = reader.GetString("NumeroSeguridadSocial"),
                            CategoriaProfesional = reader.GetInt32("CategoriaProfesional"),
                            SalarioBruto = salarioBruto,
                            Deducciones = totalDeducciones,
                            SalarioNeto = salarioNeto,
                            Mes = mes,
                            Año = año
                        };
                    }
                    else
                    {
                        MessageBox.Show("No se encontraron datos previos del trabajador para generar la nómina.");
                        return null;
                    }
                }
            }

            // Insertar la nueva nómina
            string insertQuery = @"
        INSERT INTO Nomina (TrabajadorId, Mes, Anio, SalarioBruto, Deducciones, SalarioNeto)
        VALUES (@trabajadorId, @mes, @año, @salarioBruto, @deducciones, @salarioNeto)";

            using (var cmdInsert = new MySqlCommand(insertQuery, conexion.ObtenerConexion()))
            {
                cmdInsert.Parameters.AddWithValue("@trabajadorId", trabajadorId);
                cmdInsert.Parameters.AddWithValue("@mes", mes);
                cmdInsert.Parameters.AddWithValue("@año", año);
                cmdInsert.Parameters.AddWithValue("@salarioBruto", trabajador.SalarioBruto);
                cmdInsert.Parameters.AddWithValue("@deducciones", trabajador.Deducciones);
                cmdInsert.Parameters.AddWithValue("@salarioNeto", trabajador.SalarioNeto);

                try
                {
                    cmdInsert.ExecuteNonQuery();
                    MessageBox.Show("Nueva nómina generada con éxito.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al insertar la nueva nómina: {ex.Message}");
                    return null;
                }
            }

            return trabajador;
        }



        public string GenerarPDFNomina(Trabajadores trabajador)
        {

            // Obtener el porcentaje de IRPF del TextBox
            decimal porcentajeIRPF;
            if (!decimal.TryParse(IrpfTextBox.Text, out porcentajeIRPF))
            {
                porcentajeIRPF = 0; // valor por defecto si hay error
            }

            // Crear la estructura de carpetas en el escritorio
            string escritorioPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string nominasPath = Path.Combine(escritorioPath, "Nominas");
            string trabajadorPath = Path.Combine(nominasPath, trabajador.NombreCompleto);
            string mesAñoPath = Path.Combine(trabajadorPath, $"{trabajador.Mes}_{trabajador.Año}");
            string rutaPDF = Path.Combine(mesAñoPath, $"Nomina_{trabajador.NombreCompleto}_{trabajador.Mes}_{trabajador.Año}.pdf");

            // Crear las carpetas si no existen
            Directory.CreateDirectory(nominasPath);
            Directory.CreateDirectory(trabajadorPath);
            Directory.CreateDirectory(mesAñoPath);

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
                AddDevengo(devengosTable, "Salario Base", trabajador.SalarioBruto.ToString("C"));
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
                AddDeduccion(deduccionesTable, $"IRPF ({porcentajeIRPF}%)", irpf.ToString("C"));

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
            conceptoCell.Border = Rectangle.NO_BORDER;
            importeCell.Border = Rectangle.NO_BORDER;
            table.AddCell(conceptoCell);
            table.AddCell(importeCell);
        }

        private void AddDeduccion(PdfPTable table, string concepto, string importe)
        {
            AddDevengo(table, concepto, importe); // Reutilizamos el método AddDevengo ya que el formato es el mismo
        }

    }
}
