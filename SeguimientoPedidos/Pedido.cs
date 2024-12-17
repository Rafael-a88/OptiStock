using System;
using System.Windows.Media;
using System.ComponentModel;

namespace TFG.SeguimientoPedidos
{
    public class Pedido : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; }
        public int ClienteWebId { get; set; }
        public decimal PrecioTotal { get; set; }
        public DateTime FechaPedido { get; set; }
        public string Estado { get; set; }
        public bool IsDelivered { get; set; }

        private SolidColorBrush _background;
        public SolidColorBrush Background
        {
            get { return _background; }
            set
            {
                _background = value;
                OnPropertyChanged(nameof(Background));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
