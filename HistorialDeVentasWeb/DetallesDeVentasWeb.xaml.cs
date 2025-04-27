using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;

namespace TFG.HistorialDeVentasWebNamespace
{
    public partial class DetallesDeVentaWeb : Window
    {
        public string NumeroDeDocumento { get; set; }
        public string CorreoUsuario { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public int ClienteWebId { get; set; }
        private List<DetalleVentaWeb> detallesVentaWeb;

        public DetallesDeVentaWeb(string numeroDeDocumento, string correoUsuario, DateTime fechaMovimiento, int clienteWebId)
        {
            InitializeComponent();
            NumeroDeDocumento = numeroDeDocumento;
            CorreoUsuario = correoUsuario;
            FechaMovimiento = fechaMovimiento;
            ClienteWebId = clienteWebId;
            detallesVentaWeb = new List<DetalleVentaWeb>();
            CargarDetallesDeVentaWeb();
        }

        private void CargarDetallesDeVentaWeb()
        {
            NumeroDeDocumentoTextBlock.Text = NumeroDeDocumento;
            CorreoUsuarioTextBlock.Text = CorreoUsuario;
            FechaMovimientoTextBlock.Text = FechaMovimiento.ToString("dd/MM/yyyy");

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                var connection = conexion.ObtenerConexion();

                string query = @"
            SELECT p.Descripcion AS Producto, mw.ProductoEAN AS EAN, SUM(mw.Cantidad) AS Cantidad
            FROM MovimientosWeb mw
            JOIN productos p ON mw.ProductoEAN = p.EAN
            WHERE mw.ClienteWebId = @ClienteWebId
            AND DATE_FORMAT(mw.FechaMovimiento, '%Y-%m-%d %H:%i:%s') = DATE_FORMAT(@FechaMovimiento, '%Y-%m-%d %H:%i:%s')
            GROUP BY mw.ProductoEAN, p.Descripcion
        ";

                try
                {
                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@ClienteWebId", ClienteWebId);
                        command.Parameters.AddWithValue("@FechaMovimiento", FechaMovimiento);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                detallesVentaWeb.Add(new DetalleVentaWeb
                                {
                                    Producto = reader.GetString("Producto"),
                                    EAN = reader.GetString("EAN"),
                                    Cantidad = reader.GetInt32("Cantidad")
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar los detalles: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

                // Si no se encontraron detalles, mostrar un mensaje
                if (detallesVentaWeb.Count == 0)
                {
                    MessageBox.Show("No se encontraron detalles para esta venta web.", "Sin detalles", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Agregar los detalles de la venta al DataGrid
                DetallesDataGrid.ItemsSource = detallesVentaWeb;
            }
        }


        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class DetalleVentaWeb
    {
        public string Producto { get; set; }
        public string EAN { get; set; }
        public int Cantidad { get; set; }
    }
}
