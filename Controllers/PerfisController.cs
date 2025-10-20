using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrotaTaxi.Data;
using FrotaTaxi.Models;
using FrotaTaxi.Attributes;

namespace FrotaTaxi.Controllers
{
    [Permission("Perfis")]
    public class PerfisController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PerfisController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Perfis
        public async Task<IActionResult> Index()
        {
            return View(await _context.Perfis.ToListAsync());
        }

        // GET: Perfis/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfil = await _context.Perfis
                .Include(p => p.PerfilFuncionalidades)
                .ThenInclude(pf => pf.Funcionalidade)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (perfil == null)
            {
                return NotFound();
            }

            return View(perfil);
        }

        // GET: Perfis/Create
        [Permission("Perfis", "Edit")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Perfis/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Perfis", "Edit")]
        public async Task<IActionResult> Create([Bind("Id,Nome")] Perfil perfil)
        {
            if (ModelState.IsValid)
            {
                // Verificar se nome já existe
                var nomeExistente = await _context.Perfis
                    .FirstOrDefaultAsync(p => p.Nome == perfil.Nome);
                
                if (nomeExistente != null)
                {
                    ModelState.AddModelError("Nome", "Já existe um perfil com este nome.");
                    return View(perfil);
                }

                _context.Add(perfil);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Perfil criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }
            return View(perfil);
        }

        // GET: Perfis/Edit/5
        [Permission("Perfis", "Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfil = await _context.Perfis.FindAsync(id);
            if (perfil == null)
            {
                return NotFound();
            }
            return View(perfil);
        }

        // POST: Perfis/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Perfis", "Edit")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome")] Perfil perfil)
        {
            if (id != perfil.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar se nome já existe (exceto o próprio perfil)
                    var nomeExistente = await _context.Perfis
                        .FirstOrDefaultAsync(p => p.Nome == perfil.Nome && p.Id != perfil.Id);
                    
                    if (nomeExistente != null)
                    {
                        ModelState.AddModelError("Nome", "Já existe um perfil com este nome.");
                        return View(perfil);
                    }

                    _context.Update(perfil);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Perfil atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PerfilExists(perfil.Id))
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
            return View(perfil);
        }

        // GET: Perfis/Delete/5
        [Permission("Perfis", "Edit")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfil = await _context.Perfis
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (perfil == null)
            {
                return NotFound();
            }

            return View(perfil);
        }

        // POST: Perfis/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission("Perfis", "Edit")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var perfil = await _context.Perfis
                .Include(p => p.Usuarios)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (perfil != null)
            {
                // Verificar se há usuários usando este perfil
                if (perfil.Usuarios.Any())
                {
                    TempData["Error"] = "Não é possível excluir este perfil pois há usuários associados a ele.";
                    return RedirectToAction(nameof(Delete), new { id = id });
                }

                _context.Perfis.Remove(perfil);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Perfil excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Perfis/Permissoes/5
        [Permission("Perfis", "Edit")]
        public async Task<IActionResult> Permissoes(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfil = await _context.Perfis
                .Include(p => p.PerfilFuncionalidades)
                .ThenInclude(pf => pf.Funcionalidade)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (perfil == null)
            {
                return NotFound();
            }

            var funcionalidades = await _context.Funcionalidades.ToListAsync();
            
            var model = new PerfilPermissoesViewModel
            {
                PerfilId = perfil.Id,
                NomePerfil = perfil.Nome,
                Funcionalidades = funcionalidades.Select(f => new FuncionalidadePermissaoViewModel
                {
                    FuncionalidadeId = f.Id,
                    Nome = f.Nome,
                    Controller = f.Controller,
                    Action = f.Action,
                    PodeConsultar = perfil.PerfilFuncionalidades.Any(pf => pf.FuncionalidadeId == f.Id && pf.PodeConsultar),
                    PodeEditar = perfil.PerfilFuncionalidades.Any(pf => pf.FuncionalidadeId == f.Id && pf.PodeEditar)
                }).ToList()
            };

            return View(model);
        }

        // POST: Perfis/Permissoes/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Perfis", "Edit")]
        public async Task<IActionResult> Permissoes(PerfilPermissoesViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Remover permissões existentes
                var permissoesExistentes = await _context.PerfilFuncionalidades
                    .Where(pf => pf.PerfilId == model.PerfilId)
                    .ToListAsync();

                _context.PerfilFuncionalidades.RemoveRange(permissoesExistentes);

                // Adicionar novas permissões
                foreach (var funcionalidade in model.Funcionalidades)
                {
                    if (funcionalidade.PodeConsultar || funcionalidade.PodeEditar)
                    {
                        var perfilFuncionalidade = new PerfilFuncionalidade
                        {
                            PerfilId = model.PerfilId,
                            FuncionalidadeId = funcionalidade.FuncionalidadeId,
                            PodeConsultar = funcionalidade.PodeConsultar,
                            PodeEditar = funcionalidade.PodeEditar
                        };

                        _context.PerfilFuncionalidades.Add(perfilFuncionalidade);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Permissões atualizadas com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        private bool PerfilExists(int id)
        {
            return _context.Perfis.Any(e => e.Id == id);
        }
    }

    public class PerfilPermissoesViewModel
    {
        public int PerfilId { get; set; }
        public string NomePerfil { get; set; }
        public List<FuncionalidadePermissaoViewModel> Funcionalidades { get; set; } = new List<FuncionalidadePermissaoViewModel>();
    }

    public class FuncionalidadePermissaoViewModel
    {
        public int FuncionalidadeId { get; set; }
        public string Nome { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public bool PodeConsultar { get; set; }
        public bool PodeEditar { get; set; }
    }
}
