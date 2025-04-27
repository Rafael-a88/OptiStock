using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TFG.HistorialDeVentasWebNamespace
{
    public partial class HistorialDeVentasWeb : UserControl
    {
        public HistorialDeVentasWeb()
        {
            InitializeComponent();
            CargarDatos();
            // Añadir el evento de doble clic
            MovimientosListView.MouseDoubleClick += MovimientosListView_MouseDoubleClick;
        }

        private void MovimientosListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Obtener el elemento seleccionado del ListView
            var selectedItem = MovimientosListView.SelectedItem as DataRowView;
            if (selectedItem != null)
            {
                // Obtener los valores necesarios para abrir la ventana de detalles
                string numeroDeDocumento = selectedItem["NumeroDocumento"].ToString();
                string correoUsuario = selectedItem["CorreoUsuario"].ToString();
                DateTime fechaMovimiento = (DateTime)selectedItem["FechaMovimiento"];
                int clienteWebId = Convert.ToInt32(selectedItem["ClienteWebId"]);

                // Determinar si es una venta web o una venta normal
                bool esVentaWeb = numeroDeDocumento.StartsWith("TW");

                // Abrir la ventana de detalles correspondiente
                if (esVentaWeb)
                {
                    OpenDetallesDeVentaWebWindow(numeroDeDocumento, correoUsuario, fechaMovimiento, clienteWebId);
                }
            }
        }

        private void OpenDetallesDeVentaWebWindow(string numeroDeDocumento, string correoUsuario, DateTime fechaMovimiento, int clienteWebId)
        {
            var detallesDeVentaWebWindow = new DetallesDeVentaWeb(numeroDeDocumento, correoUsuario, fechaMovimiento, clienteWebId);
            detallesDeVentaWebWindow.Show();
        }

        private void CargarDatos(string emailFilter = null)
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();

                string query = @"
        SELECT 
            CONCAT('TW', DATE_FORMAT(MIN(m.FechaMovimiento), '%Y%m%d%H%i'), LPAD(MIN(m.Id) % 10000, 4, '0')) AS NumeroDocumento,
            cw.Correo AS CorreoUsuario,
            MIN(m.FechaMovimiento) AS FechaMovimiento,
            m.ClienteWebId
        FROM 
            MovimientosWeb m
        JOIN 
            ClienteWeb cw ON m.ClienteWebId = cw.Id
        WHERE
            (@EmailFilter IS NULL OR cw.Correo LIKE @EmailFilter)
        GROUP BY 
            m.ClienteWebId, DATE_FORMAT(m.FechaMovimiento, '%Y-%m-%d %H:%i:%s')
        ORDER BY 
            FechaMovimiento DESC;";

                using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    command.Parameters.AddWithValue("@EmailFilter", emailFilter != null ? $"%{emailFilter}%" : "%");
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    MovimientosListView.ItemsSource = dataTable.DefaultView;
                }
            }
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

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            CargarDatos(BuscarTextBox.Text);
        }

        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            BuscarTextBox.Text = ""; // Limpiar el campo de búsqueda
            CargarDatos(); // Recargar los datos sin filtro
        }
    }
}
