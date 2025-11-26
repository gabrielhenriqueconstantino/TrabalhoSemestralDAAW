using System.ComponentModel.DataAnnotations;

namespace PopStream.Models
{
    public class UsuarioAdmin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Usuario { get; set; }

        [Required]
        public string SenhaHash { get; set; }
    }
}
