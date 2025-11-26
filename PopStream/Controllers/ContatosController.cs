using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PopStream.Data;
using PopStream.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PopStream.Controllers
{
    public class ContatosController : Controller
    {
        private readonly AppDbContext _context;

        public ContatosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Contatos/Create (público)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Contatos/Create (público)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Contato contato)
        {
            if (!ModelState.IsValid)
                return View(contato);

            // usar UTC consistente ou DateTime.Now conforme preferir
            contato.DataEnvio = DateTime.UtcNow;

            _context.Contatos.Add(contato);
            await _context.SaveChangesAsync();

            TempData["Mensagem"] = "Mensagem enviada com sucesso!";
            return RedirectToAction("Create");
        }

        // GET: Contatos (admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var mensagens = _context.Contatos
                                     .OrderByDescending(c => c.DataEnvio)
                                     .ToList();
            return await Task.FromResult(View(mensagens));
        }

        // GET: Contatos/Details/5 (admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var contato = await _context.Contatos.FindAsync(id);
            if (contato == null)
                return NotFound();

            return View(contato);
        }

        // GET: Contatos/Delete/5 (admin) - confirmação
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var contato = await _context.Contatos.FindAsync(id);
            if (contato == null)
                return NotFound();

            return View(contato);
        }

        // POST: Contatos/Delete/5 (admin) - confirmação POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var contato = await _context.Contatos.FindAsync(id);
            if (contato == null)
                return NotFound();

            _context.Contatos.Remove(contato);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}

