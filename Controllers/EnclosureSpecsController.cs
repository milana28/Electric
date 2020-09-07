using System;
using System.Collections.Generic;
using Electric.Domain;
using Electric.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Electric.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EnclosureSpecsController : ControllerBase
    {
        private readonly IEnclosureSpecs _enclosureSpecs;
        private readonly ILogger<EnclosureSpecsController> _logger;

        public EnclosureSpecsController(IEnclosureSpecs enclosureSpecs, ILogger<EnclosureSpecsController> logger)
        {
            _enclosureSpecs = enclosureSpecs;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Models.EnclosureSpecs> CreateEnclosureSpecs(Models.EnclosureSpecs enclosureSpecs)
        {
            try
            {
                return Created("https://localhost:5001/EnclosureSpecs", _enclosureSpecs.CreateEnclosureSpecs(enclosureSpecs));
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
        public ActionResult<List<Models.EnclosureSpecs>> GetAll()
        {
            try
            {
                return Ok(_enclosureSpecs.GetAll());
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
        public ActionResult<Models.EnclosureSpecs> GetEnclosureSpecsById(int id)
        {
            try
            {
                return Ok(_enclosureSpecs.GetEnclosureSpecsById(id));
            }
            catch (EnclosureSpecsNotFoundException ex)
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
        public ActionResult<Models.EnclosureSpecs> DeleteEnclosureSpecs(int id)
        {
            try
            {
                _enclosureSpecs.DeleteEnclosureSpecs(id);
                return NoContent();
            }
            catch (EnclosureSpecsNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError("Error", ex);
                return BadRequest();
            }
        }
    }
}