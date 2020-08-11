using System.Collections.Generic;
using Electric.Domain;
using Electric.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Electric.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnclosureController : ControllerBase
    {
        private readonly IEnclosure _enclosure;

        public EnclosureController(IEnclosure enclosure)
        {
            _enclosure = enclosure;
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
        
    }
}