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
        private ObservableCollection<PedidoDeCompra> _pedidosDeCompra; // Lista de pedidos de compra

        public OrdenesDeCompra()
        {
            InitializeComponent();
            _pedidosDeCompra = new ObservableCollection<PedidoDeCompra>(); // Inicializar la lista de pedidos de compra
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

                    // Limpiar la colección antes de agregar nuevos elementos
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

                        // Agregar pedido a la colección
                        _pedidosDeCompra.Add(pedido);
                    }

                    reader.Close();
                    OrdenesDeCompraListView.ItemsSource = _pedidosDeCompra; // Asignar la colección al ListView

                    // Cambiar el color de fondo de las filas según el estado
                    foreach (var item in _pedidosDeCompra)
                    {
                        var listViewItem = (ListViewItem)OrdenesDeCompraListView.ItemContainerGenerator.ContainerFromItem(item);
                        if (listViewItem != null && item.Estado == "Mandado")
                        {
                            listViewItem.Background = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
                        }
                    }
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
            string busqueda = BuscarTextBox.Text.ToLower(); // Convertir a minúsculas para búsqueda insensible
            var resultados = _pedidosDeCompra.Where(o => o.ID.ToString().Contains(busqueda) ||
                                                         o.NumeroOrden.ToLower().Contains(busqueda) ||
                                                         o.Proveedor.ToLower().Contains(busqueda) ||
                                                         o.Estado.ToLower().Contains(busqueda) ||
                                                         o.FechaApertura.ToString("dd/MM/yyyy").Contains(busqueda)).ToList();

            OrdenesDeCompraListView.ItemsSource = resultados; // Asignar los resultados filtrados al ListView
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
            PedidoDeCompra pedidoSeleccionado = (PedidoDeCompra)OrdenesDeCompraListView.SelectedItem;
            if (pedidoSeleccionado != null)
            {
                // Cambiar el estado a "Mandado"
                pedidoSeleccionado.Estado = "Mandado";

                // Actualizar el estado en la base de datos
                using (Conexion conexion = new Conexion())
                {
                    try
                    {
                        conexion.AbrirConexion();
                        MySqlCommand command = new MySqlCommand("UPDATE OrdenesDeCompra SET Estado = @estado WHERE Id = @id", conexion.ObtenerConexion());
                        command.Parameters.AddWithValue("@estado", pedidoSeleccionado.Estado);
                        command.Parameters.AddWithValue("@id", pedidoSeleccionado.ID);
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error al actualizar el estado en la base de datos: " + ex.Message);
                    }
                }

                // Cambiar el color de fondo de la fila seleccionada a verde
                var listViewItem = (ListViewItem)OrdenesDeCompraListView.ItemContainerGenerator.ContainerFromItem(pedidoSeleccionado);
                if (listViewItem != null)
                {
                    listViewItem.Background = new SolidColorBrush(Color.FromArgb(128, 0, 255, 0));
                }

                // Mostrar un mensaje con la información requerida
                string mensaje = $"Fecha de Envío: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}\n" +
                                 $"Número de Orden: {pedidoSeleccionado.NumeroOrden}\n" +
                                 $"Proveedor: {pedidoSeleccionado.Proveedor}";
                MessageBox.Show(mensaje, "Detalles del Envío", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }


        private void OrdenesDeCompraListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            PedidoDeCompra pedidoSeleccionado = (PedidoDeCompra)OrdenesDeCompraListView.SelectedItem;
            if (pedidoSeleccionado != null)
            {
                DetallesOrdenCompra detallesVentana = new DetallesOrdenCompra(pedidoSeleccionado.ID, pedidoSeleccionado.NumeroOrden);
                detallesVentana.ShowDialog(); // Mostrar como un diálogo modal
            }
        }
    }
}
