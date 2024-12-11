
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace TFG
{
    public partial class AgregarProductoView : UserControl
    {
        private string imagenUrl;
        public AgregarProductoView()
        {
            InitializeComponent();
            CargarCategorias(); // Cargar las categorías al inicializar la vista
        }

        private void CargarCategorias()
        {
            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string sql = "SELECT Id, Nombre FROM categorias"; // Consulta para obtener ID y Nombre de las categorías

                using (var connection = conexion.ObtenerConexion())
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        var categorias = new List<Categoria>(); // Crear una lista para almacenar las categorías

                        while (reader.Read())
                        {
                            Categoria categoria = new Categoria
                            {
                                Id = reader.GetInt32("Id"),
                                Nombre = reader.GetString("Nombre")
                            };
                            categorias.Add(categoria);
                        }

                        CategoriaComboBox.ItemsSource = categorias; // Asignar la lista al ComboBox
                        CategoriaComboBox.DisplayMemberPath = "Nombre"; // Mostrar el nombre en el ComboBox
                        CategoriaComboBox.SelectedValuePath = "Id"; // Usar el ID como valor seleccionado
                    }
                }
            }
        }

        private void SeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            // Abre un cuadro de diálogo para seleccionar una imagen
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png"; // Especifica la extensión por defecto
            dlg.Filter = "Imagenes PNG (*.png)|*.png|Imagenes JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg"; // Filtros de archivo

            // Muestra el cuadro de diálogo y espera a que el usuario seleccione un archivo
            bool? result = dlg.ShowDialog();

            // Si el usuario seleccionó una imagen
            if (result == true)
            {
                // Obtiene el nombre de la imagen seleccionada
                string nombreImagen = System.IO.Path.GetFileName(dlg.FileName);
                NombreImagenTextBlock.Text = nombreImagen; // Actualiza el nombre de la imagen

                // Carga la imagen en el control Image
                ImagenSeleccionada.Source = new BitmapImage(new Uri(dlg.FileName));

                // Asigna la URL de la imagen a la variable de instancia
                imagenUrl = dlg.FileName;
            }
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            // Validar entradas
            if (string.IsNullOrWhiteSpace(NombreTextBox.Text) ||
                string.IsNullOrWhiteSpace(PrecioTextBox.Text) ||
                string.IsNullOrWhiteSpace(StockTextBox.Text) ||
                string.IsNullOrWhiteSpace(EanTextBox.Text) ||
                IvaComboBox.SelectedItem == null ||
                CategoriaComboBox.SelectedItem == null)
            {
                MessageBox.Show("Por favor, completa todos los campos obligatorios.");
                return;
            }

            float precio, iva, descuento = 0; // Inicializa descuento a 0
            int stock;

            // Validar conversiones
            if (!float.TryParse(PrecioTextBox.Text, out precio) ||
                !float.TryParse(((ComboBoxItem)IvaComboBox.SelectedItem).Content.ToString().Split('.')[0], out iva) ||
                !int.TryParse(StockTextBox.Text, out stock) ||
                !float.TryParse(DescuentoTextBox.Text, out descuento))
            {
                MessageBox.Show("Por favor, introduce valores válidos para precio, IVA, stock y descuento.");
                return;
            }

            // Validar el EAN
            string ean = EanTextBox.Text;
            if (!long.TryParse(ean, out long eanValue) || ean.Length != 13)
            {
                MessageBox.Show("El EAN debe ser un número válido de 13 dígitos.");
                return;
            }

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();

                // Obtener el primer ID libre
                int nuevoId = ObtenerPrimerIdLibre(conexion);

                // Obtener los valores de los campos
                string nombre = NombreTextBox.Text;
                string descripcion = DescripcionTextBox.Text;
                int categoriaId = (int)CategoriaComboBox.SelectedValue;

                // Concatenar la URL y el nombre de la imagen
                string nombreImagen = NombreImagenTextBlock.Text;
                string imagenCompleta = $"{imagenUrl}"; // Usar '|' como delimitador

                // Actualizar la consulta SQL para usar solo el campo 'Imagen'
                string query = "INSERT INTO productos (Id, Nombre, Precio, Iva, Descripcion, CategoriaId, Stock, Imagen, Descuento, Marca, Modelo, Ean) " +
                               "VALUES (@Id, @Nombre, @Precio, @Iva, @Descripcion, @CategoriaId, @Stock, @Imagen, @Descuento, @Marca, @Modelo, @Ean)";

                using (var cmd = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    // Añadir parámetros
                    cmd.Parameters.AddWithValue("@Id", nuevoId);
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@Precio", precio);
                    cmd.Parameters.AddWithValue("@Iva", iva);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);
                    cmd.Parameters.AddWithValue("@Stock", stock);
                    cmd.Parameters.AddWithValue("@Imagen", imagenCompleta); // Guardar la concatenación
                    cmd.Parameters.AddWithValue("@Descuento", descuento);
                    cmd.Parameters.AddWithValue("@Marca", MarcaTextBox.Text);
                    cmd.Parameters.AddWithValue("@Modelo", ModeloTextBox.Text);
                    cmd.Parameters.AddWithValue("@Ean", ean); // Añadir el EAN a los parámetros

                    // Ejecutar la consulta
                    try
                    {
                        int filasAfectadas = cmd.ExecuteNonQuery();
                        MessageBox.Show($"{filasAfectadas} producto(s) guardado(s) exitosamente.");
                    }
                    catch (MySqlException sqlEx)
                    {
                        MessageBox.Show("Error de SQL: " + sqlEx.Message);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error inesperado: " + ex.Message);
                    }
                }
            }
        }

        private int ObtenerPrimerIdLibre(Conexion conexion)
        {
            // Obtener todos los IDs existentes
            var idsExistentes = new HashSet<int>();
            string sql = "SELECT Id FROM productos";

            using (var command = new MySqlCommand(sql, conexion.ObtenerConexion()))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        idsExistentes.Add(reader.GetInt32("Id"));
                    }
                }
            }

            // Encontrar el primer ID libre
            int nuevoId = 1; // Suponiendo que los IDs comienzan desde 1
            while (idsExistentes.Contains(nuevoId))
            {
                nuevoId++;
            }

            return nuevoId;
        }





        private void CancelarButton_Click(object sender, RoutedEventArgs e)
        
        {
            // Cambiar el contenido del ContentControl a la vista de catálogo de productos
            var catalogoView = new CatalogoProductosView();
            // Asigna la nueva vista al ContentControl
            var parentWindow = Window.GetWindow(this) as Principal; // Asegúrate de que Principal sea el nombre de tu ventana
            if (parentWindow != null)
            {
                parentWindow.ContenidoPrincipal.Content = catalogoView;
            }
        }


        private void PrecioTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ActualizarPrecioPVP();
        }

        private void IvaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarPrecioPVP();
        }

        private void ActualizarPrecioPVP()
        {
            // Validar que el precio y el IVA sean válidos
            if (float.TryParse(PrecioTextBox.Text, out float precioBruto) &&
                IvaComboBox.SelectedItem is ComboBoxItem ivaItem &&
                float.TryParse(ivaItem.Content.ToString().Split('.')[0], out float iva)) // Solo toma el número antes del punto
            {
                // Calcular el IVA como porcentaje
                float ivaComoPorcentaje = iva / 100; // Convertir a decimal (10% -> 0.10)

                // Calcular el precio PVP
                float precioPVP = precioBruto + (precioBruto * ivaComoPorcentaje);

                // Aplicar descuento si es válido
                if (float.TryParse(DescuentoTextBox.Text, out float descuento) && descuento >= 0 && descuento <= 99)
                {
                    float descuentoDecimal = descuento / 100; // Convertir a decimal (por ejemplo, 20% -> 0.20)
                    precioPVP -= precioPVP * descuentoDecimal; // Aplicar descuento al precio PVP
                }

                PrecioTextBoxTotal.Text = precioPVP.ToString("F2"); // Formato a 2 decimales
            }
            else
            {
                PrecioTextBoxTotal.Text = string.Empty; // Limpiar el campo si hay un error
            }
        }

        // Método para validar la entrada del EAN
        private void EanTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Verificar que solo se ingresen dígitos y que no se excedan los 13 caracteres
            e.Handled = !IsTextAllowed(e.Text) || (EanTextBox.Text.Length >= 13);
        }

        // Método para validar la entrada del descuento
        private void DescuentoTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Verificar que solo se ingresen dígitos y que no se excedan los 2 caracteres
            e.Handled = !IsTextAllowed(e.Text) || (DescuentoTextBox.Text.Length >= 2);
        }

        // Método para comprobar si el texto ingresado son dígitos
        private static bool IsTextAllowed(string text)
        {
            return Regex.IsMatch(text, @"^\d+$"); // Permitir solo dígitos
        }
    }
}