using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrotaTaxi.Models
{
    public class UsuarioAutorizado
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        [StringLength(15)]
        public string Funcional { get; set; }

        [Required]
        public TipoSolicitanteEnum TipoSolicitante { get; set; }

        [Required]
        public StatusEnum Status { get; set; }

        [Required]
        [StringLength(20)]
        public string Telefone1 { get; set; }

        [Required]
        [StringLength(20)]
        public string Telefone2 { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public virtual Cliente Cliente { get; set; }

        // Navigation properties
        public virtual ICollection<Corrida> Corridas { get; set; } = new List<Corrida>();
        public virtual ICollection<Corrida> CorridasComoUsuario { get; set; } = new List<Corrida>();
    }

    public enum TipoSolicitanteEnum
    {
        Solicitante = 1,
        Usuario = 2,
        Ambos = 3
    }
}
