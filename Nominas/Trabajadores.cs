using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG.Nominas
{
    public class Trabajadores
    {
        // Propiedades de la tabla Trabajadores
        public int Id { get; set; }
        public string NombreCompleto { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string DNI { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public DateTime FechaContratacion { get; set; }
        public decimal Salario { get; set; }
        public string Usuario { get; set; }
        public string Contraseña { get; set; }
        public string NumeroSeguridadSocial { get; set; }
        public int CategoriaProfesional { get; set; }
        public decimal PorcentajeIRPF { get; set; }



        // Propiedades de la tabla Nomina
        public decimal SalarioBruto { get; set; }
        public decimal Deducciones { get; set; }
        public decimal SalarioNeto { get; set; }
        public string Mes { get; set; }
        public int Año { get; set; }
    }
}
