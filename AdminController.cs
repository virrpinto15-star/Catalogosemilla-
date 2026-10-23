using Microsoft.AspNetCore.Mvc;

namespace CatalogoPapa.Controllers
{
    public class AdminController : Controller
    {
        private readonly IConfiguration _configuration;

        public AdminController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // GET: /Admin/Login
        public IActionResult Login(string? returnUrl)
        {
            if (HttpContext.Session.GetString("AdminLogueado") == "true")
            {
                return RedirectToAction("Admin", "Producto");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string usuario, string password, string? returnUrl)
        {
            var adminUser = _configuration["Admin:Usuario"];
            var adminPass = _configuration["Admin:Password"];

            // Si las credenciales no están configuradas, denegar siempre
            if (string.IsNullOrWhiteSpace(adminUser) || string.IsNullOrWhiteSpace(adminPass))
            {
                ViewBag.Error = "El sistema no está configurado correctamente. Contacte al administrador.";
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

            if (!string.IsNullOrWhiteSpace(usuario) &&
                !string.IsNullOrWhiteSpace(password) &&
                usuario.Trim().Equals(adminUser.Trim(), StringComparison.OrdinalIgnoreCase) &&
                password.Trim() == adminPass.Trim())
            {
                HttpContext.Session.SetString("AdminLogueado", "true");

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Admin", "Producto");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Admin/Logout  (protegido con antiforgery token para evitar CSRF logout)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Remove("AdminLogueado");
            return RedirectToAction("Index", "Home");
        }
    }
}
