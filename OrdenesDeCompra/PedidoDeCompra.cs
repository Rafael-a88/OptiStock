using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG.OrdenesDeCompra
{
    public class PedidoDeCompra
    {
        public int ID { get; set; }
        public string NumeroOrden { get; set; }
        public string Proveedor { get; set; }
        public string Estado { get; set; }
        public DateTime FechaApertura { get; set; }

    }
}
