using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FrotaTaxi.Data;
using FrotaTaxi.Models;
using FrotaTaxi.Attributes;

namespace FrotaTaxi.Controllers
{
    [Permission("Unidades")]
    public class UnidadesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UnidadesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Unidades
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Unidades";
            return View(await _context.Unidades.ToListAsync());
        }

        // GET: Unidades/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidade = await _context.Unidades
                .FirstOrDefaultAsync(m => m.Id == id);
            if (unidade == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Detalhes da Unidade";
            return View(unidade);
        }

        // GET: Unidades/Create
        public IActionResult Create()
        {
            ViewBag.Title = "Nova Unidade";
            return View();
        }

        // POST: Unidades/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nome,CPF,Apelido,Celular,Status,Carro,Placa,IMEI,PercentualCorrida")] Unidade unidade)
        {
            if (ModelState.IsValid)
            {
                _context.Add(unidade);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Title = "Nova Unidade";
            return View(unidade);
        }

        // GET: Unidades/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidade = await _context.Unidades.FindAsync(id);
            if (unidade == null)
            {
                return NotFound();
            }
            ViewBag.Title = "Editar Unidade";
            return View(unidade);
        }

        // POST: Unidades/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,CPF,Apelido,Celular,Status,Carro,Placa,IMEI,PercentualCorrida")] Unidade unidade)
        {
            if (id != unidade.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(unidade);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UnidadeExists(unidade.Id))
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
            ViewBag.Title = "Editar Unidade";
            return View(unidade);
        }

        // GET: Unidades/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var unidade = await _context.Unidades
                .FirstOrDefaultAsync(m => m.Id == id);
            if (unidade == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Excluir Unidade";
            return View(unidade);
        }

        // POST: Unidades/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Verifica se existem corridas vinculadas a esta unidade
            var possuiCorridas = await _context.Corridas.AnyAsync(c => c.UnidadeId == id);
            if (possuiCorridas)
            {
                TempData["ErrorMessage"] = "Não é possível excluir a unidade porque existem corridas vinculadas a ela.";
                return RedirectToAction(nameof(Index));
            }

            var unidade = await _context.Unidades.FindAsync(id);
            if (unidade != null)
            {
                _context.Unidades.Remove(unidade);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UnidadeExists(int id)
        {
            return _context.Unidades.Any(e => e.Id == id);
        }
    }
}
