using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FrotaTaxi.Data;
using FrotaTaxi.Models;
using FrotaTaxi.Attributes;
using System.Net.Mail;
using System.Net;
using Microsoft.Extensions.Configuration;

namespace FrotaTaxi.Controllers
{
    [Permission("Corridas")]
    public class CorridasController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CorridasController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // GET: Corridas
        public async Task<IActionResult> Index(DateTime? dataInicio, DateTime? dataFim)
        {
            ViewBag.Title = "Corridas";
            
            var corridas = _context.Corridas
                .Include(c => c.Cliente)
                .Include(c => c.Solicitante)
                .Include(c => c.Unidade)
                .Include(c => c.Trecho)
                .Include(c => c.CentroCusto)
                .AsQueryable();

            if (dataInicio.HasValue)
            {
                corridas = corridas.Where(c => c.DataHoraAgendamento.Date >= dataInicio.Value.Date);
                ViewBag.DataInicio = dataInicio.Value.ToString("yyyy-MM-dd");
            }

            if (dataFim.HasValue)
            {
                corridas = corridas.Where(c => c.DataHoraAgendamento.Date <= dataFim.Value.Date);
                ViewBag.DataFim = dataFim.Value.ToString("yyyy-MM-dd");
            }

            if (!dataInicio.HasValue && !dataFim.HasValue)
            {
                // Show today's rides by default
                var hoje = DateTime.Today;
                corridas = corridas.Where(c => c.DataHoraAgendamento.Date == hoje);
                ViewBag.DataInicio = hoje.ToString("yyyy-MM-dd");
                ViewBag.DataFim = hoje.ToString("yyyy-MM-dd");
            }

            return View(await corridas.OrderByDescending(c => c.DataHoraAgendamento).ToListAsync());
        }

