using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FrotaTaxi.Models
{
    public class PerfilFuncionalidade
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PerfilId { get; set; }

        [Required]
        public int FuncionalidadeId { get; set; }

        public bool PodeConsultar { get; set; }

        public bool PodeEditar { get; set; }

        [ForeignKey("PerfilId")]
        public virtual Perfil Perfil { get; set; }

        [ForeignKey("FuncionalidadeId")]
        public virtual Funcionalidade Funcionalidade { get; set; }
    }
}
