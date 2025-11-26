using System.ComponentModel.DataAnnotations;

namespace PopStream.Models
{
    public class ProducaoArtista
    {
        public int ProducaoId { get; set; }
        public Producao Producao { get; set; }

        public int ArtistaId { get; set; }
        public Artista Artista { get; set; }

        [StringLength(200)]
        public string NomePersonagem { get; set; }
    }
}
