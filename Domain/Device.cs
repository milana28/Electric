using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Electric.Exceptions;
using Electric.Models;
using Electric.Utils;

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
        private static IDbConnection _database;

        public Device(IDatabase database)
        {
            _database = database.Get();
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
            
            const string insertQuery = 
                "INSERT INTO Electric.Device VALUES (@name, @width, @height, @amperes, @price); SELECT * FROM Electric.Device WHERE id = SCOPE_IDENTITY()";
            
            return _database.QueryFirst<Models.Device>(insertQuery, newDevice);
        }
        public List<Models.Device> GetAll()
        {
            return _database.Query<Models.Device>("SELECT * FROM Electric.Device").ToList();
        }
        public List<DeviceDto> GetDevicesForEnclosure(int enclosureId)
        {
            const string sql =
                "SELECT ed.enclosureId, d.*, ed.row, ed.[column] FROM Electric.Enclosure_Device AS ed LEFT JOIN Electric.Device AS d ON ed.deviceId = d.id WHERE ed.enclosureId = @enclosureID";
            return  _database.Query<DeviceDto>(sql, new {enclosureID = enclosureId}).ToList();
        }
        public List<DeviceDto> GetDeviceForEnclosureById(int enclosureId, int deviceId)
        {
            const string sql =
                "SELECT ed.enclosureId, d.*, ed.row, ed.[column] FROM Electric.Enclosure_Device AS ed LEFT JOIN Electric.Device AS d ON ed.deviceId = d.id WHERE d.id = @deviceID AND ed.enclosureId = @enclosureID";
            return  _database.Query<DeviceDto>(sql, new {deviceID = deviceId, enclosureID = enclosureId}).ToList();
        }
        public Models.Device GetDeviceById(int id)
        {
            const string sql = "SELECT * FROM Electric.Device WHERE id = @deviceId";
            var device =  _database.QueryFirstOrDefault<Models.Device>(sql, new {deviceId = id} );

            if (device == null)
            {
                throw new DeviceNotFoundException("Device does not exist!");
            }

            return device;
        }
        public Models.Device DeleteDevice(int id)
        {
            var device = GetDeviceById(id);
            if (device == null)
            {
                throw new DeviceNotFoundException("Device does not exist!");
            }
            
            const string sql= "DELETE FROM Electric.Device WHERE id = @deviceId";
            _database.Execute(sql, new {deviceId = id});

            return device;
        }
    }
}