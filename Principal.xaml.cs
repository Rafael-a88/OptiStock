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


namespace TFG
{
    
    public partial class Principal : Window
    {
        public Principal()
        {
            InitializeComponent();
        }

        private void MostrarCatalogoProductos(object sender, RoutedEventArgs e)
        {
            // Crea una instancia de la vista "CatalogoProductosView"
            CatalogoProductosView catalogoView = new CatalogoProductosView();

            // Obtén una referencia al ContentControl del área de contenido principal
            ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");

            // Limpia cualquier contenido anterior en el área de contenido principal
            contenidoPrincipal.Content = null; // O simplemente no es necesario limpiar si vas a reemplazar el contenido

            // Agrega la vista "CatalogoProductosView" al área de contenido principal
            contenidoPrincipal.Content = catalogoView;
        }

        private void MostrarCategorias(object sender, RoutedEventArgs e)
        {
            // Crea una instancia de la vista "CategoriasView"
            CategoriasView categoriasView = new CategoriasView();

            // Obtén una referencia al ContentControl del área de contenido principal
            ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");

            // Limpia cualquier contenido anterior en el área de contenido principal
            contenidoPrincipal.Content = null;

            // Agrega la vista "CategoriasView" al área de contenido principal
            contenidoPrincipal.Content = categoriasView;
        }

        private void SeguimientoPedidos(object sender, RoutedEventArgs e)
        {
            // Crea una instancia de la vista "SeguimientoPedidoView"
            var seguimientoView = new SeguimientoPedidos.SeguimientoPedidosControl();

            // Obtén una referencia al ContentControl del área de contenido principal
            ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");

            // Limpia cualquier contenido anterior en el área de contenido principal
            contenidoPrincipal.Content = null;

            // Agrega la vista "CategoriasView" al área de contenido principal
            contenidoPrincipal.Content = seguimientoView;
        }


        private void ChangeBackgroundColor(object sender, RoutedEventArgs e)
        {
            // Obtener el botón que fue presionado
            Button button = sender as Button;

            // Cambiar el color de fondo del Grid izquierdo
            if (button != null)
            {
                var color = (SolidColorBrush)new BrushConverter().ConvertFromString(button.Background.ToString());
                ContenidoPrincipal.Background = color;
            }
        }


    }
}