using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFG.Nominas.Test
{
    [TestFixture]
    public class TrabajadoresTests
    {
        [Test]
        public void Trabajadores_Properties_ShouldSetAndGetCorrectly()
        {
            // Arrange
            var fechaNacimiento = new DateTime(1990, 5, 15);
            var fechaContratacion = new DateTime(2020, 1, 1);

            // Act
            var trabajador = new Trabajadores
            {
                Id = 1,
                NombreCompleto = "Ana López",
                FechaNacimiento = fechaNacimiento,
                DNI = "12345678A",
                Telefono = "600123456",
                Email = "ana.lopez@empresa.com",
                Direccion = "Calle Mayor 10",
                FechaContratacion = fechaContratacion,
                Salario = 25000.50m,
                Usuario = "ana.lopez",
                Contraseña = "contraseña123",
                NumeroSeguridadSocial = "123456789012",
                CategoriaProfesional = 2,
                PorcentajeIRPF = 15.0m,
                Departamento = "Recursos Humanos",
                SalarioBruto = 3000.00m,
                Deducciones = 500.00m,
                SalarioNeto = 2500.00m,
                Mes = "Abril",
                Año = 2024
            };

            // Assert
            Assert.That(trabajador.Id, Is.EqualTo(1));
            Assert.That(trabajador.NombreCompleto, Is.EqualTo("Ana López"));
            Assert.That(trabajador.FechaNacimiento, Is.EqualTo(fechaNacimiento));
            Assert.That(trabajador.DNI, Is.EqualTo("12345678A"));
            Assert.That(trabajador.Telefono, Is.EqualTo("600123456"));
            Assert.That(trabajador.Email, Is.EqualTo("ana.lopez@empresa.com"));
            Assert.That(trabajador.Direccion, Is.EqualTo("Calle Mayor 10"));
            Assert.That(trabajador.FechaContratacion, Is.EqualTo(fechaContratacion));
            Assert.That(trabajador.Salario, Is.EqualTo(25000.50m));
            Assert.That(trabajador.Usuario, Is.EqualTo("ana.lopez"));
            Assert.That(trabajador.Contraseña, Is.EqualTo("contraseña123"));
            Assert.That(trabajador.NumeroSeguridadSocial, Is.EqualTo("123456789012"));
            Assert.That(trabajador.CategoriaProfesional, Is.EqualTo(2));
            Assert.That(trabajador.PorcentajeIRPF, Is.EqualTo(15.0m));
            Assert.That(trabajador.Departamento, Is.EqualTo("Recursos Humanos"));
            Assert.That(trabajador.SalarioBruto, Is.EqualTo(3000.00m));
            Assert.That(trabajador.Deducciones, Is.EqualTo(500.00m));
            Assert.That(trabajador.SalarioNeto, Is.EqualTo(2500.00m));
            Assert.That(trabajador.Mes, Is.EqualTo("Abril"));
            Assert.That(trabajador.Año, Is.EqualTo(2024));
        }
    }
}