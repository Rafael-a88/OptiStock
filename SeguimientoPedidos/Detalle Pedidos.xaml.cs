using MySqlConnector;
using System.Windows;
using TFG.ViewModels;

namespace TFG.SeguimientoPedidos
{
    public partial class Detalle_Pedidos : Window
    {
        private PedidoViewModel viewModel;

        public Detalle_Pedidos()
        {
            InitializeComponent();
            viewModel = new PedidoViewModel();
            this.DataContext = viewModel;
        }

        public void Inicializar(Pedido pedido)
        {
            viewModel.NumeroPedido = pedido.NumeroPedido;
            viewModel.ClienteWebId = pedido.ClienteWebId;
            viewModel.Id = pedido.Id;
            viewModel.PrecioTotal = pedido.PrecioTotal;
            viewModel.FechaPedido = pedido.FechaPedido;
            viewModel.Estado = pedido.Estado;
        }

        private void PrepararProductos_Click(object sender, RoutedEventArgs e)
        {
            // Crear una instancia de la nueva ventana pasando el número de pedido
            PreparacionPedidos preparacionPedidos = new PreparacionPedidos(viewModel.NumeroPedido);

            // Si el diálogo retorna true (pedido finalizado), actualizar la vista actual
            if (preparacionPedidos.ShowDialog() == true)
            {
                // Actualizar los datos del pedido actual
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    string query = @"SELECT PrecioTotal, Estado 
                           FROM pedidos_web 
                           WHERE NumeroPedido = @NumeroPedido";

                    using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@NumeroPedido", viewModel.NumeroPedido);
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                viewModel.PrecioTotal = reader.GetDecimal("PrecioTotal");
                                viewModel.Estado = reader.GetString("Estado");
                            }
                        }
                    }
                }
            }
        }

    }
}
