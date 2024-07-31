using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MercDevs_ej2.Models;
using System.Net.Mail;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Rotativa.AspNetCore;

namespace MercDevs_ej2.Controllers
{
    public class DatosfichatecnicasController : Controller
    {
        private readonly MercyDeveloperContext _context;

        public DatosfichatecnicasController(MercyDeveloperContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> FichaTecnica(int? id)
        {
            if (id == null)
            {
                return NotFound(); // Redirige a una pagina de error si el id es nulo
            }

            var fichaTecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                    .ThenInclude(re => re.IdClienteNavigation) // Incluir cliente
                .Include(d => d.RecepcionEquipo)
                    .ThenInclude(re => re.IdServicioNavigation) // Incluir servicio
                .Include(d => d.Diagnosticosolucions) // Incluir diagnosticos
                .FirstOrDefaultAsync(d => d.IdDatosFichaTecnica == id);

            if (fichaTecnica == null)
            {
                return NotFound(); // Redirige a una pagina de error si la ficha tecnica no se encuentra
            }

            return View(fichaTecnica);
        }

        [Authorize]
        public async Task<IActionResult> Inicio()
        {
            var mercydevsEjercicio2Context = _context.Datosfichatecnicas.Include(d => d.RecepcionEquipo);
            return View(await mercydevsEjercicio2Context.ToListAsync());
        }

        // GET: Datosfichatecnicas
        [Authorize]
        public async Task<IActionResult> Index(int id)
        {
            var fichaTecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .Include(d => d.Diagnosticosolucions)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .FirstOrDefaultAsync(d => d.RecepcionEquipoId == id);

            if (fichaTecnica == null)
            {
                return RedirectToAction("Create", new { id });
            }

            return View(fichaTecnica);
        }

        //Listar Datos ficha Tecnica por Recepción de Equipos de Cliente: VerDatosFichaTecnicaPorRecepcion
        [Authorize]
        public async Task<IActionResult> VerDatosFichaTecnicaPorRecepcion(int id)
        {
            var mercydevsEjercicio2Context = _context.Datosfichatecnicas
                .Where(d => d.RecepcionEquipoId == id)
                .Include(d => d.RecepcionEquipo);
            ViewData["IdRecepcionEquipo"] = id;
            return View(await mercydevsEjercicio2Context.ToListAsync());
        }


        // GET: Datosfichatecnicas/Diagnosticosolucionpordatosficha/5
        [Authorize]
        public async Task<IActionResult> Diagnosticosolucionpordatosficha(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var verdiagnostico = await _context.Datosfichatecnicas
                .Include(r => r.Diagnosticosolucions)
                .Include(r => r.RecepcionEquipo)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .FirstOrDefaultAsync(m => m.IdDatosFichaTecnica == id);
            if (verdiagnostico == null)
            {
                return NotFound();
            }

            ViewData["DatosFichaTecnicaId"] = id;
            return View(verdiagnostico);
        }

        // GET: Datosfichatecnicas/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var datosfichatecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .FirstOrDefaultAsync(m => m.IdDatosFichaTecnica == id);
            if (datosfichatecnica == null)
            {
                return NotFound();
            }

            return View(datosfichatecnica);
        }

