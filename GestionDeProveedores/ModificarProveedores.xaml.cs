using System;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace TFG.GestionDeProveedores
{
    public partial class ModificarProveedores : UserControl
    {
        private int proveedorIdActual = -1;

        // Constructor que recibe un proveedor
        public ModificarProveedores(Proveedor proveedor)
        {
            InitializeComponent();
            CargarDatosProveedor(proveedor);
        }

        private void CargarDatosProveedor(Proveedor proveedor)
        {
            proveedorIdActual = proveedor.ID;
            NombreProveedorTextBox.Text = proveedor.Nombre;
            ContactoTextBox.Text = proveedor.Contacto;
            TelefonoTextBox.Text = proveedor.Telefono;
            EmailTextBox.Text = proveedor.Email;
            DireccionTextBox.Text = proveedor.Direccion;
            CiudadTextBox.Text = proveedor.Ciudad;
            ProvinciaTextBox.Text = proveedor.Provincia;
            CodigoPostalTextBox.Text = proveedor.CodigoPostal;
            PaisTextBox.Text = proveedor.Pais;
            TipoProveedorTextBox.Text = proveedor.TipoProveedor;
            NotasTextBox.Text = proveedor.Notas;
            SitioWebTextBox.Text = proveedor.SitioWeb;
        }

        private void ModificarProveedor_Click(object sender, RoutedEventArgs e)
        {
            if (proveedorIdActual == -1)
            {
                MessageBox.Show("Primero debe seleccionar un proveedor.", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();

                    string query = @"UPDATE Proveedores 
                        SET Nombre = @Nombre,
                            Contacto = @Contacto, 
                            Telefono = @Telefono, 
                            Email = @Email, 
                            Direccion = @Direccion, 
                            Ciudad = @Ciudad, 
                            Provincia = @Provincia, 
                            CodigoPostal = @CodigoPostal, 
                            Pais = @Pais, 
                            TipoProveedor = @TipoProveedor, 
                            Notas = @Notas, 
                            SitioWeb = @SitioWeb
                        WHERE Id = @Id";

                    using (var comando = new MySqlCommand(query, conexion.ObtenerConexion()))
                    {
                        comando.Parameters.AddWithValue("@Id", proveedorIdActual);
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

                        int resultado = comando.ExecuteNonQuery();

                        if (resultado > 0)
                        {
                            MessageBox.Show("Proveedor modificado exitosamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            // Volver a la ventana de gestión de proveedores
                            ContentControl contenidoPrincipal = this.Parent as ContentControl;
                            if (contenidoPrincipal != null)
                            {
                                GestionDeProveedores gestionProveedores = new GestionDeProveedores();
                                contenidoPrincipal.Content = gestionProveedores;
                            }
                        }
                        else
                        {
                            MessageBox.Show("No se pudo modificar el proveedor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al modificar proveedor: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
    }
}
