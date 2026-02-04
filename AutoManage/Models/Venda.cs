using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AutoManage.Models
{
    /// <summary>
    /// representa uma venda realizada na concessionaria
    /// </summary>
    public class Venda
    {
        [Key]
        public int Id { get; set; }

        // chave estrangeira para veiculo
        [Required]
        [StringLength(17)]
        public string VeiculoId { get; set; } = string.Empty;

        // chave estrangeira para vendedor
        [Required]
        public int VendedorId { get; set; }

        [Required]
        public DateTime DataVenda { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorFinal { get; set; }

        // navegacao para o veiculo vendido
        [ForeignKey("VeiculoId")]
        public Veiculo? Veiculo { get; set; }

        // navegacao para o vendedor responsavel
        [ForeignKey("VendedorId")]
        public Vendedor? Vendedor { get; set; }
    }
}
