using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PopStream.ViewModels;
using System.Threading.Tasks;

namespace PopStream.Controllers
{
    [Route("Login")]
    public class LoginController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<LoginController> _logger;

        public LoginController(
            SignInManager<IdentityUser> signInManager,
            UserManager<IdentityUser> userManager,
            ILogger<LoginController> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet("Entrar")]
        public IActionResult Entrar(string returnUrl = null)
        {
            var vm = new LoginViewModel { ReturnUrl = returnUrl };
            return View(vm);
        }

        [HttpPost("Entrar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Entrar(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

            _logger.LogInformation("Login attempt for {Email}: Succeeded={Succeeded}, LockedOut={LockedOut}, NotAllowed={NotAllowed}, Requires2FA={Requires2FA}",
                model.Email, result.Succeeded, result.IsLockedOut, result.IsNotAllowed, result.RequiresTwoFactor);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    return Redirect(model.ReturnUrl);

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                    // <-- redireciona para o AdminController na raiz (Views/Admin/Index.cshtml)
                    return RedirectToAction("Index", "Admin");

                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
                ModelState.AddModelError(string.Empty, "Conta bloqueada. Tente novamente mais tarde.");
            else if (result.IsNotAllowed)
                ModelState.AddModelError(string.Empty, "Login não permitido. Confirme o e‑mail ou contate o administrador.");
            else if (result.RequiresTwoFactor)
                ModelState.AddModelError(string.Empty, "Autenticação em dois fatores requerida.");
            else
                ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");

            return View(model);
        }
    }
}
