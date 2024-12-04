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
        public double Precio { get; set; }
        public double Iva { get; set; } // IVA como porcentaje
        public double PrecioTotal { get; set; } // Precio total con IVA incluido
        public string Descripcion { get; set; }
        public string Categoria { get; set; }
        public int Stock { get; set; }
        public string Imagen { get; set; }
        public DateTime FechaCreacion { get; set; }
        public double Descuento { get; set; }
        public string EAN { get; set; }

    }
}
