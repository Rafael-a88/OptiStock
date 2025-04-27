using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Linq;
using OfficeOpenXml;
using System.IO;

namespace TFG.Ubicaciones
{
    public partial class Ubicaciones : UserControl
    {
        private string currentSort = ""; // Variable para controlar la columna actual de ordenamiento
        private bool isAscending = true;
        private ICollectionView view; // Vista para gestionar el ordenamiento

        public Ubicaciones()
        {
            InitializeComponent();
            CargarUbicaciones();
        }

        private void CargarUbicaciones()
        {
            var ubicaciones = ObtenerUbicacionesDesdeBaseDeDatos();
            UbicacionesListView.ItemsSource = ubicaciones;

            // Configurar la vista para ordenamiento
            view = CollectionViewSource.GetDefaultView(UbicacionesListView.ItemsSource);
            view.SortDescriptions.Clear();
        }

        private List<Ubicacion> ObtenerUbicacionesDesdeBaseDeDatos()
        {
            var ubicaciones = new List<Ubicacion>();

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                var command = new MySqlCommand("SELECT Nombre FROM ubicaciones", conexion.ObtenerConexion());

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string nombreCompleto = reader.GetString(0);
                        ubicaciones.Add(new Ubicacion(nombreCompleto));
                    }
                }
            }

            return ubicaciones;
        }

        // Método para refrescar
        public void RefrescarButton_Click(object sender, EventArgs e)
        {
            CargarUbicaciones();
        }

        // Método para agregar ubicación
        private void AgregarUbicacionButton_Click(object sender, EventArgs e)
        {
            AgregarUbicacionView agregarUbicacionesView = new AgregarUbicacionView();
            Window mainWindow = Window.GetWindow(this);
            if (mainWindow is Principal principal)
            {
                principal.ContenidoPrincipal.Content = agregarUbicacionesView;
            }
            else
            {
                MessageBox.Show("No se pudo obtener la ventana principal.");
            }
        }


        // Método para eliminar ubicación
        public void EliminarUbicacionButton_Click(object sender, EventArgs e)
        {
            // Lógica para eliminar ubicación
        }

        // Método para ordenar por columna
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

                if (UbicacionesListView.ItemsSource == null || UbicacionesListView.Items.Count == 0)
                {
                    MessageBox.Show("No hay datos para ordenar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                view = CollectionViewSource.GetDefaultView(UbicacionesListView.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(propertyName, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));

                var gridView = UbicacionesListView.View as GridView;
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

        private void ExportarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var package = new ExcelPackage())
                {
                    var worksheet = package.Workbook.Worksheets.Add("Ubicaciones");

                    // Agregar encabezados
                    worksheet.Cells[1, 1].Value = "Almacén";
                    worksheet.Cells[1, 2].Value = "Calle";
                    worksheet.Cells[1, 3].Value = "Lado";
                    worksheet.Cells[1, 4].Value = "Módulo";
                    worksheet.Cells[1, 5].Value = "Altura";
                    worksheet.Cells[1, 6].Value = "Ubicación Total";

                    int row = 2;
                    foreach (var item in UbicacionesListView.Items)
                    {
                        // Obtener los valores de los campos del ListView
                        var almacen = (item as Ubicacion).Almacen;
                        var calle = (item as Ubicacion).Calle;
                        var lado = (item as Ubicacion).Lado;
                        var modulo = (item as Ubicacion).Modulo;
                        var altura = (item as Ubicacion).Altura;
                        var ubicacionTotal = (item as Ubicacion).UbicacionTotal;

                        // Llenar las celdas con los datos
                        worksheet.Cells[row, 1].Value = almacen;
                        worksheet.Cells[row, 2].Value = calle;
                        worksheet.Cells[row, 3].Value = lado;
                        worksheet.Cells[row, 4].Value = modulo;
                        worksheet.Cells[row, 5].Value = altura;
                        worksheet.Cells[row, 6].Value = ubicacionTotal;
                        row++;
                    }

                    // Guardar el archivo en el escritorio
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                    string filePath = Path.Combine(desktopPath, "Ubicaciones.xlsx");

                    // Guardar el archivo
                    FileInfo fi = new FileInfo(filePath);
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
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar a Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
