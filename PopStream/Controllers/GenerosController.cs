using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PopStream.Data;
using PopStream.Models;
using Microsoft.AspNetCore.Authorization;

namespace PopStream.Controllers
{
    public class GenerosController : Controller
    {
        private readonly AppDbContext _context;

        public GenerosController(AppDbContext context)
        {
            _context = context;
        }

        // GET: Generos
        public async Task<IActionResult> Index()
        {
            return View(await _context.Generos.ToListAsync());
        }

        // GET: Generos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var genero = await _context.Generos.FirstOrDefaultAsync(m => m.Id == id);
            if (genero == null) return NotFound();

            return View(genero);
        }

        // GET: Generos/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Generos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Id,Nome")] Genero genero)
        {
            if (ModelState.IsValid)
            {
                _context.Add(genero);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(genero);
        }

        // GET: Generos/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var genero = await _context.Generos.FindAsync(id);
            if (genero == null) return NotFound();

            return View(genero);
        }

        // POST: Generos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] Genero genero)
        {
            if (id != genero.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(genero);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GeneroExists(genero.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(genero);
        }

        // GET: Generos/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var genero = await _context.Generos.FirstOrDefaultAsync(m => m.Id == id);
            if (genero == null) return NotFound();

            return View(genero);
        }

        // POST: Generos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genero = await _context.Generos.FindAsync(id);
            if (genero != null) _context.Generos.Remove(genero);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GeneroExists(int id) => _context.Generos.Any(e => e.Id == id);
    }
}
