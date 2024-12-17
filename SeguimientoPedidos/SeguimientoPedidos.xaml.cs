using MySqlConnector;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
using TFG.SeguimientoPedidos;


namespace TFG.SeguimientoPedidos
{
    /// <summary>
    /// Lógica de interacción para SeguimientoPedidos.xaml
    /// </summary>
    public partial class SeguimientoPedidosControl : UserControl
    {
        private ObservableCollection<Pedido> _pedidos;
        private string currentSort;
        private bool isAscending = true;

        public SeguimientoPedidosControl()
        {
            InitializeComponent();
            _pedidos = new ObservableCollection<Pedido>();
            PedidosListView.ItemsSource = _pedidos;

            // Texto placeholder inicial
            BuscarTextBox.Text = "Introduce el ID o Numero de Pedido";
            BuscarTextBox.Foreground = Brushes.Gray;

            // Cargar los pedidos al ListView
            CargarPedidos();
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            string searchText = BuscarTextBox.Text;
            if (string.IsNullOrWhiteSpace(searchText) ||
                searchText == "Introduce el ID o Numero de Pedido")
            {
                MessageBox.Show("Por favor, introduce un término de búsqueda válido.");
                return;
            }


            MessageBox.Show($"Buscando: {searchText}");
        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BuscarTextBox.Text == "Introduce el numero de pedido")
            {
                BuscarTextBox.Text = "";
                BuscarTextBox.Foreground = Brushes.Black;
            }
        }

        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            // Limpiar la colección de pedidos
            _pedidos.Clear();

            // Cargar los pedidos de nuevo
            CargarPedidos();

