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
    }
    
    public class EnclosureSpecs : IEnclosureSpecs
    {
        private const string DatabaseConnectionString = "Server=localhost;Database=electric;User Id=sa;Password=yourStrong(!)Password;";

        public Models.EnclosureSpecs CreateEnclosureSpecs(Models.EnclosureSpecs enclosureSpecs)
        {
            var newEnclosureSpecs = new Models.EnclosureSpecs()
            {
                Rows = enclosureSpecs.Rows,
                DevicePerRow = enclosureSpecs.DevicePerRow,
                EnclosureId = enclosureSpecs.EnclosureId
            };

            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string insertQuery = "INSERT INTO Electric.EnclosureSpecs VALUES (@rows, @devicePerRow, @enclosureId); SELECT * FROM Electric.EnclosureSpecs WHERE id = SCOPE_IDENTITY()";

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
            
            return database.QuerySingle<Models.EnclosureSpecs>(sql, new {enclosureId = id});
        }
    }
}