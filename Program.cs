using Microsoft.EntityFrameworkCore;
using CatalogoPapa.Data;
using CatalogoPapa.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "La cadena de conexión 'DefaultConnection' no está configurada. " +
        "Definila en appsettings.json, appsettings.Production.json o como variable de entorno.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Asegurar que la base de datos y las tablas existan
using (var scope = app.Services.CreateScope())
{
    try
    {
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var categoriasExistentes = context.Categorias
            .Select(c => c.Nombre.ToLower())
            .ToHashSet();

        var categoriasDefault = new[]
        {
            new Categoria { Nombre = "Frutos secos", Descripcion = "Frutos secos seleccionados" },
            new Categoria { Nombre = "Yerbas", Descripcion = "Yerbas y productos relacionados" },
            new Categoria { Nombre = "Condimentos", Descripcion = "Condimentos y especias" },
            new Categoria { Nombre = "Frascos de vidrio", Descripcion = "Frascos de vidrio de diferentes tamaños" },
            new Categoria { Nombre = "Snacks", Descripcion = "Snacks saludables y naturales" },
            new Categoria { Nombre = "Granola", Descripcion = "Granolas artesanales y cereales" },
            new Categoria { Nombre = "Frutas deshidratadas", Descripcion = "Frutas secas y deshidratadas" }
        };

        var nuevasCategorias = categoriasDefault
            .Where(cat => !categoriasExistentes.Contains(cat.Nombre.ToLower()))
            .ToList();

        if (nuevasCategorias.Any())
        {
            context.Categorias.AddRange(nuevasCategorias);
            context.SaveChanges();
        }

        Console.WriteLine("[Inicio] Base de datos PostgreSQL inicializada correctamente.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Error DB]: {ex.Message}");
    }
}

app.Run();
