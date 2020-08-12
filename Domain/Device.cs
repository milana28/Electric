using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Electric.Models;
using Microsoft.Extensions.Configuration;

namespace Electric.Domain
{
    public interface IDevice
    {
        List<Models.Device> GetAll();
        Models.Device CreateDevice(Models.Device device);
        Models.Device GetDeviceById(int id);
        Models.Device DeleteDevice(int id);
        List<DeviceDto> GetDevicesForEnclosure(int enclosureId);
        List<DeviceDto> GetDeviceForEnclosureById(int enclosureId, int deviceId);
    }
    
    public class Device : IDevice
    {
        private static IConfiguration _configuration;

        public Device(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Models.Device CreateDevice(Models.Device device)
        {
            var newDevice = new Models.Device()
            {
                Name = device.Name,
                Width = device.Width,
                Height = device.Height,
                Amperes = device.Amperes,
                Price = device.Price
            };
            
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string insertQuery = 
                "INSERT INTO Electric.Device VALUES (@name, @width, @height, @amperes, @price); SELECT * FROM Electric.Device WHERE id = SCOPE_IDENTITY()";
            
            return database.QueryFirst<Models.Device>(insertQuery, newDevice);
        }
        
        public List<Models.Device> GetAll()
        {
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            return database.Query<Models.Device>("SELECT * FROM Electric.Device").ToList();
        }
     
        public List<DeviceDto> GetDevicesForEnclosure(int enclosureId)
        {
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string sql =
                "SELECT ed.enclosureId, d.*, ed.row, ed.[column] FROM Electric.Enclosure_Device AS ed LEFT JOIN Electric.Device AS d ON ed.deviceId = d.id WHERE ed.enclosureId = @enclosureID";
            var devicesWithPosition = database.Query<DeviceDto>(sql, new {enclosureID = enclosureId}).ToList();

            return devicesWithPosition;
        }
        public List<DeviceDto> GetDeviceForEnclosureById(int enclosureId, int deviceId)
        {
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string sql =
                "SELECT ed.enclosureId, d.*, ed.row, ed.[column] FROM Electric.Enclosure_Device AS ed LEFT JOIN Electric.Device AS d ON ed.deviceId = d.id WHERE d.id = @deviceID AND ed.enclosureId = @enclosureID";
            var devicesWithPosition = database.Query<DeviceDto>(sql, new {deviceID = deviceId, enclosureID = enclosureId}).ToList();

            return devicesWithPosition;
        }

        public Models.Device GetDeviceById(int id)
        {
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string sql = "SELECT * FROM Electric.Device WHERE id = @deviceId";
            
            return database.QueryFirstOrDefault<Models.Device>(sql, new {deviceId = id} );
        }
        
        public Models.Device DeleteDevice(int id)
        {
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string sql= "DELETE FROM Electric.Device WHERE id = @deviceId";
            
            database.Execute(sql, new {deviceId = id});

            return GetDeviceById(id);
        }
    }
}