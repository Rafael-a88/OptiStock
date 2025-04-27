using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace TFG.GestionDeProveedores
{
    public partial class AgregarProveedor : UserControl
    {
        public AgregarProveedor()
        {
            InitializeComponent();
        }

        private void AñadirProveedor_Click(object sender, RoutedEventArgs e)
        {
            // Validar campos obligatorios
            if (!ValidarCampos())
            {
                return;
            }

            try
            {
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();

                    string query = @"INSERT INTO Proveedores 
                        (Nombre, Contacto, Telefono, Email, Direccion, Ciudad, 
                        Provincia, CodigoPostal, Pais, TipoProveedor, Notas, SitioWeb) 
                        VALUES 
                        (@Nombre, @Contacto, @Telefono, @Email, @Direccion, @Ciudad, 
                        @Provincia, @CodigoPostal, @Pais, @TipoProveedor, @Notas, @SitioWeb)";

                    using (var comando = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        // Añadir parámetros
                        comando.Parameters.AddWithValue("@Nombre", NombreProveedorTextBox.Text);
                        comando.Parameters.AddWithValue("@Contacto", ContactoTextBox.Text);
                        comando.Parameters.AddWithValue("@Telefono", TelefonoTextBox.Text);
                        comando.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                        comando.Parameters.AddWithValue("@Direccion", DireccionTextBox.Text);
                        comando.Parameters.AddWithValue("@Ciudad", CiudadTextBox.Text);
                        comando.Parameters.AddWithValue("@Provincia", ProvinciaTextBox.Text);
                        comando.Parameters.AddWithValue("@CodigoPostal", CodigoPostalTextBox.Text);
                        comando.Parameters.AddWithValue("@Pais", PaisTextBox.Text);
                        comando.Parameters.AddWithValue("@TipoProveedor", TipoProveedorTextBox.Text);
                        comando.Parameters.AddWithValue("@Notas", NotasTextBox.Text);
                        comando.Parameters.AddWithValue("@SitioWeb", SitioWebTextBox.Text);

                        // Ejecutar la inserción
                        int resultado = comando.ExecuteNonQuery();

                        if (resultado > 0)
                        {
                            MessageBox.Show("Proveedor añadido exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            LimpiarCampos();
                        }
                        else
                        {
                            MessageBox.Show("No se pudo añadir el proveedor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                // Manejo de errores específicos de MySQL
                if (ex.Number == 1062) // Código de error para entrada duplicada
                {
                    MessageBox.Show("Ya existe un proveedor con este nombre.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"Error al añadir proveedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error inesperado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool ValidarCampos()
        {
            // Validar nombre (obligatorio)
            if (string.IsNullOrWhiteSpace(NombreProveedorTextBox.Text))
            {
                MessageBox.Show("El nombre del proveedor es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Validar contacto (obligatorio)
            if (string.IsNullOrWhiteSpace(ContactoTextBox.Text))
            {
                MessageBox.Show("El contacto es obligatorio.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Validar teléfono (opcional, pero debe tener formato si se proporciona)
            if (!string.IsNullOrWhiteSpace(TelefonoTextBox.Text) && !EsTelefonoValido(TelefonoTextBox.Text))
            {
                MessageBox.Show("El número de teléfono no es válido. Debe comenzar con '+' y contener solo números, con una longitud de entre 10 y 15 caracteres (sin el '+').", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Validar email (opcional, pero debe tener formato si se proporciona)
            if (!string.IsNullOrWhiteSpace(EmailTextBox.Text) && !EsEmailValido(EmailTextBox.Text))
            {
                MessageBox.Show("El formato del correo electrónico no es válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            // Validar código postal (opcional, pero debe tener formato si se proporciona)
            if (!string.IsNullOrWhiteSpace(CodigoPostalTextBox.Text) && !EsCodigoPostalValido(CodigoPostalTextBox.Text))
            {
                MessageBox.Show("El formato del código postal no es válido. Debe ser un código postal español de 5 dígitos.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }

            return true;
        }

        private bool EsTelefonoValido(string telefono)
        {
            // Validar que comience con '+' y contenga solo números, y tenga entre 10 y 15 caracteres (sin contar el '+')
            return Regex.IsMatch(telefono, @"^\+[0-9]{9,14}$");
        }

        private bool EsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool EsCodigoPostalValido(string codigoPostal)
        {
            // Ejemplo para validar un código postal español de 5 dígitos
            return Regex.IsMatch(codigoPostal, @"^\d{5}$");
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            // Volver a la ventana de gestión de proveedores
            ContentControl contenidoPrincipal = this.Parent as ContentControl;
            if (contenidoPrincipal != null)
            {
                GestionDeProveedores gestionProveedores = new GestionDeProveedores();
                contenidoPrincipal.Content = gestionProveedores;
            }
        }

        private void LimpiarCampos()
        {
            // Limpiar todos los TextBox
            NombreProveedorTextBox.Clear();
            ContactoTextBox.Clear();
            TelefonoTextBox.Clear();
            EmailTextBox.Clear();
            DireccionTextBox.Clear();
            CiudadTextBox.Clear();
            ProvinciaTextBox.Clear();
            CodigoPostalTextBox.Clear();
            PaisTextBox.Clear();
            TipoProveedorTextBox.Clear();
            NotasTextBox.Clear();
            SitioWebTextBox.Clear();
        }
    }
}
