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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TFG.ControlDeStock
{
    public partial class ModificarMaximasyMinimas : UserControl
    {
        public ModificarMaximasyMinimas()
        {
            InitializeComponent();
        }

        public void SetDataContext(Producto producto)
        {
            DataContext = producto;
        }

        private void GuardarCambiosButton_Click(object sender, RoutedEventArgs e)
        {
            // Obtener el contexto de datos actual
            var producto = DataContext as Producto;

            if (producto != null)
            {
                using (Conexion conexion = new Conexion())
                {
                    // Abrir conexión
                    conexion.AbrirConexion();
                    string query = @"
                UPDATE productos 
                SET CantidadMaxima = @CantidadMaxima, 
                    CantidadMinima = @CantidadMinima 
                WHERE EAN = @EAN";

                    using (MySqlCommand command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        // Asignar parámetros
                        command.Parameters.AddWithValue("@CantidadMaxima", producto.CantidadMaxima);
                        command.Parameters.AddWithValue("@CantidadMinima", producto.CantidadMinima);
                        command.Parameters.AddWithValue("@EAN", producto.EAN);

                        try
                        {
                            // Ejecutar el comando
                            int rowsAffected = command.ExecuteNonQuery();
                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Cambios guardados exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show("No se encontró el producto para actualizar.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al guardar cambios: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Error al obtener los datos del producto.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelarCambiosButton_Click(object sender, EventArgs e)
        {
            // Cambiar el contenido del ContentControl a la vista de Control de Stock
            var controlStockView = new ControlDeStock();
            // Asigna la nueva vista al ContentControl
            var parentWindow = Window.GetWindow(this) as Principal; 
            if (parentWindow != null)
            {
                parentWindow.ContenidoPrincipal.Content = controlStockView;
            }
        }
    }
}
