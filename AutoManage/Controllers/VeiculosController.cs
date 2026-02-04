using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoManage.Data;
using AutoManage.Models;

namespace AutoManage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VeiculosController : ControllerBase
    {
        private readonly AutoManageContext _context;

        public VeiculosController(AutoManageContext context)
        {
            _context = context;
        }

        /// <summary>
        /// lista todos os caminhoes volvo com filtros opcionais
        /// </summary>
        /// <param name="versaoMotor">filtro por versao do motor</param>
        /// <returns>lista de caminhoes ordenada por quilometragem</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Veiculo>>> GetVeiculos([FromQuery] string? versaoMotor = null)
        {
            // LINQ: Filtro opcional e ordenação por quilometragem
            var query = _context.Veiculos.AsQueryable();

            if (!string.IsNullOrEmpty(versaoMotor))
            {
                query = query.Where(v => v.VersaoMotor == versaoMotor);
            }

            var veiculos = await query
                .OrderBy(v => v.Quilometragem)
                .ToListAsync();

            return Ok(veiculos);
        }

        /// <summary>
        /// busca um caminhao volvo especifico por chassi (com dados do proprietario)
        /// </summary>
        /// <param name="chassi">chassi do caminhao</param>
        /// <returns>caminhao com dados completos do proprietario</returns>
        [HttpGet("{chassi}")]
        public async Task<ActionResult<Veiculo>> GetVeiculo(string chassi)
        {
            // LINQ: Include para buscar dados relacionados do proprietário
            var veiculo = await _context.Veiculos
                .Include(v => v.Proprietario)
                .FirstOrDefaultAsync(v => v.Chassi == chassi);

            if (veiculo == null)
            {
                return NotFound(new { message = "Caminhão não encontrado" });
            }

            return Ok(veiculo);
        }

        /// <summary>
        /// cria um novo veiculo
        /// </summary>
        /// <param name="veiculo">dados do veiculo</param>
        /// <returns>veiculo criado</returns>
        [HttpPost]
        public async Task<ActionResult<Veiculo>> PostVeiculo(Veiculo veiculo)
        {
            // Validar se o proprietário existe (se informado)
            if (veiculo.ProprietarioId.HasValue)
            {
                var proprietarioExiste = await _context.Proprietarios
                    .AnyAsync(p => p.Id == veiculo.ProprietarioId.Value);

                if (!proprietarioExiste)
                {
                    return BadRequest(new { message = "Proprietário não encontrado" });
                }
            }

            // Verificar se chassi já existe
            var chassiExiste = await _context.Veiculos
                .AnyAsync(v => v.Chassi == veiculo.Chassi);

            if (chassiExiste)
            {
                return BadRequest(new { message = "Chassi já cadastrado" });
            }

            _context.Veiculos.Add(veiculo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVeiculo), new { chassi = veiculo.Chassi }, veiculo);
        }

        /// <summary>
        /// atualiza um veiculo existente
        /// </summary>
        /// <param name="chassi">chassi do veiculo</param>
        /// <param name="veiculo">dados atualizados</param>
        /// <returns>sem conteudo</returns>
        [HttpPut("{chassi}")]
        public async Task<IActionResult> PutVeiculo(string chassi, Veiculo veiculo)
        {
            if (chassi != veiculo.Chassi)
            {
                return BadRequest(new { message = "Chassi não corresponde" });
            }

            // Validar se o proprietário existe (se informado)
            if (veiculo.ProprietarioId.HasValue)
            {
                var proprietarioExiste = await _context.Proprietarios
                    .AnyAsync(p => p.Id == veiculo.ProprietarioId.Value);

                if (!proprietarioExiste)
                {
                    return BadRequest(new { message = "Proprietário não encontrado" });
                }
            }

            _context.Entry(veiculo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await VeiculoExists(chassi))
                {
                    return NotFound(new { message = "Veículo não encontrado" });
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// deleta um veiculo
        /// </summary>
        /// <param name="chassi">chassi do veiculo</param>
        /// <returns>sem conteudo</returns>
        [HttpDelete("{chassi}")]
        public async Task<IActionResult> DeleteVeiculo(string chassi)
        {
            var veiculo = await _context.Veiculos.FindAsync(chassi);
            if (veiculo == null)
            {
                return NotFound(new { message = "Veículo não encontrado" });
            }

            // Verificar se há vendas associadas
            var temVendas = await _context.Vendas.AnyAsync(v => v.VeiculoId == chassi);
            if (temVendas)
            {
                return BadRequest(new { message = "Não é possível deletar veículo com vendas registradas" });
            }

            _context.Veiculos.Remove(veiculo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private async Task<bool> VeiculoExists(string chassi)
        {
            return await _context.Veiculos.AnyAsync(e => e.Chassi == chassi);
        }
    }
}
