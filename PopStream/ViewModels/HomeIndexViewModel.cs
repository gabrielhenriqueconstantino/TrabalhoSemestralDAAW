using System.Collections.Generic;
using PopStream.Models;

namespace PopStream.ViewModels
{
    public class HomeIndexViewModel
    {
        public IEnumerable<Producao> Producoes { get; set; } = new List<Producao>();
        public IEnumerable<Genero> Generos { get; set; } = new List<Genero>();
        public string Termo { get; set; }
        public int? GeneroId { get; set; }
        public string Sort { get; set; } = "date_desc";
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
    }
}