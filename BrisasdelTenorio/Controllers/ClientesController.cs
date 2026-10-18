using BrisasdelTenorio.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BrisasdelTenorio.Controllers 
{
    [Authorize]
    public class ClientesController : Controller
    {
        private readonly BrisasDelTenorioContext _context;

        public ClientesController(BrisasDelTenorioContext context)
        {
            _context = context;
        }

        // GET: CLIENTES
        public async Task<IActionResult> Index()
        {
            var clientesActivos = await _context.Clientes
                .Where(c => c.Activo == true)
                .ToListAsync();

            return View(clientesActivos);
        }

        // GET: CLIENTES/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.ClienteId == id);

            if (cliente == null)
            {
                return NotFound();
            }

            // Traemos solo la lista de precios especiales ya cargada
            ViewBag.PreciosEspeciales = await _context.PreciosEspeciales
                .Include(pe => pe.Producto)
                .Where(pe => pe.ClienteId == cliente.ClienteId)
                .ToListAsync();

            return View(cliente);
        }

        // GET: CLIENTES/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: CLIENTES/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Telefono,Email,Direccion,Activo")] Cliente cliente)
        {
            cliente.TokenPedido = GenerarTokenUnico();
            cliente.FechaCreacion = DateTime.Now;
            cliente.Activo = true;

            ModelState.Remove("TokenPedido");
            ModelState.Remove("FechaCreacion");

            if (ModelState.IsValid)
            {
                _context.Add(cliente);
                await _context.SaveChangesAsync();

                TempData["NuevoToken"] = cliente.TokenPedido;
                TempData["NombreCliente"] = cliente.Nombre;

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return View(cliente);
        }

        // Método auxiliar para generar tokens únicos
        private string GenerarTokenUnico(int longitud = 20)
        {
            const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
            var random = new Random();
            var token = new char[longitud];

            for (int i = 0; i < longitud; i++)
            {
                token[i] = caracteres[random.Next(caracteres.Length)];
            }

            return new string(token);
        }

        // GET: CLIENTES/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            return View(cliente);
        }

        // POST: CLIENTES/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ClienteId,Nombre,Telefono,Email,Direccion,Activo")] Cliente cliente)
        {
            if (id != cliente.ClienteId)
            {
                return NotFound();
            }

            var clienteOriginal = await _context.Clientes.FindAsync(id);
            if (clienteOriginal == null)
            {
                return NotFound();
            }

            clienteOriginal.Nombre = cliente.Nombre;
            clienteOriginal.Telefono = cliente.Telefono;
            clienteOriginal.Email = cliente.Email;
            clienteOriginal.Direccion = cliente.Direccion;
            clienteOriginal.Activo = cliente.Activo;

            ModelState.Remove("TokenPedido");
            ModelState.Remove("FechaCreacion");

            if (ModelState.IsValid)
            {
                try
                {
                    await _context.SaveChangesAsync();
                    TempData["MensajeExito"] = "Cliente actualizado correctamente.";
                }
                catch (DbUpdateException ex)
                {
                    ViewBag.Errores = new List<string> { ex.InnerException?.Message ?? ex.Message };
                    return View(cliente);
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return View(cliente);
        }

        // GET: CLIENTES/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.ClienteId == id);

            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: CLIENTES/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                cliente.Activo = false;
                cliente.Nombre = $"[Inactivo] {cliente.Nombre}";
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "Cliente desactivado correctamente (se conserva el historial de pedidos).";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/AgregarPrecioEspecial/5
        public async Task<IActionResult> AgregarPrecioEspecial(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();

            ViewBag.Cliente = cliente;
            ViewBag.Productos = new SelectList(await _context.Productos.Where(p => p.Activo).ToListAsync(), "ProductoId", "Nombre");

            return View();
        }

        // POST: Clientes/AgregarPrecioEspecial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarPrecioEspecial(int clienteId, int productoId, decimal precio)
        {
            var precioExistente = await _context.PreciosEspeciales
                .FirstOrDefaultAsync(pe => pe.ClienteId == clienteId && pe.ProductoId == productoId);

            if (precioExistente != null)
            {
                precioExistente.Precio = precio;
            }
            else
            {
                var nuevoPrecio = new PrecioEspecial
                {
                    ClienteId = clienteId,
                    ProductoId = productoId,
                    Precio = precio
                };
                _context.PreciosEspeciales.Add(nuevoPrecio);
            }

            await _context.SaveChangesAsync();
            TempData["MensajeExito"] = "Precio especial guardado correctamente.";

            return RedirectToAction("Details", new { id = clienteId });
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.ClienteId == id);
        }
    }
}