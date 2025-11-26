using Microsoft.AspNetCore.Http;
using PopStream.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PopStream.ViewModels
{
    public class ProducaoFormViewModel
    {
        public Producao Producao { get; set; } = new Producao();

        public IFormFile CapaUpload { get; set; }

        public List<Genero> TodosGeneros { get; set; } = new List<Genero>();

        // Importante! Evita ModelState inválido se nada vier do formulário
        public List<int> GenerosSelecionados { get; set; } = new List<int>();

        // Lista de todos os artistas para exibir no formulário (checkboxes)
        public List<Artista> TodosArtistas { get; set; } = new List<Artista>();

        // IDs dos artistas selecionados no formulário
        public List<int> ArtistasSelecionados { get; set; } = new List<int>();

        // Dicionário (ArtistaId -> NomePersonagem) usado para prefiller no Edit e leitura no servidor
        public Dictionary<int, string> Personagens { get; set; } = new Dictionary<int, string>();
    }
}
