using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MySqlConnector;

namespace TFG.Ubicaciones
{
    public partial class AgregarUbicacionView : UserControl
    {
        public AgregarUbicacionView()
        {
            InitializeComponent();

            // Configurar eventos para actualizar la ubicación total
            AlmacenTextBox.TextChanged += ActualizarUbicacionTotal;
            CalleTextBox.TextChanged += ActualizarUbicacionTotal;
            LadoTextBox.TextChanged += ActualizarUbicacionTotal;
            ModuloTextBox.TextChanged += ActualizarUbicacionTotal;
            AlturaTextBox.TextChanged += ActualizarUbicacionTotal;
        }

        private void NumeroTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Permitir números
            Regex regex = new Regex("[^0-9]+");
            e.Handled = !regex.IsMatch(e.Text);
        }

        private void ActualizarUbicacionTotal(object sender, EventArgs e)
        {
            // Obtener valores de los campos
            string almacen = AlmacenTextBox.Text;
            string calle = CalleTextBox.Text;
            string lado = LadoTextBox.Text;
            string modulo = ModuloTextBox.Text;
            string altura = AlturaTextBox.Text;

            // Construir la ubicación total
            if (!string.IsNullOrEmpty(almacen) && !string.IsNullOrEmpty(calle) &&
                !string.IsNullOrEmpty(lado) && !string.IsNullOrEmpty(modulo) &&
                !string.IsNullOrEmpty(altura))
            {
                // Formato: Almacén-Calle-Lado-Módulo-Altura
                UbicacionTotalTextBox.Text = $"{almacen}{calle}{lado}{modulo}{altura}";
            }
            else
            {
                UbicacionTotalTextBox.Text = string.Empty;
            }
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            // Validar que todos los campos estén completos
            if (string.IsNullOrWhiteSpace(AlmacenTextBox.Text) || string.IsNullOrWhiteSpace(CalleTextBox.Text) ||
                string.IsNullOrWhiteSpace(LadoTextBox.Text) || string.IsNullOrWhiteSpace(ModuloTextBox.Text) ||
                string.IsNullOrWhiteSpace(AlturaTextBox.Text))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Obtener los valores
                string ubicacionTotal = UbicacionTotalTextBox.Text;

                // Guardar en la base de datos
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    string query = "INSERT INTO ubicaciones (Nombre) VALUES (@Nombre)";

                    var command = new MySqlCommand(query, conexion.ObtenerConexion());
                    command.Parameters.AddWithValue("@Nombre", ubicacionTotal);

                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Ubicación guardada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Limpiar los campos después de guardar
                LimpiarCampos();

                // Notificar al padre que se ha agregado una ubicación (opcional)
                // Si tienes un evento para esto, puedes dispararlo aquí
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar la ubicación: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            // Cambiar el contenido del ContentControl a la vista de Ubicaciones
            var ubicaciones = new Ubicaciones();
            // Asigna la nueva vista al ContentControl
            var parentWindow = Window.GetWindow(this) as Principal;
            if (parentWindow != null)
            {
                parentWindow.ContenidoPrincipal.Content = ubicaciones;
            }
        }

        private void LimpiarCampos()
        {
            AlmacenTextBox.Text = string.Empty;
            CalleTextBox.Text = string.Empty;
            LadoTextBox.Text = string.Empty;
            ModuloTextBox.Text = string.Empty;
            AlturaTextBox.Text = string.Empty;
            UbicacionTotalTextBox.Text = string.Empty;
        }
    }
}
