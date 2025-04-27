using System;

namespace TFG.GestionDeProveedores
{
    public class Proveedor
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Contacto { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Ciudad { get; set; }
        public string Provincia { get; set; }
        public string CodigoPostal { get; set; }
        public string Pais { get; set; }
        public string TipoProveedor { get; set; }
        public string Notas { get; set; }
        public string SitioWeb { get; set; }

        // Método para validar datos del proveedor
        public bool ValidarDatos()
        {
            return !string.IsNullOrWhiteSpace(Nombre) &&
                   !string.IsNullOrWhiteSpace(Contacto);
        }
    }
}
