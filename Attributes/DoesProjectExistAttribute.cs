using System;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using Electric.Exceptions;
using Electric.Models;
using Microsoft.AspNetCore.Http;

namespace Electric.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public sealed class DoesProjectExistAttribute : Attribute
    {
        private const string MyConnectionString =
            "Server=localhost;Database=electric;User Id=sa;Password=yourStrong(!)Password;";
        private readonly IHttpContextAccessor _httpContextAccessor = new HttpContextAccessor();

        public DoesProjectExistAttribute()
        {
            var context = _httpContextAccessor.HttpContext;
            var id = context.Items["id"];

            using IDbConnection database = new SqlConnection(MyConnectionString);
            const string sql = "SELECT * FROM Electric.Project WHERE id = @id"; 
            var project = database.QueryFirstOrDefault<ProjectDao>(sql, new {id = id});
          
            if (project == null)
            {
                throw new ProjectNotFountException("Project does not exist!");
            }
        }
    }
}