using MySqlConnector;
using System;
using System.Windows;
using System.Windows.Controls;

namespace TFG.Trabajadores
{
    public partial class ModificarTrabajadores : UserControl
    {
        private int trabajadorId;

        public ModificarTrabajadores(int id)
        {
            InitializeComponent();
            trabajadorId = id;
            CargarDatosTrabajador();
        }

        private void CargarDatosTrabajador()
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string query = "SELECT * FROM Trabajadores WHERE Id = @Id";
                using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    command.Parameters.AddWithValue("@Id", trabajadorId);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            NombreCompletoTextBox.Text = reader.GetString("NombreCompleto");
                            FechaNacimientoDatePicker.SelectedDate = reader.GetDateTime("FechaNacimiento");
                            DniTextBox.Text = reader.GetString("DNI");
                            TelefonoTextBox.Text = reader.GetString("Telefono");
                            EmailTextBox.Text = reader.GetString("Email");
                            DireccionTextBox.Text = reader.GetString("Direccion");
                            FechaContratacionDatePicker.SelectedDate = reader.GetDateTime("FechaContratacion");
                            SalarioTextBox.Text = reader.GetDecimal("Salario").ToString();
                            UsuarioTextBox.Text = reader.GetString("Usuario");
                            NumeroSSTextBox.Text = reader.GetString("NumeroSeguridadSocial");

                            // Cargar la contraseña
                            ContrasenaPasswordBox.Password = reader.GetString("Contraseña");

                            // Ajuste para la categoría profesional
                            int categoriaProfesional = reader.GetInt32("CategoriaProfesional");
                            CategoriaComboBox.SelectedItem = CategoriaComboBox.Items[categoriaProfesional - 1]; // Asumiendo que las categorías empiezan desde 1

                            DepartamentoComboBox.SelectedItem = reader.GetString("Departamento");
                        }
                    }
                }
            }
        }

        private void ActualizarButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validación de la contraseña
                if (ContrasenaPasswordBox.Password != ConfirmarContrasenaPasswordBox.Password)
                {
                    MessageBox.Show("Las contraseñas no coinciden.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return; // Salir del método si las contraseñas no son iguales
                }

                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    string query = @"UPDATE Trabajadores SET 
                                    NombreCompleto = @NombreCompleto, 
                                    FechaNacimiento = @FechaNacimiento, 
                                    DNI = @DNI, 
                                    Telefono = @Telefono, 
                                    Email = @Email, 
                                    Direccion = @Direccion, 
                                    FechaContratacion = @FechaContratacion,
                                    Salario = @Salario,
                                    Usuario = @Usuario,
                                    NumeroSeguridadSocial = @NumeroSeguridadSocial,
                                    CategoriaProfesional = @CategoriaProfesional,
                                    Departamento = @Departamento
                                    WHERE Id = @Id";

                    using (var command = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        command.Parameters.AddWithValue("@NombreCompleto", NombreCompletoTextBox.Text);
                        command.Parameters.AddWithValue("@FechaNacimiento", FechaNacimientoDatePicker.SelectedDate.Value);
                        command.Parameters.AddWithValue("@DNI", DniTextBox.Text);
                        command.Parameters.AddWithValue("@Telefono", TelefonoTextBox.Text);
                        command.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                        command.Parameters.AddWithValue("@Direccion", DireccionTextBox.Text);
                        command.Parameters.AddWithValue("@FechaContratacion", FechaContratacionDatePicker.SelectedDate.Value);
                        command.Parameters.AddWithValue("@Salario", decimal.Parse(SalarioTextBox.Text));
                        command.Parameters.AddWithValue("@Usuario", UsuarioTextBox.Text);
                        command.Parameters.AddWithValue("@NumeroSeguridadSocial", NumeroSSTextBox.Text);

                        // Ajuste para obtener la categoría profesional
                        if (CategoriaComboBox.SelectedItem is ComboBoxItem selectedItem)
                        {
                            int categoriaProfesional = int.Parse(selectedItem.Content.ToString());
                            command.Parameters.AddWithValue("@CategoriaProfesional", categoriaProfesional);
                        }
                        else
                        {
                            throw new InvalidOperationException("No se ha seleccionado una categoría válida.");
                        }

                        command.Parameters.AddWithValue("@Departamento", ((ComboBoxItem)DepartamentoComboBox.SelectedItem).Content.ToString());
                        command.Parameters.AddWithValue("@Id", trabajadorId);

                        command.ExecuteNonQuery();
                    }

                    // Actualizar la contraseña en una consulta separada
                    string updatePasswordQuery = "UPDATE Trabajadores SET Contraseña = @Contraseña WHERE Id = @Id";
                    using (var passwordCommand = new MySqlCommand(updatePasswordQuery, conexion.ObtenerConexion()))
                    {
                        passwordCommand.Parameters.AddWithValue("@Contraseña", ContrasenaPasswordBox.Password);
                        passwordCommand.Parameters.AddWithValue("@Id", trabajadorId);
                        passwordCommand.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Datos actualizados correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al actualizar los datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
