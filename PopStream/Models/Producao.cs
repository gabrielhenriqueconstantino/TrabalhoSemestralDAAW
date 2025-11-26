using PopStream.Models;
using System.ComponentModel.DataAnnotations;

namespace PopStream.Models
{
    public class Producao
    {
        public int Id { get; set; }

        [Required]
        [StringLength(300)]
        public string Titulo { get; set; } = "";

        [DataType(DataType.Date)]
        public DateTime? DataLancamento { get; set; }

        [StringLength(200)]
        public string? Diretor { get; set; }

        public string? CapaCaminho { get; set; }

        public ICollection<ProducaoGenero> Generos { get; set; }
            = new List<ProducaoGenero>();

        public ICollection<ProducaoArtista> Artistas { get; set; }
            = new List<ProducaoArtista>();
    }

}
