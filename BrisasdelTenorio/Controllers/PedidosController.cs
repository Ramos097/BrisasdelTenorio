using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrisasdelTenorio.Models;
using System.Linq;

namespace BrisasdelTenorio.Controllers
{
    public class PedidosController : Controller
    {
        private readonly BrisasDelTenorioContext _context;

        public PedidosController(BrisasDelTenorioContext context)
        {
            _context = context;
        }

        // GET: /Pedidos/Cliente?token=abcXYZ123
        public async Task<IActionResult> Cliente(string token)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.TokenPedido == token && c.Activo == true);

            if (cliente == null)
            {
                ViewBag.ErrorMessage = "El link de pedido no es válido o ha expirado.";
                return View("ErrorCliente");
            }

            // 1. Obtener todos los productos activos
            var productosBase = await _context.Productos
                .Where(p => p.Activo == true)
                .ToListAsync();

            // 2. Obtener SOLO los precios especiales de ESTE cliente
            var preciosEspeciales = await _context.PreciosEspeciales
                .Where(pe => pe.ClienteId == cliente.ClienteId)
                .ToListAsync();

            // 3. Fusionar: Si el producto tiene precio especial, usarlo. Si no, usar el precio base.
            var productosParaMostrar = productosBase.Select(p =>
            {
                var precioEsp = preciosEspeciales.FirstOrDefault(pe => pe.ProductoId == p.ProductoId);
                if (precioEsp != null)
                {
                    p.Precio = precioEsp.Precio; // Sobrescribimos el precio temporalmente para la vista
                }
                return p;
            }).ToList();

            ViewBag.Cliente = cliente;
            ViewBag.Productos = productosParaMostrar;

            return View();
        }

        // POST: /Pedidos/GuardarPedido
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuardarPedido(string token, Dictionary<int, int> cantidades)
        {
            // 1. Buscar al cliente por su token
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.TokenPedido == token && c.Activo == true);

            if (cliente == null)
            {
                ViewBag.ErrorMessage = "Cliente no encontrado";
                return View("ErrorCliente");
            }

            // 2. Verificar que haya al menos un producto con cantidad > 0
            if (cantidades == null || !cantidades.Any(c => c.Value > 0))
            {
                TempData["Error"] = "Debes seleccionar al menos un producto";
                return RedirectToAction("Cliente", new { token = token });
            }

            // 3. Crear el pedido
            var pedido = new Pedido
            {
                ClienteId = cliente.ClienteId,
                FechaPedido = DateTime.Now,
                FechaEntrega = DateOnly.FromDateTime(DateTime.Now.AddDays(2)),
                Estado = "Pendiente",
                EstadoPago = "Pendiente",
                Total = 0, // Lo calcularemos después
                Observaciones = ""
            };

            // 4. Agregar el pedido a la base de datos
            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync(); // Guardamos para obtener el PedidoId

            // 5. Crear los detalles del pedido

            // OBTENER PRECIOS ESPECIALES UNA SOLA VEZ (Eficiencia)
            var preciosEspecialesDict = await _context.PreciosEspeciales
                .Where(pe => pe.ClienteId == cliente.ClienteId)
                .ToDictionaryAsync(pe => pe.ProductoId, pe => pe.Precio);

            decimal totalGeneral = 0;

            foreach (var item in cantidades)
            {
                int productoId = item.Key;
                int cantidad = item.Value;

                // Si la cantidad es 0 o menor, lo saltamos
                if (cantidad <= 0) continue;

                // Buscar el producto
                var producto = await _context.Productos.FindAsync(productoId);
                if (producto == null) continue;

                // 🔑 AQUÍ ESTÁ LA MAGIA:
                // Si existe en el diccionario de precios especiales, úsalo. Si no, usa el precio base.
                decimal precioFinal = preciosEspecialesDict.ContainsKey(productoId)
                    ? preciosEspecialesDict[productoId]
                    : producto.Precio;

                // Calcular subtotal con el precio correcto
                decimal subtotal = precioFinal * cantidad;
                totalGeneral += subtotal;

                // Crear el detalle
                var detalle = new DetallePedido
                {
                    PedidoId = pedido.PedidoId,
                    ProductoId = producto.ProductoId,
                    Cantidad = cantidad,
                    PrecioUnitario = precioFinal, // Guardamos el precio real que se le cobró (950 o 1000)
                    Subtotal = subtotal
                };

                _context.DetallePedidos.Add(detalle);
            }

            // 6. Actualizar el total del pedido
            pedido.Total = totalGeneral;
            _context.Pedidos.Update(pedido);

            await _context.SaveChangesAsync();

            // 7. Redirigir a la confirmación
            return RedirectToAction("Confirmacion", new { token = token, pedidoId = pedido.PedidoId });
        }

        // GET: /Pedidos/Confirmacion
        public async Task<IActionResult> Confirmacion(string token, int pedidoId)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.TokenPedido == token);

            var pedido = await _context.Pedidos
                .Include(p => p.DetallePedidos)
                .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(p => p.PedidoId == pedidoId);

            if (cliente == null || pedido == null)
            {
                // Corregido para usar la misma vista de error segura que creamos antes
                ViewBag.ErrorMessage = "Pedido no encontrado o link inválido.";
                return View("ErrorCliente");
            }

            ViewBag.Cliente = cliente;
            ViewBag.Pedido = pedido;

            return View();
        }
    }
}