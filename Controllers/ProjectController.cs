using System.Collections.Generic;
using System.Threading.Tasks;
using Electric.Domain;
using Electric.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Electric.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProjectController : ControllerBase
    {
        private readonly IProject _project;
        private readonly IEnclosure _enclosure;

        public ProjectController(IProject project, IEnclosure enclosure)
        {
            _project = project;
            _enclosure = enclosure;
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Project> CreateProject(ProjectDao project)
        {
            return _project.CreateProject(project);
        }
        
        [HttpPost("{projectId}/enclosure/{enclosureId}/device")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> AddDeviceToEnclosure(int projectId, int enclosureId, Enclosure_Device enclosureDevice)
        {
            var enclosure = _enclosure.GetEnclosureById(enclosureId);
            if (enclosure == null)
            {
                return NotFound("Enclosure with that ID doesn't exist!");
            }

            if (enclosure.ProjectId != projectId)
            {
                return NotFound("Enclosure with that ProjectID doesn't exist!");
            }
            
            var enclosureWithDevice = _enclosure.AddNewDevice(projectId, enclosureId, enclosureDevice);

            Task.Run(() =>
            {
                _enclosure.RecalculateTotalPrice(enclosure);
            });

            return enclosureWithDevice;
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<List<Models.Project>> GetAll()
        {
            return _project.GetAll();
        }
        
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Project> GetProjectById(int id)
        {
            var project = _project.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }

            return project;
        }
        
        [HttpGet("{projectId}/enclosure/{enclosureId}/device/{deviceId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> GetEnclosureWithDevice(int projectId, int enclosureId, int deviceId)
        {
            var enclosure = _enclosure.GetEnclosureById(enclosureId);
            if (enclosure == null)
            {
                return NotFound("Enclosure with that ID doesn't exist!");
            }

            if (enclosure.ProjectId != projectId)
            {
                return NotFound("Enclosure with that ProjectID doesn't exist!");
            }
            
            return _enclosure.GetEnclosureWithDevice(enclosureId, deviceId);
        }

        
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Project> DeleteProject(int id)
        {
            var project = _project.GetProjectById(id);
            if (project == null)
            {
                return NotFound();
            }

            return _project.DeleteProject(id);
        }
        
        [HttpDelete("{projectId}/enclosure/{enclosureId}/device/{deviceId}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Enclosure> DeleteDevice(int projectId, int enclosureId, int deviceId)
        {
            var enclosure = _enclosure.GetEnclosureById(enclosureId);
            if (enclosure == null)
            {
                return NotFound("Enclosure with that ID doesn't exist!");
            }

            if (enclosure.ProjectId != projectId)
            {
                return NotFound("Enclosure with that ProjectID doesn't exist!");
            }
            
            var enclosureWithDevice = _enclosure.RemoveDevice(projectId, enclosureId, deviceId);

            Task.Run(() =>
            {
                _enclosure.RecalculateTotalPrice(enclosure);
            });

            return enclosureWithDevice;
        }
    }
}