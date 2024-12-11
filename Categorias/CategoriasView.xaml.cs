using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using MySqlConnector;
using System.Collections.ObjectModel;
using TFG.Categorias;
using OfficeOpenXml;

namespace TFG
{
    public partial class CategoriasView : UserControl
    {
        private string currentSort = "";
        private bool isAscending = true;
        private ICollectionView view;
        public ObservableCollection<Categoria> Categorias { get; set; }

        public CategoriasView()
        {
            InitializeComponent();
            Categorias = new ObservableCollection<Categoria>();
            CargarCategorias(); // Cargar las categorías al inicializar el UserControl
            CategoriasListView.ItemsSource = Categorias;
        }

        private void CargarCategorias()
        {
            var categorias = ObtenerCategorias();
            // Ordenar las categorías por ID antes de agregarlas
            var categoriasOrdenadas = categorias.OrderBy(c => c.Id).ToList();
            foreach (var categoria in categoriasOrdenadas)
            {
                Categorias.Add(categoria); // Agregar a la ObservableCollection
            }
        }

        private List<Categoria> ObtenerCategorias()
        {
            List<Categoria> categorias = new List<Categoria>();

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string query = "SELECT id, nombre FROM categorias"; // Asegúrate de que estos nombres coincidan con tu base de datos
                using (MySqlCommand cmd = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categorias.Add(new Categoria
                            {
                                Id = reader.GetInt32("id"),
                                Nombre = reader.GetString("nombre")
                            });
                        }
                    }
                }
                conexion.CerrarConexion();
            }

            return categorias;
        }

        private void Header_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock header)
            {
                string propertyName = header.Tag.ToString();

                if (currentSort == propertyName)
                {
                    isAscending = !isAscending;
                }
                else
                {
                    currentSort = propertyName;
                    isAscending = true;
                }

                view = CollectionViewSource.GetDefaultView(CategoriasListView.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(propertyName, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));

                var gridView = CategoriasListView.View as GridView;
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

        private void CategoriasListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (CategoriasListView.SelectedItem is Categoria categoria)
            {
                DetallesCategoriaView detallesView = new DetallesCategoriaView
                {
                    DataContext = categoria // Asignar el contexto de datos a la categoría seleccionada
                };
                detallesView.ShowDialog(); // Mostrar la ventana como un diálogo
            }
        }

        private void AgregarCategoria_Click(object sender, RoutedEventArgs e)
        {
            AgregarCategoriaControl agregarCategoriaControl = new AgregarCategoriaControl();
            Window mainWindow = Window.GetWindow(this);
            if (mainWindow is Principal principal)
            {
                principal.ContenidoPrincipal.Content = agregarCategoriaControl;
            }
            else
            {
                MessageBox.Show("No se pudo obtener la ventana principal.");
            }
        }
        private void EliminarCategoria_Click(object sender, RoutedEventArgs e)
        {
            if (CategoriasListView.SelectedItem is Categoria categoria)
            {
                MessageBoxResult resultado = MessageBox.Show($"¿Estás seguro de que deseas eliminar la categoría '{categoria.Nombre}'?", "Confirmar Eliminación", MessageBoxButton.YesNo);

                if (resultado == MessageBoxResult.Yes)
                {
                    // Obtener productos asociados a la categoría
                    List<Producto> productosAsociados = ObtenerProductosPorCategoria(categoria.Id);

                    using (var conexion = new Conexion())
                    {
                        conexion.AbrirConexion();

                        if (productosAsociados.Count > 0)
                        {
                            // Construir mensaje con los nombres de los productos
                            string nombresProductos = string.Join(", ", productosAsociados.Select(p => p.Nombre));
                            MessageBoxResult eliminarProductos = MessageBox.Show($"La categoría '{categoria.Nombre}' tiene los siguientes productos asociados: {nombresProductos}.\n¿Deseas eliminar también estos productos?", "Eliminar Productos", MessageBoxButton.YesNo);

                            // Si el usuario desea eliminar los productos, ejecutar la eliminación
                            if (eliminarProductos == MessageBoxResult.Yes)
                            {
                                string queryEliminarProductos = "DELETE FROM productos WHERE CategoriaID = @categoriaId";
                                using (MySqlCommand cmd = new MySqlCommand(queryEliminarProductos, conexion.ObtenerConexion()))
                                {
                                    cmd.Parameters.AddWithValue("@categoriaId", categoria.Id);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                            else
                            {
                                // Si el usuario no desea eliminar los productos, actualizar la categoría a NULL
                                string queryActualizarProductos = "UPDATE productos SET CategoriaID = NULL WHERE CategoriaID = @categoriaId";
                                using (MySqlCommand cmd = new MySqlCommand(queryActualizarProductos, conexion.ObtenerConexion()))
                                {
                                    cmd.Parameters.AddWithValue("@categoriaId", categoria.Id);
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }

                        // Eliminar la categoría después de manejar los productos
                        string queryEliminarCategoria = "DELETE FROM categorias WHERE id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(queryEliminarCategoria, conexion.ObtenerConexion()))
                        {
                            cmd.Parameters.AddWithValue("@id", categoria.Id);
                            cmd.ExecuteNonQuery();
                        }

                        conexion.CerrarConexion();
                    }

                    // Eliminar de la ObservableCollection
                    Categorias.Remove(categoria);
                }
            }
            else
            {
                MessageBox.Show("No se ha seleccionado ninguna categoría.");
            }
        }


        private List<Producto> ObtenerProductosPorCategoria(int categoriaId)
        {
            List<Producto> productos = new List<Producto>();

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string query = "SELECT id, nombre FROM productos WHERE CategoriaID = @categoriaId";
                using (MySqlCommand cmd = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    cmd.Parameters.AddWithValue("@categoriaId", categoriaId);
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            productos.Add(new Producto
                            {
                                Id = reader.GetInt32("id"),
                                Nombre = reader.GetString("nombre")
                            });
                        }
                    }
                }
                conexion.CerrarConexion();
            }

            return productos;
        }

        private void Exportar_Click(object sender, RoutedEventArgs e)
        {
            // Crear un nuevo archivo Excel
            using (ExcelPackage excel = new ExcelPackage())
            {
                // Crear una hoja de trabajo
                var worksheet = excel.Workbook.Worksheets.Add("Categorias");

                // Agregar encabezados
                worksheet.Cells[1, 1].Value = "ID";
                worksheet.Cells[1, 2].Value = "Nombre";

                // Llenar la hoja de trabajo con los datos del ListView
                int row = 2; // Comenzar en la segunda fila
                foreach (var categoria in Categorias)
                {
                    worksheet.Cells[row, 1].Value = categoria.Id;
                    worksheet.Cells[row, 2].Value = categoria.Nombre;
                    row++;
                }

                // Guardar el archivo en el escritorio
                string filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "Categorias.xlsx");
                System.IO.File.WriteAllBytes(filePath, excel.GetAsByteArray());

                // Abrir el archivo en Excel
                System.Diagnostics.Process.Start(filePath);
            }
        }

    }
}
