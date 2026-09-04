using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrisasdelTenorio.Models;
using System.Security.Claims;

namespace BrisasdelTenorio.Controllers
{
    public class AccountController : Controller
    {
        private readonly BrisasDelTenorioContext _context;

        public AccountController(BrisasDelTenorioContext context)
        {
            _context = context;
        }

        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(string usuario, string password)
        {
            // 1. Buscar el usuario en la base de datos
            var user = await _context.UsuariosAdmins
                .FirstOrDefaultAsync(u => u.NombreUsuario == usuario && u.Activo == true);

            if (user == null)
            {
                ViewBag.Error = "Usuario no encontrado o inactivo.";
                return View();
            }

            // 2. Verificar la contraseña con BCrypt
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewBag.Error = "Contraseña incorrecta.";
                return View();
            }

            // 3. Crear las "Claims" (la identidad del usuario)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Nombre),
                new Claim(ClaimTypes.Role, user.Rol)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // "Recuérdame"
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            // 4. Iniciar sesión (guardar la cookie)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // 5. Redirigir al Dashboard
            return RedirectToAction("Index", "Dashboard");
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }
    }
}