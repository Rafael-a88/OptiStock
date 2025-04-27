using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TFG.OrdenesDeCompra
{
    public partial class OrdenesDeCompra : UserControl
    {
        private ObservableCollection<PedidoDeCompra> _pedidosDeCompra;

        public OrdenesDeCompra()
        {
            InitializeComponent();
            _pedidosDeCompra = new ObservableCollection<PedidoDeCompra>();
            CargarPedidosDeCompra();
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

                    _pedidosDeCompra.Clear();

                    while (reader.Read())
                    {
                        var pedido = new PedidoDeCompra
                        {
                            ID = (int)reader["Id"],
                            NumeroOrden = reader["NumeroOrden"].ToString(),
                            Proveedor = reader["Proveedor"].ToString(),
                            Estado = reader["Estado"].ToString(),
                            FechaApertura = (DateTime)reader["FechaApertura"]
                        };

                        _pedidosDeCompra.Add(pedido);
                    }

                    reader.Close();
                    OrdenesDeCompraListView.ItemsSource = _pedidosDeCompra;

                    ActualizarColoresFilas();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar los pedidos de compra: {ex.Message}",
                                  "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void ActualizarColoresFilas()
        {
            foreach (var item in _pedidosDeCompra)
            {
                var listViewItem = (ListViewItem)OrdenesDeCompraListView.ItemContainerGenerator.ContainerFromItem(item);
                if (listViewItem != null)
                {
                    if (item.Estado == "Mandado")
                    {
                        listViewItem.Background = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
                    }
                    else if (item.Estado == "Recibido")
                    {
                        listViewItem.Background = new SolidColorBrush(Color.FromArgb(128, 255, 0, 0));
                    }
                }
            }
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            string busqueda = BuscarTextBox.Text.ToLower();
            var resultados = _pedidosDeCompra.Where(o =>
                o.ID.ToString().Contains(busqueda) ||
                o.NumeroOrden.ToLower().Contains(busqueda) ||
                o.Proveedor.ToLower().Contains(busqueda) ||
                o.Estado.ToLower().Contains(busqueda) ||
                o.FechaApertura.ToString("dd/MM/yyyy").Contains(busqueda)).ToList();

            OrdenesDeCompraListView.ItemsSource = resultados;
        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BuscarTextBox.Text == "Buscar por ID, Proveedor o Número de Orden")
            {
                BuscarTextBox.Text = "";
                BuscarTextBox.Foreground = Brushes.Black;
            }
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(BuscarTextBox.Text) || BuscarTextBox.Text.Trim() == "")
            {
                BuscarTextBox.Text = "Buscar por ID, Proveedor o Número de Orden";
                BuscarTextBox.Foreground = Brushes.Gray;
            }
        }

        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            CargarPedidosDeCompra();
        }

        private void MandarOrdenButton_Click(object sender, RoutedEventArgs e)
        {
            PedidoDeCompra pedidoSeleccionado = (PedidoDeCompra)OrdenesDeCompraListView.SelectedItem;
            if (pedidoSeleccionado != null)
            {
                pedidoSeleccionado.Estado = "Mandado";

                using (Conexion conexion = new Conexion())
                {
                    try
                    {
                        conexion.AbrirConexion();
                        MySqlCommand command = new MySqlCommand(
                            "UPDATE OrdenesDeCompra SET Estado = @estado WHERE Id = @id",
                            conexion.ObtenerConexion());
                        command.Parameters.AddWithValue("@estado", pedidoSeleccionado.Estado);
                        command.Parameters.AddWithValue("@id", pedidoSeleccionado.ID);
                        command.ExecuteNonQuery();

                        var listViewItem = (ListViewItem)OrdenesDeCompraListView.ItemContainerGenerator
                            .ContainerFromItem(pedidoSeleccionado);
                        if (listViewItem != null)
                        {
                            listViewItem.Background = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
                        }

                        string mensaje = $"Fecha de Envío: {DateTime.Now:dd/MM/yyyy HH:mm:ss}\n" +
                                       $"Número de Orden: {pedidoSeleccionado.NumeroOrden}\n" +
                                       $"Proveedor: {pedidoSeleccionado.Proveedor}";
                        MessageBox.Show(mensaje, "Detalles del Envío",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al actualizar el estado: {ex.Message}",
                                      "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione una orden para mandar.",
                              "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void EliminarOrdenButton_Click(object sender, RoutedEventArgs e)
        {
            PedidoDeCompra pedidoSeleccionado = (PedidoDeCompra)OrdenesDeCompraListView.SelectedItem;
            if (pedidoSeleccionado == null)
            {
                MessageBox.Show("Por favor, seleccione una orden para eliminar.",
                              "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = MessageBox.Show(
                $"¿Está seguro que desea eliminar la orden {pedidoSeleccionado.NumeroOrden}?",
                "Confirmar eliminación",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                using (Conexion conexion = new Conexion())
                {
                    try
                    {
                        conexion.AbrirConexion();
                        using (MySqlTransaction transaction = conexion.ObtenerConexion().BeginTransaction())
                        {
                            try
                            {
                                // Primero eliminar los detalles de la orden
                                MySqlCommand cmdDetalles = new MySqlCommand(
                                    "DELETE FROM DetallesOrdenDeCompra WHERE OrdenDeCompraId = @id",
                                    conexion.ObtenerConexion());
                                cmdDetalles.Transaction = transaction;
                                cmdDetalles.Parameters.AddWithValue("@id", pedidoSeleccionado.ID);
                                cmdDetalles.ExecuteNonQuery();

                                // Luego eliminar la orden
                                MySqlCommand cmdOrden = new MySqlCommand(
                                    "DELETE FROM OrdenesDeCompra WHERE Id = @id",
                                    conexion.ObtenerConexion());
                                cmdOrden.Transaction = transaction;
                                cmdOrden.Parameters.AddWithValue("@id", pedidoSeleccionado.ID);
                                cmdOrden.ExecuteNonQuery();

                                transaction.Commit();
                                _pedidosDeCompra.Remove(pedidoSeleccionado);

                                MessageBox.Show("Orden eliminada correctamente.",
                                              "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception("Error al eliminar la orden: " + ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error al eliminar la orden: {ex.Message}",
                                      "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void OrdenesDeCompraListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PedidoDeCompra pedidoSeleccionado = (PedidoDeCompra)OrdenesDeCompraListView.SelectedItem;
            if (pedidoSeleccionado != null)
            {
                DetallesOrdenCompra detallesVentana = new DetallesOrdenCompra(
                    pedidoSeleccionado.ID,
                    pedidoSeleccionado.NumeroOrden);
                detallesVentana.ShowDialog();
            }
        }
    }
}
