using CatalogoPapa.Models;
using CatalogoPapa.Data;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CatalogoPapa.Controllers
{
    public class CarritoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CarritoSessionKey = "Carrito";

        public CarritoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Carrito
        public IActionResult Index()
        {
            var carrito = ObtenerCarrito();
            return View(carrito);
        }

        private List<CarritoItem> ObtenerCarrito()
        {
            var carritoJson = HttpContext.Session.GetString(CarritoSessionKey);

            if (string.IsNullOrEmpty(carritoJson))
            {
                return new List<CarritoItem>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<CarritoItem>>(carritoJson) ?? new List<CarritoItem>();
            }
            catch
            {
                return new List<CarritoItem>();
            }
        }

        private void GuardarCarrito(List<CarritoItem> carrito)
        {
            var carritoJson = JsonSerializer.Serialize(carrito);
            HttpContext.Session.SetString(CarritoSessionKey, carritoJson);
        }

        // POST: /Carrito/Agregar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Agregar(int id, string? returnUrl)
        {
            var producto = _context.Productos.FirstOrDefault(p => p.Id == id);
            if (producto == null)
            {
                return NotFound();
            }

            if (producto.Stock <= 0)
            {
                return RedirectToAction("Index");
            }

            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(p => p.ProductoId == id);

            if (item != null)
            {
                if (item.Cantidad < producto.Stock)
                {
                    item.Cantidad++;
                }
            }
            else
            {
                carrito.Add(new CarritoItem
                {
                    ProductoId = producto.Id,
                    Nombre = producto.Nombre,
                    Precio = producto.Precio,
                    Cantidad = 1,
                    Imagen = producto.Imagen ?? string.Empty
                });
            }

            GuardarCarrito(carrito);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        // POST: /Carrito/Aumentar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Aumentar(int id)
        {
            var carrito = ObtenerCarrito();
            var producto = _context.Productos.FirstOrDefault(p => p.Id == id);
            var item = carrito.FirstOrDefault(p => p.ProductoId == id);

            if (item != null && producto != null && item.Cantidad < producto.Stock)
            {
                item.Cantidad++;
                GuardarCarrito(carrito);
            }

            return RedirectToAction("Index");
        }

        // POST: /Carrito/Disminuir
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Disminuir(int id)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(p => p.ProductoId == id);

            if (item != null)
            {
                if (item.Cantidad > 1)
                {
                    item.Cantidad--;
                }
                else
                {
                    carrito.Remove(item);
                }

                GuardarCarrito(carrito);
            }

            return RedirectToAction("Index");
        }

        // POST: /Carrito/Eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var carrito = ObtenerCarrito();
            var item = carrito.FirstOrDefault(p => p.ProductoId == id);

            if (item != null)
            {
                carrito.Remove(item);
                GuardarCarrito(carrito);
            }

            return RedirectToAction("Index");
        }

        // POST: /Carrito/Vaciar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Vaciar()
        {
            HttpContext.Session.Remove(CarritoSessionKey);
            return RedirectToAction("Index");
        }
    }
}
