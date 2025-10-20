using System.ComponentModel.DataAnnotations;

namespace FrotaTaxi.Models
{
    public class Funcionalidade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [StringLength(100)]
        public string Controller { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; }

        // Navigation properties
        public virtual ICollection<PerfilFuncionalidade> PerfilFuncionalidades { get; set; } = new List<PerfilFuncionalidade>();
    }
}
