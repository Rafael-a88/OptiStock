using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.SqlClient;
using System.Data;
using MySqlConnector;
using System.ComponentModel;
using System.Windows.Data;
using System.Collections.ObjectModel;
using TFG.SeguimientoPedidos;

namespace TFG.PreciosyPromociones
{
    public partial class PreciosyPromociones : UserControl
    {

       
        private string currentSort;
        private bool isAscending = true;
        public PreciosyPromociones()
        {
            InitializeComponent();
        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            // Código para manejar el evento GotFocus
            if (BuscarTextBox.Text == "Introduce el producto por ID, Nombre, Marca o Categoria")
            {
                BuscarTextBox.Text = "";
                BuscarTextBox.Foreground = Brushes.Black;
            }
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // Código para manejar el evento LostFocus
            if (string.IsNullOrWhiteSpace(BuscarTextBox.Text))
            {
                BuscarTextBox.Text = "Introduce el producto por ID, Nombre, Marca o Categoria";
                BuscarTextBox.Foreground = Brushes.Gray;
            }
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            string searchTerm = BuscarTextBox.Text.Trim();
            string query = @"
            SELECT p.*, c.Nombre AS CategoriaNombre
            FROM productos p
            JOIN categorias c ON p.CategoriaId = c.Id
            WHERE p.Id = @searchTerm OR p.Nombre LIKE @searchTerm OR p.Marca LIKE @searchTerm OR c.Nombre LIKE @searchTerm;";

            using (Conexion conexion = new Conexion())
            {
                try
                {
                    conexion.AbrirConexion(); // Abrir la conexión
                    using (MySqlCommand command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@searchTerm", "%" + searchTerm + "%");

                        // Ejecutar la consulta y llenar el DataTable
                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(command))
                        {
                            DataTable dataTable = new DataTable();
                            adapter.Fill(dataTable);
                            PromocionesListView.ItemsSource = dataTable.DefaultView; // Asignar el resultado al ListView
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
                finally
                {
                    conexion.CerrarConexion(); // Cerrar la conexión
                }
            }
        }




        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            BuscarButton_Click(sender, e);
        }

        private void AplicarDescuento_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el valor del TextBox
            string input = DescuentoTextBox.Text.Trim();

            // Validar si la entrada es un número
            if (decimal.TryParse(input, out decimal porcentajeDescuento))
            {
                // Verificar si hay un elemento seleccionado en el ListView
                if (PromocionesListView.SelectedItem is DataRowView selectedRow)
                {
                    // Obtener el ID y el nombre del producto seleccionado
                    int productoId = Convert.ToInt32(selectedRow["Id"]);
                    string nombreProducto = selectedRow["Nombre"].ToString();

                    // Obtener el precio original del producto
                    decimal precioOriginal = Convert.ToDecimal(selectedRow["Precio"]);
                    decimal descuentoAplicado = precioOriginal * (porcentajeDescuento / 100);
                    decimal nuevoPrecio = precioOriginal - descuentoAplicado;

                    // Actualizar el precio y el descuento en el DataTable
                    selectedRow["Precio"] = nuevoPrecio;

                    // Actualizar el campo Precio y Descuento en la base de datos
                    string query = "UPDATE productos SET Precio = @nuevoPrecio, Descuento = @descuento WHERE Id = @productoId;";

                    using (Conexion conexion = new Conexion())
                    {
                        try
                        {
                            conexion.AbrirConexion(); // Abrir la conexión
                            using (MySqlCommand command = new MySqlCommand(query, conexion.ObtenerConexion()))
                            {
                                command.Parameters.AddWithValue("@nuevoPrecio", nuevoPrecio);
                                command.Parameters.AddWithValue("@descuento", porcentajeDescuento);
                                command.Parameters.AddWithValue("@productoId", productoId);

                                command.ExecuteNonQuery(); // Ejecutar la actualización
                            }

                            // Mensaje de éxito
                            MessageBox.Show($"Descuento aplicado al producto: {nombreProducto}");
                        }
                        catch (Exception ex)
                        {
                            // Mensaje de error
                            MessageBox.Show($"Error al aplicar el descuento al producto: {nombreProducto}. Detalles: {ex.Message}");
                        }
                        finally
                        {
                            conexion.CerrarConexion(); // Cerrar la conexión
                        }
                    }

                    // Opcional: Actualizar el ListView para reflejar el cambio
                    PromocionesListView.Items.Refresh();
                }
                else
                {
                    MessageBox.Show("Por favor, seleccione un producto para aplicar el descuento.");
                }
            }
            else
            {
                // Mostrar un mensaje de error si la entrada no es válida
                MessageBox.Show("Por favor, introduzca un número válido.");
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
                var view = CollectionViewSource.GetDefaultView(PromocionesListView.ItemsSource);
                view.SortDescriptions.Clear(); // Limpiar descripciones de ordenamiento

                // Añadir la nueva descripción de ordenamiento
                view.SortDescriptions.Add(new SortDescription(propertyName, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));

                // Actualizar las cabeceras para mostrar la dirección del orden
                var gridView = PromocionesListView.View as GridView;
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


 

        

    }
}
