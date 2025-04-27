using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using MySqlConnector;

using TFG.Nominas;

namespace TFG.Trabajadores
{
    public partial class Trabajadores : UserControl
    {
        private List<TFG.Nominas.Trabajadores> trabajadores;
        private string sortColumn = "Id";
        private bool sortAscending = true;
        private Window _mainWindow;

        public Trabajadores(Window mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;
            CargarTrabajadores();
            BuscarTextBox.Text = "Introduce el trabajador por ID, Nombre, DNI, Categoría, Departamento o Usuario";
        }

        private void BuscarTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (BuscarTextBox.Text == "Introduce el trabajador por ID, Nombre, DNI, Categoría, Departamento o Usuario")
            {
                BuscarTextBox.Text = "";
                BuscarTextBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void BuscarTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(BuscarTextBox.Text))
            {
                BuscarTextBox.Text = "Introduce el trabajador por ID, Nombre, DNI, Categoría, Departamento o Usuario";
                BuscarTextBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        private void Header_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = sender as TextBlock;
            if (headerClicked != null && headerClicked.Tag != null)
            {
                if (sortColumn == headerClicked.Tag.ToString())
                {
                    sortAscending = !sortAscending;
                }
                else
                {
                    sortColumn = headerClicked.Tag.ToString();
                    sortAscending = true;
                }

                var view = CollectionViewSource.GetDefaultView(TrabajadoresListView.ItemsSource);
                view.SortDescriptions.Clear();
                view.SortDescriptions.Add(new SortDescription(sortColumn,
                    sortAscending ? ListSortDirection.Ascending : ListSortDirection.Descending));
            }
        }

