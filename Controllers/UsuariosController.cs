using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FrotaTaxi.Data;
using FrotaTaxi.Models;
using System.Security.Cryptography;
using System.Text;
using System.ComponentModel.DataAnnotations;
using FrotaTaxi.Attributes;

namespace FrotaTaxi.Controllers
{
    [Permission("Usuarios")]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Usuarios
        public async Task<IActionResult> Index()
        {
            var usuarios = _context.Usuarios.Include(u => u.Perfil);
            return View(await usuarios.ToListAsync());
        }

        // GET: Usuarios/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // GET: Usuarios/Create
        [Permission("Usuarios", "Edit")]
        public IActionResult Create()
        {
            ViewBag.PerfilId = new SelectList(_context.Perfis, "Id", "Nome");
            return View();
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Usuarios", "Edit")]
        public async Task<IActionResult> Create([Bind("Nome,Login,Email,Senha,PerfilId,Ativo")] Usuario usuario)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Verificar se login já existe
                    var loginExistente = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Login == usuario.Login);
                    
                    if (loginExistente != null)
                    {
                        ModelState.AddModelError("Login", "Este login já está em uso.");
                        ViewBag.PerfilId = new SelectList(_context.Perfis, "Id", "Nome", usuario.PerfilId);
                        return View(usuario);
                    }

                    // Verificar se email já existe
                    var emailExistente = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Email == usuario.Email);
                
                    if (emailExistente != null)
                    {
                        ModelState.AddModelError("Email", "Este e-mail já está em uso.");
                        ViewBag.PerfilId = new SelectList(_context.Perfis, "Id", "Nome", usuario.PerfilId);
                        return View(usuario);
                    }

                    // Criptografar senha
                    usuario.Senha = HashPassword(usuario.Senha);
                    usuario.DataCriacao = DateTime.Now;

                    _context.Add(usuario);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Usuário criado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Erro ao salvar usuário: {ex.Message}");
            }
            ViewBag.PerfilId = new SelectList(_context.Perfis, "Id", "Nome", usuario.PerfilId);
            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        [Permission("Usuarios", "Edit")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            ViewData["PerfilId"] = new SelectList(_context.Perfis, "Id", "Nome", usuario.PerfilId);
            return View(usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Usuarios", "Edit")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,Login,Email,PerfilId,Ativo,DataCriacao")] Usuario usuario)
        {
            Console.WriteLine($"DEBUG EDIT: ID recebido: {id}");
            Console.WriteLine($"DEBUG EDIT: Usuario.Id: {usuario.Id}");
            Console.WriteLine($"DEBUG EDIT: Usuario.Nome: {usuario.Nome}");
            Console.WriteLine($"DEBUG EDIT: Usuario.PerfilId: {usuario.PerfilId}");
            Console.WriteLine($"DEBUG EDIT: Usuario.Ativo: {usuario.Ativo}");
            
            if (id != usuario.Id)
            {
                return NotFound();
            }

            // Como a propriedade Senha é [Required] no modelo, mas não faz parte do formulário de edição,
            // removemos sua entrada do ModelState para evitar validação indevida aqui.
            ModelState.Remove("Senha");

            if (ModelState.IsValid)
            {
                try
                {
                    // Verificar se login já existe (exceto o próprio usuário)
                    var loginExistente = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Login == usuario.Login && u.Id != usuario.Id);
                    
                    if (loginExistente != null)
                    {
                        ModelState.AddModelError("Login", "Este login já está em uso.");
                        ViewData["PerfilId"] = new SelectList(_context.Perfis, "Id", "Nome", usuario.PerfilId);
                        return View(usuario);
                    }

                    // Verificar se email já existe (exceto o próprio usuário)
                    var emailExistente = await _context.Usuarios
                        .FirstOrDefaultAsync(u => u.Email == usuario.Email && u.Id != usuario.Id);
                    
                    if (emailExistente != null)
                    {
                        ModelState.AddModelError("Email", "Este e-mail já está em uso.");
                        ViewData["PerfilId"] = new SelectList(_context.Perfis, "Id", "Nome", usuario.PerfilId);
                        return View(usuario);
                    }

                    // Manter a senha existente
                    var usuarioExistente = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                    usuario.Senha = usuarioExistente.Senha;

                    Console.WriteLine($"DEBUG EDIT: Antes do Update - PerfilId: {usuario.PerfilId}");
                    Console.WriteLine($"DEBUG EDIT: Usuario existente PerfilId: {usuarioExistente.PerfilId}");
                    
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                    
                    Console.WriteLine($"DEBUG EDIT: Após SaveChanges - sucesso!");
                    TempData["Success"] = "Usuário atualizado com sucesso!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id))
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
            ViewData["PerfilId"] = new SelectList(_context.Perfis, "Id", "Nome", usuario.PerfilId);
            return View(usuario);
        }

        // GET: Usuarios/Delete/5
        [Permission("Usuarios", "Edit")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Perfil)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Permission("Usuarios", "Edit")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null)
            {
                _context.Usuarios.Remove(usuario);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Usuário excluído com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Usuarios/AlterarSenha/5
        [Permission("Usuarios", "Edit")]
        public async Task<IActionResult> AlterarSenha(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            var model = new AlterarSenhaViewModel
            {
                UsuarioId = usuario.Id,
                NomeUsuario = usuario.Nome
            };

            return View(model);
        }

        // POST: Usuarios/AlterarSenha/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Permission("Usuarios", "Edit")]
        public async Task<IActionResult> AlterarSenha(AlterarSenhaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var usuario = await _context.Usuarios.FindAsync(model.UsuarioId);
                if (usuario == null)
                {
                    return NotFound();
                }

                usuario.Senha = HashPassword(model.NovaSenha);
                _context.Update(usuario);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = "Senha alterada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }

    public class AlterarSenhaViewModel
    {
        public int UsuarioId { get; set; }
        public string NomeUsuario { get; set; }

        [Required(ErrorMessage = "A nova senha é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A senha deve ter entre 6 e 100 caracteres")]
        [Display(Name = "Nova Senha")]
        [DataType(DataType.Password)]
        public string NovaSenha { get; set; }

        [Required(ErrorMessage = "A confirmação da senha é obrigatória")]
        [Display(Name = "Confirmar Nova Senha")]
        [DataType(DataType.Password)]
        [Compare("NovaSenha", ErrorMessage = "As senhas não coincidem")]
        public string ConfirmarSenha { get; set; }
    }
}
