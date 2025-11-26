using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopStream.Data;
using System.Linq;
using System.Threading.Tasks;

namespace PopStream.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // Painel administrativo (Views/Admin/Index.cshtml)
        public IActionResult Index()
        {
            var model = new
            {
                TotalProducoes = _context.Producoes.Count(),
                TotalArtistas = _context.Artistas.Count(),
                TotalGeneros = _context.Generos.Count(),
                TotalMensagens = _context.Contatos.Count()
            };

            return View(model);
        }

        // GET: /Admin/Producoes  (lista administrativa com CRUD)
        public async Task<IActionResult> Producoes()
        {
            var producoes = await _context.Producoes
                .Include(p => p.Generos).ThenInclude(pg => pg.Genero)
                .Include(p => p.Artistas).ThenInclude(pa => pa.Artista)
                .ToListAsync();

            return View(producoes);
        }

        // GET: /Admin/Artistas  (lista administrativa com CRUD)
        public async Task<IActionResult> Artistas()
        {
            var artistas = await _context.Artistas
                .Include(a => a.Producoes).ThenInclude(pa => pa.Producao)
                .ToListAsync();

            return View(artistas);
        }
    }
}

