using System.ComponentModel;

public class ProductoOrden : INotifyPropertyChanged
{
    private int _id;
    private string _nombreProducto;
    private string _ean;
    private int _cantidad;
    private decimal _precioUnitario;
    private bool _recibido;

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

    public int Cantidad
    {
        get => _cantidad;
        set
        {
            _cantidad = value;
            OnPropertyChanged(nameof(Cantidad));
            OnPropertyChanged(nameof(PrecioSubtotal)); // Notificar cambio en PrecioSubtotal
            OnPropertyChanged(nameof(TotalOrden)); // Notificar cambio en TotalOrden
        }
    }

    public decimal PrecioUnitario
    {
        get => _precioUnitario;
        set
        {
            _precioUnitario = value;
            OnPropertyChanged(nameof(PrecioUnitario));
            OnPropertyChanged(nameof(PrecioSubtotal)); // Notificar cambio en PrecioSubtotal
            OnPropertyChanged(nameof(TotalOrden)); // Notificar cambio en TotalOrden
        }
    }

    public decimal PrecioSubtotal => Cantidad * PrecioUnitario; // Calcular PrecioSubtotal

    public decimal TotalOrden => Cantidad * PrecioUnitario; // Calcular TotalOrden

    public bool Recibido
    {
        get => _recibido;
        set
        {
            _recibido = value;
            OnPropertyChanged(nameof(Recibido));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
