using MySqlConnector;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Text.RegularExpressions;

namespace TFG.Trabajadores
{
    public partial class AgregarTrabajadores : UserControl
    {
        public AgregarTrabajadores()
        {
            InitializeComponent();
        }

        private bool ValidarCampos()
        {
            if (string.IsNullOrWhiteSpace(NombreCompletoTextBox.Text))
            {
                MessageBox.Show("El nombre completo es obligatorio.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (FechaNacimientoDatePicker.SelectedDate == null)
            {
                MessageBox.Show("La fecha de nacimiento es obligatoria.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DniTextBox.Text) || !ValidarDNI(DniTextBox.Text))
            {
                MessageBox.Show("El DNI no es válido.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(TelefonoTextBox.Text) || !ValidarTelefono(TelefonoTextBox.Text))
            {
                MessageBox.Show("El teléfono no es válido.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailTextBox.Text) || !ValidarEmail(EmailTextBox.Text))
            {
                MessageBox.Show("El email no es válido.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(DireccionTextBox.Text))
            {
                MessageBox.Show("La dirección es obligatoria.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (FechaContratacionDatePicker.SelectedDate == null)
            {
                MessageBox.Show("La fecha de contratación es obligatoria.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (!decimal.TryParse(SalarioTextBox.Text, out _))
            {
                MessageBox.Show("El salario debe ser un número válido.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(UsuarioTextBox.Text))
            {
                MessageBox.Show("El usuario es obligatorio.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (ContrasenaPasswordBox.Password != ConfirmarContrasenaPasswordBox.Password)
            {
                MessageBox.Show("Las contraseñas no coinciden.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(NumeroSSTextBox.Text))
            {
                MessageBox.Show("El número de seguridad social es obligatorio.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (CategoriaComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar una categoría profesional.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (DepartamentoComboBox.SelectedIndex == -1)
            {
                MessageBox.Show("Debe seleccionar un departamento.", "Error de validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool ValidarDNI(string dni)
        {
            return Regex.IsMatch(dni, @"^\d{8}[A-Z]$");
        }

        private bool ValidarTelefono(string telefono)
        {
            return Regex.IsMatch(telefono, @"^\d{9}$");
        }

        private bool ValidarEmail(string email)
        {
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarCampos())
                return;

            try
            {
                string nombreCompleto = NombreCompletoTextBox.Text;
                DateTime fechaNacimiento = FechaNacimientoDatePicker.SelectedDate.Value;
                string dni = DniTextBox.Text;
                string telefono = TelefonoTextBox.Text;
                string email = EmailTextBox.Text;
                string direccion = DireccionTextBox.Text;
                DateTime fechaContratacion = FechaContratacionDatePicker.SelectedDate.Value;
                decimal salario = decimal.Parse(SalarioTextBox.Text);
                string usuario = UsuarioTextBox.Text;
                string contrasena = ContrasenaPasswordBox.Password;
                string numeroSeguridadSocial = NumeroSSTextBox.Text;
                int categoriaProfesional = CategoriaComboBox.SelectedIndex + 1;
                string departamento = ((ComboBoxItem)DepartamentoComboBox.SelectedItem).Content.ToString();

                string fechaNacimientoFormato = fechaNacimiento.ToString("yyyy/MM/dd");
                string fechaContratacionFormato = fechaContratacion.ToString("yyyy/MM/dd");

                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();

                    // Verificar si el usuario ya existe
                    string verificarUsuario = "SELECT COUNT(*) FROM Trabajadores WHERE Usuario = @Usuario";
                    using (var cmdVerificar = new MySqlCommand(verificarUsuario, conexion.ObtenerConexion()))
                    {
                        cmdVerificar.Parameters.AddWithValue("@Usuario", usuario);
                        int count = Convert.ToInt32(cmdVerificar.ExecuteScalar());
                        if (count > 0)
                        {
                            MessageBox.Show("El nombre de usuario ya existe. Por favor, elija otro.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    string query = @"INSERT INTO Trabajadores (NombreCompleto, FechaNacimiento, DNI, Telefono, 
                                   Email, Direccion, FechaContratacion, Salario, Usuario, Contraseña, 
                                   NumeroSeguridadSocial, CategoriaProfesional, Departamento) 
                                   VALUES (@NombreCompleto, @FechaNacimiento, @DNI, @Telefono, @Email, 
                                   @Direccion, @FechaContratacion, @Salario, @Usuario, @Contraseña, 
                                   @NumeroSeguridadSocial, @CategoriaProfesional, @Departamento)";

                    using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@NombreCompleto", nombreCompleto);
                        command.Parameters.AddWithValue("@FechaNacimiento", fechaNacimientoFormato);
                        command.Parameters.AddWithValue("@DNI", dni);
                        command.Parameters.AddWithValue("@Telefono", telefono);
                        command.Parameters.AddWithValue("@Email", email);
                        command.Parameters.AddWithValue("@Direccion", direccion);
                        command.Parameters.AddWithValue("@FechaContratacion", fechaContratacionFormato);
                        command.Parameters.AddWithValue("@Salario", salario);
                        command.Parameters.AddWithValue("@Usuario", usuario);
                        command.Parameters.AddWithValue("@Contraseña", contrasena);
                        command.Parameters.AddWithValue("@NumeroSeguridadSocial", numeroSeguridadSocial);
                        command.Parameters.AddWithValue("@CategoriaProfesional", categoriaProfesional);
                        command.Parameters.AddWithValue("@Departamento", departamento);

                        command.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Empleado guardado correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                LimpiarCampos();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al guardar el empleado: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarCampos()
        {
            NombreCompletoTextBox.Clear();
            FechaNacimientoDatePicker.SelectedDate = null;
            DniTextBox.Clear();
            TelefonoTextBox.Clear();
            EmailTextBox.Clear();
            DireccionTextBox.Clear();
            FechaContratacionDatePicker.SelectedDate = null;
            SalarioTextBox.Clear();
            UsuarioTextBox.Clear();
            ContrasenaPasswordBox.Clear();
            ConfirmarContrasenaPasswordBox.Clear();
            NumeroSSTextBox.Clear();
            CategoriaComboBox.SelectedIndex = -1;
            DepartamentoComboBox.SelectedIndex = -1;
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            var trabajadoresView = new Trabajadores(Window.GetWindow(this));
            ContentControl contenidoPrincipal = this.Parent as ContentControl;

            if (contenidoPrincipal != null)
            {
                contenidoPrincipal.Content = trabajadoresView;
            }
        }
    }
}
