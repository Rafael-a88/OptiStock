using System;

namespace TFG.GestionDeClientes
{
    public class Cliente
    {
        public int ID { get; set; }
        public string NombreCompleto { get; set; }
        public DateTime FechaNacimiento { get; set; }
        public string DNI { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }

        public bool ValidarDatos()
        {
            return !string.IsNullOrWhiteSpace(NombreCompleto) &&
                   !string.IsNullOrWhiteSpace(DNI);
        }
    }
}
