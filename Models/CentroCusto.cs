using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrotaTaxi.Models
{
    public class CentroCusto
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(10)]
        public string Codigo { get; set; }

        [Required]
        [StringLength(50)]
        public string Descricao { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        // Navigation properties
        public virtual ICollection<Corrida> Corridas { get; set; } = new List<Corrida>();
    }
}
