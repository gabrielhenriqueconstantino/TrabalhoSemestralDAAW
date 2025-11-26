using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopStream.Data;
using PopStream.Models;
using Microsoft.AspNetCore.Http;

namespace PopStream.Controllers
{
    public class ArtistasController : Controller
    {
        private readonly AppDbContext _context;
        private readonly string _pastaImagens = "wwwroot/imagens/artistas";

        public ArtistasController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // GET: Artistas
        // Suporta busca por 'termo' (nome ou país)
        // ============================================================
        public async Task<IActionResult> Index(string termo)
        {
            ViewData["Title"] = "Artistas";
            ViewBag.Termo = termo;

            var query = _context.Artistas
                .AsNoTracking()
                .Include(a => a.Producoes)
                    .ThenInclude(pa => pa.Producao)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(termo))
            {
                var t = termo.Trim();
                // Busca por nome OU país (LIKE)
                query = query.Where(a =>
                    EF.Functions.Like(a.Nome, $"%{t}%") ||
                    EF.Functions.Like(a.PaisNascimento ?? "", $"%{t}%")
                );
            }

            var artistas = await query.ToListAsync();
            return View(artistas);
        }

        // ============================================================
        // GET: Artistas/Details/5
        // ============================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var artista = await _context.Artistas
                .Include(a => a.Producoes)
                    .ThenInclude(pa => pa.Producao)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artista == null)
                return NotFound();

            return View(artista);
        }

        // ============================================================
        // GET: Artistas/Create (admin)
        // ============================================================
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // ============================================================
        // POST: Artistas/Create (admin)
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Nome,DataNascimento,PaisNascimento")] Artista artista,
            IFormFile FotoUpload)
        {
            ModelState.Remove("Producoes");
            ModelState.Remove("FotoCaminho");

            if (!ModelState.IsValid)
                return View(artista);

            // Upload da foto do artista
            if (FotoUpload != null && FotoUpload.Length > 0)
            {
                if (!Directory.Exists(_pastaImagens))
                    Directory.CreateDirectory(_pastaImagens);

                var nomeArquivo = Guid.NewGuid() + Path.GetExtension(FotoUpload.FileName);
                var caminhoArquivo = Path.Combine(_pastaImagens, nomeArquivo);

                using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
                {
                    await FotoUpload.CopyToAsync(stream);
                }

                artista.FotoCaminho = "/imagens/artistas/" + nomeArquivo;
            }

            _context.Add(artista);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: Artistas/Edit/5
        // ============================================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var artista = await _context.Artistas.FindAsync(id);

            if (artista == null)
                return NotFound();

            return View(artista);
        }

        // ============================================================
        // POST: Artistas/Edit/5
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Nome,DataNascimento,PaisNascimento,FotoCaminho")] Artista artista,
            IFormFile FotoUpload)
        {
            if (id != artista.Id)
                return NotFound();

            ModelState.Remove("Producoes");

            if (!ModelState.IsValid)
                return View(artista);

            // Novo upload substitui imagem anterior
            if (FotoUpload != null && FotoUpload.Length > 0)
            {
                if (!Directory.Exists(_pastaImagens))
                    Directory.CreateDirectory(_pastaImagens);

                var nomeArquivo = Guid.NewGuid() + Path.GetExtension(FotoUpload.FileName);
                var caminhoArquivo = Path.Combine(_pastaImagens, nomeArquivo);

                using (var stream = new FileStream(caminhoArquivo, FileMode.Create))
                {
                    await FotoUpload.CopyToAsync(stream);
                }

                artista.FotoCaminho = "/imagens/artistas/" + nomeArquivo;
            }

            try
            {
                _context.Update(artista);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Artistas.Any(e => e.Id == artista.Id))
                    return NotFound();

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // ============================================================
        // GET: Artistas/Delete/5
        // ============================================================
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var artista = await _context.Artistas
                .FirstOrDefaultAsync(a => a.Id == id);

            if (artista == null)
                return NotFound();

            return View(artista);
        }

        // ============================================================
        // POST: Artistas/Delete/5
        // ============================================================
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artista = await _context.Artistas.FindAsync(id);

            if (artista != null)
            {
                _context.Artistas.Remove(artista);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
