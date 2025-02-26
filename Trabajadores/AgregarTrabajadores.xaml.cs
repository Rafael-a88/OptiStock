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

namespace TFG.Trabajadores
{
    /// <summary>
    /// Lógica de interacción para AgregarTrabajadores.xaml
    /// </summary>
    public partial class AgregarTrabajadores : UserControl
    {
        public AgregarTrabajadores()
        {
            InitializeComponent();
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            // Obtener los datos de los controles
            string nombreCompleto = NombreCompletoTextBox.Text;
            DateTime fechaNacimiento = FechaNacimientoDatePicker.SelectedDate.Value;
            string dni = DniTextBox.Text;
            string telefono = TelefonoTextBox.Text;
            string email = EmailTextBox.Text;
            string direccion = DireccionTextBox.Text;
            DateTime fechaContratacion = FechaContratacionDatePicker.SelectedDate.Value;
            decimal salario = decimal.Parse(SalarioTextBox.Text);
            string usuario = UsuarioTextBox.Text;
            string contrasena = ContrasenaTextBox.Text;
            string numeroSeguridadSocial = NumeroSSTextBox.Text;

            // Obtener la categoría seleccionada
            int categoriaProfesional = CategoriaComboBox.SelectedIndex + 1;

            // Formato de fechas
            string fechaNacimientoFormato = fechaNacimiento.ToString("yyyy/MM/dd");
            string fechaContratacionFormato = fechaContratacion.ToString("yyyy/MM/dd");

            // Usar la clase Conexion
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();

                string query = "INSERT INTO Trabajadores (NombreCompleto, FechaNacimiento, DNI, Telefono, Email, Direccion, FechaContratacion, Salario, Usuario, Contraseña, NumeroSeguridadSocial, CategoriaProfesional) " +
                               "VALUES (@NombreCompleto, @FechaNacimiento, @DNI, @Telefono, @Email, @Direccion, @FechaContratacion, @Salario, @Usuario, @Contraseña, @NumeroSeguridadSocial, @CategoriaProfesional)";

                using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    // Asignar parámetros
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

                    // Ejecutar la consulta
                    command.ExecuteNonQuery();
                }
            }

            // Mensaje de éxito o limpiar campos
            MessageBox.Show("Empleado guardado correctamente.");
            LimpiarCampos();
        }


        private void LimpiarCampos()
        {
            // Limpia los campos después de guardar
            NombreCompletoTextBox.Clear();
            FechaNacimientoDatePicker.SelectedDate = null;
            DniTextBox.Clear();
            TelefonoTextBox.Clear();
            EmailTextBox.Clear();
            DireccionTextBox.Clear();
            FechaContratacionDatePicker.SelectedDate = null;
            SalarioTextBox.Clear();
            UsuarioTextBox.Clear();
            ContrasenaTextBox.Clear();
            NumeroSSTextBox.Clear();
            CategoriaComboBox.SelectedIndex = -1;
        }

        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            var principalWindow = new Principal();
            principalWindow.Show();
            parentWindow?.Close();
        }
    }
}
