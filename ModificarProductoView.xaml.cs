
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace TFG
{
    public partial class ModificarProductoView : UserControl
    {
        private Producto _producto;

        public ModificarProductoView(Producto producto)
        {
            InitializeComponent();
            _producto = producto;
            CargarCategorias();
            CargarDatos();
        }

        private void CargarCategorias()
        {
            try
            {
                using (var conexion = new Conexion())
                {
                    conexion.AbrirConexion();
                    string sql = "SELECT Id, Nombre FROM categorias";

                    using (var connection = conexion.ObtenerConexion())
                    using (MySqlCommand command = new MySqlCommand(sql, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            var categorias = new List<Categoria>();

                            while (reader.Read())
                            {
                                Categoria categoria = new Categoria
                                {
                                    Id = reader.GetInt32("Id"),
                                    Nombre = reader.GetString("Nombre")
                                };
                                categorias.Add(categoria);
                            }

                            CategoriaComboBox.ItemsSource = categorias;
                            CategoriaComboBox.DisplayMemberPath = "Nombre";
                            CategoriaComboBox.SelectedValuePath = "Id";
                            CategoriaComboBox.SelectedValue = _producto.CategoriaId;
                        }
                    }
                }
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

        private void CargarDatos()
        {
            NombreTextBox.Text = _producto.Nombre;
            MarcaTextBox.Text = _producto.Marca;
            ModeloTextBox.Text = _producto.Modelo;
            DescripcionTextBox.Text = _producto.Descripcion;
            EanTextBox.Text = _producto.EAN;
            PrecioTextBox.Text = _producto.Precio.ToString("F2");
            StockTextBox.Text = _producto.Stock.ToString();
            CategoriaComboBox.Text = _producto.CategoriaNombre;

            string ivaString = _producto.Iva.ToString("F2").Replace('.', ',');
            bool encontrado = false;

            foreach (ComboBoxItem item in IvaComboBox.Items)
            {
                string itemContent = item.Content.ToString().Replace('.', ',');
                if (itemContent == ivaString)
                {
                    IvaComboBox.SelectedItem = item;
                    encontrado = true;
                    break;
                }
            }

            if (!encontrado)
            {
                IvaComboBox.Text = ivaString;
            }

            // Cargar la imagen
            if (!string.IsNullOrEmpty(_producto.Imagen))
            {
                try
                {
                    Uri uriImagen = new Uri(_producto.Imagen, UriKind.RelativeOrAbsolute);

                    if (!uriImagen.IsAbsoluteUri)
                    {
                        // Construye la ruta absoluta si es relativa
                        uriImagen = new Uri(Path.GetFullPath(_producto.Imagen));
                    }

                    ImagenSeleccionada.Source = new BitmapImage(uriImagen);
                    NombreImagenTextBlock.Text = Path.GetFileName(_producto.Imagen);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al cargar la imagen: {ex.Message}");
                }
            }
        }

        private void GuardarCambiosButton_Click(object sender, RoutedEventArgs e)
        {
            _producto.Nombre = NombreTextBox.Text;
            _producto.Marca = MarcaTextBox.Text;
            _producto.Modelo = ModeloTextBox.Text;
            _producto.Descripcion = DescripcionTextBox.Text;

            if (CategoriaComboBox.SelectedValue != null)
            {
                _producto.CategoriaId = (int)CategoriaComboBox.SelectedValue;
            }

            _producto.EAN = EanTextBox.Text;
            _producto.Iva = double.Parse(((ComboBoxItem)IvaComboBox.SelectedItem).Content.ToString().Split('.')[0]);
            _producto.Precio = double.Parse(PrecioTextBox.Text);
            _producto.Stock = int.Parse(StockTextBox.Text);

            // Actualizar la imagen en la base de datos si se seleccionó una nueva
            if (ImagenSeleccionada.Source != null)
            {
                _producto.Imagen = ((BitmapImage)ImagenSeleccionada.Source).UriSource.LocalPath;
            }

            using (var conexion = new Conexion())
            {
                conexion.AbrirConexion();
                string sql = "UPDATE productos SET Nombre = @nombre, Marca = @marca, Modelo = @modelo, " +
                             "Descripcion = @descripcion, CategoriaId = @categoria, Ean = @ean, Iva = @iva, " +
                             "Precio = @precio, Stock = @stock, Imagen = @imagen WHERE Id = @id";

                using (var connection = conexion.ObtenerConexion())
                using (MySqlCommand command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@nombre", _producto.Nombre);
                    command.Parameters.AddWithValue("@marca", _producto.Marca);
                    command.Parameters.AddWithValue("@modelo", _producto.Modelo);
                    command.Parameters.AddWithValue("@descripcion", _producto.Descripcion);
                    command.Parameters.AddWithValue("@categoria", _producto.CategoriaId);
                    command.Parameters.AddWithValue("@ean", _producto.EAN);
                    command.Parameters.AddWithValue("@iva", _producto.Iva);
                    command.Parameters.AddWithValue("@precio", _producto.Precio);
                    command.Parameters.AddWithValue("@stock", _producto.Stock);
                    command.Parameters.AddWithValue("@imagen", _producto.Imagen);
                    command.Parameters.AddWithValue("@id", _producto.Id);

                    int filasAfectadas = command.ExecuteNonQuery();

                    if (filasAfectadas > 0)
                    {
                        MessageBox.Show("Producto modificado exitosamente.");
                    }
                    else
                    {
                        MessageBox.Show("No se pudo modificar el producto.");
                    }
                }
            }
        }

        private void SeleccionarImagen_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "Imagenes PNG (*.png)|*.png|Imagenes JPEG (*.jpg;*.jpeg)|*.jpg;*.jpeg";

            bool? result = dlg.ShowDialog();

            if (result == true)
            {
                string rutaImagen = dlg.FileName;
                ImagenSeleccionada.Source = new BitmapImage(new Uri(rutaImagen));
                NombreImagenTextBlock.Text = Path.GetFileName(rutaImagen);
                // Actualiza la propiedad del producto con la nueva ruta de la imagen
                _producto.Imagen = rutaImagen;
            }
        }

        private void GuardarButton_Click(object sender, RoutedEventArgs e)
        {
            // Validar entradas
            if (string.IsNullOrWhiteSpace(NombreTextBox.Text) ||
                string.IsNullOrWhiteSpace(PrecioTextBox.Text) ||
                string.IsNullOrWhiteSpace(StockTextBox.Text) ||
                string.IsNullOrWhiteSpace(EanTextBox.Text) || // Verifica que se complete el campo EAN
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

                // Obtener los valores de los campos
                string nombre = NombreTextBox.Text;
                string descripcion = DescripcionTextBox.Text;
                int categoriaId = (int)CategoriaComboBox.SelectedValue;
                string imagen = NombreImagenTextBlock.Text;

                // Consulta SQL para insertar los datos
                string query = "UPDATE productos SET Nombre = @nombre, Marca = @marca, Modelo = @modelo, " +
                             "Descripcion = @descripcion, CategoriaId = @categoria, Ean = @ean, Iva = @iva, " +
                             "Precio = @precio, Stock = @stock, Imagen = @imagen WHERE Id = @id";

                using (var cmd = new MySqlCommand(query, conexion.ObtenerConexion()))
                {
                    // Añadir parámetros
                    cmd.Parameters.AddWithValue("@Nombre", nombre);
                    cmd.Parameters.AddWithValue("@Precio", precio);
                    cmd.Parameters.AddWithValue("@Iva", iva);
                    cmd.Parameters.AddWithValue("@Descripcion", descripcion);
                    cmd.Parameters.AddWithValue("@CategoriaId", categoriaId);
                    cmd.Parameters.AddWithValue("@Stock", stock);
                    cmd.Parameters.AddWithValue("@Imagen", imagen);
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

        private void IvaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ActualizarPrecioPVP();
        }

        private void PrecioTextBox_TextChanged(object sender, TextChangedEventArgs e)
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

    }
}
