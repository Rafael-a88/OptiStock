using System;
using System.Linq;
using System.Windows;
using MySqlConnector;

namespace TFG.SeguimientoPedidos
{
    public partial class PreparacionPedidos : Window
    {
        private readonly string _numeroPedido;

        public PreparacionPedidos(string numeroPedido)
        {
            InitializeComponent();
            _numeroPedido = numeroPedido;
            DataContext = new PreparacionPedidosViewModel(numeroPedido);
        }

        private void FinalizarPedido_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = (this.DataContext as PreparacionPedidosViewModel);
            var productos = viewModel.Productos;

            // Verificar si todos los productos están preparados
            bool todosPreparados = productos.All(p => p.IsPreparado);

            if (!todosPreparados)
            {
                MessageBox.Show("Para finalizar el pedido el estado de los productos deben estar preparados.",
                    "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    string query = @"UPDATE pedidos_web 
                                   SET Estado = 'Preparado', 
                                       PrecioTotal = @PrecioTotal 
                                   WHERE NumeroPedido = @NumeroPedido";

                    using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@PrecioTotal", viewModel.TotalCompra);
                        command.Parameters.AddWithValue("@NumeroPedido", _numeroPedido);

                        int filasAfectadas = command.ExecuteNonQuery();

                        if (filasAfectadas > 0)
                        {
                            MessageBox.Show("Pedido finalizado con éxito.",
                                "Información", MessageBoxButton.OK, MessageBoxImage.Information);

                            this.DialogResult = true;
                            this.Close();
                        }
                        else
                        {
                            MessageBox.Show("No se pudo actualizar el pedido.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar el pedido: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
