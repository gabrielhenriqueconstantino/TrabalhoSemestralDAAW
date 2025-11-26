using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Area("Admin")]
[Authorize]
public class PainelController : Controller
{
    public IActionResult Index() => View();
}

