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

                // Forzar la conversión a string para la categoría
                if (propertyName == "CategoriaNombre")
                {
                    view.SortDescriptions.Add(new SortDescription(propertyName, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                }
                else
                {
                    view.SortDescriptions.Add(new SortDescription(propertyName, isAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
                }

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

        private void AgregarCategoria_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para agregar categoría
            MessageBox.Show("Agregar categoría no implementado.");
        }

        private void ModificarCategoria_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para modificar categoría
            MessageBox.Show("Modificar categoría no implementado.");
        }

        private void EliminarCategoria_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para eliminar categoría
            MessageBox.Show("Eliminar categoría no implementado.");
        }
    }
}
