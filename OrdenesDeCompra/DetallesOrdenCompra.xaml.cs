using MySqlConnector;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;

namespace TFG.OrdenesDeCompra
{
    public partial class DetallesOrdenCompra : Window, INotifyPropertyChanged
    {
        private string _numeroOrden;
        private List<ProductoOrden> _productosOrden;
        private decimal _totalOrden;
        private int _ordenId;

        public event PropertyChangedEventHandler PropertyChanged;

        public string NumeroOrden
        {
            get => _numeroOrden;
            set
            {
                _numeroOrden = value;
                OnPropertyChanged(nameof(NumeroOrden));
            }
        }

        public List<ProductoOrden> ProductosOrden
        {
            get => _productosOrden;
            set
            {
                _productosOrden = value;
                OnPropertyChanged(nameof(ProductosOrden));
                RecalcularTotal(); // Recalcular total al establecer productos
            }
        }

        public decimal TotalOrden
        {
            get => _totalOrden;
            set
            {
                _totalOrden = value;
                OnPropertyChanged(nameof(TotalOrden));
            }
        }

        public DetallesOrdenCompra(int ordenId, string numeroOrden)
        {
            InitializeComponent();
            DataContext = this;
            _ordenId = ordenId;
            NumeroOrden = numeroOrden;
            CargarProductosOrden();
        }

        private void CargarProductosOrden()
        {
            ProductosOrden = new List<ProductoOrden>();

            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    string query = @"SELECT d.*, p.Nombre as NombreProducto 
                    FROM DetallesOrdenDeCompra d 
                    INNER JOIN productos p ON d.ProductoId = p.Id 
                    WHERE d.OrdenDeCompraId = @OrdenId";

                    MySqlCommand cmd = new MySqlCommand(query, conexion.ObtenerConexion());
                    cmd.Parameters.AddWithValue("@OrdenId", _ordenId);

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var producto = new ProductoOrden
                            {
                                Id = reader.GetInt32("Id"),
                                NombreProducto = reader.GetString("NombreProducto"),
                                EAN = reader.GetString("EAN"),
                                Cantidad = reader.GetInt32("Cantidad"),
                                PrecioUnitario = reader.GetDecimal("PrecioUnitario")
                            };

                            // Suscribirse a los cambios del producto
                            producto.PropertyChanged += Producto_PropertyChanged;
                            ProductosOrden.Add(producto);
                        }
                    }

                    RecalcularTotal(); // Calcular el total inicial
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar los productos: {ex.Message}");
                }
            }
        }

        private void Producto_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ProductoOrden.PrecioSubtotal))
            {
                RecalcularTotal(); // Recalcular el total cuando cambie el subtotal de un producto
            }
        }


    


        private void RecalcularTotal()
        {
            TotalOrden = ProductosOrden?.Sum(p => p.PrecioSubtotal) ?? 0; // Sumar todos los subtotales
        }


        private void ActualizarDetallesOrdenEnBaseDeDatos()
        {
            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();

                    foreach (var producto in ProductosOrden)
                    {
                        string query = @"
                    UPDATE DetallesOrdenDeCompra 
                    SET Cantidad = @Cantidad, 
                        Subtotal = @Subtotal, 
                        TotalOrden = @TotalOrden 
                    WHERE Id = @Id";

                        MySqlCommand cmd = new MySqlCommand(query, conexion.ObtenerConexion());
                        cmd.Parameters.AddWithValue("@Cantidad", producto.Cantidad);
                        cmd.Parameters.AddWithValue("@Subtotal", producto.PrecioSubtotal);
                        cmd.Parameters.AddWithValue("@TotalOrden", TotalOrden); // Usar el TotalOrden de la ventana
                        cmd.Parameters.AddWithValue("@Id", producto.Id);

                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al actualizar los detalles de la orden: {ex.Message}");
                }
            }
        }




        private void ConfirmarOrden_Click(object sender, RoutedEventArgs e)
        {
            // Actualizar los detalles de la orden en la base de datos
            ActualizarDetallesOrdenEnBaseDeDatos();

            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion();
                    string query = "UPDATE OrdenesDeCompra SET Estado = 'Revisado' WHERE Id = @OrdenId";
                    MySqlCommand cmd = new MySqlCommand(query, conexion.ObtenerConexion());
                    cmd.Parameters.AddWithValue("@OrdenId", _ordenId);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Recepción confirmada correctamente");
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al confirmar la recepción: {ex.Message}");
                }
            }
        }



        private void Cerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
