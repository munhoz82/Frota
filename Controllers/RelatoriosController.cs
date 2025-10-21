using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FrotaTaxi.Data;
using FrotaTaxi.Models;
using FrotaTaxi.Attributes;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace FrotaTaxi.Controllers
{
    [Authorize]
    [Permission("Relatorios", "View")]
    public class RelatoriosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConverter _converter;

        public RelatoriosController(ApplicationDbContext context, IConverter converter)
        {
            _context = context;
            _converter = converter;
        }

        // GET: Relatorios
        public IActionResult Index()
        {
            return View();
        }

        // GET: Relatorios/Corridas
        public async Task<IActionResult> Corridas(DateTime? dataInicio, DateTime? dataFim, int? clienteId, bool exportPdf = false)
        {
            // Definir período padrão (último mês)
            if (!dataInicio.HasValue)
                dataInicio = DateTime.Now.AddMonths(-1).Date;
            
            if (!dataFim.HasValue)
                dataFim = DateTime.Now.Date;

            var corridasQuery = _context.Corridas
                .Include(c => c.Cliente)
                .Include(c => c.Solicitante)
                .Include(c => c.Usuario)
                .Include(c => c.Trecho)
                .Include(c => c.Unidade)
                .Include(c => c.CentroCusto)
                .Where(c => c.DataHoraAgendamento.Date >= dataInicio.Value.Date && 
                           c.DataHoraAgendamento.Date <= dataFim.Value.Date);

            // Aplicar filtro de cliente se especificado
            if (clienteId.HasValue && clienteId.Value > 0)
            {
                corridasQuery = corridasQuery.Where(c => c.ClienteId == clienteId.Value);
            }

            var corridas = await corridasQuery
                .OrderBy(c => c.DataHoraAgendamento)
                .ToListAsync();

            // Buscar lista de clientes para o dropdown
            var clientes = await _context.Clientes
                .OrderBy(c => c.Nome)
                .ToListAsync();

            var model = new RelatorioCorridasViewModel
            {
                DataInicio = dataInicio.Value,
                DataFim = dataFim.Value,
                ClienteId = clienteId,
                Corridas = corridas,
                Clientes = clientes,
                TotalGeral = corridas.Sum(c => c.Valor),
                TotalPorData = corridas
                    .GroupBy(c => c.DataHoraAgendamento.Date)
                    .Select(g => new TotalPorDataViewModel
                    {
                        Data = g.Key,
                        Quantidade = g.Count(),
                        Valor = g.Sum(c => c.Valor)
                    })
                    .OrderBy(t => t.Data)
                    .ToList()
            };

            if (exportPdf)
            {
                return await GerarPdf(model);
            }

            return View(model);
        }

        private async Task<IActionResult> GerarPdf(RelatorioCorridasViewModel model)
        {
            // Retorna a view HTML otimizada para impressão/PDF
            // O usuário pode usar Ctrl+P no navegador para salvar como PDF
            return View("CorridasPdf", model);
        }

        private async Task<string> RenderViewToStringAsync(string viewName, object model)
        {
            ViewData.Model = model;
            
            using (var writer = new StringWriter())
            {
                var viewEngine = HttpContext.RequestServices.GetService(typeof(ICompositeViewEngine)) as ICompositeViewEngine;
                var viewResult = viewEngine!.FindView(ControllerContext, viewName, false);
                var viewContext = new ViewContext(ControllerContext, viewResult.View!, ViewData, TempData, writer, new HtmlHelperOptions());
                
                await viewResult.View!.RenderAsync(viewContext);
                return writer.ToString();
            }
        }
    }

    public class RelatorioCorridasViewModel
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int? ClienteId { get; set; }
        public List<Corrida> Corridas { get; set; } = new List<Corrida>();
        public List<Cliente> Clientes { get; set; } = new List<Cliente>();
        public decimal TotalGeral { get; set; }
        public List<TotalPorDataViewModel> TotalPorData { get; set; } = new List<TotalPorDataViewModel>();
    }

    public class TotalPorDataViewModel
    {
        public DateTime Data { get; set; }
        public int Quantidade { get; set; }
        public decimal Valor { get; set; }
    }
}
