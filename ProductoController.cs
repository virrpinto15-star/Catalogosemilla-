using Microsoft.AspNetCore.Mvc;
using CatalogoPapa.Models;
using Microsoft.EntityFrameworkCore;
using CatalogoPapa.Data;
using CatalogoPapa.Filters;

namespace CatalogoPapa.Controllers
{
    public class ProductoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Extensiones de imagen permitidas
        private static readonly HashSet<string> ExtensionesPermitidas =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        // Tamaño máximo: 5 MB
        private const long MaxImagenBytes = 5 * 1024 * 1024;

        public ProductoController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Producto
        public async Task<IActionResult> Index(int? categoriaId, string? buscar)
        {
            var query = _context.Productos
                .Include(p => p.Categoria)
                .AsQueryable();

            if (categoriaId.HasValue && categoriaId.Value > 0)
            {
                query = query.Where(p => p.CategoriaId == categoriaId.Value);
            }

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                var buscarLower = buscar.Trim().ToLower();
                query = query.Where(p => p.Nombre.ToLower().Contains(buscarLower) ||
                                         (p.Descripcion != null && p.Descripcion.ToLower().Contains(buscarLower)));
            }

            ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
            ViewBag.CategoriaSeleccionada = categoriaId;
            ViewBag.Buscar = buscar;

            var productos = await query.OrderBy(p => p.Nombre).ToListAsync();

            ViewBag.TotalSinFiltros = await _context.Productos.CountAsync();
            ViewBag.CantidadProductos = productos.Count;

            return View(productos);
        }

        // Panel de Administración de Productos
        [RequiereAdmin]
        public async Task<IActionResult> Admin()
        {
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .OrderBy(p => p.CategoriaId)
                .ThenBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();

            return View(productos);
        }

        // POST: Producto/CrearCategoria
        [HttpPost]
        [RequiereAdmin]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCategoria(string nombre, string? descripcion)
        {
            if (!string.IsNullOrWhiteSpace(nombre))
            {
                var nombreLimpio = nombre.Trim();
                var yaExiste = await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == nombreLimpio.ToLower());

                if (!yaExiste)
                {
                    var categoria = new Categoria
                    {
                        Nombre = nombreLimpio,
                        Descripcion = descripcion?.Trim()
                    };
                    _context.Categorias.Add(categoria);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Admin");
        }

        // POST: Producto/EliminarCategoria/5
        [HttpPost]
        [RequiereAdmin]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarCategoria(int id)
        {
            var categoria = await _context.Categorias.Include(c => c.Productos).FirstOrDefaultAsync(c => c.Id == id);
            if (categoria != null)
            {
                // Si tiene productos asociados, no eliminar para no dejar productos huérfanos
                if (categoria.Productos == null || !categoria.Productos.Any())
                {
                    _context.Categorias.Remove(categoria);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction("Admin");
        }

        // GET: Producto/Crear
        [RequiereAdmin]
        public async Task<IActionResult> Crear()
        {
            ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
            return View();
        }

        // POST: Producto/Crear
        [HttpPost]
        [RequiereAdmin]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Producto producto, IFormFile? archivoImagen)
        {
            ModelState.Remove(nameof(producto.Categoria));

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
                return View(producto);
            }

            if (archivoImagen != null && archivoImagen.Length > 0)
            {
                var errorImagen = ValidarImagen(archivoImagen);
                if (errorImagen != null)
                {
                    ModelState.AddModelError("archivoImagen", errorImagen);
                    ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
                    return View(producto);
                }

                producto.Imagen = await GuardarImagenAsync(archivoImagen);
            }

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return RedirectToAction("Admin");
        }

        // GET: Producto/Editar/5
        [RequiereAdmin]
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
            return View(producto);
        }

        // POST: Producto/Editar/5
        [HttpPost]
        [RequiereAdmin]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Producto producto, IFormFile? archivoImagen)
        {
            ModelState.Remove(nameof(producto.Categoria));

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
                return View(producto);
            }

            var prodEnDb = await _context.Productos.FindAsync(producto.Id);
            if (prodEnDb == null)
            {
                return NotFound();
            }

            prodEnDb.Nombre = producto.Nombre;
            prodEnDb.Descripcion = producto.Descripcion;
            prodEnDb.Precio = producto.Precio;
            prodEnDb.Stock = producto.Stock;
            prodEnDb.CategoriaId = producto.CategoriaId;

            if (archivoImagen != null && archivoImagen.Length > 0)
            {
                var errorImagen = ValidarImagen(archivoImagen);
                if (errorImagen != null)
                {
                    ModelState.AddModelError("archivoImagen", errorImagen);
                    ViewBag.Categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
                    return View(producto);
                }

                // Eliminar imagen anterior si existe
                EliminarImagenAnterior(prodEnDb.Imagen);

                prodEnDb.Imagen = await GuardarImagenAsync(archivoImagen);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Admin");
        }

        // POST: Producto/Eliminar/5
        [HttpPost]
        [RequiereAdmin]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                EliminarImagenAnterior(producto.Imagen);
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Admin");
        }

        // ─── Helpers privados ────────────────────────────────────────────────────

        private string GetImagesFolder()
        {
            var webRoot = _webHostEnvironment.WebRootPath;
            if (string.IsNullOrEmpty(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var imagesFolder = Path.Combine(webRoot, "images");
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            return imagesFolder;
        }

        /// <summary>
        /// Valida tipo y tamaño del archivo de imagen.
        /// Retorna null si es válido, o el mensaje de error si no.
        /// </summary>
        private static string? ValidarImagen(IFormFile archivo)
        {
            if (archivo.Length > MaxImagenBytes)
                return $"La imagen no puede superar los {MaxImagenBytes / 1024 / 1024} MB.";

            var extension = Path.GetExtension(archivo.FileName);
            if (!ExtensionesPermitidas.Contains(extension))
                return "Solo se permiten imágenes en formato JPG, PNG, WEBP o GIF.";

            return null;
        }

        /// <summary>
        /// Guarda la imagen en wwwroot/images y retorna el nombre de archivo generado.
        /// </summary>
        private async Task<string> GuardarImagenAsync(IFormFile archivo)
        {
            var imagesFolder = GetImagesFolder();
            var safeFileName = Path.GetFileName(archivo.FileName);
            var extension = Path.GetExtension(safeFileName);
            var uniqueFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(imagesFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return uniqueFileName;
        }

        /// <summary>
        /// Elimina físicamente la imagen anterior de wwwroot/images si existe.
        /// </summary>
        private void EliminarImagenAnterior(string? nombreImagen)
        {
            if (string.IsNullOrEmpty(nombreImagen)) return;

            var filePath = Path.Combine(GetImagesFolder(), nombreImagen);
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
