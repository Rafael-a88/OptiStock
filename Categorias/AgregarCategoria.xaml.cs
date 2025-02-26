using MySqlConnector;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace TFG.Categorias
{
    /// <summary>
    /// Lógica de interacción para AgregarCategoria.xaml
    /// </summary>
    public partial class AgregarCategoriaControl : UserControl
    {
        public AgregarCategoriaControl()
        {
            InitializeComponent();
        }


        private void AgregarCategoria_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el nombre de la categoría del TextBox
            string nombreCategoria = NombreCategoriaTextBox.Text.Trim();

            // Validar que el nombre de la categoría no esté vacío
            if (string.IsNullOrEmpty(nombreCategoria))
            {
                MessageBox.Show("Debe ingresar un nombre de categoría.");
                return;
            }

            using (Conexion conexion = new Conexion())
            {
                try
                {
                    // Abrir la conexión
                    conexion.AbrirConexion();

                    // Obtener todos los IDs ocupados
                    string query = "SELECT Id FROM categorias ORDER BY Id";
                    MySqlCommand command = new MySqlCommand(query, conexion.ObtenerConexion());
                    MySqlDataReader reader = command.ExecuteReader();

                    List<int> ocupados = new List<int>();
                    while (reader.Read())
                    {
                        ocupados.Add(reader.GetInt32("Id"));
                    }
                    reader.Close();

                    // Encontrar el primer ID libre
                    int nuevoId = 1; // Comenzar desde 1
                    while (ocupados.Contains(nuevoId))
                    {
                        nuevoId++;
                    }

                    // Crear el comando SQL para insertar la categoría con el nuevo ID
                    string insertQuery = "INSERT INTO categorias (Id, nombre) VALUES (@id, @nombre)";
                    MySqlCommand insertCommand = new MySqlCommand(insertQuery, conexion.ObtenerConexion());
                    insertCommand.Parameters.AddWithValue("@id", nuevoId);
                    insertCommand.Parameters.AddWithValue("@nombre", nombreCategoria);

                    // Ejecutar el comando SQL
                    int rowsAffected = insertCommand.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        MessageBox.Show($"La categoría '{nombreCategoria}' se ha agregado exitosamente con ID {nuevoId}.");
                        // Limpiar el TextBox
                        NombreCategoriaTextBox.Text = string.Empty;
                    }
                    else
                    {
                        MessageBox.Show("No se pudo agregar la categoría. Intente de nuevo.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al agregar la categoría: {ex.Message}");
                }
            }
        }


        private void CancelarCategoria_Click(object sender, RoutedEventArgs e)
        {
            // Cambiar el contenido del ContentControl a la vista de categorías
            var categoriasView = new CategoriasView(); 
                                                     
            var parentWindow = Window.GetWindow(this) as Principal; 
            if (parentWindow != null)
            {
                parentWindow.ContenidoPrincipal.Content = categoriasView;
            }
        }


    }
}
