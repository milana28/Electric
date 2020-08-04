using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Electric.Models;

namespace Electric.Domain
{
    public interface IEnclosure
    {
        Models.Enclosure CreateEnclosure(EnclosureDao enclosure);
        List<Models.Enclosure> GetAll();
        Models.Enclosure GetEnclosureById(int id);
        Models.Enclosure DeleteEnclosure(int id);
        List<Models.Enclosure> GetEnclosuresByProjectId(int id);
    }
    
    public class Enclosure : IEnclosure
    {
        private const string DatabaseConnectionString = "Server=localhost;Database=electric;User Id=sa;Password=yourStrong(!)Password;";
        private readonly IEnclosureSpecs _enclosureSpecs;
        private readonly IDevice _device;

        public Enclosure(IEnclosureSpecs enclosureSpecs, IDevice device)
        {
            _enclosureSpecs = enclosureSpecs;
            _device = device;
        }

        public Models.Enclosure CreateEnclosure(EnclosureDao enclosure)
        {
            var enclosureDao = new EnclosureDao()
            {
                Name = enclosure.Name,
                Date = DateTime.Now,
                ProjectId = enclosure.ProjectId
            };

            if (!DoesProjectExist(enclosure.ProjectId))
            {
                return null;
            }

            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string insertQuery = "INSERT INTO Electric.Enclosure VALUES (@name, @date, @projectId); SELECT * FROM Electric.Enclosure WHERE id = SCOPE_IDENTITY()";
          
            return TransformDaoToBusinessLogicEnclosure(database.QueryFirst<EnclosureDao>(insertQuery, enclosureDao));
        }
        
        public List<Models.Enclosure> GetAll()
        {
            var enclosureList = new List<Models.Enclosure>();
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            var enclosures = database.Query<EnclosureDao>("SELECT * FROM Electric.Enclosure").ToList();

            enclosures.ForEach(e => enclosureList.Add(TransformDaoToBusinessLogicEnclosure(e)));

            return enclosureList;
        }
        
        public Models.Enclosure GetEnclosureById(int id)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql= "SELECT * FROM Electric.Enclosure WHERE id = @enclosureId";
            
            var enclosure = database.QuerySingle<EnclosureDao>(sql, new {enclosureId = id});

            return TransformDaoToBusinessLogicEnclosure(enclosure);
        }
        
        public List<Models.Enclosure> GetEnclosuresByProjectId(int id)
        {
            var enclosureList = new List<Models.Enclosure>();
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql= "SELECT * FROM Electric.Enclosure WHERE projectId = @projectId";
            
            var enclosures = database.Query<EnclosureDao>(sql, new {projectId = id}).ToList();

            enclosures.ForEach(e => enclosureList.Add(TransformDaoToBusinessLogicEnclosure(e)));

            return enclosureList;
        }
        
        public Models.Enclosure DeleteEnclosure(int id)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql= "DELETE FROM Electric.Enclosure WHERE id = @enclosureId";
            
            database.Execute(sql, new {enclosureId = id});

            return GetEnclosureById(id);
        }
        
        private Models.Enclosure TransformDaoToBusinessLogicEnclosure(EnclosureDao enclosureDao)
        {
            var enclosure = new Models.Enclosure()
            {
                Id = enclosureDao.Id,
                Name = enclosureDao.Name,
                Date = enclosureDao.Date,
                ProjectId = enclosureDao.ProjectId,
                Devices = null,
                TotalPrice = 0,
                EnclosureSpecs = _enclosureSpecs.GetEnclosureSpecsByEnclosureId(enclosureDao.Id),
            };

            return enclosure;
        }

        private static bool DoesProjectExist(int projectId)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql = "SELECT * FROM Electric.Project WHERE id = @id";
            var project = database.QuerySingle<ProjectDao>(sql, new {id = projectId});

            return project != null;
        }
    }
}