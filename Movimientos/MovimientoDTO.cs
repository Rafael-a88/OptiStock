using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG.Movimientos
{
    public class MovimientoDTO
    {
        public string ProductoEan { get; set; }
        public DateTime FechaMovimiento { get; set; }
        public string TipoMovimiento { get; set; }
        public int Cantidad { get; set; }
        
        public int StockActual {  get; set; }
    }
}