        // GET: Corridas/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var corrida = await _context.Corridas
                .Include(c => c.Cliente)
                .Include(c => c.Solicitante)
                .Include(c => c.Unidade)
                .Include(c => c.Trecho)
                .Include(c => c.CentroCusto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (corrida == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Detalhes da Corrida";
            return View(corrida);
        }

        // GET: Corridas/Create
        public IActionResult Create()
        {
            ViewBag.Title = "Nova Corrida";
            LoadViewBags();
            return View();
        }

        // POST: Corridas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ClienteId,SolicitanteId,TipoTarifa,EnderecoInicial,EnderecoFinal,KmInicial,KmFinal,TrechoId,DataHoraAgendamento,UnidadeId,Valor,Observacao,Status,CentroCustoId")] Corrida corrida)
        {
            Console.WriteLine($"DEBUG CORRIDA CREATE: POST method called");
            Console.WriteLine($"DEBUG CORRIDA CREATE: ModelState.IsValid = {ModelState.IsValid}");
            Console.WriteLine($"DEBUG CORRIDA CREATE: ClienteId = {corrida.ClienteId}, SolicitanteId = {corrida.SolicitanteId}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("DEBUG CORRIDA CREATE: ModelState errors:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  {error.Key}: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
            }
            
            if (ModelState.IsValid)
            {
                // If tipo tarifa is Trecho, get the value from the selected trecho
                if (corrida.TipoTarifa == TipoTarifaEnum.Trecho && corrida.TrechoId.HasValue)
                {
                    var trecho = await _context.Trechos.FindAsync(corrida.TrechoId.Value);
                    if (trecho != null)
                    {
                        corrida.Valor = trecho.Valor;
                    }
                }

                Console.WriteLine($"DEBUG CORRIDA CREATE: Adding corrida to context");
                _context.Add(corrida);
                Console.WriteLine($"DEBUG CORRIDA CREATE: Calling SaveChangesAsync");
                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG CORRIDA CREATE: SaveChangesAsync completed successfully");

                // Send email if status is Realizado
                if (corrida.Status == StatusCorridaEnum.Realizado)
                {
                    await SendCompletionEmail(corrida.Id);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Title = "Nova Corrida";
            LoadViewBags(corrida);
            return View(corrida);
        }

        // GET: Corridas/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var corrida = await _context.Corridas.FindAsync(id);
            if (corrida == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Editar Corrida";
            LoadViewBags(corrida);
            return View(corrida);
        }

        // POST: Corridas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,SolicitanteId,TipoTarifa,EnderecoInicial,EnderecoFinal,KmInicial,KmFinal,TrechoId,DataHoraAgendamento,UnidadeId,Valor,Observacao,Status,CentroCustoId")] Corrida corrida)
        {
            if (id != corrida.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var originalCorrida = await _context.Corridas.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    var statusChanged = originalCorrida?.Status != corrida.Status;

                    // If tipo tarifa is Trecho, get the value from the selected trecho
                    if (corrida.TipoTarifa == TipoTarifaEnum.Trecho && corrida.TrechoId.HasValue)
                    {
                        var trecho = await _context.Trechos.FindAsync(corrida.TrechoId.Value);
                        if (trecho != null)
                        {
                            corrida.Valor = trecho.Valor;
                        }
                    }

                    _context.Update(corrida);
                    await _context.SaveChangesAsync();

                    // Send email if status changed to Realizado
                    if (statusChanged && corrida.Status == StatusCorridaEnum.Realizado)
                    {
                        await SendCompletionEmail(corrida.Id);
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CorridaExists(corrida.Id))
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

            ViewBag.Title = "Editar Corrida";
            LoadViewBags(corrida);
            return View(corrida);
        }

        // GET: Corridas/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var corrida = await _context.Corridas
                .Include(c => c.Cliente)
                .Include(c => c.Solicitante)
                .Include(c => c.Unidade)
                .Include(c => c.Trecho)
                .Include(c => c.CentroCusto)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (corrida == null)
            {
                return NotFound();
            }

            ViewBag.Title = "Excluir Corrida";
            return View(corrida);
        }

        // POST: Corridas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var corrida = await _context.Corridas.FindAsync(id);
            if (corrida != null)
            {
                _context.Corridas.Remove(corrida);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // API Methods
        [HttpGet]
        public async Task<IActionResult> GetSolicitantesByCliente(int clienteId)
        {
            var solicitantes = await _context.UsuariosAutorizados
                .Where(ua => ua.ClienteId == clienteId && ua.Status == StatusEnum.Ativo)
                .Select(ua => new { ua.Id, ua.Nome })
                .ToListAsync();
            return Json(solicitantes);
        }

        [HttpGet]
        public async Task<IActionResult> GetCentrosCustoByCliente(int clienteId)
        {
            var centrosCusto = await _context.CentrosCusto
                .Where(cc => cc.ClienteId == clienteId)
                .Select(cc => new { cc.Id, cc.Codigo, cc.Descricao })
                .ToListAsync();
            return Json(centrosCusto);
        }

        private void LoadViewBags(Corrida? corrida = null)
        {
            ViewData["ClienteId"] = new SelectList(_context.Clientes.Where(c => c.Status == StatusEnum.Ativo), "Id", "Nome", corrida?.ClienteId);
            ViewData["UnidadeId"] = new SelectList(_context.Unidades.Where(u => u.Status == StatusEnum.Ativo), "Id", "Nome", corrida?.UnidadeId);
            
            if (corrida?.ClienteId > 0)
            {
                ViewData["SolicitanteId"] = new SelectList(_context.UsuariosAutorizados.Where(ua => ua.ClienteId == corrida.ClienteId && ua.Status == StatusEnum.Ativo), "Id", "Nome", corrida?.SolicitanteId);
                ViewData["TrechoId"] = new SelectList(_context.Trechos.Where(t => t.ClienteId == corrida.ClienteId && t.Status == StatusEnum.Ativo), "Id", "NomeTrecho", corrida?.TrechoId);
                ViewData["CentroCustoId"] = new SelectList(_context.CentrosCusto.Where(cc => cc.ClienteId == corrida.ClienteId), "Id", "Codigo", corrida?.CentroCustoId);
            }
            else
            {
                ViewData["SolicitanteId"] = new SelectList(new List<UsuarioAutorizado>(), "Id", "Nome");
                ViewData["TrechoId"] = new SelectList(new List<Trecho>(), "Id", "NomeTrecho");
                ViewData["CentroCustoId"] = new SelectList(new List<CentroCusto>(), "Id", "Codigo");
            }
        }

        private async Task SendCompletionEmail(int corridaId)
        {
            try
            {
                var corrida = await _context.Corridas
                    .Include(c => c.Cliente)
                    .Include(c => c.Solicitante)
                    .Include(c => c.Unidade)
                    .Include(c => c.Trecho)
                    .Include(c => c.CentroCusto)
                    .FirstOrDefaultAsync(c => c.Id == corridaId);

                if (corrida == null) return;

                var subject = $"Corrida Realizada - {corrida.Cliente.Nome}";
                var body = $@"
                    <h2>Corrida Realizada</h2>
                    <p><strong>Cliente:</strong> {corrida.Cliente.Nome}</p>
                    <p><strong>Solicitante:</strong> {corrida.Solicitante.Nome}</p>
                    <p><strong>Data/Hora:</strong> {corrida.DataHoraAgendamento:dd/MM/yyyy HH:mm}</p>
                    <p><strong>Unidade:</strong> {corrida.Unidade.Nome}</p>
                    <p><strong>Tipo de Tarifa:</strong> {corrida.TipoTarifa}</p>
                    {(corrida.TipoTarifa == TipoTarifaEnum.Livre ? $"<p><strong>Endereço Inicial:</strong> {corrida.EnderecoInicial}</p><p><strong>Endereço Final:</strong> {corrida.EnderecoFinal}</p>" : "")}
                    {(corrida.TipoTarifa == TipoTarifaEnum.KM ? $"<p><strong>KM Inicial:</strong> {corrida.KmInicial}</p><p><strong>KM Final:</strong> {corrida.KmFinal}</p>" : "")}
                    {(corrida.TipoTarifa == TipoTarifaEnum.Trecho ? $"<p><strong>Trecho:</strong> {corrida.Trecho?.NomeTrecho}</p>" : "")}
                    <p><strong>Valor:</strong> {corrida.Valor:C}</p>
                    {(!string.IsNullOrEmpty(corrida.Observacao) ? $"<p><strong>Observação:</strong> {corrida.Observacao}</p>" : "")}
                ";

                // Enviar para o e-mail solicitado pelo usuário
                //await SendEmailSmtp(new[] { "munhoz82@gmail.com" }, subject, body);
                await SendEmailSmtp("jmunhoz@dnways.com", subject, body);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        public async Task SendEmailSmtp(string to, string subject, string body, bool isHtmlBody = true)
        {
            try
            {
                string appPassword = "tcov fjhi gnpq dulj";
                var fromMail = new MailAddress("munhoz82@gmail.com", $"Frota");
                var msg = new MailMessage();

                msg.From = fromMail;
                foreach (var item in to.Split(';'))
                {
                    msg.To.Add(item);
                }
                msg.Subject = subject;
                msg.Body = body;
                msg.IsBodyHtml = isHtmlBody;

                //Obter do appsettings
                SmtpClient smtp = GetSmtpClient(true, "munhoz82@gmail.com", appPassword);
                smtp.Host = "smtp.gmail.com";
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("munhoz82@gmail.com", appPassword);

                ServicePointManager.ServerCertificateValidationCallback =
                    (s, certificate, chain, sslPolicyErrors) => true;

                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;

                try
                {
                    smtp.Send(msg);
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static SmtpClient GetSmtpClient(bool enableSsl, string userName, string pwd)
        {
            return new SmtpClient
            {
                Credentials = new NetworkCredential(userName, pwd),
                EnableSsl = enableSsl,
                Port = 587,
                UseDefaultCredentials = false
            };
        }

        private async Task SendEmail(string[] to, string subject, string body)
        {
            try
            {
                //var host = _configuration["smtp.gmail.com"];
                //var portStr = _configuration["587"]; // e.g., 587
                //var user = _configuration["munhoz82@gmail.com"]; // e.g., SMTP username or email
                //var pass = _configuration["tcov fjhi gnpq dulj"]; // e.g., SMTP password or app password
                //var from = _configuration["munhoz82@gmail.com"] ?? user;
                //var enableSslStr = _configuration["false"]; // "true"/"false"

                var host = "smtp.gmail.com";
                var portStr = "587"; // e.g., 587
                var user = "munhoz82@gmail.com"; // e.g., SMTP username or email
                var pass = "tcov fjhi gnpq dulj"; // e.g., SMTP password or app password
                var from = "munhoz82@gmail.com" ?? user;
                var enableSslStr = "false"; // "true"/"false"

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
                {
                    Console.WriteLine("SendEmail skipped: SMTP configuration missing (Smtp:Host/Smtp:From)");
                    return;
                }

                var port = 587;
                if (!string.IsNullOrWhiteSpace(portStr) && int.TryParse(portStr, out var p)) port = p;
                var enableSsl = true;
                if (!string.IsNullOrWhiteSpace(enableSslStr) && bool.TryParse(enableSslStr, out var ssl)) enableSsl = ssl;

                using (var client = new SmtpClient(host, port))
                {
                    client.EnableSsl = enableSsl;
                    if (!string.IsNullOrWhiteSpace(user))
                    {
                        client.Credentials = new NetworkCredential(user, pass);
                    }
                    using (var message = new MailMessage())
                    {
                        message.From = new MailAddress(from);
                        foreach (var recipient in to.Distinct())
                        {
                            if (!string.IsNullOrWhiteSpace(recipient))
                                message.To.Add(recipient);
                        }
                        message.Subject = subject;
                        message.Body = body;
                        message.IsBodyHtml = true;

                        await client.SendMailAsync(message);
                        Console.WriteLine("Email sent successfully.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendEmail: {ex.Message}");
            }
        }

        // GET: Corridas/MarcarRealizado/5
        public async Task<IActionResult> MarcarRealizado(int id)
        {
            Console.WriteLine($"DEBUG MARCAR REALIZADO: Method called for corrida ID: {id}");
            
            var corrida = await _context.Corridas.FindAsync(id);
            if (corrida == null)
            {
                Console.WriteLine($"DEBUG MARCAR REALIZADO: Corrida not found");
                return NotFound();
            }

            Console.WriteLine($"DEBUG MARCAR REALIZADO: Current status: {corrida.Status}");
            
            if (corrida.Status != StatusCorridaEnum.Realizado)
            {
                corrida.Status = StatusCorridaEnum.Realizado;
                _context.Update(corrida);
                await _context.SaveChangesAsync();
                Console.WriteLine($"DEBUG MARCAR REALIZADO: Status updated to Realizado");

                // Send email notification
                await SendCompletionEmail(corrida.Id);
            }
            else
            {
                Console.WriteLine($"DEBUG MARCAR REALIZADO: Corrida already marked as Realizado");
            }

            return RedirectToAction(nameof(Index));
        }

        private bool CorridaExists(int id)
        {
            return _context.Corridas.Any(e => e.Id == id);
        }
    }
}
