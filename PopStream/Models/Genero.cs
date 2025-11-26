using System.ComponentModel.DataAnnotations;

namespace PopStream.Models
{
    public class Genero
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; }
    }
}
