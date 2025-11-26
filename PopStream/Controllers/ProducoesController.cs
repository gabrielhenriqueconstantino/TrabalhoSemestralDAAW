using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopStream.Data;
using PopStream.Models;
using PopStream.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PopStream.Controllers
{
    public class ProducoesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ProducoesController(AppDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // GET: Producoes
        public async Task<IActionResult> Index()
        {
            var producoes = await _context.Producoes
                .Include(p => p.Generos).ThenInclude(pg => pg.Genero)
                .Include(p => p.Artistas).ThenInclude(pa => pa.Artista)
                .ToListAsync();

            return View(producoes);
        }

        // GET: Producoes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var producao = await _context.Producoes
                .Include(p => p.Generos).ThenInclude(pg => pg.Genero)
                .Include(p => p.Artistas).ThenInclude(pa => pa.Artista)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producao == null) return NotFound();

            return View(producao);
        }

        // GET: Producoes/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            var vm = new ProducaoFormViewModel
            {
                Producao = new Producao(),
                TodosGeneros = _context.Generos.ToList(),
                TodosArtistas = _context.Artistas.OrderBy(a => a.Nome).ToList()
            };

            return View(vm);
        }

        // POST: Producoes/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProducaoFormViewModel vm)
        {
            // Remover campos que sempre invalidam o ModelState
            ModelState.Remove("Producao.Generos");
            ModelState.Remove("Producao.Artistas");
            ModelState.Remove("TodosGeneros");
            ModelState.Remove("GenerosSelecionados");
            ModelState.Remove("Producao.CapaCaminho");
            ModelState.Remove("TodosArtistas");
            ModelState.Remove("ArtistasSelecionados");
            ModelState.Remove("Personagens");

            if (!ModelState.IsValid)
            {
                vm.TodosGeneros = _context.Generos.ToList();
                vm.TodosArtistas = _context.Artistas.OrderBy(a => a.Nome).ToList();
                return View(vm);
            }

            // Upload da capa
            if (vm.CapaUpload != null && vm.CapaUpload.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(vm.CapaUpload.FileName);
                var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadPath);
                var filePath = Path.Combine(uploadPath, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await vm.CapaUpload.CopyToAsync(stream);

                vm.Producao.CapaCaminho = "/uploads/" + fileName;
            }

            // Salvar produção
            _context.Producoes.Add(vm.Producao);
            await _context.SaveChangesAsync();

            // Salvar gêneros selecionados
            if (vm.GenerosSelecionados != null && vm.GenerosSelecionados.Any())
            {
                foreach (var generoId in vm.GenerosSelecionados)
                {
                    _context.ProducaoGeneros.Add(new ProducaoGenero
                    {
                        ProducaoId = vm.Producao.Id,
                        GeneroId = generoId
                    });
                }
            }

            // Salvar elenco selecionado (com nome do personagem lido do formulário)
            if (vm.ArtistasSelecionados != null && vm.ArtistasSelecionados.Any())
            {
                foreach (var artistaId in vm.ArtistasSelecionados)
                {
                    var personagem = Request.Form[$"Personagem_{artistaId}"].ToString();
                    _context.ProducaoArtistas.Add(new ProducaoArtista
                    {
                        ProducaoId = vm.Producao.Id,
                        ArtistaId = artistaId,
                        NomePersonagem = string.IsNullOrWhiteSpace(personagem) ? null : personagem
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Producoes/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var producao = await _context.Producoes
                .Include(p => p.Generos)
                .Include(p => p.Artistas)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producao == null) return NotFound();

            var vm = new ProducaoFormViewModel
            {
                Producao = producao,
                TodosGeneros = _context.Generos.ToList(),
                GenerosSelecionados = producao.Generos?.Select(pg => pg.GeneroId).ToList() ?? new System.Collections.Generic.List<int>(),
                TodosArtistas = _context.Artistas.OrderBy(a => a.Nome).ToList(),
                ArtistasSelecionados = producao.Artistas?.Select(pa => pa.ArtistaId).ToList() ?? new System.Collections.Generic.List<int>(),
                Personagens = producao.Artistas?.ToDictionary(pa => pa.ArtistaId, pa => pa.NomePersonagem ?? "") ?? new System.Collections.Generic.Dictionary<int, string>()
            };

            return View(vm);
        }

        // POST: Producoes/Edit
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProducaoFormViewModel vm)
        {
            // Remover campos que invalidam o ModelState (especialmente campos não-postados)
            ModelState.Remove("Producao.Generos");
            ModelState.Remove("Producao.Artistas");
            ModelState.Remove("TodosGeneros");
            ModelState.Remove("GenerosSelecionados");
            ModelState.Remove("Producao.CapaCaminho");
            ModelState.Remove("TodosArtistas");
            ModelState.Remove("ArtistasSelecionados");
            ModelState.Remove("Personagens");
            ModelState.Remove("CapaUpload"); // evita validação relacionada ao arquivo quando não enviado

            // Validar apenas o objeto Producao (campos obrigatórios como Titulo)
            if (!TryValidateModel(vm.Producao, nameof(vm.Producao)))
            {
                vm.TodosGeneros = _context.Generos.ToList();
                vm.TodosArtistas = _context.Artistas.OrderBy(a => a.Nome).ToList();
                return View(vm);
            }

            var producaoDb = await _context.Producoes
                .Include(p => p.Generos)
                .Include(p => p.Artistas)
                .FirstOrDefaultAsync(p => p.Id == vm.Producao.Id);

            if (producaoDb == null) return NotFound();

            // Atualiza campos básicos
            producaoDb.Titulo = vm.Producao.Titulo;
            producaoDb.DataLancamento = vm.Producao.DataLancamento;
            producaoDb.Diretor = vm.Producao.Diretor;

            // Upload nova capa (substitui) — opcional
            if (vm.CapaUpload != null && vm.CapaUpload.Length > 0)
            {
                if (!string.IsNullOrEmpty(producaoDb.CapaCaminho))
                {
                    string caminhoAntigo = Path.Combine(_hostEnvironment.WebRootPath, producaoDb.CapaCaminho.TrimStart('/'));
                    if (System.IO.File.Exists(caminhoAntigo))
                        System.IO.File.Delete(caminhoAntigo);
                }

                var pasta = Path.Combine(_hostEnvironment.WebRootPath, "uploads");
                Directory.CreateDirectory(pasta);

                string nomeArquivo = Guid.NewGuid().ToString() + Path.GetExtension(vm.CapaUpload.FileName);
                string caminhoCompleto = Path.Combine(pasta, nomeArquivo);

                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await vm.CapaUpload.CopyToAsync(stream);
                }

                producaoDb.CapaCaminho = "/uploads/" + nomeArquivo;
            }
            // Se nenhum upload foi feito, mantemos o valor atual em producaoDb.CapaCaminho

            // Atualizar gêneros: remover todos e adicionar os selecionados
            var existentesGeneros = _context.ProducaoGeneros.Where(pg => pg.ProducaoId == producaoDb.Id);
            _context.ProducaoGeneros.RemoveRange(existentesGeneros);

            if (vm.GenerosSelecionados != null && vm.GenerosSelecionados.Any())
            {
                foreach (var generoId in vm.GenerosSelecionados)
                {
                    _context.ProducaoGeneros.Add(new ProducaoGenero
                    {
                        ProducaoId = producaoDb.Id,
                        GeneroId = generoId
                    });
                }
            }

            // Atualizar elenco: remover todos e adicionar os selecionados com personagem
            var existentesArtistas = _context.ProducaoArtistas.Where(pa => pa.ProducaoId == producaoDb.Id);
            _context.ProducaoArtistas.RemoveRange(existentesArtistas);

            if (vm.ArtistasSelecionados != null && vm.ArtistasSelecionados.Any())
            {
                foreach (var artistaId in vm.ArtistasSelecionados)
                {
                    var personagem = Request.Form[$"Personagem_{artistaId}"].ToString();
                    _context.ProducaoArtistas.Add(new ProducaoArtista
                    {
                        ProducaoId = producaoDb.Id,
                        ArtistaId = artistaId,
                        NomePersonagem = string.IsNullOrWhiteSpace(personagem) ? null : personagem
                    });
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Producoes/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var producao = await _context.Producoes
                .FirstOrDefaultAsync(m => m.Id == id);

            if (producao == null) return NotFound();

            return View(producao);
        }

        // POST: Producoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producao = await _context.Producoes.FindAsync(id);

            if (producao != null)
            {
                // Excluir imagem também
                if (!string.IsNullOrEmpty(producao.CapaCaminho))
                {
                    string caminhoCompleto = Path.Combine(_hostEnvironment.WebRootPath, producao.CapaCaminho.TrimStart('/'));
                    if (System.IO.File.Exists(caminhoCompleto))
                        System.IO.File.Delete(caminhoCompleto);
                }

                _context.Producoes.Remove(producao);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
