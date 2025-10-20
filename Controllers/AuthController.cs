using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrotaTaxi.Data;
using FrotaTaxi.Models;
using System.Security.Cryptography;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.ComponentModel.DataAnnotations;

namespace FrotaTaxi.Controllers
{
    public class AuthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Auth/Login
        public IActionResult Login(string returnUrl = null)
        {
            // Se já estiver logado, redirecionar para home
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            var model = new LoginViewModel
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }

        // POST: Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var hashedPassword = HashPassword(model.Senha);
                
                var usuario = await _context.Usuarios
                    .Include(u => u.Perfil)
                    .ThenInclude(p => p.PerfilFuncionalidades)
                    .ThenInclude(pf => pf.Funcionalidade)
                    .FirstOrDefaultAsync(u => u.Login == model.Login && u.Senha == hashedPassword && u.Ativo);

                if (usuario != null)
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                        new Claim(ClaimTypes.Name, usuario.Nome),
                        new Claim("Login", usuario.Login),
                        new Claim(ClaimTypes.Email, usuario.Email),
                        new Claim("PerfilId", usuario.PerfilId.ToString()),
                        new Claim("PerfilNome", usuario.Perfil.Nome)
                    };

                    // Adicionar permissões como claims
                    Console.WriteLine($"DEBUG: Usuario {usuario.Login} - Perfil: {usuario.Perfil.Nome}");
                    Console.WriteLine($"DEBUG: Total de PerfilFuncionalidades: {usuario.Perfil.PerfilFuncionalidades.Count}");
                    
                    foreach (var pf in usuario.Perfil.PerfilFuncionalidades)
                    {
                        Console.WriteLine($"DEBUG: Funcionalidade: {pf.Funcionalidade.Controller} - Consultar: {pf.PodeConsultar} - Editar: {pf.PodeEditar}");
                        
                        if (pf.PodeConsultar)
                        {
                            claims.Add(new Claim("Permission", $"{pf.Funcionalidade.Controller}:View"));
                            Console.WriteLine($"DEBUG: Adicionada permissão: {pf.Funcionalidade.Controller}:View");
                        }
                        if (pf.PodeEditar)
                        {
                            claims.Add(new Claim("Permission", $"{pf.Funcionalidade.Controller}:Edit"));
                            Console.WriteLine($"DEBUG: Adicionada permissão: {pf.Funcionalidade.Controller}:Edit");
                        }
                    }
                    
                    var permissionCount = claims.Count(c => c.Type == "Permission");
                    Console.WriteLine($"DEBUG: Total de claims Permission: {permissionCount}");

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = model.LembrarMe,
                        ExpiresUtc = model.LembrarMe ? DateTimeOffset.UtcNow.AddDays(30) : DateTimeOffset.UtcNow.AddHours(8)
                    };

                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                        new ClaimsPrincipal(claimsIdentity), authProperties);

                    // Redirecionar para a URL de retorno ou home
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", "Login ou senha inválidos, ou usuário inativo.");
                }
            }

            return View(model);
        }

        // POST: Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }

        // GET: Auth/AccessDenied
        public IActionResult AccessDenied()
        {
            return View();
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

    public class LoginViewModel
    {
        [Required(ErrorMessage = "O login é obrigatório")]
        [Display(Name = "Login")]
        public string Login { get; set; }

        [Required(ErrorMessage = "A senha é obrigatória")]
        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        public string Senha { get; set; }

        [Display(Name = "Lembrar-me")]
        public bool LembrarMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
