using System;
using System.Data;
using MySqlConnector;
using System.Windows;
using System.Windows.Controls;
using TFG.HistorialDeVentas;

namespace TFG.HistorialDeVentasNamespace
{
    public partial class HistorialDeVentas : UserControl
    {
        public HistorialDeVentas()
        {
            InitializeComponent();
            CargarDatos();
            MovimientosListView.MouseDoubleClick += MovimientosListView_MouseDoubleClick;
        }

        private void MovimientosListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // Obtener el elemento seleccionado del ListView
            var selectedItem = MovimientosListView.SelectedItem as DataRowView;
            if (selectedItem != null)
            {
                // Obtener los valores necesarios para abrir la ventana de detalles
                string numeroDeDocumento = selectedItem["NumeroDocumento"].ToString();
                string correoUsuario = selectedItem["CorreoUsuario"].ToString();
                DateTime fechaMovimiento = (DateTime)selectedItem["FechaMovimiento"];

                // Abrir la ventana de detalles de la venta
                OpenDetallesDeVentaWindow(numeroDeDocumento, correoUsuario, fechaMovimiento);
            }
        }

        private void OpenDetallesDeVentaWindow(string numeroDeDocumento, string correoUsuario, DateTime fechaMovimiento)
        {
            var detallesDeVentaWindow = new DetallesDeVenta(numeroDeDocumento, correoUsuario, fechaMovimiento);
            detallesDeVentaWindow.Show();
        }

        private void CargarDatos(string emailFilter = null)
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();

                string query = @"
        SELECT 
            hv.NumeroDocumento,
            c.Email AS CorreoUsuario,
            v.Fecha AS FechaMovimiento
        FROM 
            HistorialVentas hv
        JOIN 
            Ventas v ON hv.NumeroDocumento = v.NumeroDocumento
        JOIN 
            clientes c ON v.DniCliente = c.DNI
        WHERE 
            (@EmailFilter IS NULL OR c.Email LIKE @EmailFilter)
        GROUP BY 
            hv.NumeroDocumento, c.Email, v.Fecha";

                using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    command.Parameters.AddWithValue("@EmailFilter", emailFilter ?? "%");
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    MovimientosListView.ItemsSource = dataTable.DefaultView;
                }
            }
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos(BuscarTextBox.Text); // Recargar los datos con el filtro
        }

        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            BuscarTextBox.Text = ""; // Limpiar el campo de búsqueda
            CargarDatos(); // Recargar los datos sin filtro
        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            BuscarTextBox.Text = "";
            BuscarTextBox.Foreground = System.Windows.Media.Brushes.Black;
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BuscarTextBox.Text))
            {
                BuscarTextBox.Text = "Introduce el correo electronico del usuario";
                BuscarTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }
    }
}