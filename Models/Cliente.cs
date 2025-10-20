using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrotaTaxi.Models
{
    public class Cliente
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }

        [Required]
        [StringLength(18)]
        public string CNPJ { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public StatusEnum Status { get; set; }

        [Required]
        [StringLength(200)]
        public string Logradouro { get; set; }

        [StringLength(10)]
        public string Numero { get; set; }

        [StringLength(50)]
        public string Complemento { get; set; }

        [Required]
        [StringLength(9)]
        public string CEP { get; set; }

        [Required]
        [StringLength(2)]
        public string Estado { get; set; }

        [Required]
        [StringLength(100)]
        public string Cidade { get; set; }

        // Navigation properties
        public virtual ICollection<CentroCusto> CentrosCusto { get; set; } = new List<CentroCusto>();
        public virtual ICollection<UsuarioAutorizado> UsuariosAutorizados { get; set; } = new List<UsuarioAutorizado>();
        public virtual ICollection<Trecho> Trechos { get; set; } = new List<Trecho>();
        public virtual ICollection<Corrida> Corridas { get; set; } = new List<Corrida>();
    }

    public enum StatusEnum
    {
        Ativo = 1,
        Inativo = 2
    }
}
