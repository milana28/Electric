using System.Collections.Generic;
using Electric.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Electric.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnclosureSpecsController : ControllerBase
    {
        private readonly IEnclosureSpecs _enclosureSpecs;

        public EnclosureSpecsController(IEnclosureSpecs enclosureSpecs)
        {
            _enclosureSpecs = enclosureSpecs;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.EnclosureSpecs> CreateEnclosureSpecs(Models.EnclosureSpecs enclosureSpecs)
        {
            return _enclosureSpecs.CreateEnclosureSpecs(enclosureSpecs);
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<Models.EnclosureSpecs>> GetAll()
        {
            return _enclosureSpecs.GetAll();
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.EnclosureSpecs> GetEnclosureSpecsById(int id)
        {
            return _enclosureSpecs.GetEnclosureSpecsById(id);
        }
    }
}