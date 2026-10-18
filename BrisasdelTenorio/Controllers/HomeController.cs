using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using BrisasdelTenorio.Models;
using Microsoft.EntityFrameworkCore;

namespace BrisasdelTenorio.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly BrisasDelTenorioContext _context;

        public HomeController(ILogger<HomeController> logger, BrisasDelTenorioContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Página principal (Landing Page)
        public IActionResult Index()
        {
            return View();
        }

        // Página Nosotros
        public IActionResult Nosotros()
        {
            return View();
        }

        // Página Productos (muestra productos reales de la BD)
        public async Task<IActionResult> Productos()
        {
            var productos = await _context.Productos
                .Where(p => p.Activo == true)
                .ToListAsync();

            return View(productos);
        }

        // Página Contacto
        public IActionResult Contacto()
        {
            return View();
        }

        // Página Privacy (la que viene por defecto)
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}