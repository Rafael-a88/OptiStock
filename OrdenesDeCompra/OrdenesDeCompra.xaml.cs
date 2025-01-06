using MySqlConnector;
using System;
using System.Collections.Generic;
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

namespace TFG.OrdenesDeCompra
{
    public partial class OrdenesDeCompra : UserControl
    {
        private List<PedidoDeCompra> _pedidosDeCompra; // Lista de pedidos de compra

        public OrdenesDeCompra()
        {
            InitializeComponent();
            _pedidosDeCompra = new List<PedidoDeCompra>(); // Inicializar la lista de pedidos de compra
            CargarPedidosDeCompra(); // Cargar los pedidos de compra iniciales
        }

        private void CargarPedidosDeCompra()
        {
            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    MySqlCommand command = new MySqlCommand("SELECT * FROM OrdenesDeCompra", conexion.ObtenerConexion());
                    MySqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        _pedidosDeCompra.Add(new PedidoDeCompra
                        {
                            ID = (int)reader["Id"],
                            NumeroOrden = reader["NumeroOrden"].ToString(),
                            Proveedor = reader["Proveedor"].ToString(),
                            Estado = reader["Estado"].ToString(),
                            FechaApertura = (DateTime)reader["FechaApertura"]
                        });
                    }

                    reader.Close();
                    OrdenesDeCompraListView.ItemsSource = _pedidosDeCompra;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error al cargar los pedidos de compra: " + ex.Message);
                }
            }
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            // Implementar la lógica de búsqueda de pedidos de compra
            string busqueda = BuscarTextBox.Text;
            // Filtrar la lista de pedidos de compra y asignar el resultado a la fuente de datos del ListView
            OrdenesDeCompraListView.ItemsSource = _pedidosDeCompra.Where(o => o.ID.ToString().Contains(busqueda) ||
                                                                               o.NumeroOrden.Contains(busqueda) ||
                                                                               o.Proveedor.Contains(busqueda) ||
                                                                               o.Estado.Contains(busqueda) ||
                                                                               o.FechaApertura.ToString().Contains(busqueda));
        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Lógica a ejecutar cuando el TextBox de búsqueda obtiene el foco
            if (BuscarTextBox.Text == "Buscar por ID, Proveedor o Número de Orden")
            {
                BuscarTextBox.Text = "";
                BuscarTextBox.Foreground = Brushes.Black;
            }
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Lógica a ejecutar cuando el TextBox de búsqueda pierde el foco
            if (string.IsNullOrEmpty(BuscarTextBox.Text) || BuscarTextBox.Text.Trim() == "")
            {
                BuscarTextBox.Text = "Buscar por ID, Proveedor o Número de Orden";
                BuscarTextBox.Foreground = Brushes.Gray;
            }
        }



        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            // Implementar la lógica de actualización de la lista de pedidos de compra
            CargarPedidosDeCompra();
        }

        private void MandarOrdenButton_Click(object sender, RoutedEventArgs e)
        {
            // Implementar la lógica de envío del pedido de compra al proveedor
            PedidoDeCompra pedidoSeleccionado = (PedidoDeCompra)OrdenesDeCompraListView.SelectedItem;
            if (pedidoSeleccionado != null)
            {
                // Enviar el pedido de compra al proveedor
                pedidoSeleccionado.Estado = "Enviada";
                // Actualizar la lista de pedidos de compra
                CargarPedidosDeCompra();
            }
        }

        private void OrdenesDeCompraListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Implementar la lógica de visualización de los detalles del pedido de compra
            PedidoDeCompra pedidoSeleccionado = (PedidoDeCompra)OrdenesDeCompraListView.SelectedItem;
            if (pedidoSeleccionado != null)
            {
                // Abrir la vista de detalles del pedido de compra
                // Por ejemplo, crear una nueva instancia de un UserControl "DetallesPedidoDeCompra"
                // y mostrarlo en un contenedor adecuado de la interfaz de usuario.
            }
        }
    }

}
