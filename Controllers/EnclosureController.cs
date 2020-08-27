using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using DinkToPdf;
using DinkToPdf.Contracts;
using Electric.Domain;
using Electric.Models;
using Electric.Pdf;
using IronPdf;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Electric.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnclosureController : ControllerBase
    {
        private readonly IEnclosure _enclosure;
        private readonly IConverter _converter;
        private readonly TemplateGenerator _template;

        public EnclosureController(IEnclosure enclosure, IConverter converter, TemplateGenerator templateGenerator)
        {
            _enclosure = enclosure;
            _converter = converter;
            _template = templateGenerator;
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> CreateEnclosure(EnclosureDao enclosure)
        {
            return _enclosure.CreateEnclosure(enclosure);
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<Models.Enclosure>> GetEnclosures([FromQuery(Name = "projectId")] int? projectId)
        {
            return _enclosure.GetEnclosures(projectId);
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> GetEnclosureById(int id)
        {
            var enclosure = _enclosure.GetEnclosureById(id);
            if (enclosure == null)
            {
                return NotFound();
            }

            return enclosure;
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> DeleteEnclosure(int id)
        {
            var enclosure = _enclosure.GetEnclosureById(id);
            if (enclosure == null)
            {
                return NotFound();
            }

            return _enclosure.DeleteEnclosure(id);
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> UpdateEnclosure(int id, Models.Enclosure enclosureDao)
        {
            var enclosure = _enclosure.GetEnclosureById(id);
            if (enclosure == null)
            {
                return NotFound();
            }

            return _enclosure.UpdateEnclosure(id, enclosureDao.Name, enclosureDao.EnclosureSpecs.Rows, enclosureDao.EnclosureSpecs.Columns);
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
