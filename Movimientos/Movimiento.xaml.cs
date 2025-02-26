using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TFG.Movimientos;

namespace TFG
{
    public partial class Movimiento : UserControl
    {
        public Movimiento()
        {
            InitializeComponent();
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            CargarMovimientos();
        }

        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            // Limpia el campo de búsqueda y la lista de movimientos
            BuscarTextBox.Text = string.Empty; // Limpia el campo de búsqueda
            MovimientosListView.ItemsSource = null; // Limpia la lista de movimientos
        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Limpia el texto del campo de búsqueda cuando tiene el foco
            if (BuscarTextBox.Text == "Introduce el codigo EAN del producto")
            {
                BuscarTextBox.Text = string.Empty;
                BuscarTextBox.Foreground = System.Windows.Media.Brushes.Black; // Cambia el color del texto
            }
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Restaura el texto de marcador si el campo está vacío
            if (string.IsNullOrWhiteSpace(BuscarTextBox.Text))
            {
                BuscarTextBox.Text = "Introduce el codigo EAN del producto";
                BuscarTextBox.Foreground = System.Windows.Media.Brushes.Gray; // Cambia el color del texto a gris
            }
        }

        private void CargarMovimientos()
        {
            string productoEan = BuscarTextBox.Text.Trim();

            if (string.IsNullOrEmpty(productoEan) || productoEan == "Introduce el producto por ID, Nombre, Marca o Categoria")
            {
                MessageBox.Show("Por favor, introduce un EAN para buscar.");
                return;
            }

            List<MovimientoDTO> movimientos = ObtenerMovimientos(productoEan);

            if (movimientos != null && movimientos.Count > 0)
            {
                MovimientosListView.ItemsSource = movimientos;
            }
            else
            {
                MessageBox.Show("No se encontraron movimientos para el EAN proporcionado.");
            }
        }

        private List<MovimientoDTO> ObtenerMovimientos(string productoEan)
        {
            List<MovimientoDTO> movimientos = new List<MovimientoDTO>();
            int stockInicial = 0; // Variable para almacenar el stock inicial

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion(); // Asegúrate de abrir la conexión

                // Consulta para obtener el stock inicial
                string stockQuery = "SELECT StockInicial FROM Productos WHERE EAN = @ProductoEan";
                using (var command = new MySqlCommand(stockQuery, conexion.ObtenerConexion()))
                {
                    command.Parameters.AddWithValue("@ProductoEan", productoEan);
                    stockInicial = Convert.ToInt32(command.ExecuteScalar());
                }

                // Consulta para obtener los movimientos
                string query = @"
                    SELECT 
                        ProductoId AS ProductoEan, 
                        FechaMovimiento, 
                        TipoMovimiento, 
                        Cantidad 
                    FROM 
                        Movimientos
                    WHERE 
                        ProductoId = @ProductoEan

                    UNION ALL

                    SELECT 
                        ProductoEAN, 
                        FechaMovimiento, 
                        TipoMovimiento, 
                        Cantidad 
                    FROM 
                        MovimientosWeb
                    WHERE 
                        ProductoEAN = @ProductoEan
                    ORDER BY FechaMovimiento;"; // Asegúrate de ordenar por fecha

                using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    command.Parameters.AddWithValue("@ProductoEan", productoEan);

                    using (var reader = command.ExecuteReader())
                    {
                        int stockActual = stockInicial; // Inicializa el stock actual con el stock inicial

                        while (reader.Read())
                        {
                            MovimientoDTO movimiento = new MovimientoDTO
                            {
                                ProductoEan = reader.GetString(0), // Código EAN del producto
                                FechaMovimiento = reader.GetDateTime(1), // Fecha del movimiento
                                TipoMovimiento = reader.GetString(2), // Tipo de movimiento
                                Cantidad = reader.GetInt32(3) // Cantidad del producto
                            };

                            // Calcular el stock actual en función del tipo de movimiento
                            if (movimiento.TipoMovimiento == "Venta Física" || movimiento.TipoMovimiento == "Venta Web")
                            {
                                stockActual -= movimiento.Cantidad; // Resta la cantidad para ventas
                            }
                            else if (movimiento.TipoMovimiento == "Compra") // Asegúrate de manejar otros tipos de movimientos
                            {
                                stockActual += movimiento.Cantidad; // Suma la cantidad para ingresos
                            }

                            movimiento.StockActual = stockActual; // Asigna el stock actual al movimiento
                            movimientos.Add(movimiento);
                        }
                    }
                }
            }

            return movimientos;
        }
    }
}
