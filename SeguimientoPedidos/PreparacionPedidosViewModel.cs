using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace TFG.SeguimientoPedidos
{
    public class PreparacionPedidosViewModel : INotifyPropertyChanged
    {
        private readonly Conexion _conexion;
        public ObservableCollection<Producto> Productos { get; set; }

        public PreparacionPedidosViewModel(string numeroPedido)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture =
                new System.Globalization.CultureInfo("es-ES");

            _conexion = new Conexion();
            Productos = new ObservableCollection<Producto>();
            Productos.CollectionChanged += (s, e) => OnPropertyChanged(nameof(TotalCompra));
            CargarProductosPedido(numeroPedido);
        }

        private void CargarProductosPedido(string numeroPedido)
        {
            try
            {
                _conexion.AbrirConexion();
                using (var connection = _conexion.ObtenerConexion())
                {
                    string query = @"
                    SELECT p.Nombre, p.EAN, p.PrecioTotal as PrecioUnitario, dp.Cantidad
                    FROM pedidos_web pw
                    JOIN detalle_pedido dp ON pw.Id = dp.PedidoId
                    JOIN productos p ON dp.ProductoId = p.Id
                    WHERE pw.NumeroPedido = @numeroPedido";

                    using (var command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@numeroPedido", numeroPedido);
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var producto = new Producto
                                {
                                    Nombre = reader["Nombre"].ToString(),
                                    EAN = reader["EAN"].ToString(),
                                    PrecioUnitario = Convert.ToDouble(reader["PrecioUnitario"]),
                                    Cantidad = Convert.ToInt32(reader["Cantidad"]),
                                    IsPreparado = false
                                };

                                // Suscribirse al evento PropertyChanged del producto
                                producto.PropertyChanged += (s, e) =>
                                {
                                    if (e.PropertyName == nameof(Producto.PrecioTotal) || e.PropertyName == nameof(Producto.Cantidad))
                                    {
                                        OnPropertyChanged(nameof(TotalCompra)); // Actualiza TotalCompra
                                    }
                                };

                                Productos.Add(producto);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar productos: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _conexion.CerrarConexion();
            }
        }

        public double TotalCompra
        {
            get
            {
                return Productos.Sum(p => p.PrecioTotal);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _conexion?.Dispose();
        }
    }
}
