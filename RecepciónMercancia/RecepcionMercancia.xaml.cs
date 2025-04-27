using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TFG.OrdenesDeCompra;

namespace TFG.RecepcionMercancia
{
    public partial class RecepcionMercanciaView : UserControl
    {
        private ObservableCollection<PedidoDeCompra> _pedidosPendientes;

        public RecepcionMercanciaView()
        {
            InitializeComponent();
            _pedidosPendientes = new ObservableCollection<PedidoDeCompra>();
            CargarPedidosPendientes();
        }

        private void CargarPedidosPendientes()
        {
            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    string query = "SELECT * FROM OrdenesDeCompra WHERE Estado = 'Mandado'";
                    MySqlCommand cmd = new MySqlCommand(query, conexion.ObtenerConexion());

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        _pedidosPendientes.Clear();
                        while (reader.Read())
                        {
                            _pedidosPendientes.Add(new PedidoDeCompra
                            {
                                ID = reader.GetInt32("Id"),
                                NumeroOrden = reader.GetString("NumeroOrden"),
                                Proveedor = reader.GetString("Proveedor"),
                                Estado = reader.GetString("Estado"),
                                FechaApertura = reader.GetDateTime("FechaApertura")
                            });
                        }
                    }

                    lvPedidosPendientes.ItemsSource = _pedidosPendientes;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar los pedidos pendientes: {ex.Message}");
                }
            }
        }

        private void TxtBuscar_GotFocus(object sender, RoutedEventArgs e)
        {
            if (txtBuscar.Text == "Buscar por Número de Orden o Proveedor...")
            {
                txtBuscar.Text = "";
            }
        }

        private void TxtBuscar_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBuscar.Text))
            {
                txtBuscar.Text = "Buscar por Número de Orden o Proveedor...";
            }
        }

        private void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            string busqueda = txtBuscar.Text.ToLower();
            var resultados = _pedidosPendientes.Where(p =>
                p.NumeroOrden.ToLower().Contains(busqueda) ||
                p.Proveedor.ToLower().Contains(busqueda)).ToList();

            lvPedidosPendientes.ItemsSource = resultados;
        }

        private void BtnRefrescar_Click(object sender, RoutedEventArgs e)
        {
            CargarPedidosPendientes();
        }

        private void LvPedidosPendientes_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var pedidoSeleccionado = lvPedidosPendientes.SelectedItem as PedidoDeCompra;
            if (pedidoSeleccionado != null)
            {
                var ventanaDetalles = new DetallesRecepcion(pedidoSeleccionado.ID, pedidoSeleccionado.NumeroOrden);
                ventanaDetalles.ShowDialog();

                // Actualizar la lista después de cerrar la ventana de detalles
                CargarPedidosPendientes();
            }
        }
    }
}