            // Forzar la actualización visual del ListView
            PedidosListView.Items.Refresh();
        }



        private void CargarPedidos()
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion(); // Abre la conexión

                string query = "SELECT Id, NumeroPedido, ClienteWebId, PrecioTotal, FechaPedido, Estado FROM pedidos_web";

                using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string estado = reader.GetString("Estado");
                            Pedido pedido = new Pedido
                            {
                                Id = reader.GetInt32("Id"),
                                NumeroPedido = reader.GetString("NumeroPedido"),
                                ClienteWebId = reader.GetInt32("ClienteWebId"),
                                PrecioTotal = reader.GetDecimal("PrecioTotal"),
                                FechaPedido = reader.GetDateTime("FechaPedido"),
                                Estado = estado,
                                Background = estado == "Entregado" ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Transparent)
                            };
                            _pedidos.Add(pedido);
                        }
                    }
                }
            }
        }



        private void Header_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock header)
            {
                string propertyName = header.Tag.ToString();

                // Cambiar el estado de ordenamiento
                if (currentSort == propertyName)
                {
                    isAscending = !isAscending; // Cambia de ascendente a descendente
                }
                else
                {
                    currentSort = propertyName; // Actualiza la propiedad actual
                    isAscending = true; // Por defecto, ascendente
                }

                // Obtener la vista de colección
                var view = CollectionViewSource.GetDefaultView(PedidosListView.ItemsSource);
                view.SortDescriptions.Clear(); // Limpiar descripciones de ordenamiento

                // Añadir la nueva descripción de ordenamiento
                view.SortDescriptions.Add(new SortDescription(propertyName, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));

                // Actualizar las cabeceras para mostrar la dirección del orden
                var gridView = PedidosListView.View as GridView;
                if (gridView != null)
                {
                    foreach (GridViewColumn column in gridView.Columns)
                    {
                        if (column.Header is TextBlock headerText)
                        {
                            if (headerText.Tag.ToString() == propertyName)
                            {
                                headerText.Text = $"{headerText.Tag} {(isAscending ? "▲" : "▼")}";
                            }
                            else
                            {
                                headerText.Text = headerText.Tag.ToString();
                            }
                        }
                    }
                }
            }
        }



        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BuscarTextBox.Text))
            {
                BuscarTextBox.Text = "Introduce el numero de pedido";
                BuscarTextBox.Foreground = Brushes.Gray;
            }
        }


        private void PedidosListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = PedidosListView.SelectedItem as Pedido;
            if (selectedItem != null)
            {
                Detalle_Pedidos detallePedidos = new Detalle_Pedidos();
                detallePedidos.Inicializar(selectedItem); // Pasar el objeto completo
                detallePedidos.ShowDialog(); // Usar ShowDialog para que sea modal
            }
        }




        private void PedidoEntregadoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PedidosListView.SelectedItem is Pedido pedidoSeleccionado)
            {
                // Cambiar el color de fondo de la fila seleccionada
                var listViewItem = (ListViewItem)PedidosListView.ItemContainerGenerator.ContainerFromItem(pedidoSeleccionado);
                if (listViewItem != null)
                {
                    listViewItem.Background = new SolidColorBrush(Colors.LightGreen); // Cambiar a un color que indique entregado
                }

                // Actualizar el estado del pedido
                pedidoSeleccionado.Estado = "Entregado";
                pedidoSeleccionado.IsDelivered = true; // Marcar como entregado

                // Actualizar el estado en la base de datos
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    string query = "UPDATE pedidos_web SET Estado = @Estado WHERE Id = @Id";

                    using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@Estado", pedidoSeleccionado.Estado);
                        command.Parameters.AddWithValue("@Id", pedidoSeleccionado.Id);
                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Pedido marcado como entregado.");
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un pedido para marcar como entregado.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void EliminarPedidoButton_Click(object sender, RoutedEventArgs e)
        {
            if (PedidosListView.SelectedItem is Pedido pedidoSeleccionado)
            {
                MessageBoxResult resultado = MessageBox.Show("¿Estás seguro de que deseas eliminar este pedido?",
                    "Confirmar Eliminación", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (resultado == MessageBoxResult.Yes)
                {
                    using (var conexion = new Conexion())
                    {
                        conexion.AbrirConexion();

                        // Primero, eliminar los detalles del pedido
                        string deleteDetallesQuery = "DELETE FROM detalle_pedido WHERE PedidoId = @PedidoId";
                        using (var command = new MySqlCommand(deleteDetallesQuery, conexion.ObtenerConexion()))
                        {
                            command.Parameters.AddWithValue("@PedidoId", pedidoSeleccionado.Id);
                            command.ExecuteNonQuery();
                        }

                        // Luego, eliminar el pedido
                        string deletePedidoQuery = "DELETE FROM pedidos_web WHERE Id = @Id";
                        using (var command = new MySqlCommand(deletePedidoQuery, conexion.ObtenerConexion()))
                        {
                            command.Parameters.AddWithValue("@Id", pedidoSeleccionado.Id);
                            command.ExecuteNonQuery();
                        }
                    }

                    // Eliminar el pedido de la lista
                    var pedidos = PedidosListView.ItemsSource as ObservableCollection<Pedido>;
                    if (pedidos != null)
                    {
                        pedidos.Remove(pedidoSeleccionado);
                    }
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un pedido para eliminar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


        private void ExportarButton_Click(object sender, RoutedEventArgs e)
        {
            var pedidos = PedidosListView.ItemsSource as ObservableCollection<Pedido>;

            if (pedidos == null || pedidos.Count == 0)
            {
                MessageBox.Show("No hay pedidos para exportar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Crear un nuevo paquete Excel
            using (var package = new ExcelPackage())
            {
                // Agregar una nueva hoja de trabajo
                var worksheet = package.Workbook.Worksheets.Add("Seguimiento Pedidos");

                // Agregar encabezados
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Número Pedido";
                worksheet.Cells[1, 3].Value = "Cliente Web ID";
                worksheet.Cells[1, 4].Value = "Precio PVP";
                worksheet.Cells[1, 5].Value = "Fecha Pedido";
                worksheet.Cells[1, 6].Value = "Estado";

                int row = 2; // Comenzar en la segunda fila para los datos
                foreach (var pedido in pedidos)
                {
                    worksheet.Cells[row, 1].Value = pedido.Id;
                    worksheet.Cells[row, 2].Value = pedido.NumeroPedido;
                    worksheet.Cells[row, 3].Value = pedido.ClienteWebId;
                    worksheet.Cells[row, 4].Value = pedido.PrecioTotal;
                    worksheet.Cells[row, 5].Value = pedido.FechaPedido.ToString("dd/MM/yyyy"); // Formato de fecha
                    worksheet.Cells[row, 6].Value = pedido.Estado;
                    row++;
                }

                // Guardar el archivo en el escritorio
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string filePath = System.IO.Path.Combine(desktopPath, "Seguimiento_Pedidos.xlsx");

                // Guardar el archivo
                System.IO.FileInfo fi = new System.IO.FileInfo(filePath);
                package.SaveAs(fi);
                MessageBox.Show("Exportación a Excel completada.");

                // Abrir el archivo Excel
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = fi.FullName,
                    UseShellExecute = true // Importante para abrir el archivo
                });
            }


        }



    }
}
