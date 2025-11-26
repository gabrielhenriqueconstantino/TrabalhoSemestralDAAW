using PopStream.Data;
using PopStream.Models;
using System.Security.Cryptography;
using System.Text;

public static class SeedData
{
    public static void Inicializar(AppDbContext context)
    {
        context.Database.EnsureCreated();

        if (!context.UsuariosAdmin.Any())
        {
            context.UsuariosAdmin.Add(new UsuarioAdmin
            {
                Usuario = "admin",
                SenhaHash = ComputeHash("1234")
            });
        }

        if (!context.Generos.Any())
        {
            context.Generos.AddRange(
                new Genero { Nome = "Ação" },
                new Genero { Nome = "Drama" },
                new Genero { Nome = "Comédia" },
                new Genero { Nome = "Ficção" }
            );
        }

        context.SaveChanges();
    }

    private static string ComputeHash(string senha)
    {
        using var sha = SHA256.Create();
        return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(senha)));
    }
}

