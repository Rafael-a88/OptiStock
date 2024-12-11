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
        public string Descripcion { get; set; }
        public int CategoriaId { get; set; } // Mantener esta propiedad para el ID
        public string CategoriaNombre { get; set; } // Nueva propiedad para el nombre
        public int Stock { get; set; }
        public string Imagen { get; set; }
        public DateTime FechaCreacion { get; set; }
        public double Descuento { get; set; }
        public string EAN { get; set; }
    }

}