using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FrotaTaxi.Data;
using FrotaTaxi.Models;
using FrotaTaxi.Attributes;

namespace FrotaTaxi.Controllers
{
    [Permission("Trechos")]
    public class TrechosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrechosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trechos
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Trechos";
            var trechos = _context.Trechos.Include(t => t.Cliente);
            return View(await trechos.ToListAsync());
        }

        // GET: Trechos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trecho = await _context.Trechos
                .Include(t => t.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trecho == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Detalhes do Trecho";
            return View(trecho);
        }

        // GET: Trechos/Create
        public IActionResult Create()
        {
            ViewBag.Title = "Novo Trecho";
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Status == StatusEnum.Ativo), "Id", "Nome");
            return View();
        }

        // POST: Trechos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClienteId,NomeTrecho,TrechoInicio,TrechoTermino,Valor,Status")] Trecho trecho)
        {
            Console.WriteLine($"DEBUG TRECHO CREATE: POST method called");
            Console.WriteLine($"DEBUG TRECHO CREATE: ModelState.IsValid = {ModelState.IsValid}");
            Console.WriteLine($"DEBUG TRECHO CREATE: ClienteId = {trecho.ClienteId}, NomeTrecho = '{trecho.NomeTrecho}'");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("DEBUG TRECHO CREATE: ModelState errors:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            
            if (ModelState.IsValid)
            {
                Console.WriteLine($"DEBUG TRECHO CREATE: Adding trecho to context");
                _context.Add(trecho);
                Console.WriteLine($"DEBUG TRECHO CREATE: Calling SaveChangesAsync");
                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG TRECHO CREATE: SaveChangesAsync completed successfully");
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Title = "Novo Trecho";
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Status == StatusEnum.Ativo), "Id", "Nome", trecho.ClienteId);
            return View(trecho);
        }

        // GET: Trechos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trecho = await _context.Trechos.FindAsync(id);
            if (trecho == null)
            {
                return NotFound();
            }
            ViewBag.Title = "Editar Trecho";
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Status == StatusEnum.Ativo), "Id", "Nome", trecho.ClienteId);
            return View(trecho);
        }

        // POST: Trechos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,NomeTrecho,TrechoInicio,TrechoTermino,Valor,Status")] Trecho trecho)
        {
            if (id != trecho.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trecho);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrechoExists(trecho.Id))
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
            ViewBag.Title = "Editar Trecho";
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Status == StatusEnum.Ativo), "Id", "Nome", trecho.ClienteId);
            return View(trecho);
        }

        // GET: Trechos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trecho = await _context.Trechos
                .Include(t => t.Cliente)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trecho == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Excluir Trecho";
            return View(trecho);
        }

        // POST: Trechos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trecho = await _context.Trechos.FindAsync(id);
            if (trecho != null)
            {
                _context.Trechos.Remove(trecho);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // API method to get trechos by cliente
        [HttpGet]
        public async Task<IActionResult> GetTrechosByCliente(int clienteId)
        {
            var trechos = await _context.Trechos
                .Where(t => t.ClienteId == clienteId && t.Status == StatusEnum.Ativo)
                .Select(t => new { t.Id, t.NomeTrecho, t.Valor })
                .ToListAsync();
            return Json(trechos);
        }

        private bool TrechoExists(int id)
        {
            return _context.Trechos.Any(e => e.Id == id);
        }
    }
}