        // GET: Datosfichatecnicas/Create
        [Authorize]
        public IActionResult Create(int? id)
        {
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id");
            ViewData["IdRecepcionEquipo"] = id;

            ViewData["Soinstalado"] = new SelectList(new[]
            {
                new { Value = 0, Text = "Windows 10" },
                new { Value = 1, Text = "Windows 11" },
                new { Value = 2, Text = "Linux" },
                new { Value = 3, Text = "Windows y Linux" }
            }, "Value", "Text");

            ViewData["SuiteOfficeInstalada"] = new SelectList(new[]
            {
                new { Value = 0, Text = "Microsoft Office 2013" },
                new { Value = 1, Text = "Microsoft Office 2019" },
                new { Value = 2, Text = "Microsoft Office 365" },
                new { Value = 3, Text = "OpenOffice" }
            }, "Value", "Text");

            ViewData["LectorPdfinstalado"] = new SelectList(new[]
            {
                new { Value = 0, Text = "No Instalado" },
                new { Value = 1, Text = "Instalado" },
                new { Value = 2, Text = "No Aplica" }
            }, "Value", "Text");

            ViewData["NavegadorWebInstalado"] = new SelectList(new[]
            {
                new { Value = 0, Text = "No Instalado" },
                new { Value = 1, Text = "Chrome" },
                new { Value = 2, Text = "Firefox" },
                new { Value = 3, Text = "Chrome y Firefox" }
            }, "Value", "Text");

            return View();
        }

        // POST: Datosfichatecnicas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int? id,[Bind("IdDatosFichaTecnica,FechaInicio,FechaFinalizacion,PobservacionesRecomendaciones,Soinstalado,SuiteOfficeInstalada,LectorPdfinstalado,NavegadorWebInstalado,AntivirusInstalado,RecepcionEquipoId")] Datosfichatecnica datosfichatecnica)
        {
            if (datosfichatecnica.FechaInicio != null)
            {
                datosfichatecnica.RecepcionEquipoId = Convert.ToInt32(id);
                datosfichatecnica.Estado = "1";
                _context.Add(datosfichatecnica);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Recepcionequipoes");
            }
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id", datosfichatecnica.RecepcionEquipoId);
            return View(datosfichatecnica);
        }

        // GET: Datosfichatecnicas/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var datosfichatecnica = await _context.Datosfichatecnicas.FindAsync(id);
            if (datosfichatecnica == null)
            {
                return NotFound();
            }
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id", datosfichatecnica.RecepcionEquipoId);

            ViewData["Soinstalado"] = new SelectList(new[]
            {
                new { Value = 0, Text = "Windows 10" },
                new { Value = 1, Text = "Windows 11" },
                new { Value = 2, Text = "Linux" },
                new { Value = 3, Text = "Windows y Linux" }
            }, "Value", "Text");

            ViewData["SuiteOfficeInstalada"] = new SelectList(new[]
            {
                new { Value = 0, Text = "Microsoft Office 2013" },
                new { Value = 1, Text = "Microsoft Office 2019" },
                new { Value = 2, Text = "Microsoft Office 365" },
                new { Value = 3, Text = "OpenOffice" }
            }, "Value", "Text");

            ViewData["LectorPdfinstalado"] = new SelectList(new[]
            {
                new { Value = 0, Text = "No Instalado" },
                new { Value = 1, Text = "Instalado" },
                new { Value = 2, Text = "No Aplica" }
            }, "Value", "Text");

            ViewData["NavegadorWebInstalado"] = new SelectList(new[]
            {
                new { Value = 0, Text = "No Instalado" },
                new { Value = 1, Text = "Chrome" },
                new { Value = 2, Text = "Firefox" },
                new { Value = 3, Text = "Chrome y Firefox" }
            }, "Value", "Text");

            return View(datosfichatecnica);
        }

