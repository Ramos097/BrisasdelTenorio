using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrisasdelTenorio.Models;
using Microsoft.AspNetCore.Authorization;

namespace BrisasdelTenorio.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
       
        private readonly BrisasDelTenorioContext _context;

        public DashboardController(BrisasDelTenorioContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Calcular datos de los últimos 7 días
            var hace7Dias = DateTime.Now.Date.AddDays(-7);

            // Obtener pedidos de la semana (sin cancelados)
            var pedidosSemana = await _context.Pedidos
                .Where(p => p.FechaPedido >= hace7Dias && p.Estado != "Cancelado")
                .ToListAsync();

            // Guardar totales en ViewBag
            ViewBag.VentasSemana = pedidosSemana.Sum(p => p.Total);
            ViewBag.PorCobrar = pedidosSemana.Where(p => p.EstadoPago == "Pendiente").Sum(p => p.Total);
            ViewBag.TotalPedidos = pedidosSemana.Count;

            // Preparar datos para la gráfica (ventas por día)
            var ventasPorDia = pedidosSemana
                .GroupBy(p => p.FechaPedido.Date)
                .Select(g => new { Dia = g.Key, Total = g.Sum(p => p.Total) })
                .OrderBy(x => x.Dia)
                .ToList();

            ViewBag.LabelsDias = string.Join(",", ventasPorDia.Select(x => x.Dia.ToString("dd/MM")));
            ViewBag.DataVentas = string.Join(",", ventasPorDia.Select(x => x.Total));

            return View();
        }
    }
}