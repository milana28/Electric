using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Electric.Models;

namespace Electric.Domain
{
    public interface IEnclosureSpecs
    {
        Models.EnclosureSpecs CreateEnclosureSpecs(Models.EnclosureSpecs enclosureSpecs);
        List<Models.EnclosureSpecs> GetAll();
        Models.EnclosureSpecs GetEnclosureSpecsById(int id);
        Models.EnclosureSpecs GetEnclosureSpecsByEnclosureId(int id);
        Models.EnclosureSpecs DeleteEnclosureSpecs(int id);
    }
    
    public class EnclosureSpecs : IEnclosureSpecs
    {
        private const string DatabaseConnectionString = "Server=localhost;Database=electric;User Id=sa;Password=yourStrong(!)Password;";
        private delegate bool CheckIfObjectExist(int id);

        public Models.EnclosureSpecs CreateEnclosureSpecs(Models.EnclosureSpecs enclosureSpecs)
        {
            if (!DoesEnclosureExist(enclosureSpecs.EnclosureId))
            {
                return null;
            }
            
            var newEnclosureSpecs = new Models.EnclosureSpecs()
            {
                Rows = enclosureSpecs.Rows,
                Columns = enclosureSpecs.Columns,
                EnclosureId = enclosureSpecs.EnclosureId
            };
            
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string insertQuery = 
                "INSERT INTO Electric.EnclosureSpecs VALUES (@rows, @columns, @enclosureId); SELECT * FROM Electric.EnclosureSpecs WHERE id = SCOPE_IDENTITY()";

            return database.QueryFirst<Models.EnclosureSpecs>(insertQuery, newEnclosureSpecs);
        }
        
        public List<Models.EnclosureSpecs> GetAll()
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            return database.Query<Models.EnclosureSpecs>("SELECT * FROM Electric.EnclosureSpecs").ToList();
        }
        
        public Models.EnclosureSpecs GetEnclosureSpecsById(int id)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql= "SELECT * FROM Electric.EnclosureSpecs WHERE id = @enclosureSpecsId";
            
            return database.QuerySingle<Models.EnclosureSpecs>(sql, new {enclosureSpecsId = id});
        }
        
        public Models.EnclosureSpecs GetEnclosureSpecsByEnclosureId(int id)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql= "SELECT * FROM Electric.EnclosureSpecs WHERE enclosureId = @enclosureId";
            var enclosureSpecs = database.QueryFirstOrDefault<Models.EnclosureSpecs>(sql, new {enclosureId = id});

            return enclosureSpecs;
        }
        
        public Models.EnclosureSpecs DeleteEnclosureSpecs(int id)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql= "DELETE FROM Electric.EnclosureSpecs WHERE id = @deviceId";
            
            database.Execute(sql, new {deviceId = id});

            return GetEnclosureSpecsById(id);
        }

        private static bool DoesEnclosureExist(int enclosureId)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql = "SELECT * FROM Electric.Enclosure WHERE id = @id";
            var enclosure = database.QuerySingle<EnclosureDao>(sql, new {id = enclosureId});

            return enclosure != null;
        }
    }
}