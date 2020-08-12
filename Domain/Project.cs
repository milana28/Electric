using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Electric.Models;
using Electric.Utils;

namespace Electric.Domain
{
    public interface IProject
    {
        Models.Project CreateProject(ProjectDao project);
        List<Models.Project> GetAll();
        Models.Project GetProjectById(int id);
        Models.Project DeleteProject(int id);
    }
    
    public class Project : IProject
    {
        private readonly IEnclosure _enclosure;
        private static IDbConnection _database;

        public Project(IDatabase database, IEnclosure enclosure)
        {
            _database = database.Get();
            _enclosure = enclosure;
        }
        
        public Models.Project CreateProject(ProjectDao project)
        {
            var projectDao = new ProjectDao()
            {
                Name = project.Name,
                Date = DateTime.Now,
                UpdateDate = null
            };
            
            const string insertQuery = "INSERT INTO Electric.Project VALUES (@name, @date, @updateDate); SELECT * FROM Electric.Project WHERE id = SCOPE_IDENTITY()";

            return TransformDaoToBusinessLogicProject(_database.QueryFirst<ProjectDao>(insertQuery, projectDao));
        }

        public List<Models.Project> GetAll()
        {
            var projectList = new List<Models.Project>();
         
            var projects = _database.Query<ProjectDao>("SELECT * FROM Electric.Project").ToList();

            projects.ForEach(p => projectList.Add(TransformDaoToBusinessLogicProject(p)));

            return projectList;
        }
        
        public Models.Project GetProjectById(int id)
        {
            const string sql= "SELECT * FROM Electric.Project WHERE id = @projectId";
            var project = _database.QuerySingle<ProjectDao>(sql, new {projectId = id});

            return TransformDaoToBusinessLogicProject(project);
        }
        
        public Models.Project DeleteProject(int id)
        {
            const string sql= "DELETE FROM Electric.Project WHERE id = @projectId";
            _database.Execute(sql, new {projectId = id});

            return GetProjectById(id);
        }

        public static void UpdateProjectDate(int projectId)
        {
            var updateDate = DateTime.Now;
          
            const string sql= "UPDATE Electric.Project SET updateDate = @date WHERE id = @id";
            _database.Execute(sql, new {id = projectId, date = updateDate});
        }
        
        private Models.Project TransformDaoToBusinessLogicProject(ProjectDao projectDao)
        {
            var enclosures = _enclosure.GetEnclosuresByProjectId(projectDao.Id);
            return new Models.Project()
            {
                Id = projectDao.Id,
                Name = projectDao.Name,
                Date = projectDao.Date,
                UpdateDate = projectDao.UpdateDate,
                Enclosures = enclosures
            };
        }
    }
}