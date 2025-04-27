using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFG.Ubicaciones;

namespace TFG.Inventario
{
    public class InventarioItem
    {
        public Producto Producto { get; set; }
        public Ubicacion Ubicacion { get; set; }
        public int Cantidad { get; set; }
    }
}
