using System.ComponentModel;

namespace TFG.RecepcionMercancia
{
    public class ProductoRecepcion : INotifyPropertyChanged
    {
        private int _id;
        private string _nombreProducto;
        private string _ean;
        private int _cantidadPedida;
        private int _cantidadRecibida;
        private bool _recibido;
        private string _estado;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string NombreProducto
        {
            get => _nombreProducto;
            set
            {
                _nombreProducto = value;
                OnPropertyChanged(nameof(NombreProducto));
            }
        }

        public string EAN
        {
            get => _ean;
            set
            {
                _ean = value;
                OnPropertyChanged(nameof(EAN));
            }
        }

        public int CantidadPedida
        {
            get => _cantidadPedida;
            set
            {
                _cantidadPedida = value;
                OnPropertyChanged(nameof(CantidadPedida));
            }
        }

        public int CantidadRecibida
        {
            get => _cantidadRecibida;
            set
            {
                _cantidadRecibida = value;
                OnPropertyChanged(nameof(CantidadRecibida));
                ValidarCantidad();
            }
        }

        public bool Recibido
        {
            get => _recibido;
            set
            {
                _recibido = value;
                Estado = value ? "Validado" : "Pendiente";
                OnPropertyChanged(nameof(Recibido));
            }
        }

        public string Estado
        {
            get => _estado;
            set
            {
                _estado = value;
                OnPropertyChanged(nameof(Estado));
            }
        }

        private void ValidarCantidad()
        {
            if (CantidadRecibida < 0)
            {
                Estado = "Pendiente";
                Recibido = false;
            }
        }

        public string ObtenerMensajeDiferencia()
        {
            if (CantidadRecibida == 0)
            {
                return "No se ha recibido producto.";
            }
            else if (CantidadRecibida > CantidadPedida)
            {
                int diferencia = CantidadRecibida - CantidadPedida;
                return $"La cantidad recibida es mayor que la pedida por {diferencia} unidades.";
            }
            else if (CantidadRecibida < CantidadPedida && CantidadRecibida > 0)
            {
                int diferencia = CantidadPedida - CantidadRecibida;
                return $"La cantidad recibida es menor que la pedida por {diferencia} unidades.";
            }
            return null;
        }

        public ProductoRecepcion()
        {
            Estado = "Pendiente";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
