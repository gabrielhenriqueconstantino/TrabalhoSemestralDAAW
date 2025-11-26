namespace PopStream.Models
{
    public class ProducaoGenero
    {
        public int ProducaoId { get; set; }
        public Producao Producao { get; set; }

        public int GeneroId { get; set; }
        public Genero Genero { get; set; }
    }
}
