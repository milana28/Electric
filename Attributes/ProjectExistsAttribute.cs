using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Electric.Exceptions;
using Electric.Models;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Electric.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class ProjectExistsAttribute : ActionFilterAttribute
    {
        private const string MyConnectionString =
            "Server=localhost;Database=electric;User Id=sa;Password=yourStrong(!)Password;";

        private EnclosureDao _enclosureDao = new EnclosureDao();

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _enclosureDao = (EnclosureDao) context.ActionArguments["enclosure"];

            using IDbConnection database = new SqlConnection(MyConnectionString);
            const string sql = "SELECT * FROM Electric.Project WHERE id = @projectId";
            var project = database.QueryFirstOrDefault<ProjectDao>(sql, new {projectId = _enclosureDao.ProjectId});
            
            if (project == null)
            {
                throw new ProjectNotFountException("Project does not exist!");
            }
            
            base.OnActionExecuting(context);
        }

    }
}
 