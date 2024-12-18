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
            CatalogoProductosView catalogoView = new CatalogoProductosView();
            ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
            contenidoPrincipal.Content = null;
            contenidoPrincipal.Content = catalogoView;
        }

        private void MostrarCategorias(object sender, RoutedEventArgs e)
        {
            CategoriasView categoriasView = new CategoriasView();
            ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
            contenidoPrincipal.Content = null;
            contenidoPrincipal.Content = categoriasView;
        }

        private void NuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            NuevaVentaView nuevaVentaView = new NuevaVentaView();
            ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
            contenidoPrincipal.Content = null;
            contenidoPrincipal.Content = nuevaVentaView;
        }

        private void SeguimientoPedidos(object sender, RoutedEventArgs e)
        {
            var seguimientoView = new SeguimientoPedidos.SeguimientoPedidosControl();
            ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
            contenidoPrincipal.Content = null;
            contenidoPrincipal.Content = seguimientoView;
        }

        private void MostrarNominas(object sender, RoutedEventArgs e)
        {
            Nominas.Nominas nominasView = new Nominas.Nominas();
            ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
            contenidoPrincipal.Content = null;
            contenidoPrincipal.Content = nominasView;
        }

        private void ChangeBackgroundColor(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                var color = (SolidColorBrush)new BrushConverter().ConvertFromString(button.Background.ToString());
                ContenidoPrincipal.Background = color;
            }
        }
    }
}
