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
    /// <summary>
    /// Lógica de interacción para Principal.xaml
    /// </summary>
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

            // Obtén una referencia al Grid del área de contenido principal (Grid.Column="1")
            Grid contenidoPrincipal = (Grid)FindName("ContenidoPrincipal"); 

            // Limpia cualquier contenido anterior en el área de contenido principal
            contenidoPrincipal.Children.Clear();

            // Agrega la vista "CatalogoProductosView" al área de contenido principal
            contenidoPrincipal.Children.Add(catalogoView);
        }

        
    }
}
