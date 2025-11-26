using System.ComponentModel.DataAnnotations;

namespace PopStream.Models
{
    public class Contato
    {
        public int Id { get; set; }

        [Required]
        public string NomeCompleto { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Assunto { get; set; }

        [Required]
        public string Mensagem { get; set; }

        public DateTime DataEnvio { get; set; } = DateTime.UtcNow;
    }
}
