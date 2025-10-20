using System.ComponentModel.DataAnnotations;

namespace FrotaTaxi.Models
{
    public class Perfil
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Nome { get; set; }

        // Navigation properties
        public virtual ICollection<PerfilFuncionalidade> PerfilFuncionalidades { get; set; } = new List<PerfilFuncionalidade>();
        public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}
