using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace TFG
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Marca { get; set; }
        public string Modelo { get; set; }
        public double Precio { get; set; }
        public double Iva { get; set; }
        public double PrecioTotal { get; set; }
        public int StockInicial { get; set; }
        public string Descripcion { get; set; }
        public int CategoriaId { get; set; } 
        public string CategoriaNombre { get; set; } 
        public int Stock { get; set; }
        public string Imagen { get; set; }
        public DateTime FechaCreacion { get; set; }
        public double Descuento { get; set; }
        public string EAN { get; set; }
        public int CantidadMaxima { get; set; } 
        public int CantidadMinima { get; set; }
        public int Recuento { get; set; } 
        public string UbicacionRecuento { get; set; }

        public int Diferencia { get; set; }

        public bool IsOutOfStock => Stock <= CantidadMinima;
    }

}