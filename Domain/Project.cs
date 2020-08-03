using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Electric.Models;

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
        private const string DatabaseConnectionString = "Server=localhost;Database=electric;User Id=sa;Password=yourStrong(!)Password;";
        private readonly IEnclosure _enclosure;

        public Project(IEnclosure enclosure)
        {
            _enclosure = enclosure;
        }
        
        public Models.Project CreateProject(ProjectDao project)
        {
            var projectDao = new ProjectDao()
            {
                Name = project.Name,
                Date = DateTime.Now
            };

            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string insertQuery = "INSERT INTO Electric.Project VALUES (@name, @date); SELECT * FROM Electric.Project WHERE id = SCOPE_IDENTITY()";

            return TransformDaoToBusinessLogicProject(database.QueryFirst<ProjectDao>(insertQuery, projectDao));
        }

        public List<Models.Project> GetAll()
        {
            var projectList = new List<Models.Project>();
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            var projects = database.Query<ProjectDao>("SELECT * FROM Electric.Project").ToList();

            projects.ForEach(p => projectList.Add(TransformDaoToBusinessLogicProject(p)));

            return projectList;
        }
        
        public Models.Project GetProjectById(int id)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql= "SELECT * FROM Electric.Project WHERE id = @projectId";
            
            var project = database.QuerySingle<ProjectDao>(sql, new {projectId = id});

            return TransformDaoToBusinessLogicProject(project);
        }
        
        public Models.Project DeleteProject(int id)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql= "DELETE FROM Electric.Project WHERE id = @projectId";
            
            database.Execute(sql, new {projectId = id});

            return GetProjectById(id);
        }
        
        private Models.Project TransformDaoToBusinessLogicProject(ProjectDao projectDao)
        {
            var enclosures = _enclosure.GetEnclosuresByProjectId(projectDao.Id);
            return new Models.Project()
            {
                Id = projectDao.Id,
                Name = projectDao.Name,
                Date = projectDao.Date,
                Enclosures = enclosures
            };
        }
    }
}