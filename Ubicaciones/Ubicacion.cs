
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG.Ubicaciones
{
    public class Ubicacion
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Almacen { get; set; }
        public string Calle { get; set; }
        public string Lado { get; set; }
        public string Modulo { get; set; }
        public string Altura { get; set; }
        public string UbicacionTotal { get; set; }

        public Ubicacion(string codigo)
        {
            Almacen = codigo.Substring(0, 3);
            Calle = codigo.Substring(3, 2);
            Lado = codigo.Substring(5, 1);
            Modulo = codigo.Substring(6, 2);
            Altura = codigo.Substring(8, 1);
            UbicacionTotal = $"{Almacen}{Calle}{Lado}{Modulo}{Altura}";
        }
    }
}