
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BrisasdelTenorio.Models;
using Microsoft.AspNetCore.Authorization;


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
        return View(await _context.Clientes.ToListAsync());
    }

    // GET: CLIENTES/Details/5
    public async Task<IActionResult> Details(int? clienteid)
    {
        if (clienteid == null)
        {
            return NotFound();
        }

        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(m => m.ClienteId == clienteid);
        if (cliente == null)
        {
            return NotFound();
        }

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
        // Generar token único INMEDIATAMENTE (antes de validar)
        cliente.TokenPedido = GenerarTokenUnico();
        cliente.FechaCreacion = DateTime.Now;
        cliente.Activo = true;

        // Eliminar errores de validación de campos que generamos por código
        ModelState.Remove("TokenPedido");
        ModelState.Remove("FechaCreacion");

        if (ModelState.IsValid)
        {
            _context.Add(cliente);
            await _context.SaveChangesAsync();

            // Guardar el token en TempData para mostrarlo
            TempData["NuevoToken"] = cliente.TokenPedido;
            TempData["NombreCliente"] = cliente.Nombre;

            return RedirectToAction(nameof(Index));
        }

        // Si hay error, mostrar qué está fallando
        ViewBag.Errores = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

        return View(cliente);
    }

    // Método auxiliar para generar tokens únicos
    private string GenerarTokenUnico(int longitud = 20)
    {
        const string caracteres = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // Sin 0, O, 1, I para evitar confusión
        var random = new Random();
        var token = new char[longitud];

        for (int i = 0; i < longitud; i++)
        {
            token[i] = caracteres[random.Next(caracteres.Length)];
        }

        return new string(token);
    }



    // GET: CLIENTES/Edit/5
    public async Task<IActionResult> Edit(int? clienteid)
    {
        if (clienteid == null)
        {
            return NotFound();
        }

        var cliente = await _context.Clientes.FindAsync(clienteid);
        if (cliente == null)
        {
            return NotFound();
        }
        return View(cliente);
    }

    // POST: CLIENTES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? clienteid, [Bind("ClienteId,Nombre,Telefono,Email,Direccion,TokenPedido,Activo,FechaCreacion,Pedidos")] Cliente cliente)
    {
        if (clienteid != cliente.ClienteId)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(cliente);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClienteExists(cliente.ClienteId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(cliente);
    }

    // GET: CLIENTES/Delete/5
    public async Task<IActionResult> Delete(int? clienteid)
    {
        if (clienteid == null)
        {
            return NotFound();
        }

        var cliente = await _context.Clientes
            .FirstOrDefaultAsync(m => m.ClienteId == clienteid);
        if (cliente == null)
        {
            return NotFound();
        }

        return View(cliente);
    }

    // POST: CLIENTES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? clienteid)
    {
        var cliente = await _context.Clientes.FindAsync(clienteid);
        if (cliente != null)
        {
            _context.Clientes.Remove(cliente);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ClienteExists(int? clienteid)
    {
        return _context.Clientes.Any(e => e.ClienteId == clienteid);
    }
}
