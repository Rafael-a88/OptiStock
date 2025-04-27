using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG.Categorias.Test
{
    [TestFixture]
    public class CategoriaTests
    {
        [Test]
        public void Categoria_Properties_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var categoria = new Categoria();

            // Act
            categoria.Id = 42;
            categoria.Nombre = "Electrónica";

            // Assert
            Assert.That(categoria.Id, Is.EqualTo(42));
            Assert.That(categoria.Nombre, Is.EqualTo("Electrónica"));
        }

        [Test]
        public void Categoria_CanBeInitializedWithObjectInitializer()
        {
            // Act
            var categoria = new Categoria { Id = 1, Nombre = "Libros" };

            // Assert
            Assert.That(categoria.Id, Is.EqualTo(1));
            Assert.That(categoria.Nombre, Is.EqualTo("Libros"));
        }
    }
}
