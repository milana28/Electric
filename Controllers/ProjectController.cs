using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Electric.Domain;
using Electric.Exceptions;
using Electric.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Electric.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProject _project;
        private readonly IEnclosure _enclosure;
        private readonly ILogger<ProjectController> _logger;

        public ProjectController(IProject project, IEnclosure enclosure, ILogger<ProjectController> logger)
        {
            _project = project;
            _enclosure = enclosure;
            _logger = logger;
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<Models.Project> CreateProject(ProjectDao project)
        {
            try
            {
                return Created("https://localhost:5001/Project", _project.CreateProject(project));
            }
            catch (Exception ex)
            {
                _logger.LogError("Error", ex);
                return BadRequest();
            }
        }
        
        [HttpPost("{projectId}/enclosure/{enclosureId}/device")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> AddDeviceToEnclosure(int projectId, int enclosureId, Enclosure_Device enclosureDevice)
        {
            try
            {
                var enclosure = _enclosure.GetEnclosureById(enclosureId);
                if (enclosure.ProjectId != projectId)
                {
                    return NotFound("Enclosure with that ProjectID doesn't exist!");
                }

                Task.Run(() => { _enclosure.RecalculateTotalPrice(enclosure); });

                return Created("https://localhost:5001/Project", _enclosure.AddNewDevice(projectId, enclosureId, enclosureDevice));
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
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<List<Models.Project>> GetAll()
        {
            try
            {
                return Ok(_project.GetAll());
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
        public ActionResult<Models.Project> GetProjectById(int id)
        {
            try
            {
               return Ok(_project.GetProjectById(id));
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
        
        [HttpGet("{projectId}/enclosure/{enclosureId}/device/{deviceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> GetEnclosureWithDevice(int projectId, int enclosureId, int deviceId)
        {
            try
            {
                var enclosure = _enclosure.GetEnclosureById(enclosureId);
                if (enclosure.ProjectId != projectId)
                {
                    return NotFound("Enclosure with that ProjectID doesn't exist!");
                }

                return Ok(_enclosure.GetEnclosureWithDevice(enclosureId, deviceId));
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
        public ActionResult<Models.Project> DeleteProject(int id)
        {
            try
            {
                _project.DeleteProject(id);
                return NoContent();
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
        
        [HttpDelete("{projectId}/enclosure/{enclosureId}/device/{deviceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> DeleteDevice(int projectId, int enclosureId, int deviceId)
        {
            try
            {
                var enclosure = _enclosure.GetEnclosureById(enclosureId);
                if (enclosure.ProjectId != projectId)
                {
                    return NotFound("Enclosure with that ProjectID doesn't exist!");
                }

                Task.Run(() => { _enclosure.RecalculateTotalPrice(enclosure); });

                return Ok(_enclosure.RemoveDevice(projectId, enclosureId, deviceId));
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
    }
}