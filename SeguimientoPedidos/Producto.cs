using System.ComponentModel;

namespace TFG.SeguimientoPedidos
{
    public class Producto : INotifyPropertyChanged
    {
        private string nombre;
        private string ean;
        private double precioUnitario;
        private int cantidad;
        private bool isPreparado;

        public string Nombre
        {
            get => nombre;
            set
            {
                nombre = value;
                OnPropertyChanged(nameof(Nombre));
            }
        }

        public string EAN
        {
            get => ean;
            set
            {
                ean = value;
                OnPropertyChanged(nameof(EAN));
            }
        }

        public double PrecioUnitario
        {
            get => precioUnitario;
            set
            {
                precioUnitario = value;
                OnPropertyChanged(nameof(PrecioUnitario));
                OnPropertyChanged(nameof(PrecioTotal)); // Asegúrate de notificar que PrecioTotal ha cambiado
            }
        }

        public int Cantidad
        {
            get => cantidad;
            set
            {
                cantidad = value >= 0 ? value : 0; // Validación para no permitir cantidades negativas
                OnPropertyChanged(nameof(Cantidad));
                OnPropertyChanged(nameof(PrecioTotal)); // Notificar que PrecioTotal ha cambiado
            }
        }

        public bool IsPreparado
        {
            get => isPreparado;
            set
            {
                isPreparado = value;
                OnPropertyChanged(nameof(IsPreparado));
            }
        }

        public double PrecioTotal => PrecioUnitario * Cantidad;

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
