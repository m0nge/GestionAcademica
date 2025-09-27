using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GestionAcademica.Pages
{
    public class LogoutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Limpiar sesión
            HttpContext.Session.Clear();

            // Limpiar cookies
            HttpContext.Response.Cookies.Delete("DocenteRemembered");

            TempData["SuccessMessage"] = "Sesión cerrada exitosamente";
            return RedirectToPage("/Login");
        }
    }
}