        // POST: Datosfichatecnicas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdDatosFichaTecnica,FechaInicio,FechaFinalizacion,PobservacionesRecomendaciones,Soinstalado,SuiteOfficeInstalada,LectorPdfinstalado,NavegadorWebInstalado,AntivirusInstalado, Estado,RecepcionEquipoId")] Datosfichatecnica datosfichatecnica)
        {
            if (id != datosfichatecnica.IdDatosFichaTecnica)
            {
                return NotFound();
            }

            if (datosfichatecnica.FechaInicio != null)
            {
                try
                {
                    _context.Update(datosfichatecnica);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DatosfichatecnicaExists(datosfichatecnica.IdDatosFichaTecnica))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Inicio));
            }
            ViewData["RecepcionEquipoId"] = new SelectList(_context.Recepcionequipos, "Id", "Id", datosfichatecnica.RecepcionEquipoId);
            return View(datosfichatecnica);
        }

        // GET: Datosfichatecnicas/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var datosfichatecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .FirstOrDefaultAsync(m => m.IdDatosFichaTecnica == id);
            if (datosfichatecnica == null)
            {
                return NotFound();
            }

            return View(datosfichatecnica);
        }

        // POST: Datosfichatecnicas/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var datosfichatecnica = await _context.Datosfichatecnicas.FindAsync(id);
            if (datosfichatecnica != null)
            {
                _context.Datosfichatecnicas.Remove(datosfichatecnica);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> EnviarFichaTecnica(int id)
        {
            var fichaTecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .Include(d => d.Diagnosticosolucions)
                .FirstOrDefaultAsync(d => d.IdDatosFichaTecnica == id);

            if (fichaTecnica == null)
            {
                return Json(new { success = false });
            }

            var cliente = fichaTecnica.RecepcionEquipo.IdClienteNavigation;
            var enlace = Url.Action("FichaTecnicaPublica", "DatosFichaTecnicas", new { id = fichaTecnica.IdDatosFichaTecnica }, Request.Scheme);
            var pdf = new ViewAsPdf("FichaTecnicaPublica", fichaTecnica) { FileName = $"Ficha_Tecnica_{id}.pdf" };

            var pdfBytes = await pdf.BuildFile(ControllerContext);

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("felipe.castillo.barraza@cftmail.cl", "ihwwcraydabkowan"),
                EnableSsl = true,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress("felipe.castillo.barraza@cftmail.cl"),
                Subject = $"MercyDeveloper Ficha Tecnica n°{fichaTecnica.IdDatosFichaTecnica}",
                Body = $"Estimado/a {cliente.Nombre},\n\nPuede ver su ficha técnica en el siguiente enlace: {enlace}",
                IsBodyHtml = false,
            };
            mailMessage.To.Add(cliente.Correo);
            mailMessage.Attachments.Add(new Attachment(new MemoryStream(pdfBytes), $"Ficha_Tecnica_{id}.pdf"));

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        public async Task<IActionResult> FichaTecnicaPublica(int id)
        {
            var fichaTecnica = await _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .Include(d => d.RecepcionEquipo.IdClienteNavigation)
                .Include(d => d.Diagnosticosolucions)
                .FirstOrDefaultAsync(d => d.IdDatosFichaTecnica == id);

            if (fichaTecnica == null)
            {
                return NotFound();
            }

            return View(fichaTecnica);
        }

        public IActionResult ExportPdf(int id)
        {
            var datosFicha = _context.Datosfichatecnicas
                .Include(d => d.RecepcionEquipo)
                .ThenInclude(r => r.IdClienteNavigation)
                .Include(d => d.Diagnosticosolucions)
                .FirstOrDefault(d => d.IdDatosFichaTecnica == id);

            if (datosFicha == null)
            {
                return NotFound();
            }

            var pdfResult = new ViewAsPdf("FichaTecnicaPublica", datosFicha)
            {
                FileName = $"Ficha_Tecnica_{id}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                PageMargins = new Rotativa.AspNetCore.Options.Margins(10, 10, 10, 10)
            };

            return pdfResult;
        }

        public async Task<IActionResult> Finalizar(int id)
        {
            var datofichatecnica = await _context.Datosfichatecnicas.FindAsync(id);
            if (datofichatecnica == null)
            {
                return NotFound();
            }

            datofichatecnica.Estado = "0";

            try
            {
                _context.Update(datofichatecnica);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DatosfichatecnicaExists(datofichatecnica.IdDatosFichaTecnica))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToAction("Index", "Home");
        }

        private bool DatosfichatecnicaExists(int id)
        {
            return _context.Datosfichatecnicas.Any(e => e.IdDatosFichaTecnica == id);
        }
    }
}
