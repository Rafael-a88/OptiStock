using System;
using System.Collections.Generic;

namespace TFG
{
    // Clase que representa una Venta
    public class Venta
    {
        public int Id { get; set; } // Identificador único de la venta
        public DateTime Fecha { get; set; } // Fecha de la venta
        public List<DetalleVenta> Detalles { get; set; } // Lista de detalles de la venta (productos)
        public double Total { get; private set; } // Total calculado de la venta

        // Constructor
        public Venta()
        {
            Detalles = new List<DetalleVenta>();
            Fecha = DateTime.Now; // Fecha por defecto: ahora
        }

        // Constructor con parámetros
        public Venta(int id, List<DetalleVenta> detalles = null)
        {
            Id = id;
            Detalles = detalles ?? new List<DetalleVenta>();
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
        public void AgregarProducto(DetalleVenta detalle)
        {
            if (detalle == null)
                throw new ArgumentNullException(nameof(detalle), "El detalle no puede ser nulo.");

            if (detalle.Cantidad <= 0 || detalle.PrecioUnitario < 0)
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
        public int ProductoId { get; set; } // Identificador del producto
        public string ProductoNombre { get; set; } // Nombre del producto
        public double PrecioUnitario { get; set; } // Precio unitario del producto
        public int Cantidad { get; set; } // Cantidad de productos vendidos
        public double Subtotal => PrecioUnitario * Cantidad; // Subtotal calculado

        // Constructor
        public DVenta(int productoId, string productoNombre, double precioUnitario, int cantidad)
        {
            if (precioUnitario < 0)
                throw new ArgumentException("El precio unitario no puede ser negativo.");
            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero.");

            ProductoId = productoId;
            ProductoNombre = productoNombre;
            PrecioUnitario = precioUnitario;
            Cantidad = cantidad;
        }
    }
}