        private void EliminarTrabajadorButton_Click(object sender, RoutedEventArgs e)
        {
            var trabajadorSeleccionado = TrabajadoresListView.SelectedItem as TFG.Nominas.Trabajadores;
            if (trabajadorSeleccionado != null)
            {
                var resultado = MessageBox.Show(
                    $"¿Está seguro que desea eliminar al trabajador {trabajadorSeleccionado.NombreCompleto}?",
                    "Confirmar eliminación",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (resultado == MessageBoxResult.Yes)
                {
                    // Implementar la lógica de eliminación aquí
                    EliminarTrabajador(trabajadorSeleccionado.Id);
                    CargarTrabajadores();
                }
            }
            else
            {
                MessageBox.Show("Por favor, seleccione un trabajador para eliminar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void EliminarTrabajador(int id)
        {
            try
            {
                using (Conexion conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    string query = "DELETE FROM Trabajadores WHERE Id = @Id";
                    MySqlCommand cmd = new MySqlCommand(query, conexion.ObtenerConexion());
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al eliminar el trabajador: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = "Trabajadores",
                    DefaultExt = ".csv",
                    Filter = "Archivos CSV (.csv)|*.csv"
                };

                if (dlg.ShowDialog() == true)
                {
                    using (StreamWriter sw = new StreamWriter(dlg.FileName, false, Encoding.UTF8))
                    {
                        // Escribir encabezados
                        sw.WriteLine("ID,Nombre,DNI,Teléfono,Salario,Categoría,Departamento,Fecha Contratación,Número SS,Usuario,Contraseña");

                        // Escribir datos
                        foreach (var t in trabajadores)
                        {
                            sw.WriteLine($"{t.Id},{t.NombreCompleto},{t.DNI},{t.Telefono}," +
                                       $"{t.Salario},{t.CategoriaProfesional},{t.Departamento},{t.FechaContratacion:dd/MM/yyyy}," +
                                       $"{t.NumeroSeguridadSocial},{t.Usuario},{t.Contraseña}");
                        }
                    }
                    MessageBox.Show("Exportación completada con éxito.", "Éxito",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarTrabajadores()
        {
            // Obtener la lista de trabajadores desde la base de datos
            trabajadores = ObtenerTrabajadoresDesdeBaseDeDatos();

            // Asignar la lista de trabajadores a la ListView
            TrabajadoresListView.ItemsSource = trabajadores;
        }

        private List<TFG.Nominas.Trabajadores> ObtenerTrabajadoresDesdeBaseDeDatos()
        {
            List<TFG.Nominas.Trabajadores> trabajadores = new List<TFG.Nominas.Trabajadores>();

            using (Conexion conexion = new Conexion())
            {
                conexion.AbrirConexion();
                MySqlCommand command = new MySqlCommand("SELECT * FROM Trabajadores", conexion.ObtenerConexion());
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    TFG.Nominas.Trabajadores trabajador = new TFG.Nominas.Trabajadores
                    {
                        Id = reader.GetInt32("Id"),
                        NombreCompleto = reader.GetString("NombreCompleto"),
                        FechaNacimiento = reader.GetDateTime("FechaNacimiento"),
                        DNI = reader.GetString("DNI"),
                        Telefono = reader.GetString("Telefono"),
                        Email = reader.GetString("Email"),
                        Direccion = reader.GetString("Direccion"),
                        FechaContratacion = reader.GetDateTime("FechaContratacion"),
                        Salario = reader.GetDecimal("Salario"),
                        Usuario = reader.GetString("Usuario"),
                        Contraseña = reader.GetString("Contraseña"),
                        NumeroSeguridadSocial = reader.GetString("NumeroSeguridadSocial"),
                        CategoriaProfesional = reader.GetInt32("CategoriaProfesional"),
                        Departamento = reader.GetString("Departamento")
                    };

                    trabajadores.Add(trabajador);
                }
            }

            return trabajadores;
        }

        private void TrabajadoresListView_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var trabajadorSeleccionado = TrabajadoresListView.SelectedItem as TFG.Nominas.Trabajadores;
            if (trabajadorSeleccionado != null)
            {
                ModificarTrabajador(trabajadorSeleccionado);
            }
        }

        private void ModificarTrabajador(TFG.Nominas.Trabajadores trabajador)
        {
            // Implementar la lógica de modificación aquí
        }

        private void BuscarButton_Click(object sender, RoutedEventArgs e)
        {
            // Filtrar la lista de trabajadores según el texto de búsqueda
            string busqueda = BuscarTextBox.Text.ToLower();
            var trabajadoresFiltrados = trabajadores.Where(t =>
                t.Id.ToString().Contains(busqueda) ||
                t.NombreCompleto.ToLower().Contains(busqueda) ||
                t.DNI.ToLower().Contains(busqueda) ||
                t.CategoriaProfesional.ToString().Contains(busqueda) ||
                t.Departamento.ToLower().Contains(busqueda) ||
                t.Usuario.ToLower().Contains(busqueda)
            ).ToList();

            // Asignar la lista filtrada a la ListView
            TrabajadoresListView.ItemsSource = trabajadoresFiltrados;
        }

        private void RefrescarButton_Click(object sender, RoutedEventArgs e)
        {
            // Volver a cargar la lista completa de trabajadores
            CargarTrabajadores();
        }

        private void AgregarTrabajadorButton_Click(object sender, RoutedEventArgs e)
        {
            AgregarTrabajadores agregarTrabajadoresView = new AgregarTrabajadores();
            ContentControl contenidoPrincipal = this.Parent as ContentControl;

            if (contenidoPrincipal != null)
            {
                this.Visibility = Visibility.Collapsed;
                contenidoPrincipal.Content = agregarTrabajadoresView;
            }
        }

        private void ModificarTrabajadorButton_Click(object sender, RoutedEventArgs e)
        {
            // Verificar si hay un trabajador seleccionado en la lista
            if (TrabajadoresListView.SelectedItem is TFG.Nominas.Trabajadores trabajadorSeleccionado) // Mantener como Trabajadores
            {
                // Crear una nueva instancia de ModificarTrabajadores pasando el ID del trabajador seleccionado
                ModificarTrabajadores modificarTrabajadorView = new ModificarTrabajadores(trabajadorSeleccionado.Id);

                // Cambiar el contenido del control principal a la vista de modificación
                ContentControl contenidoPrincipal = this.Parent as ContentControl;
                if (contenidoPrincipal != null)
                {
                    contenidoPrincipal.Content = modificarTrabajadorView;
                }
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un trabajador para modificar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }


    }
}
