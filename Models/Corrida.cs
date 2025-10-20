using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrotaTaxi.Models
{
    public class Corrida
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        public int SolicitanteId { get; set; }

        [Required]
        public TipoTarifaEnum TipoTarifa { get; set; }

        [StringLength(200)]
        public string? EnderecoInicial { get; set; }

        [StringLength(200)]
        public string? EnderecoFinal { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? KmInicial { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? KmFinal { get; set; }

        public int? TrechoId { get; set; }

        [Required]
        public DateTime DataHoraAgendamento { get; set; }

        [Required]
        public int UnidadeId { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }

        public string? Observacao { get; set; }

        [Required]
        public StatusCorridaEnum Status { get; set; }

        public int? CentroCustoId { get; set; }

        // Navigation properties
        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        [ForeignKey("SolicitanteId")]
        public virtual UsuarioAutorizado? Solicitante { get; set; }

        [ForeignKey("TrechoId")]
        public virtual Trecho? Trecho { get; set; }

        [ForeignKey("UnidadeId")]
        public virtual Unidade? Unidade { get; set; }

        [ForeignKey("CentroCustoId")]
        public virtual CentroCusto? CentroCusto { get; set; }
    }

    public enum TipoTarifaEnum
    {
        Livre = 1,
        KM = 2,
        Trecho = 3
    }

    public enum StatusCorridaEnum
    {
        Agendado = 1,
        Realizado = 2
    }
}
