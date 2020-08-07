using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using Dapper;
using Electric.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Electric.Domain
{
    public interface IEnclosure
    {
        Models.Enclosure CreateEnclosure(EnclosureDao enclosure);
        List<Models.Enclosure> GetAll();
        Models.Enclosure GetEnclosureById(int id);
        Models.Enclosure DeleteEnclosure(int id);
        List<Models.Enclosure> GetEnclosuresByProjectId(int? id);
        List<Models.Enclosure> GetEnclosures(int? projectId);
        Models.Enclosure AddNewDevice(int projectId, int enclosureId, Enclosure_Device enclosureDevice);
        Models.Enclosure RemoveDevice(int projectId, int enclosureId, int deviceId);
    }
    
    public class Enclosure : IEnclosure
    {
        private const string DatabaseConnectionString = "Server=localhost;Database=electric;User Id=sa;Password=yourStrong(!)Password;";
        private readonly IEnclosureSpecs _enclosureSpecs;
        private readonly IDevice _device;
        private readonly EnclosureSpecs.CheckIfObjectExist _checkIfObjectExist = DoesProjectExist;

        public Enclosure(IEnclosureSpecs enclosureSpecs, IDevice device)
        {
            _enclosureSpecs = enclosureSpecs;
            _device = device;
        }

        public Models.Enclosure CreateEnclosure(EnclosureDao enclosure)
        {
            // if (_checkIfObjectExist(enclosure.ProjectId))
            // {
            //     return null;
            // }
            if (!DoesProjectExist(enclosure.ProjectId))
            {
                return null;
            }

            var enclosureDao = new EnclosureDao()
            {
                Name = enclosure.Name,
                Date = DateTime.Now,
                ProjectId = enclosure.ProjectId
            };

            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string insertQuery = "INSERT INTO Electric.Enclosure VALUES (@name, @date, @projectId); SELECT * FROM Electric.Enclosure WHERE id = SCOPE_IDENTITY()";
          
            return TransformDaoToBusinessLogicEnclosure(database.QueryFirst<EnclosureDao>(insertQuery, enclosureDao));
        }

        public List<Models.Enclosure> GetEnclosures(int? projectId)
        {
            return projectId == null ? GetAll() : GetEnclosuresByProjectId(projectId);
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
        
        public List<Models.Enclosure> GetEnclosuresByProjectId(int? id)
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

        public Models.Enclosure AddNewDevice(int projectId, int enclosureId, Enclosure_Device enclosureDevice)
        {
            if (!CheckIfRowsAndColumnsAreSuitableForEnclosure(enclosureId, enclosureDevice))
            {
                return null;
            }

            if (!CheckIfPositionIsAvailable(enclosureId, enclosureDevice))
            {
                return null;
            }
            
            var enclosure = GetEnclosureById(enclosureId);
            var device = _device.GetDeviceById(enclosureDevice.DeviceId);
            
            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string insertEnclosureDevice = "INSERT INTO Electric.Enclosure_Device VALUES (@enclosureID, @deviceID, @row, @column)";
            database.Execute(insertEnclosureDevice, 
                new {enclosureID = enclosureId, deviceID = enclosureDevice.DeviceId, row = enclosureDevice.Row, column = enclosureDevice.Column});
            
            var devices = _device.GetDevicesForEnclosure(enclosureId);

            return  new Models.Enclosure()
            {
                Id = enclosureId,
                Name = enclosure.Name,
                Date = enclosure.Date,
                ProjectId = projectId,
                Devices = devices,
                TotalPrice = device.Price,
                EnclosureSpecs = _enclosureSpecs.GetEnclosureSpecsByEnclosureId(enclosure.Id),
            };
        }
        
        public Models.Enclosure RemoveDevice(int projectId, int enclosureId, int deviceId)
        {
            var enclosure = GetEnclosureById(enclosureId);
            var device = _device.GetDeviceById(deviceId);
            var totalPrice = new float();

            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string enclosureDevice = "DELETE FROM Electric.Enclosure_Device WHERE deviceId = @deviceID AND enclosureId = @enclosureID";
            database.Execute(enclosureDevice, new {enclosureID = enclosureId, deviceID = deviceId});
            
            var devices = _device.GetDevicesForEnclosure(enclosureId);
            devices.ForEach(el => totalPrice += el.Price);

            return  new Models.Enclosure()
            {
                Id = enclosureId,
                Name = enclosure.Name,
                Date = enclosure.Date,
                ProjectId = projectId,
                Devices = devices,
                TotalPrice = totalPrice,
                EnclosureSpecs = _enclosureSpecs.GetEnclosureSpecsByEnclosureId(enclosure.Id),
            };
        }
        
        private Models.Enclosure TransformDaoToBusinessLogicEnclosure(EnclosureDao enclosureDao)
        {
            var devices = _device.GetDevicesForEnclosure(enclosureDao.Id);
            var totalPrice = new float();
            devices.ForEach(el => totalPrice += el.Price);

            var enclosure = new Models.Enclosure()
            {
                Id = enclosureDao.Id,
                Name = enclosureDao.Name,
                Date = enclosureDao.Date,
                ProjectId = enclosureDao.ProjectId,
                Devices = devices,
                TotalPrice = totalPrice,
                EnclosureSpecs = _enclosureSpecs.GetEnclosureSpecsByEnclosureId(enclosureDao.Id),
            };

            return enclosure;
        }

        private bool CheckIfRowsAndColumnsAreSuitableForEnclosure(int enclosureId, Enclosure_Device enclosureDevice)
        {
            var enclosure = GetEnclosureById(enclosureId);
            var device = _device.GetDeviceById(enclosureDevice.DeviceId);
            return (enclosureDevice.Row <= enclosure.EnclosureSpecs.Rows && enclosureDevice.Column <= enclosure.EnclosureSpecs.Columns) ||
                   (enclosureDevice.Row + device.Height <= enclosure.EnclosureSpecs.Rows && enclosureDevice.Column + device.Width <= enclosure.EnclosureSpecs.Columns);
        }

        private bool CheckIfPositionIsAvailable(int enclosureId, Enclosure_Device enclosureDevice)
        {
            var device = _device.GetDeviceById(enclosureDevice.DeviceId);
            var row = enclosureDevice.Row;
            var column = enclosureDevice.Column;
            var existingEnclosureDevices = new List<Enclosure_Device>();

            using IDbConnection database = new SqlConnection(DatabaseConnectionString);
            const string sql = "SELECT * FROM Electric.Enclosure_Device WHERE enclosureId = @enclosureID";
            var enclosureDevices = database.Query<Enclosure_Device>(sql, new {enclosureID = enclosureId}).ToList();
            enclosureDevices.ForEach(ed =>
            {
                var deviceForEd = _device.GetDeviceById(ed.DeviceId);
                if (((column >= ed.Column && column < ed.Column + deviceForEd.Width && (row <= ed.Row || row <= ed.Row + deviceForEd.Height)) ||
                       (column > ed.Column && column + device.Width <= ed.Column + deviceForEd.Width)) ||
                       ((column <= ed.Column && column + device.Width > ed.Column && (row <= ed.Row || row <= ed.Row + deviceForEd.Height)) ||
                        (column < ed.Column && column + device.Width >= ed.Column)))
                {
                    existingEnclosureDevices.Add(ed);
                }
            });

            return existingEnclosureDevices.Count == 0;
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