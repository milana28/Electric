using System.Collections.Generic;
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

        public ProjectController(IProject project)
        {
            _project = project;
        }
        
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<Models.Project> CreateProject(ProjectDao project)
        {
            return _project.CreateProject(project);
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
    }
}