using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace TFG.RecepcionMercancia
{
    public partial class DetallesRecepcion : Window, INotifyPropertyChanged
    {
        private int _ordenId;
        private string _numeroOrden;
        private string _proveedor;
        private ObservableCollection<ProductoRecepcion> _productos;

        public string NumeroOrden
        {
            get => _numeroOrden;
            set
            {
                _numeroOrden = value;
                OnPropertyChanged(nameof(NumeroOrden));
            }
        }

        public string Proveedor
        {
            get => _proveedor;
            set
            {
                _proveedor = value;
                OnPropertyChanged(nameof(Proveedor));
            }
        }

        public ObservableCollection<ProductoRecepcion> Productos
        {
            get => _productos;
            set
            {
                _productos = value;
                OnPropertyChanged(nameof(Productos));
            }
        }

        public DetallesRecepcion(int ordenId, string numeroOrden)
        {
            InitializeComponent();
            DataContext = this;
            _ordenId = ordenId;
            NumeroOrden = numeroOrden;
            Productos = new ObservableCollection<ProductoRecepcion>();
            CargarProductos();
        }

        private void CargarProductos()
        {
            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    CargarDatosProveedor(conexion);
                    CargarListaProductos(conexion);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar los datos: {ex.Message}",
                                  "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void CargarDatosProveedor(Conexion conexion)
        {
            try
            {
                string queryProveedor = "SELECT Proveedor FROM OrdenesDeCompra WHERE Id = @OrdenId";
                MySqlCommand cmdProveedor = new MySqlCommand(queryProveedor, conexion.ObtenerConexion());
                cmdProveedor.Parameters.AddWithValue("@OrdenId", _ordenId);

                var resultado = cmdProveedor.ExecuteScalar();
                if (resultado == null)
                {
                    throw new Exception($"No se encontró la orden de compra con ID {_ordenId}");
                }

                Proveedor = resultado.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar datos del proveedor: {ex.Message}");
            }
        }

        private void CargarListaProductos(Conexion conexion)
        {
            try
            {
                string queryProductos = @"
                    SELECT 
                        d.ProductoId,
                        p.Nombre as NombreProducto,
                        p.EAN,
                        d.Cantidad as CantidadPedida
                    FROM DetallesOrdenDeCompra d
                    JOIN Productos p ON d.ProductoId = p.Id
                    WHERE d.OrdenDeCompraId = @OrdenId";

                MySqlCommand cmdProductos = new MySqlCommand(queryProductos, conexion.ObtenerConexion());
                cmdProductos.Parameters.AddWithValue("@OrdenId", _ordenId);

                using (MySqlDataReader reader = cmdProductos.ExecuteReader())
                {
                    if (!reader.HasRows)
                    {
                        throw new Exception("No se encontraron productos para esta orden");
                    }

                    while (reader.Read())
                    {
                        Productos.Add(new ProductoRecepcion
                        {
                            Id = reader.GetInt32("ProductoId"),
                            NombreProducto = reader.GetString("NombreProducto"),
                            EAN = reader.GetString("EAN"),
                            CantidadPedida = reader.GetInt32("CantidadPedida"),
                            CantidadRecibida = 0,
                            Recibido = false
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al cargar la lista de productos: {ex.Message}");
            }
        }

        private bool ValidarProductos(out string mensaje)
        {
            // Verificar productos sin validar
            var productosNoValidados = Productos.Where(p => p.Estado != "Validado").ToList();
            if (productosNoValidados.Any())
            {
                mensaje = "Los siguientes productos aún no están validados:\n";
                foreach (var producto in productosNoValidados)
                {
                    mensaje += $"- {producto.NombreProducto}\n";
                }
                return false;
            }

            // Verificar cantidades negativas
            var productosInvalidos = Productos.Where(p => p.CantidadRecibida < 0).ToList();
            if (productosInvalidos.Any())
            {
                mensaje = "Los siguientes productos tienen cantidades inválidas (negativas):\n";
                foreach (var producto in productosInvalidos)
                {
                    mensaje += $"- {producto.NombreProducto}\n";
                }
                return false;
            }

            mensaje = string.Empty;
            return true;
        }

        private bool MostrarResumenDiferencias()
        {
            bool hayDiferencias = false;
            string mensajeResumen = "Resumen de la recepción:\n\n";

            foreach (var producto in Productos)
            {
                string mensajeDiferencia = producto.ObtenerMensajeDiferencia();
                if (!string.IsNullOrEmpty(mensajeDiferencia))
                {
                    hayDiferencias = true;
                    mensajeResumen += $"{producto.NombreProducto}: {mensajeDiferencia}\n";
                }
            }

            if (hayDiferencias)
            {
                mensajeResumen += "\n¿Desea continuar con la recepción?";
                return MessageBox.Show(mensajeResumen, "Resumen de recepción",
                                     MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes;
            }

            return true;
        }

        private void ActualizarOrdenCompra(MySqlConnection conexion, MySqlTransaction transaction)
        {
            try
            {
                string updateOrden = "UPDATE OrdenesDeCompra SET Estado = 'Recibido' WHERE Id = @OrdenId";
                MySqlCommand cmdOrden = new MySqlCommand(updateOrden, conexion);
                cmdOrden.Transaction = transaction;
                cmdOrden.Parameters.AddWithValue("@OrdenId", _ordenId);

                int filasAfectadas = cmdOrden.ExecuteNonQuery();
                if (filasAfectadas == 0)
                {
                    throw new Exception("No se pudo actualizar el estado de la orden de compra");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al actualizar la orden de compra: {ex.Message}");
            }
        }

        private void ActualizarStockYMovimientos(MySqlConnection conexion, MySqlTransaction transaction)
        {
            foreach (var producto in Productos.Where(p => p.CantidadRecibida > 0))
            {
                try
                {
                    // Verificar si el producto existe
                    string checkProducto = "SELECT EAN FROM Productos WHERE Id = @ProductoId";
                    MySqlCommand cmdCheckProducto = new MySqlCommand(checkProducto, conexion);
                    cmdCheckProducto.Transaction = transaction;
                    cmdCheckProducto.Parameters.AddWithValue("@ProductoId", producto.Id);
                    var ean = cmdCheckProducto.ExecuteScalar()?.ToString();

                    if (string.IsNullOrEmpty(ean))
                    {
                        throw new Exception($"No se encontró el EAN para el producto {producto.NombreProducto}");
                    }

                    // Actualizar stock
                    string updateStock = @"
                UPDATE Productos 
                SET Stock = Stock + @CantidadRecibida 
                WHERE Id = @ProductoId";

                    MySqlCommand cmdStock = new MySqlCommand(updateStock, conexion);
                    cmdStock.Transaction = transaction;
                    cmdStock.Parameters.AddWithValue("@CantidadRecibida", producto.CantidadRecibida);
                    cmdStock.Parameters.AddWithValue("@ProductoId", producto.Id);

                    int filasAfectadas = cmdStock.ExecuteNonQuery();
                    if (filasAfectadas == 0)
                    {
                        throw new Exception($"No se pudo actualizar el stock del producto {producto.NombreProducto}");
                    }

                    // Registrar movimiento (usando el EAN del producto)
                    string insertMovimiento = @"
                INSERT INTO Movimientos (ProductoId, TipoMovimiento, Cantidad) 
                VALUES (@ProductoId, 'Compra', @Cantidad)";

                    MySqlCommand cmdMovimiento = new MySqlCommand(insertMovimiento, conexion);
                    cmdMovimiento.Transaction = transaction;
                    cmdMovimiento.Parameters.AddWithValue("@ProductoId", ean); // Usamos el EAN
                    cmdMovimiento.Parameters.AddWithValue("@Cantidad", producto.CantidadRecibida);
                    cmdMovimiento.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error al procesar el producto {producto.NombreProducto}: {ex.Message}");
                }
            }
        }

        private void BtnRecepcionar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar productos
                if (!ValidarProductos(out string mensajeValidacion))
                {
                    MessageBox.Show(mensajeValidacion, "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Mostrar resumen y confirmar
                if (!MostrarResumenDiferencias())
                {
                    return;
                }

                // Proceder con la recepción
                using (Conexion conexion = new Conexion())
                {
                    try
                    {
                        conexion.AbrirConexion();
                        using (MySqlTransaction transaction = conexion.ObtenerConexion().BeginTransaction())
                        {
                            try
                            {
                                ActualizarOrdenCompra(conexion.ObtenerConexion(), transaction);
                                ActualizarStockYMovimientos(conexion.ObtenerConexion(), transaction);

                                transaction.Commit();
                                MessageBox.Show("Mercancía recepcionada con éxito.\nSe ha actualizado el stock y registrado los movimientos.",
                                              "Operación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
                                DialogResult = true;
                                Close();
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                throw new Exception($"Error en la transacción: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error durante la recepción:\n{ex.Message}",
                                      "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error general:\n{ex.Message}",
                              "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
