using FrotaTaxi.Attributes;
using FrotaTaxi.Data;
using FrotaTaxi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Drawing;
using System.Globalization;
using System.Net;
using System.Net.Mail;

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

            if (!dataInicio.HasValue && !dataFim.HasValue)
            {
                // Show today's rides by default
                var hoje = DateTime.Today;
                var inicio = hoje.AddDays((hoje.Day - 1) * -1);

                dataInicio = inicio;
                dataFim = hoje;

                ViewBag.DataInicio = inicio.ToString("yyyy-MM-dd");
                ViewBag.DataFim = hoje.ToString("yyyy-MM-dd");
            }

            var corridas = _context.Corridas
                .Include(c => c.Cliente)
                .Include(c => c.Solicitante)
                .Include(c => c.Usuario)
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
                .Include(c => c.Usuario)
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
        public async Task<IActionResult> Create([Bind("Id,ClienteId,SolicitanteId,UsuarioId,TipoTarifa,EnderecoInicial,EnderecoFinal,KmInicial,KmFinal,TrechoId,DataHoraAgendamento,UnidadeId,Valor,Observacao,Status,CentroCustoId")] Corrida corrida)
        {
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

                _context.Add(corrida);
                await _context.SaveChangesAsync();
                
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
        public async Task<IActionResult> Edit(int id, [Bind("Id,ClienteId,SolicitanteId,UsuarioId,TipoTarifa,EnderecoInicial,EnderecoFinal,KmInicial,KmFinal,TrechoId,DataHoraAgendamento,UnidadeId,Valor,Observacao,Status,CentroCustoId")] Corrida corrida)
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
                .Where(ua => ua.ClienteId == clienteId 
                             && ua.Status == StatusEnum.Ativo 
                             && (ua.TipoSolicitante == TipoSolicitanteEnum.Solicitante 
                                 || ua.TipoSolicitante == TipoSolicitanteEnum.Ambos))
                .Select(ua => new { ua.Id, ua.Nome })
                .ToListAsync();
            return Json(solicitantes);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsuariosByCliente(int clienteId)
        {
            var usuarios = await _context.UsuariosAutorizados
                .Where(ua => ua.ClienteId == clienteId 
                             && ua.Status == StatusEnum.Ativo 
                             && (ua.TipoSolicitante == TipoSolicitanteEnum.Usuario 
                                 || ua.TipoSolicitante == TipoSolicitanteEnum.Ambos))
                .Select(ua => new { ua.Id, ua.Nome })
                .ToListAsync();
            return Json(usuarios);
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
                ViewData["SolicitanteId"] = new SelectList(
                    _context.UsuariosAutorizados.Where(ua => ua.ClienteId == corrida.ClienteId 
                        && ua.Status == StatusEnum.Ativo 
                        && (ua.TipoSolicitante == TipoSolicitanteEnum.Solicitante || ua.TipoSolicitante == TipoSolicitanteEnum.Ambos)),
                    "Id", "Nome", corrida?.SolicitanteId);
                ViewData["UsuarioId"] = new SelectList(
                    _context.UsuariosAutorizados.Where(ua => ua.ClienteId == corrida.ClienteId 
                        && ua.Status == StatusEnum.Ativo 
                        && (ua.TipoSolicitante == TipoSolicitanteEnum.Usuario || ua.TipoSolicitante == TipoSolicitanteEnum.Ambos)),
                    "Id", "Nome", corrida?.UsuarioId);
                ViewData["TrechoId"] = new SelectList(_context.Trechos.Where(t => t.ClienteId == corrida.ClienteId && t.Status == StatusEnum.Ativo), "Id", "NomeTrecho", corrida?.TrechoId);
                ViewData["CentroCustoId"] = new SelectList(_context.CentrosCusto.Where(cc => cc.ClienteId == corrida.ClienteId), "Id", "Codigo", corrida?.CentroCustoId);
            }
            else
            {
                ViewData["SolicitanteId"] = new SelectList(new List<UsuarioAutorizado>(), "Id", "Nome");
                ViewData["UsuarioId"] = new SelectList(new List<UsuarioAutorizado>(), "Id", "Nome");
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
                    .Include(c => c.Usuario)
                    .FirstOrDefaultAsync(c => c.Id == corridaId);

                if (corrida == null) return;

                var subject = $"Corrida Realizada - {corrida.Cliente.Nome}";
                CultureInfo culturaBR = new CultureInfo("pt-BR");
                var dadosBoleto = new boleto()
                {
                    DataHora = corrida.DataHoraAgendamento,
                    Cliente = corrida.Cliente.Nome,
                    Solicitante = corrida.Solicitante.Nome,
                    Usuario = corrida.Usuario.Nome ?? "",
                    Unidade = corrida.Unidade.Nome,
                    TipoTarifa = Enum.GetName(typeof(TipoTarifaEnum), corrida.TipoTarifa),
                    EndInicial = corrida.EnderecoInicial,
                    EndFinal = corrida.EnderecoFinal,
                    Codigo = corrida.Id.ToString(),
                    Valor = corrida.Valor.ToString("N2", culturaBR),
                    CentroCusto = corrida.CentroCusto.Descricao ?? ""
                };
                var body = gerarBoleto(dadosBoleto);                
                var to = corrida.Cliente.Email + ";" + corrida.Solicitante.Email + ";" + "agendamento@jdstransp.com.br";
                
                // Enviar para o e-mail solicitado pelo usuário
                //await SendEmailSmtp(new[] { "munhoz82@gmail.com" }, subject, body);
                await SendEmailSmtp(to, subject, body);
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
        }

        private string gerarBoleto(boleto dadosBoleto)
        {
            var body = "";

            body = $@"
                    <!doctype html>
                    <html lang=""pt-BR"">
                    <head>
                      <meta charset=""utf-8"" />
                      <meta name=""viewport"" content=""width=device-width,initial-scale=1"" />
                      <title>Boleto de Corrida - JDS</title>
                      <style>
                        /* Alguns clients de e-mail aceitam media queries; mantivemos simples */
                        @media only screen and (max-width: 600px) {{
                          .container {{ width: 100% !important; padding: 16px !important; }}
                          .two-col {{ display:block !important; width:100% !important; }}
                          .logo {{ text-align:center !important; margin-bottom:12px !important; }}
                        }}
                      </style>
                    </head>
                    <body style=""margin:0; padding:0; background-color:#f2f4f7; font-family:Arial, Helvetica, sans-serif; -webkit-font-smoothing:antialiased;"">
                      <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f2f4f7; padding:24px 0;"">
                        <tr>
                          <td align=""center"">
                            <table role=""presentation"" class=""container"" width=""600"" cellpadding=""0"" cellspacing=""0"" style=""width:600px; max-width:100%; background:#ffffff; border-radius:12px; overflow:hidden; box-shadow:0 6px 18px rgba(17,24,39,0.06);"">
          
                              <!-- Header -->
                              <tr>
                                <td style=""padding:20px 24px; background:linear-gradient(90deg,#0ea5a4,#2563eb); color:#fff;"">
                                  <table role=""presentation"" width=""100%"">
                                    <tr>
                                      <td class=""logo"" style=""vertical-align:middle;"">
                                        <!-- Simple SVG icon as logo -->
                                        <div style=""display:flex; align-items:center; gap:12px;"">
                                          <div style=""width:48px; height:48px; background:rgba(255,255,255,0.12); border-radius:10px; display:flex; align-items:center; justify-content:center;"">
                                            <svg width=""26"" height=""26"" viewBox=""0 0 24 24"" fill=""none"" xmlns=""http://www.w3.org/2000/svg"" aria-hidden=""true"">
                                              <path d=""M3 11L5 6H19L21 11V18C21 18.5523 20.5523 19 20 19H19C18.4477 19 18 18.5523 18 18V17H6V18C6 18.5523 5.55228 19 5 19H4C3.44772 19 3 18.5523 3 18V11Z"" stroke=""white"" stroke-width=""1.2"" stroke-linecap=""round"" stroke-linejoin=""round""/>
                                              <circle cx=""7.5"" cy=""16.5"" r=""1.5"" fill=""white""/>
                                              <circle cx=""17.5"" cy=""16.5"" r=""1.5"" fill=""white""/>
                                            </svg>
                                          </div>
                                          <div>
                                            <div style=""font-size:16px; font-weight:700; line-height:1;"">Recibo de Corrida</div>
                                            <div style=""font-size:12px; opacity:0.95;"">JDS | Documento Eletrônico</div>
                                          </div>
                                        </div>
                                      </td>
                                      <td align=""right"" style=""vertical-align:middle; font-size:12px; opacity:0.95;"">
                                        <div style=""font-weight:600;"">Código: <span style=""font-weight:800;"">{dadosBoleto.Codigo}</span></div>
                                        <div style=""margin-top:6px;"">Data/Hora: <strong>{dadosBoleto.DataHora.ToString("dd/MM/yyyy HH:mm")}</strong></div>
                                      </td>
                                    </tr>
                                  </table>
                                </td>
                              </tr>

                              <!-- Body -->
                              <tr>
                                <td style=""padding:20px 24px;"">
              
                                  <!-- Cliente / Solicitante -->
                                  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom:14px;"">
                                    <tr>
                                      <td style=""padding:12px; background:#f8fafc; border-radius:8px;"">
                                        <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom:14px;"">
								          <tr>
									        <td style=""padding:12px; background:#f8fafc; border-radius:8px;"">
									          <table role=""presentation"" width=""100%"" cellpadding=""5"" cellspacing=""0"">
										        <!-- Linha 1: Empresa / Centro de Custo -->
										        <tr style=""margin-bottom:8px;"">
										          <td style=""margin-top:12px; font-size:13px; color:#6b7280; "">Cliente</td>
										          <td style=""margin-top:12px; font-size:13px; color:#6b7280; "">Centro de Custo</td>
										        </tr>
										        <tr style=""margin-bottom:12px;"">
										          <td style=""font-size:14px; font-weight:700;"">{dadosBoleto.Cliente}</td>
										          <td style=""font-size:13px; font-weight:600;"">{dadosBoleto.CentroCusto}</td>
										        </tr>

										        <!-- Linha 2: Solicitante / Usuário / Unidade -->
										        <tr>
										          <td style=""font-size:13px; color:#6b7280; font-weight:600;"">Solicitante</td>
										          <td style=""font-size:13px; color:#6b7280; font-weight:600;"">Usuário</td>
										          <td style=""font-size:13px; color:#6b7280; font-weight:600;"">Unidade</td>
										        </tr>
										        <tr>
										          <td style=""font-size:13px; font-weight:600;"">{dadosBoleto.Solicitante}</td>
										          <td style=""font-size:13px; font-weight:600;"">{dadosBoleto.Usuario}</td>
										          <td style=""font-size:13px; font-weight:600;"">{dadosBoleto.Unidade}</td>
										        </tr>
									          </table>
                                      </td>
                                    </tr>
                                  </table>

                                  <!-- Endereços -->
                                  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom:14px;"">
                                    <tr>
                                      <td style=""padding:14px; border-radius:8px; background:#ffffff; border:1px solid #eef2f7;"">
                                        <div style=""display:flex; gap:12px; align-items:flex-start;"">
                                          <div style=""width:56px; text-align:center; font-size:12px; color:#6b7280;"">
                                            <div style=""font-weight:700; font-size:18px;"">📍</div>
                                            <div style=""margin-top:6px;"">Origem</div>
                                          </div>
                                          <div style=""flex:1;"">
                                            <div style=""font-size:13px; color:#6b7280;"">Endereço Inicial</div>
                                            <div style=""font-weight:600; font-size:15px; margin-top:6px;"">{dadosBoleto.EndInicial}</div>
                                          </div>
                                        </div>

                                        <div style=""height:12px;""></div>

                                        <div style=""display:flex; gap:12px; align-items:flex-start;"">
                                          <div style=""width:56px; text-align:center; font-size:12px; color:#6b7280;"">
                                            <div style=""font-weight:700; font-size:18px;"">🏁</div>
                                            <div style=""margin-top:6px;"">Destino</div>
                                          </div>
                                          <div style=""flex:1;"">
                                            <div style=""font-size:13px; color:#6b7280;"">Endereço Final</div>
                                            <div style=""font-weight:600; font-size:15px; margin-top:6px;"">{dadosBoleto.EndFinal}</div>
                                          </div>
                                        </div>
                                      </td>
                                    </tr>
                                  </table>

                                  <!-- Tarifas e Valor -->
                                  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""margin-bottom:18px;"">
                                    <tr>
                                      <td style=""padding:16px; background:#f8fafc; border-radius:8px;"">
                                        <table role=""presentation"" width=""100%"">
                                          <tr>
                                            <td style=""vertical-align:top;"">
                                              <div style=""font-size:13px; color:#6b7280;"">Tipo de Tarifa</div>
                                              <div style=""font-size:15px; font-weight:700; margin-top:6px;"">{dadosBoleto.TipoTarifa}</div>
                                            </td>
                                            <td align=""right"" style=""vertical-align:top;"">
                                              <div style=""font-size:13px; color:#6b7280;"">Valor</div>
                                              <div style=""font-size:20px; font-weight:900; margin-top:6px; color:#0f172a;"">R$ {dadosBoleto.Valor}</div>
                                            </td>
                                          </tr>
                                        </table>
                                      </td>
                                    </tr>
                                  </table>

                                  <!-- Observações / QR / Ações -->
                                  <table role=""presentation"" width=""100%"" cellpadding=""0"" cellspacing=""0"">
                                    <tr>
                                      <td style=""padding:12px; background:#ffffff; border-radius:8px; border:1px solid #eef2f7;"">
                                        <table role=""presentation"" width=""100%"">
                                          <tr>
                                            <td style=""width:65%; vertical-align:top;"">
                                              <div style=""font-size:13px; color:#6b7280; margin-bottom:8px;"">Observações</div>
                                              <div style=""font-size:13px; color:#111827; line-height:1.4;"">
                                                Recibo gerado automaticamente. Guarde este comprovante para fins de reembolso ou controle interno.
                                              </div>
                                            </td>                                            
                                          </tr>
                                        </table>
                                      </td>
                                    </tr>
                                  </table>

                                </td>
                              </tr>

                              <!-- Footer -->
                              <tr>
                                <td style=""padding:14px 24px; background:#ffffff; border-top:1px solid #f1f5f9;"">
                                  <table role=""presentation"" width=""100%"">
                                    <tr>
                                      <td style=""font-size:12px; color:#6b7280;"">
                                        <div style=""font-weight:600;"">Contato</div>
                                        <div style=""margin-top:6px;"">agendamento@jdstransp.com.br • Tel: (11) 2289-1784</div>
                                      </td>
                                      <td align=""right"" style=""font-size:12px; color:#9aa5b1;"">
                                        <div>Documento não fiscal</div>
                                        <div style=""margin-top:6px;"">&copy; 2025 GER Frota</div>
                                      </td>
                                    </tr>
                                  </table>
                                </td>
                              </tr>

                            </table>
                          </td>
                        </tr>
                      </table>
                    </body>
                    </html>

                ";

            return body;
        }

        public async Task SendEmailSmtp(string to, string subject, string body, bool isHtmlBody = true)
        {
            try
            {
                string appPassword = "tcov fjhi gnpq dulj";
                var fromMail = new MailAddress("munhoz82@gmail.com", $"Notificação JDS");
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
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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

    public class boleto
    {
        public DateTime DataHora { get; set; }
        public string Cliente { get; set; }
        public string Usuario { get; set; }
        public string Solicitante { get; set; }
        public string Codigo { get; set; }
        public string Unidade { get; set; }
        public string EndInicial { get; set; }
        public string EndFinal { get; set; }
        public string TipoTarifa { get; set; }
        public string Valor { get; set; }
        public string CentroCusto { get; set; }
    }
}
