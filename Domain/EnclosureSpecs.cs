using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Electric.Exceptions;
using Electric.Models;
using Electric.Utils;

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
        private static IDbConnection _database;

        public EnclosureSpecs(IDatabase database)
        {
            _database = database.Get();
        }
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
            
            const string insertQuery = 
                "INSERT INTO Electric.EnclosureSpecs VALUES (@rows, @columns, @enclosureId); SELECT * FROM Electric.EnclosureSpecs WHERE id = SCOPE_IDENTITY()";

            return _database.QueryFirst<Models.EnclosureSpecs>(insertQuery, newEnclosureSpecs);
        }
        public List<Models.EnclosureSpecs> GetAll()
        {
            return _database.Query<Models.EnclosureSpecs>("SELECT * FROM Electric.EnclosureSpecs").ToList();
        }
        public Models.EnclosureSpecs GetEnclosureSpecsById(int id)
        {
            const string sql= "SELECT * FROM Electric.EnclosureSpecs WHERE id = @enclosureSpecsId";
            return _database.QuerySingle<Models.EnclosureSpecs>(sql, new {enclosureSpecsId = id});
        }
        public Models.EnclosureSpecs GetEnclosureSpecsByEnclosureId(int id)
        {
            const string sql= "SELECT * FROM Electric.EnclosureSpecs WHERE enclosureId = @enclosureId";
            var enclosureSpecs =  _database.QueryFirstOrDefault<Models.EnclosureSpecs>(sql, new {enclosureId = id});

            // if (enclosureSpecs == null)
            // {
            //     throw new EnclosureSpecsNotFoundException("Enclosure specs does not exist!");
            // }

            return enclosureSpecs;
        }
        public Models.EnclosureSpecs DeleteEnclosureSpecs(int id)
        {
            var enclosureSpecs = GetEnclosureSpecsById(id);
            if (enclosureSpecs == null)
            {
                throw new EnclosureSpecsNotFoundException("Enclosure specs does not exist!");
            }
            
            const string sql= "DELETE FROM Electric.EnclosureSpecs WHERE id = @deviceId";
            _database.Execute(sql, new {deviceId = id});

            return enclosureSpecs;
        }
        private static bool DoesEnclosureExist(int enclosureId)
        {
            const string sql = "SELECT * FROM Electric.Enclosure WHERE id = @id";
            var enclosure = _database.QuerySingle<EnclosureDao>(sql, new {id = enclosureId});

            return enclosure != null;
        }
    }
}