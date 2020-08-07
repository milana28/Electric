using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Electric.Models;
using Microsoft.EntityFrameworkCore;

namespace Electric.Domain
{
    public interface IDevice
    {
        List<Models.Device> GetAll();
        Models.Device CreateDevice(Models.Device device);
        Models.Device GetDeviceById(int id);
        Models.Device DeleteDevice(int id);
        List<Models.Device> GetDevicesForEnclosure(int enclosureId);
        List<DeviceWithPosition> GetDeviceWithPosition(int enclosureId, int deviceId);
    }
    
    public class Device : IDevice
    {
        private const string DatabaseConnectionString = "Server=localhost;Database=electric;User Id=sa;Password=yourStrong(!)Password;";

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
            
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string insertQuery = "INSERT INTO Electric.Device VALUES (@name, @width, @height, @amperes, @price); SELECT * FROM Electric.Device WHERE id = SCOPE_IDENTITY()";
            
            return database.QueryFirst<Models.Device>(insertQuery, newDevice);
        }
        
        public List<Models.Device> GetAll()
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            return database.Query<Models.Device>("SELECT * FROM Electric.Device").ToList();
        }
        
        public List<Models.Device> GetDevicesForEnclosure(int enclosureId)
        {
            var devices = new List<Models.Device>();
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql = "SELECT * FROM Electric.Enclosure_Device WHERE enclosureId = @id";
            var enclosureDevices = database.Query<Enclosure_Device>(sql, new {id = enclosureId}).ToList();
            enclosureDevices.ForEach(el => devices.Add(GetDeviceById(el.DeviceId)));

            return devices;
        }
        
        public List<DeviceWithPosition> GetDeviceWithPosition(int enclosureId, int deviceId)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            var device = new Models.Device();
            const string sql =
                "SELECT ed.enclosureId, d.*, ed.row, ed.[column] FROM Electric.Enclosure_Device AS ed LEFT JOIN Electric.Device AS d ON ed.deviceId = d.id WHERE d.id = @deviceID AND ed.enclosureId = @enclosureID";
            var deviceWithPosition = database.Query<DeviceWithPosition>(sql, new {deviceID = deviceId, enclosureID = enclosureId}).ToList();

            return deviceWithPosition;
        }

        public Models.Device GetDeviceById(int id)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql = "SELECT * FROM Electric.Device WHERE id = @deviceId";
            
            return database.QueryFirstOrDefault<Models.Device>(sql, new {deviceId = id} );
        }
        
        public Models.Device DeleteDevice(int id)
        {
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql= "DELETE FROM Electric.Device WHERE id = @deviceId";
            
            database.Execute(sql, new {deviceId = id});

            return GetDeviceById(id);
        }
    }
}