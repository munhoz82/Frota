using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrotaTaxi.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Login")]
        public string Login { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        public string Senha { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Perfil")]
        public int PerfilId { get; set; }

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.Now;

        [ForeignKey("PerfilId")]
        public virtual Perfil? Perfil { get; set; }
    }
}
