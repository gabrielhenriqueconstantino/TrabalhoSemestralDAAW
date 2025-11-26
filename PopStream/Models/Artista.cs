using PopStream.Models;
using System.ComponentModel.DataAnnotations;

namespace PopStream.Models
{
    public class Artista
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Nome { get; set; } = "";

        [DataType(DataType.Date)]
        public DateTime? DataNascimento { get; set; }

        [StringLength(100)]
        public string? PaisNascimento { get; set; }

        public string? FotoCaminho { get; set; }

        public ICollection<ProducaoArtista> Producoes { get; set; }
            = new List<ProducaoArtista>();
    }

}
