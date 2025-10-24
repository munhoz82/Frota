using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FrotaTaxi.Data;
using FrotaTaxi.Models;
using System.Text.Json;
using FrotaTaxi.Attributes;

namespace FrotaTaxi.Controllers
{
    [Permission("Clientes")]
    public class ClientesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public ClientesController(ApplicationDbContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
        }

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            ViewBag.Title = "Clientes";
            return View(await _context.Clientes.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.CentrosCusto)
                .Include(c => c.UsuariosAutorizados)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (cliente == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Detalhes do Cliente";
            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            ViewBag.Title = "Novo Cliente";
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(cliente);
                    await _context.SaveChangesAsync();

                    // Add CentrosCusto from form data
                    var centrosCustoData = Request.Form.Where(x => x.Key.StartsWith("centrosCusto[")).ToList();
                    Console.WriteLine($"DEBUG: Found {centrosCustoData.Count} centro custo form fields");
                    if (centrosCustoData.Any())
                    {
                        var centrosCustoGroups = centrosCustoData
                            .GroupBy(x => x.Key.Split('[')[1].Split(']')[0])
                            .Where(g => g.Any(x => x.Key.Contains(".Codigo") && !string.IsNullOrEmpty(x.Value)));

                        foreach (var group in centrosCustoGroups)
                        {
                            var codigoItem = group.FirstOrDefault(x => x.Key.Contains(".Codigo"));
                            var descricaoItem = group.FirstOrDefault(x => x.Key.Contains(".Descricao"));
                            var codigo = codigoItem.Value.ToString() ?? "";
                            var descricao = descricaoItem.Value.ToString() ?? "";

                            if (!string.IsNullOrEmpty(codigo) && !string.IsNullOrEmpty(descricao))
                            {
                                var centroCusto = new CentroCusto
                                {
                                    ClienteId = cliente.Id,
                                    Codigo = codigo,
                                    Descricao = descricao
                                };
                                _context.CentrosCusto.Add(centroCusto);
                                Console.WriteLine($"DEBUG: Added centro custo - Codigo: {codigo}, Descricao: {descricao}");
                            }
                        }
                    }

                    // Add UsuariosAutorizados from form data
                    var usuariosData = Request.Form.Where(x => x.Key.StartsWith("usuariosAutorizados[")).ToList();
                    Console.WriteLine($"DEBUG: Found {usuariosData.Count} usuario autorizado form fields");
                    if (usuariosData.Any())
                    {
                        var usuariosGroups = usuariosData
                            .GroupBy(x => x.Key.Split('[')[1].Split(']')[0])
                            .Where(g => g.Any(x => x.Key.Contains(".Nome") && !string.IsNullOrEmpty(x.Value)));

                        foreach (var group in usuariosGroups)
                        {
                            var nome = group.Where(x => x.Key.Contains(".Nome")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var funcional = group.Where(x => x.Key.Contains(".Funcional")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var tipoSolicitante = group.Where(x => x.Key.Contains(".TipoSolicitante")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var status = group.Where(x => x.Key.Contains(".Status")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var telefone1 = group.Where(x => x.Key.Contains(".Telefone1")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var telefone2 = group.Where(x => x.Key.Contains(".Telefone2")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var email = group.Where(x => x.Key.Contains(".Email")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";

                            // Telefone2 não será mais obrigatório no Create (alinha com Edit)
                            if (!string.IsNullOrEmpty(nome) && !string.IsNullOrEmpty(email) && 
                                !string.IsNullOrEmpty(tipoSolicitante) && !string.IsNullOrEmpty(status) &&
                                !string.IsNullOrEmpty(telefone1))
                            {
                                if (Enum.TryParse<TipoSolicitanteEnum>(tipoSolicitante, out var tipoSolicitanteEnum) &&
                                    Enum.TryParse<StatusEnum>(status, out var statusEnum))
                                {
                                    var usuario = new UsuarioAutorizado
                                    {
                                        ClienteId = cliente.Id,
                                        Nome = nome,
                                        Funcional = funcional,
                                        TipoSolicitante = tipoSolicitanteEnum,
                                        Status = statusEnum,
                                        Telefone1 = telefone1,
                                        Telefone2 = telefone2,
                                        Email = email
                                    };
                                    _context.UsuariosAutorizados.Add(usuario);
                                    Console.WriteLine($"DEBUG: Added usuario - Nome: {nome}, Email: {email}");
                                }
                                else
                                {
                                    Console.WriteLine($"DEBUG: Failed to parse enums - TipoSolicitante: {tipoSolicitante}, Status: {status}");
                                }
                            }
                            else
                            {
                                Console.WriteLine($"DEBUG: Failed validation - Nome: '{nome}', Email: '{email}', TipoSolicitante: '{tipoSolicitante}', Status: '{status}', Telefone1: '{telefone1}', Telefone2: '{telefone2}'");
                            }
                        }
                    }

                    Console.WriteLine("DEBUG: Calling SaveChangesAsync...");
                    await _context.SaveChangesAsync();
                    Console.WriteLine("DEBUG: SaveChangesAsync completed successfully");
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException ex)
                {
                    Console.WriteLine($"ERROR CREATE: DbUpdateException - {ex.Message} | Inner: {ex.InnerException?.Message}");
                    ModelState.AddModelError(string.Empty, "Não foi possível salvar o cliente. Verifique os dados informados e tente novamente.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR CREATE: Exception - {ex.Message}");
                    ModelState.AddModelError(string.Empty, "Ocorreu um erro inesperado ao salvar o cliente. Tente novamente.");
                }
            }
            ViewBag.Title = "Novo Cliente";
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.CentrosCusto)
                .Include(c => c.UsuariosAutorizados)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (cliente == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Editar Cliente";
            Console.WriteLine($"DEBUG EDIT GET: Cliente ID {id}, Estado: '{cliente.Estado}', Cidade: '{cliente.Cidade}'");
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nome,CNPJ,Email,Status,Telefone1,Telefone2,Logradouro,Numero,Complemento,Bairro,CEP,Estado,Cidade")] Cliente cliente)
        {
            Console.WriteLine($"DEBUG EDIT: POST method called for client ID: {id}");
            if (id != cliente.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);

                    // Centros de Custo - Upsert e remoção segura
                    var existingCentrosCusto = await _context.CentrosCusto.Where(cc => cc.ClienteId == id).ToListAsync();

                    // Add CentrosCusto from form data
                    var centrosCustoData = Request.Form.Where(x => x.Key.StartsWith("centrosCusto[")).ToList();
                    Console.WriteLine($"DEBUG EDIT: Found {centrosCustoData.Count} centro custo form fields");
                    if (centrosCustoData.Any())
                    {
                        var centrosCustoGroups = centrosCustoData
                            .GroupBy(x => x.Key.Split('[')[1].Split(']')[0])
                            .Where(g => g.Any(x => x.Key.Contains(".Codigo") && !string.IsNullOrEmpty(x.Value)));
                        var postedCentroIds = new HashSet<int>();
                        foreach (var group in centrosCustoGroups)
                        {
                            var idStr = group.Where(x => x.Key.Contains(".Id")).Select(x => x.Value.ToString()).FirstOrDefault();
                            int.TryParse(idStr, out var centroId);
                            var codigoItem = group.FirstOrDefault(x => x.Key.Contains(".Codigo"));
                            var descricaoItem = group.FirstOrDefault(x => x.Key.Contains(".Descricao"));
                            var codigo = codigoItem.Value.ToString() ?? "";
                            var descricao = descricaoItem.Value.ToString() ?? "";

                            if (!string.IsNullOrEmpty(codigo) && !string.IsNullOrEmpty(descricao))
                            {
                                if (centroId > 0)
                                {
                                    var existing = existingCentrosCusto.FirstOrDefault(cc => cc.Id == centroId);
                                    if (existing != null)
                                    {
                                        existing.Codigo = codigo;
                                        existing.Descricao = descricao;
                                        postedCentroIds.Add(existing.Id);
                                    }
                                    else
                                    {
                                        // Caso o Id postado não exista mais, trata como novo
                                        var novo = new CentroCusto { ClienteId = cliente.Id, Codigo = codigo, Descricao = descricao };
                                        _context.CentrosCusto.Add(novo);
                                    }
                                }
                                else
                                {
                                    var novo = new CentroCusto { ClienteId = cliente.Id, Codigo = codigo, Descricao = descricao };
                                    _context.CentrosCusto.Add(novo);
                                }
                            }
                        }
                        // Remover centros não postados somente se não referenciados por Corridas
                        var toRemoveCentros = existingCentrosCusto.Where(cc => !postedCentroIds.Contains(cc.Id)).ToList();
                        foreach (var cc in toRemoveCentros)
                        {
                            bool isReferenced = await _context.Corridas.AnyAsync(c => c.CentroCustoId == cc.Id);
                            if (!isReferenced)
                            {
                                _context.CentrosCusto.Remove(cc);
                            }
                        }
                    }

                    // Usuários Autorizados - Upsert e remoção segura
                    var existingUsuarios = await _context.UsuariosAutorizados.Where(ua => ua.ClienteId == id).ToListAsync();

                    var usuariosData = Request.Form.Where(x => x.Key.StartsWith("usuariosAutorizados[")).ToList();
                    Console.WriteLine($"DEBUG EDIT: Found {usuariosData.Count} usuario autorizado form fields");
                    if (usuariosData.Any())
                    {
                        var usuariosGroups = usuariosData
                            .GroupBy(x => x.Key.Split('[')[1].Split(']')[0])
                            .Where(g => g.Any(x => x.Key.Contains(".Nome") && !string.IsNullOrEmpty(x.Value)));

                        var postedUsuarioIds = new HashSet<int>();
                        foreach (var group in usuariosGroups)
                        {
                            var idStr = group.Where(x => x.Key.Contains(".Id")).Select(x => x.Value.ToString()).FirstOrDefault();
                            int.TryParse(idStr, out var usuarioId);
                            var nome = group.Where(x => x.Key.Contains(".Nome")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var funcional = group.Where(x => x.Key.Contains(".Funcional")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var tipoSolicitante = group.Where(x => x.Key.Contains(".TipoSolicitante")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var status = group.Where(x => x.Key.Contains(".Status")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var telefone1 = group.Where(x => x.Key.Contains(".Telefone1")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var telefone2 = group.Where(x => x.Key.Contains(".Telefone2")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";
                            var email = group.Where(x => x.Key.Contains(".Email")).Select(x => x.Value.ToString()).FirstOrDefault() ?? "";

                            if (!string.IsNullOrEmpty(nome) && !string.IsNullOrEmpty(email) &&
                                !string.IsNullOrEmpty(tipoSolicitante) && !string.IsNullOrEmpty(status) &&
                                !string.IsNullOrEmpty(telefone1))
                            {
                                if (Enum.TryParse<TipoSolicitanteEnum>(tipoSolicitante, out var tipoSolicitanteEnum) &&
                                    Enum.TryParse<StatusEnum>(status, out var statusEnum))
                                {
                                    if (usuarioId > 0)
                                    {
                                        var existing = existingUsuarios.FirstOrDefault(ua => ua.Id == usuarioId);
                                        if (existing != null)
                                        {
                                            existing.Nome = nome;
                                            existing.Funcional = funcional;
                                            existing.TipoSolicitante = tipoSolicitanteEnum;
                                            existing.Status = statusEnum;
                                            existing.Telefone1 = telefone1;
                                            existing.Telefone2 = telefone2;
                                            existing.Email = email;
                                            postedUsuarioIds.Add(existing.Id);
                                        }
                                        else
                                        {
                                            var novo = new UsuarioAutorizado
                                            {
                                                ClienteId = cliente.Id,
                                                Nome = nome,
                                                Funcional = funcional,
                                                TipoSolicitante = tipoSolicitanteEnum,
                                                Status = statusEnum,
                                                Telefone1 = telefone1,
                                                Telefone2 = telefone2,
                                                Email = email
                                            };
                                            _context.UsuariosAutorizados.Add(novo);
                                        }
                                    }
                                    else
                                    {
                                        var novo = new UsuarioAutorizado
                                        {
                                            ClienteId = cliente.Id,
                                            Nome = nome,
                                            Funcional = funcional,
                                            TipoSolicitante = tipoSolicitanteEnum,
                                            Status = statusEnum,
                                            Telefone1 = telefone1,
                                            Telefone2 = telefone2,
                                            Email = email
                                        };
                                        _context.UsuariosAutorizados.Add(novo);
                                    }
                                }
                            }
                        }

                        // Remover usuários não postados somente se não referenciados por Corridas
                        var toRemoveUsuarios = existingUsuarios.Where(ua => !postedUsuarioIds.Contains(ua.Id)).ToList();
                        foreach (var ua in toRemoveUsuarios)
                        {
                            bool isReferenced = await _context.Corridas.AnyAsync(c => c.SolicitanteId == ua.Id);
                            if (!isReferenced)
                            {
                                _context.UsuariosAutorizados.Remove(ua);
                            }
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.Id))
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
            ViewBag.Title = "Editar Cliente";
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(m => m.Id == id);
            if (cliente == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Excluir Cliente";
            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // API Methods for IBGE integration
        [HttpGet]
        public async Task<IActionResult> GetEstados()
        {
            try
            {
                var response = await _httpClient.GetStringAsync("https://servicodados.ibge.gov.br/api/v1/localidades/estados");
                var estados = JsonSerializer.Deserialize<List<Estado>>(response);
                return Json(estados.OrderBy(e => e.nome));
            }
            catch
            {
                return Json(new List<Estado>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCidades(string uf)
        {
            try
            {
                var response = await _httpClient.GetStringAsync($"https://servicodados.ibge.gov.br/api/v1/localidades/estados/{uf}/municipios");
                var cidades = JsonSerializer.Deserialize<List<Cidade>>(response);
                return Json(cidades.OrderBy(c => c.nome));
            }
            catch
            {
                return Json(new List<Cidade>());
            }
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.Id == id);
        }
    }

    // Helper classes for IBGE API
    public class Estado
    {
        public int id { get; set; }
        public string sigla { get; set; }
        public string nome { get; set; }
    }

    public class Cidade
    {
        public int id { get; set; }
        public string nome { get; set; }
    }
}
