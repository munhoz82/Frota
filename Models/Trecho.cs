using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrotaTaxi.Models
{
    public class Trecho
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [Required]
        [StringLength(15)]
        public string NomeTrecho { get; set; }

        [Required]
        [StringLength(100)]
        public string TrechoInicio { get; set; }

        [Required]
        [StringLength(100)]
        public string TrechoTermino { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal Valor { get; set; }

        [Required]
        public StatusEnum Status { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente? Cliente { get; set; }

        // Navigation properties
        public virtual ICollection<Corrida> Corridas { get; set; } = new List<Corrida>();
    }
}
