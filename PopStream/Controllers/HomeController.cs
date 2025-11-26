using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopStream.Models;
using PopStream.Data;
using PopStream.ViewModels;

namespace PopStream.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;
    private const int PageSize = 12;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    // Index com busca, filtro por gênero, ordenação e paginação
    public async Task<IActionResult> Index(string termo, int? generoId, string sort = "date_desc", int page = 1)
    {
        var query = _context.Producoes
            .Include(p => p.Generos).ThenInclude(pg => pg.Genero)
            .Include(p => p.Artistas).ThenInclude(pa => pa.Artista)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(termo))
        {
            var t = termo.Trim();
            query = query.Where(p =>
                (p.Titulo != null && EF.Functions.Like(p.Titulo, $"%{t}%")) ||
                (p.Diretor != null && EF.Functions.Like(p.Diretor, $"%{t}%")) ||
                p.Generos.Any(g => EF.Functions.Like(g.Genero.Nome, $"%{t}%")) ||
                p.Artistas.Any(a => EF.Functions.Like(a.Artista.Nome, $"%{t}%"))
            );
        }

        if (generoId.HasValue)
        {
            query = query.Where(p => p.Generos.Any(g => g.GeneroId == generoId.Value));
        }

        // Ordenação flexível
        switch (sort)
        {
            case "date_asc":
                query = query.OrderBy(p => p.DataLancamento ?? DateTime.MinValue).ThenBy(p => p.Titulo);
                break;
            case "title_asc":
                query = query.OrderBy(p => p.Titulo);
                break;
            case "title_desc":
                query = query.OrderByDescending(p => p.Titulo);
                break;
            case "genre_asc":
                query = query.OrderBy(p => p.Generos.Select(pg => pg.Genero.Nome).FirstOrDefault()).ThenBy(p => p.Titulo);
                break;
            case "genre_desc":
                query = query.OrderByDescending(p => p.Generos.Select(pg => pg.Genero.Nome).FirstOrDefault()).ThenBy(p => p.Titulo);
                break;
            default: // date_desc
                query = query.OrderByDescending(p => p.DataLancamento ?? DateTime.MinValue).ThenBy(p => p.Titulo);
                break;
        }

        var totalCount = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        if (page < 1) page = 1;
        if (page > totalPages && totalPages > 0) page = totalPages;

        var producoes = await query
            .Skip((page - 1) * PageSize)
            .Take(PageSize)
            .ToListAsync();

        var generos = await _context.Generos.OrderBy(g => g.Nome).ToListAsync();

        var vm = new HomeIndexViewModel
        {
            Producoes = producoes,
            Generos = generos,
            Termo = termo,
            GeneroId = generoId,
            Sort = sort,
            Page = page,
            TotalPages = totalPages
        };

        return View(vm);
    }

    public IActionResult Privacy() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}