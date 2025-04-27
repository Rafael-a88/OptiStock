using NUnit.Framework;
using TFG.GestionDeProveedores;

namespace TFG.Tests.GestionDeProveedores
{
    [TestFixture]
    public class ProveedorTests
    {
        [Test]
        public void Proveedor_Properties_ShouldSetAndGetCorrectly()
        {
            // Arrange & Act
            var proveedor = new Proveedor
            {
                ID = 10,
                Nombre = "Proveedor Ejemplo",
                Contacto = "Juan Pérez",
                Telefono = "123456789",
                Email = "proveedor@ejemplo.com",
                Direccion = "Calle Falsa 123",
                Ciudad = "Madrid",
                Provincia = "Madrid",
                CodigoPostal = "28080",
                Pais = "España",
                TipoProveedor = "Mayorista",
                Notas = "Entrega rápida",
                SitioWeb = "https://proveedor.com"
            };

            // Assert
            Assert.That(proveedor.ID, Is.EqualTo(10));
            Assert.That(proveedor.Nombre, Is.EqualTo("Proveedor Ejemplo"));
            Assert.That(proveedor.Contacto, Is.EqualTo("Juan Pérez"));
            Assert.That(proveedor.Telefono, Is.EqualTo("123456789"));
            Assert.That(proveedor.Email, Is.EqualTo("proveedor@ejemplo.com"));
            Assert.That(proveedor.Direccion, Is.EqualTo("Calle Falsa 123"));
            Assert.That(proveedor.Ciudad, Is.EqualTo("Madrid"));
            Assert.That(proveedor.Provincia, Is.EqualTo("Madrid"));
            Assert.That(proveedor.CodigoPostal, Is.EqualTo("28080"));
            Assert.That(proveedor.Pais, Is.EqualTo("España"));
            Assert.That(proveedor.TipoProveedor, Is.EqualTo("Mayorista"));
            Assert.That(proveedor.Notas, Is.EqualTo("Entrega rápida"));
            Assert.That(proveedor.SitioWeb, Is.EqualTo("https://proveedor.com"));
        }

        [Test]
        public void ValidarDatos_ShouldReturnTrue_WhenNombreAndContactoAreNotEmpty()
        {
            var proveedor = new Proveedor
            {
                Nombre = "Proveedor Ejemplo",
                Contacto = "Juan Pérez"
            };

            Assert.That(proveedor.ValidarDatos(), Is.True);
        }

        [Test]
        public void ValidarDatos_ShouldReturnFalse_WhenNombreIsEmpty()
        {
            var proveedor = new Proveedor
            {
                Nombre = "",
                Contacto = "Juan Pérez"
            };

            Assert.That(proveedor.ValidarDatos(), Is.False);
        }

        [Test]
        public void ValidarDatos_ShouldReturnFalse_WhenContactoIsEmpty()
        {
            var proveedor = new Proveedor
            {
                Nombre = "Proveedor Ejemplo",
                Contacto = ""
            };

            Assert.That(proveedor.ValidarDatos(), Is.False);
        }

        [Test]
        public void ValidarDatos_ShouldReturnFalse_WhenNombreAndContactoAreNull()
        {
            var proveedor = new Proveedor
            {
                Nombre = null,
                Contacto = null
            };

            Assert.That(proveedor.ValidarDatos(), Is.False);
        }
    }
}
