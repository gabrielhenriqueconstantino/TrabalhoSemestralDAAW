using System.ComponentModel.DataAnnotations;

namespace PopStream.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Informe o e-mail")]
        [EmailAddress(ErrorMessage = "E-mail inválido")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Informe a senha")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }

        // Tornar anulável resolve o erro de "campo obrigatório" quando não houver ReturnUrl
        public string? ReturnUrl { get; set; }
    }
}
