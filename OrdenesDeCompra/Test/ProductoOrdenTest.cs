using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG.OrdenesDeCompra.Test
{
    [TestFixture]
    public class ProductoOrdenTests
    {
        [Test]
        public void CambiarPropiedades_DisparaPropertyChanged()
        {
            // Arrange
            var producto = new ProductoOrden();
            string propiedadCambiada = null;

            producto.PropertyChanged += (s, e) =>
            {
                propiedadCambiada = e.PropertyName;
            };

            // Act
            producto.NombreProducto = "Producto Test";

            // Assert
            Assert.That(propiedadCambiada, Is.EqualTo("NombreProducto"));

            // Verificar notificación de PrecioSubtotal y TotalOrden al cambiar Cantidad
            bool subtotalNotificado = false, totalNotificado = false;
            producto.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "PrecioSubtotal") subtotalNotificado = true;
                if (e.PropertyName == "TotalOrden") totalNotificado = true;
            };

            producto.Cantidad = 5;

            Assert.That(subtotalNotificado, Is.True);
            Assert.That(totalNotificado, Is.True);

            // Verificar cálculo
            producto.PrecioUnitario = 10m;
            Assert.That(producto.PrecioSubtotal, Is.EqualTo(50m));
            Assert.That(producto.TotalOrden, Is.EqualTo(50m));
        }

        [Test]
        public void CambiarRecibido_DisparaPropertyChanged()
        {
            // Arrange
            var producto = new ProductoOrden();
            bool eventoLlamado = false;

            producto.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == "Recibido")
                    eventoLlamado = true;
            };

            // Act
            producto.Recibido = true;

            // Assert
            Assert.That(eventoLlamado, Is.True);
        }
    }
}