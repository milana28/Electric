using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using Electric.Domain;
using Electric.Exceptions;
using Electric.Models;
using Electric.Pdf;
using IronPdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Electric.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnclosureController : ControllerBase
    {
        private readonly IEnclosure _enclosure;
        private readonly IConverter _converter;
        private readonly TemplateGenerator _template;
        private readonly ILogger<EnclosureController> _logger;

        public EnclosureController(IEnclosure enclosure, IConverter converter, TemplateGenerator templateGenerator, ILogger<EnclosureController> logger)
        {
            _enclosure = enclosure;
            _converter = converter;
            _template = templateGenerator;
            _logger = logger;
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> CreateEnclosure(EnclosureDao enclosure)
        {
            try
            {
                return Created("https://localhost:5001/Enclosure", _enclosure.CreateEnclosure(enclosure));
            }
            catch (ProjectNotFountException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error", ex);
                return BadRequest();
            }
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<List<Models.Enclosure>> GetEnclosures([FromQuery(Name = "projectId")] int? projectId)
        {
            try
            {
                return Ok(_enclosure.GetEnclosures(projectId));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error", ex);
                return BadRequest();
            }
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> GetEnclosureById(int id)
        {
            try
            {
                return  Ok(_enclosure.GetEnclosureById(id));
            }
            catch (EnclosureNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error", ex);
                return BadRequest();
            }
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> DeleteEnclosure(int id)
        {
            try
            {
                _enclosure.DeleteEnclosure(id);
                return NoContent();
            }
            catch (EnclosureNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error", ex);
                return BadRequest();
            }
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> UpdateEnclosure(int id, Models.Enclosure enclosureDao)
        {
            try
            {
                return Ok(_enclosure.UpdateEnclosure(id, enclosureDao.Name, enclosureDao.EnclosureSpecs.Rows,
                    enclosureDao.EnclosureSpecs.Columns));
            }
            catch (EnclosureNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error", ex);
                return BadRequest();
            }
        }
        
        [HttpGet("pdf")]
        public async Task<IActionResult> CreatePdf()
        {
            var enclosures = _enclosure.GetAll();
            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 10 },
                DocumentTitle = "PDF Report",
                // Out = @"Enclosure.pdf"
            };
            
            var objectSettings = new ObjectSettings
            {
                PagesCount = true,
                HtmlContent = await _template.GetHtmlString(enclosures),
                WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet =  Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Style.css") },
                HeaderSettings = { FontName = "Arial", FontSize = 9, Right = "Page [page] of [toPage]", Line = true },
            };
 
            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };
          
            var file = _converter.Convert(pdf);
            return File(file, "application/pdf");
        }
     
        [HttpGet("pdf/image")]
        public FileResult CreateImagePdf()
        {
            var renderer = new HtmlToPdf
            {
                PrintOptions =
                {
                    MarginTop = 5,
                    MarginBottom = 5,
                    CssMediaType = PdfPrintOptions.PdfCssMediaType.Print,
                    Header = new SimpleHeaderFooter()
                    {
                        CenterText = "Enclosure", DrawDividerLine = true, FontSize = 16
                    },
                    Footer = new SimpleHeaderFooter()
                    {
                        LeftText = "{date} {time}",
                        RightText = "Page {page} of {total-pages}",
                        DrawDividerLine = true,
                        FontSize = 14
                    }
                }
            };
            var pdf = renderer.RenderHTMLFileAsPdf("Assets/pdf.html");
            return File(pdf.BinaryData, "application/pdf;");
        }
    }
}
