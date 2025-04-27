using NUnit.Framework;

namespace TFG.CatalogoProductos.Test
{
    [TestFixture]
    public class ProductoTests
    {
        [Test]
        public void IsOutOfStock_ReturnsTrue_WhenStockIsLessThanOrEqualToCantidadMinima()
        {
            var producto = new Producto
            {
                Stock = 5,
                CantidadMinima = 10
            };

            bool result = producto.IsOutOfStock;

            Assert.That(result, Is.True, "Se esperaba que el producto estuviera fuera de stock.");
        }

        [Test]
        public void IsOutOfStock_ReturnsFalse_WhenStockIsGreaterThanCantidadMinima()
        {
            var producto = new Producto
            {
                Stock = 15,
                CantidadMinima = 10
            };

            bool result = producto.IsOutOfStock;

            Assert.That(result, Is.False, "Se esperaba que el producto estuviera en stock.");
        }

    }
}
