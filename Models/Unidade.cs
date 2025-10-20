using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrotaTaxi.Models
{
    public class Unidade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        [Required]
        [StringLength(14)]
        public string CPF { get; set; }

        [StringLength(10)]
        public string Apelido { get; set; }

        [StringLength(15)]
        public string Celular { get; set; }

        [Required]
        public StatusEnum Status { get; set; }

        [Required]
        [StringLength(20)]
        public string Carro { get; set; }

        [Required]
        [StringLength(8)]
        public string Placa { get; set; }

        [StringLength(50)]
        public string IMEI { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        public decimal PercentualCorrida { get; set; }

        // Navigation properties
        public virtual ICollection<Corrida> Corridas { get; set; } = new List<Corrida>();
    }
}
