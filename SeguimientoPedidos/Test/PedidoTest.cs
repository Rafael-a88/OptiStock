using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TFG.SeguimientoPedidos.Test
{
    [TestFixture]
    public class PedidoTests
    {
        [Test]
        public void Cambiar_Background_DisparaPropertyChanged()
        {
            var pedido = new Pedido();
            var notificaciones = new List<string>();

            pedido.PropertyChanged += (s, e) => notificaciones.Add(e.PropertyName);

            pedido.Background = Brushes.Red;

            Assert.That(notificaciones, Contains.Item("Background"));
        }

        [Test]
        public void Asignar_Propiedades_Publicas_ValoresCorrectos()
        {
            var fecha = new DateTime(2024, 4, 25);

            var pedido = new Pedido
            {
                Id = 123,
                NumeroPedido = "PED-001",
                ClienteWebId = 456,
                PrecioTotal = 789.50m,
                FechaPedido = fecha,
                Estado = "Enviado",
                IsDelivered = true
            };

            Assert.That(pedido.Id, Is.EqualTo(123));
            Assert.That(pedido.NumeroPedido, Is.EqualTo("PED-001"));
            Assert.That(pedido.ClienteWebId, Is.EqualTo(456));
            Assert.That(pedido.PrecioTotal, Is.EqualTo(789.50m));
            Assert.That(pedido.FechaPedido, Is.EqualTo(fecha));
            Assert.That(pedido.Estado, Is.EqualTo("Enviado"));
            Assert.That(pedido.IsDelivered, Is.True);
        }

        [Test]
        public void Background_GetSet_FuncionaCorrectamente()
        {
            var pedido = new Pedido();
            var brush = new SolidColorBrush(Colors.Blue);

            pedido.Background = brush;

            Assert.That(pedido.Background, Is.EqualTo(brush));
        }
    }
}
