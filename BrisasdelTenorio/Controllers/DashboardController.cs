using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrisasdelTenorio.Models;
using System.Security.Claims;

namespace BrisasdelTenorio.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly BrisasDelTenorioContext _context;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(BrisasDelTenorioContext context, ILogger<DashboardController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var hoy = DateTime.Now.Date;
                var hace7Dias = hoy.AddDays(-7);
                var hace14Dias = hoy.AddDays(-14);

                // 1. Pedidos de los últimos 7 días (semana actual)
                var pedidosSemanaActual = await _context.Pedidos
                    .Where(p => p.FechaPedido >= hace7Dias && p.Estado != "Cancelado")
                    .Include(p => p.Cliente)
                    .ToListAsync();

                // 2. Pedidos de los 7 días anteriores (semana pasada)
                var pedidosSemanaPasada = await _context.Pedidos
                    .Where(p => p.FechaPedido >= hace14Dias && p.FechaPedido < hace7Dias && p.Estado != "Cancelado")
                    .Include(p => p.Cliente)
                    .ToListAsync();

                // KPIs Semana Actual
                ViewBag.VentasSemana = pedidosSemanaActual.Sum(p => (decimal?)p.Total) ?? 0;
                ViewBag.PorCobrar = pedidosSemanaActual
                    .Where(p => p.EstadoPago == "Pendiente")
                    .Sum(p => (decimal?)p.Total) ?? 0;
                ViewBag.TotalPedidos = pedidosSemanaActual.Count;

                // Comparación con semana anterior
                var ventasSemanaPasada = pedidosSemanaPasada.Sum(p => (decimal?)p.Total) ?? 0;
                var diferencia = ViewBag.VentasSemana - ventasSemanaPasada;
                ViewBag.PorcentajeCambio = ventasSemanaPasada > 0
                    ? ((diferencia / ventasSemanaPasada) * 100).ToString("F1")
                    : "0";
                ViewBag.Mejoro = diferencia >= 0;

                // 3. Datos para la gráfica (ventas por día - últimos 7 días)
                var ventasPorDia = pedidosSemanaActual
                    .GroupBy(p => p.FechaPedido.Date)
                    .Select(g => new { Dia = g.Key, Total = g.Sum(p => p.Total) })
                    .OrderBy(x => x.Dia)
                    .ToList();

                // Asegurar 7 días completos en la gráfica
                var diasCompletos = Enumerable.Range(0, 7)
                    .Select(i => hoy.AddDays(-(6 - i)))
                    .ToList();

                var datosGrafica = diasCompletos.Select(dia => new
                {
                    Dia = dia,
                    Total = ventasPorDia.FirstOrDefault(v => v.Dia == dia)?.Total ?? 0
                }).ToList();

                ViewBag.LabelsDias = string.Join(",", datosGrafica.Select(x => x.Dia.ToString("dd/MM")));
                ViewBag.DataVentas = string.Join(",", datosGrafica.Select(x => x.Total));

                // 4. Últimos 10 pedidos (para la tabla con botones de acción)
                ViewBag.UltimosPedidos = await _context.Pedidos
                    .Include(p => p.Cliente)
                    .Include(p => p.DetallePedidos)
                        .ThenInclude(d => d.Producto)
                    .OrderByDescending(p => p.FechaPedido)
                    .Take(10)
                    .ToListAsync();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar el Dashboard");
                ViewBag.Error = "Error al cargar los datos. Intente de nuevo.";
                return View();
            }
        }

        // ✅ BOTONES DE ACCIÓN RÁPIDA: Actualizar estado del pedido (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(int pedidoId, string campo, string valor)
        {
            try
            {
                // Validaciones de seguridad
                var camposPermitidos = new[] { "Estado", "EstadoPago" };
                if (!camposPermitidos.Contains(campo))
                    return BadRequest(new { success = false, message = "Campo no válido" });

                var valoresPermitidos = campo == "Estado"
                    ? new[] { "Pendiente", "Preparado", "EnRuta", "Entregado", "Cancelado" }
                    : new[] { "Pendiente", "Parcial", "Pagado" };

                if (!valoresPermitidos.Contains(valor))
                    return BadRequest(new { success = false, message = "Valor no válido" });

                var pedido = await _context.Pedidos.FindAsync(pedidoId);
                if (pedido == null)
                    return NotFound(new { success = false, message = "Pedido no encontrado" });

                var valorAnterior = campo == "Estado" ? pedido.Estado : pedido.EstadoPago;

                // Actualizar
                if (campo == "Estado")
                {
                    pedido.Estado = valor;
                    if (valor == "Entregado")
                        pedido.FechaEntrega = DateOnly.FromDateTime(DateTime.Now);
                }
                else if (campo == "EstadoPago")
                {
                    pedido.EstadoPago = valor;
                    if (valor == "Pagado" && pedido.FechaPago == null)
                        pedido.FechaPago = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Pedido #{PedidoId}: {Campo} cambiado de '{Anterior}' a '{Nuevo}' por {Usuario}",
                    pedidoId, campo, valorAnterior, valor,
                    User.FindFirstValue(ClaimTypes.Name) ?? "Admin"
                );

                return Json(new
                {
                    success = true,
                    message = "Actualizado correctamente",
                    campo = campo,
                    valorAnterior = valorAnterior,
                    valorNuevo = valor
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar pedido #{PedidoId}", pedidoId);
                return StatusCode(500, new { success = false, message = "Error al actualizar" });
            }
        }

        // ✅ EXTRA: Obtener estadísticas en tiempo real (para auto-refresh)
        [HttpGet]
        public async Task<IActionResult> GetEstadisticas()
        {
            try
            {
                var hoy = DateTime.Now.Date;
                var hace7Dias = hoy.AddDays(-7);

                var estadisticas = new
                {
                    ventasSemana = await _context.Pedidos
                        .Where(p => p.FechaPedido >= hace7Dias && p.Estado != "Cancelado")
                        .SumAsync(p => (decimal?)p.Total) ?? 0,

                    porCobrar = await _context.Pedidos
                        .Where(p => p.FechaPedido >= hace7Dias
                                 && p.Estado != "Cancelado"
                                 && p.EstadoPago == "Pendiente")
                        .SumAsync(p => (decimal?)p.Total) ?? 0,

                    pedidosHoy = await _context.Pedidos
                        .CountAsync(p => p.FechaPedido.Date == hoy && p.Estado != "Cancelado"),

                    pedidosPendientes = await _context.Pedidos
                        .CountAsync(p => p.Estado == "Pendiente")
                };

                return Json(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas");
                return StatusCode(500, new { error = "Error al obtener estadísticas" });
            }
        }
    }
}