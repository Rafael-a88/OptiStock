using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;

namespace TFG.HistorialDeVentas
{
    public partial class DetallesDeVenta : Window
    {
        public string NumeroDeDocumento { get; set; }
        public string CorreoUsuario { get; set; }
        public DateTime FechaMovimiento { get; set; }
        private List<DetalleVenta> detallesVenta;

        public DetallesDeVenta(string numeroDeDocumento, string correoUsuario, DateTime fechaMovimiento)
        {
            InitializeComponent();
            NumeroDeDocumento = numeroDeDocumento;
            CorreoUsuario = correoUsuario;
            FechaMovimiento = fechaMovimiento;
            detallesVenta = new List<DetalleVenta>();
            CargarDetallesDeVenta();
        }

        private void CargarDetallesDeVenta()
        {
            NumeroDeDocumentoTextBlock.Text = NumeroDeDocumento;
            CorreoUsuarioTextBlock.Text = CorreoUsuario;
            FechaMovimientoTextBlock.Text = FechaMovimiento.ToString("dd/MM/yyyy");

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                var connection = conexion.ObtenerConexion();

                string query = @"
                    SELECT h.Producto, p.Descripcion, p.EAN, h.Cantidad
                    FROM HistorialVentas h
                    JOIN productos p ON h.Producto = p.EAN
                    WHERE h.NumeroDocumento = @NumeroDocumento
                ";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@NumeroDocumento", NumeroDeDocumento);
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detallesVenta.Add(new DetalleVenta
                            {
                                Producto = reader.GetString("Descripcion"),
                                EAN = reader.GetString("EAN"),
                                Cantidad = reader.GetInt32("Cantidad")
                            });
                        }
                    }
                }

                // Agregar los detalles de la venta al DataGrid
                DetallesDataGrid.ItemsSource = detallesVenta;
            } // La conexión se cierra automáticamente aquí gracias a IDisposable
        }


        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }

    public class DetalleVenta
    {
        public string Producto { get; set; }
        public string EAN { get; set; }
        public int Cantidad { get; set; }
    }
}
