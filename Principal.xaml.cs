using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TFG.RecepcionMercancia;

namespace TFG
{
    public partial class Principal : Window
    {
        private string departamentoUsuario;

        public Principal(string departamento)
        {
            InitializeComponent();
            this.departamentoUsuario = departamento;
            StatusText.Text = $"Usuario: Administrador | Departamento: {departamentoUsuario}";
        }

        private void MostrarCatalogoProductos(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Ventas" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Marketing" || departamentoUsuario == "Almacén" || departamentoUsuario == "Administrativo" || departamentoUsuario == "Compras")
            {
                CatalogoProductosView catalogoView = new CatalogoProductosView();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = catalogoView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder al catálogo de productos.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void MostrarCategorias(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Ventas" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Marketing" || departamentoUsuario == "Administrativo")
            {
                CategoriasView categoriasView = new CategoriasView();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = categoriasView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a las categorías.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void NuevaVenta_Click(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Gerencia" || departamentoUsuario == "Ventas")
            {
                NuevaVentaView nuevaVentaView = new NuevaVentaView();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = nuevaVentaView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para realizar una nueva venta.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void SeguimientoPedidos(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Ventas" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Atención al Cliente" || departamentoUsuario == "Almacén")
            {
                var seguimientoView = new SeguimientoPedidos.SeguimientoPedidosControl();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = seguimientoView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder al seguimiento de pedidos.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void MostrarNominas(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "RRHH" || departamentoUsuario == "Gerencia")
            {
                Nominas.Nominas nominasView = new Nominas.Nominas();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = nominasView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a las nóminas.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void MostrarPreciosYPromociones(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Ventas" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Marketing")
            {
                PreciosyPromociones.PreciosyPromociones preciosyPromocionesView = new PreciosyPromociones.PreciosyPromociones();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = preciosyPromocionesView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a precios y promociones.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void ControlDeStock(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Almacén" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Administrativo")
            {
                ControlDeStock.ControlDeStock ControlDeStockView = new ControlDeStock.ControlDeStock();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = ControlDeStockView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder al control de stock.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void MisOrdenesDeCompra(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Administrativo" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Compras")
            {
                OrdenesDeCompra.OrdenesDeCompra OrdenesView = new OrdenesDeCompra.OrdenesDeCompra();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = OrdenesView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a las órdenes de compra.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void Trabajadores(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "RRHH" || departamentoUsuario == "Gerencia")
            {
                Window mainWindow = Window.GetWindow(this);
                Trabajadores.Trabajadores TrabajadoresView = new Trabajadores.Trabajadores(mainWindow);
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = TrabajadoresView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a la gestión de trabajadores.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void Movimiento(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Almacén" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Administrativo" || departamentoUsuario == "Compras")
            {
                Movimiento movimientosView = new Movimiento();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = movimientosView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a movimientos.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void HistorialDeVentas(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Ventas" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Marketing")
            {
                HistorialDeVentasNamespace.HistorialDeVentas historialView = new HistorialDeVentasNamespace.HistorialDeVentas();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = historialView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder al historial de ventas.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void HistorialDeVentasWeb(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Ventas" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Marketing")
            {
                HistorialDeVentasWebNamespace.HistorialDeVentasWeb historialWebView = new HistorialDeVentasWebNamespace.HistorialDeVentasWeb();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = historialWebView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder al historial de ventas web.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void MostrarUbicaciones(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Almacén" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Administrativo")
            {
                Ubicaciones.Ubicaciones UbicacionesView = new Ubicaciones.Ubicaciones();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = UbicacionesView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a las ubicaciones.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void MostrarInventario(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Almacén" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Administrativo")
            {
                Inventario.Inventario InventarioView = new Inventario.Inventario();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = InventarioView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder al inventario.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void Devoluciones(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Ventas" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Atención al Cliente")
            {
                DevolucionesView devolucionesView = new DevolucionesView();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = devolucionesView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a devoluciones.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void MiRecepcionMercancia(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Almacén" || departamentoUsuario == "Gerencia" || departamentoUsuario == "Administrativo" || departamentoUsuario == "Compras")
            {
                RecepcionMercanciaView recepcionMercanciaView = new RecepcionMercanciaView();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = recepcionMercanciaView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a la recepción de mercancía.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void MisProveedores(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Compras" || departamentoUsuario == "Administrativo" || departamentoUsuario == "Gerencia")
            {
                GestionDeProveedores.GestionDeProveedores gestionDeProveedoresView = new GestionDeProveedores.GestionDeProveedores();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null; 
                contenidoPrincipal.Content = gestionDeProveedoresView; 
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a la gestión de proveedores.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void MisClientes(object sender, RoutedEventArgs e)
        {
            if (departamentoUsuario == "Atención al Cliente" || departamentoUsuario == "Gerencia")
            {
                GestionDeClientes.GestionDeClientes gestionDeClientesView = new GestionDeClientes.GestionDeClientes();
                ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
                contenidoPrincipal.Content = null;
                contenidoPrincipal.Content = gestionDeClientesView;
            }
            else
            {
                MessageBox.Show("No tiene permisos para acceder a la gestión de clientes.",
                                "Acceso denegado",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        private void MisVacaciones(object sender, RoutedEventArgs e)
        {
            Vacaciones.VacacionesControl vacacionesView = new Vacaciones.VacacionesControl();
            ContentControl contenidoPrincipal = (ContentControl)FindName("ContenidoPrincipal");
            contenidoPrincipal.Content = null;
            contenidoPrincipal.Content = vacacionesView;
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
