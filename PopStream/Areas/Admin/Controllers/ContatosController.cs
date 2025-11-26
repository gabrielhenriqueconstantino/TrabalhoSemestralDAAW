using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopStream.Data;

[Area("Admin")]
[Authorize]
public class ContatosController : Controller
{
    private readonly AppDbContext _context;
    public ContatosController(AppDbContext context) => _context = context;

    public async Task<IActionResult> Index()
    {
        var mensagens = await _context.Contatos
            .OrderByDescending(c => c.DataEnvio)
            .ToListAsync();
        return View(mensagens);
    }

    public async Task<IActionResult> Details(int id)
    {
        var c = await _context.Contatos.FindAsync(id);
        if (c == null) return NotFound();
        return View(c);
    }
}

