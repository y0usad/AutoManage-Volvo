using Xunit;
using Microsoft.EntityFrameworkCore;
using AutoManage.Data;
using AutoManage.Models;
using AutoManage.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace AutoManage.Tests
{
    public class VendedoresControllerTests
    {
        private AutoManageContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<AutoManageContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new AutoManageContext(options);
        }

        [Fact]
        public async Task CalcularComissoes_DeveRetornarSalarioCorreto()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new VendedoresController(context);

            var vendedor = new Vendedor
            {
                Id = 1,
                Nome = "João Silva",
                SalarioBase = 3000.00m
            };
            context.Vendedores.Add(vendedor);

            // Adicionar vendas para o vendedor
            context.Vendas.Add(new Venda
            {
                VendedorId = 1,
                VeiculoId = "CHASSI123",
                DataVenda = new DateTime(2026, 2, 15),
                ValorFinal = 100000.00m
            });

            context.Vendas.Add(new Venda
            {
                VendedorId = 1,
                VeiculoId = "CHASSI456",
                DataVenda = new DateTime(2026, 2, 20),
                ValorFinal = 150000.00m
            });

            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetComissoes(1, 2, 2026);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            // Usar reflection para acessar propriedades do objeto anônimo
            var salarioFinalProp = value?.GetType().GetProperty("salarioFinal");
            var salarioFinal = (decimal?)salarioFinalProp?.GetValue(value);

            // Total vendas: 250.000
            // Comissão (1%): 2.500
            // Salário final: 3.000 + 2.500 = 5.500
            Assert.Equal(5500.00m, salarioFinal);
        }

        [Fact]
        public async Task CalcularComissoes_SemVendas_DeveRetornarApenasSalarioBase()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new VendedoresController(context);

            var vendedor = new Vendedor
            {
                Id = 2,
                Nome = "Maria Santos",
                SalarioBase = 2500.00m
            };
            context.Vendedores.Add(vendedor);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetComissoes(2, 2, 2026);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var value = okResult.Value;

            var salarioFinalProp = value?.GetType().GetProperty("salarioFinal");
            var salarioFinal = (decimal?)salarioFinalProp?.GetValue(value);

            Assert.Equal(2500.00m, salarioFinal);
        }

        [Fact]
        public async Task CalcularComissoes_VendedorInexistente_DeveRetornarNotFound()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new VendedoresController(context);

            // Act
            var result = await controller.GetComissoes(999, 2, 2026);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task CalcularComissoes_MesInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new VendedoresController(context);

            var vendedor = new Vendedor { Id = 1, Nome = "Teste", SalarioBase = 3000m };
            context.Vendedores.Add(vendedor);
            await context.SaveChangesAsync();

            // Act
            var result = await controller.GetComissoes(1, 13, 2026); // Mês inválido

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
    }
}
