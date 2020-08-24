using System.Net;
using System.Threading.Tasks;
using Electric.Domain;
using Electric.Pdf;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Electric.Controllers
{
    [ApiController] 
    [Route("[controller]")]
    public class RazorPageController : ControllerBase
    {
        private readonly IEnclosure _enclosure;
        private readonly TemplateGenerator _template;

        public RazorPageController(IEnclosure enclosure, TemplateGenerator templateGenerator)
        {
            _enclosure = enclosure;
            _template = templateGenerator;
        }
        
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Models.Enclosure>> GenerateRazorPage()
        {
            var enclosures = _enclosure.GetAll();
            return new ContentResult {
                ContentType = "text/html",
                StatusCode = (int) HttpStatusCode.OK,
                Content = await _template.GetHtmlString(enclosures),
            };
        }
    }
}