using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CatalogoPapa.Filters
{
    /// <summary>
    /// Filtro que protege acciones de administración.
    /// Si el usuario no está logueado, lo redirige al login.
    /// </summary>
    public class RequiereAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var session = context.HttpContext.Session.GetString("AdminLogueado");

            if (session != "true")
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Admin", new { returnUrl });
            }

            base.OnActionExecuting(context);
        }
    }
}

