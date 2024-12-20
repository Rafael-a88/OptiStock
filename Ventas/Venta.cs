using System;
using System.Collections.Generic;

namespace TFG
{
    // Clase que representa una Venta
    public class Venta
    {
        public int Id { get; set; } // Identificador único de la venta
        public DateTime Fecha { get; set; } // Fecha de la venta
        public List<DVenta> Detalles { get; set; } // Lista de detalles de la venta (productos)
        public double Total { get; private set; } // Total calculado de la venta

        // Constructor
        public Venta()
        {
            Detalles = new List<DVenta>();
            Fecha = DateTime.Now; // Fecha por defecto: ahora
        }

        // Constructor con parámetros
        public Venta(int id, List<DVenta> detalles = null)
        {
            Id = id;
            Detalles = detalles ?? new List<DVenta>();
            Fecha = DateTime.Now;
            CalcularTotal();
        }

        // Método para calcular el total de la venta
        public void CalcularTotal()
        {
            Total = 0;
            foreach (var detalle in Detalles)
            {
                Total += detalle.Subtotal;
            }
        }

        // Método para agregar un producto a la venta
        public void AgregarProducto(DVenta detalle)
        {
            if (detalle == null)
                throw new ArgumentNullException(nameof(detalle), "El detalle no puede ser nulo.");

            if (detalle.Cantidad <= 0 || detalle.ValorUnitario < 0)
                throw new ArgumentException("Cantidad o precio inválido.");

            // Verifica si el producto ya existe en la lista de detalles
            var existente = Detalles.Find(d => d.ProductoId == detalle.ProductoId);
            if (existente != null)
            {
                // Si existe, solo actualiza la cantidad
                existente.Cantidad += detalle.Cantidad;
            }
            else
            {
                // Si no existe, lo agrega como un nuevo detalle
                Detalles.Add(detalle);
            }

            // Recalcula el total después de agregar
            CalcularTotal();
        }

        // Método para cancelar la venta
        public void CancelarVenta()
        {
            Detalles.Clear();
            Total = 0;
        }
    }

    // Clase que representa un Detalle de Venta
    public class DVenta
    {
        public long ProductoId { get; set; } // EAN
        public string Nombre { get; set; }
        public int Cantidad { get; set; }
        public double ValorUnitario { get; set; }

        // Propiedad que calcula el subtotal
        public double Subtotal => Cantidad * ValorUnitario;


        public DVenta(long productoId, string nombre, double valorUnitario, int cantidad)
        {
            ProductoId = productoId;
            Nombre = nombre;
            ValorUnitario = valorUnitario;
            Cantidad = cantidad;
        }
    }

}
