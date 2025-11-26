using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PopStream.Data;
using Microsoft.Extensions.Logging;
using System.Linq;
using System;

var builder = WebApplication.CreateBuilder(args);

// --- Serviços ---

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

// 🔐 Identity + EF Core
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() // Adiciona suporte a roles
.AddEntityFrameworkStores<AppDbContext>();

// Configure o cookie que o Identity usa (não usar AddAuthentication().AddCookie aqui)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login/Entrar";
    options.ExpireTimeSpan = TimeSpan.FromHours(4);
});

var app = builder.Build();

// --- Pipeline ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Áreas (Dashboard Admin)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

// Rotas padrão
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Seed inicial
using (var scope = app.Services.CreateScope())
{
    var contexto = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedData.Inicializar(contexto);
}

// Criar usuário Admin automaticamente (com logging de erros)
async Task CriarUsuarioAdmin(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    string email = "admin@admin.com";
    string senha = "1234@Admin";
    string role = "Admin";

    if (!await roleManager.RoleExistsAsync(role))
    {
        var roleResult = await roleManager.CreateAsync(new IdentityRole(role));
        if (!roleResult.Succeeded)
            logger.LogError("Falha ao criar role {Role}: {Errors}", role, string.Join("; ", roleResult.Errors.Select(e => e.Description)));
        else
            logger.LogInformation("Role {Role} criada", role);
    }

    var user = await userManager.FindByEmailAsync(email);
    if (user == null)
    {
        user = new IdentityUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(user, senha);
        if (!createResult.Succeeded)
            logger.LogError("Falha ao criar usuário {Email}: {Errors}", email, string.Join("; ", createResult.Errors.Select(e => e.Description)));
        else
            logger.LogInformation("Usuário {Email} criado", email);
    }

    if (!await userManager.IsInRoleAsync(user, role))
    {
        var addRoleResult = await userManager.AddToRoleAsync(user, role);
        if (!addRoleResult.Succeeded)
            logger.LogError("Falha ao adicionar {Email} à role {Role}: {Errors}", email, role, string.Join("; ", addRoleResult.Errors.Select(e => e.Description)));
        else
            logger.LogInformation("Usuário {Email} adicionado à role {Role}", email, role);
    }
}

await CriarUsuarioAdmin(app);

app.Run();