using System;
using System.IO;
using System.Text;
using Electric.Csv;
using Electric.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Electric.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CsvController : ControllerBase
    {
        private readonly IEnclosure _enclosure;
        
        public CsvController(IEnclosure enclosure)
        {
            _enclosure = enclosure;
        }
        
        [HttpGet]
        public IActionResult Enclosures()
        {
            var array = GenerateCsv.MapEnclosureIntoKeyValueArray(_enclosure.GetAll());
            var data = GenerateCsv.ReturnData(array);
 
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
            var result = new FileStreamResult(stream, "text/plain")
            {
                FileDownloadName = "Enclosure_" + DateTime.Now + ".csv"
            };

            return result;
        }
    }
}