using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Electric.Models;
using Microsoft.Extensions.Configuration;

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
        Models.Enclosure GetEnclosureWithDevice(int enclosureId, int deviceId);
        void RecalculateTotalPrice(Models.Enclosure enclosure);
        Models.Enclosure UpdateEnclosure(int enclosureId, string name, int rows, int columns);
    }
    
    public class Enclosure : IEnclosure
    {
        private static IConfiguration _configuration;
        private readonly IEnclosureSpecs _enclosureSpecs;
        private readonly IDevice _device;

        public Enclosure(IConfiguration configuration, IEnclosureSpecs enclosureSpecs, IDevice device)
        {
            _configuration = configuration;
            _enclosureSpecs = enclosureSpecs;
            _device = device;
        }

        public Models.Enclosure CreateEnclosure(EnclosureDao enclosure)
        {
            if (!DoesProjectExist(enclosure.ProjectId))
            {
                return null;
            }

            var enclosureDao = new EnclosureDao()
            {
                Name = enclosure.Name,
                Date = DateTime.Now,
                ProjectId = enclosure.ProjectId,
            };

            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string insertQuery = "INSERT INTO Electric.Enclosure VALUES (@name, @date, @projectId, null) SELECT * FROM Electric.Enclosure WHERE id = SCOPE_IDENTITY()";
          
            return TransformDaoToBusinessLogicEnclosure(database.QueryFirst<EnclosureDao>(insertQuery, enclosureDao));
        }

        public List<Models.Enclosure> GetEnclosures(int? projectId)
        {
            return projectId == null ? GetAll() : GetEnclosuresByProjectId(projectId);
        }
        
        public List<Models.Enclosure> GetAll()
        {
            var enclosureList = new List<Models.Enclosure>();
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            var enclosures = database.Query<EnclosureDao>("SELECT * FROM Electric.Enclosure").ToList();

            enclosures.ForEach(e => enclosureList.Add(TransformDaoToBusinessLogicEnclosure(e)));

            return enclosureList;
        }
        
        public Models.Enclosure GetEnclosureById(int id)
        {
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string sql= "SELECT * FROM Electric.Enclosure WHERE id = @enclosureId";
            
            var enclosure = database.QuerySingle<EnclosureDao>(sql, new {enclosureId = id});

            return TransformDaoToBusinessLogicEnclosure(enclosure);
        }
        
        public List<Models.Enclosure> GetEnclosuresByProjectId(int? id)
        {
            var enclosureList = new List<Models.Enclosure>();
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string sql= "SELECT * FROM Electric.Enclosure WHERE projectId = @projectId";
            
            var enclosures = database.Query<EnclosureDao>(sql, new {projectId = id}).ToList();

            enclosures.ForEach(e => enclosureList.Add(TransformDaoToBusinessLogicEnclosure(e)));

            return enclosureList;
        }
        
        public Models.Enclosure DeleteEnclosure(int id)
        {
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
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
            
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
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
                TotalPrice = CalculateTotalPrice(enclosure, null),
                EnclosureSpecs = _enclosureSpecs.GetEnclosureSpecsByEnclosureId(enclosure.Id),
            };
        }
        
        public Models.Enclosure RemoveDevice(int projectId, int enclosureId, int deviceId)
        {
            var enclosure = GetEnclosureById(enclosureId);

            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string enclosureDevice = "DELETE FROM Electric.Enclosure_Device WHERE deviceId = @deviceID AND enclosureId = @enclosureID";
            database.Execute(enclosureDevice, new {enclosureID = enclosureId, deviceID = deviceId});

            var devices = _device.GetDevicesForEnclosure(enclosureId);

            return  new Models.Enclosure()
            {
                Id = enclosureId,
                Name = enclosure.Name,
                Date = enclosure.Date,
                ProjectId = projectId,
                Devices = devices,
                TotalPrice = CalculateTotalPrice(enclosure, null),
                EnclosureSpecs = _enclosureSpecs.GetEnclosureSpecsByEnclosureId(enclosure.Id),
            };
        }
        
        public Models.Enclosure GetEnclosureWithDevice(int enclosureId, int deviceId)
        {
            var devices = _device.GetDeviceForEnclosureById(enclosureId, deviceId);
            var enclosure = GetEnclosureById(enclosureId);

            return new Models.Enclosure()
            {
                Id = enclosureId,
                Name = enclosure.Name,
                Date = enclosure.Date,
                ProjectId = enclosure.ProjectId,
                Devices = devices,
                TotalPrice = CalculateTotalPrice(enclosure, devices),
                EnclosureSpecs = _enclosureSpecs.GetEnclosureSpecsByEnclosureId(enclosure.Id),
            };
        }
        
        public void RecalculateTotalPrice(Models.Enclosure enclosure)
        {
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string updateEnclosure = "UPDATE Electric.Enclosure SET totalPrice = @totalPrice WHERE id = @enclosureID";
            database.Execute(updateEnclosure, 
                new {enclosureID = enclosure.Id, totalPrice = CalculateTotalPrice(enclosure, null)});
        }

        public Models.Enclosure UpdateEnclosure(int enclosureId, string name, int rows, int columns)
        {
            if (!CheckIfEnclosureSpecsIsAppropriate(enclosureId, rows, columns))
            {
                return null;
            }
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string updateEnclosureSpecs = "UPDATE Electric.EnclosureSpecs SET rows = @numberOfRows, columns = @numberOfColumns WHERE enclosureId = @enclosureID";
            database.Execute(updateEnclosureSpecs, new {enclosureID = enclosureId, numberOfRows = rows, numberOfColumns = columns});
      
            const string updateEnclosure = "UPDATE Electric.Enclosure SET name = @newName WHERE id = @enclosureID";
            database.Execute(updateEnclosure, new {enclosureID = enclosureId, newName = name});

            return GetEnclosureById(enclosureId);
        }

        private Models.Enclosure TransformDaoToBusinessLogicEnclosure(EnclosureDao enclosureDao)
        {
            var devices = _device.GetDevicesForEnclosure(enclosureDao.Id);
            var enclosure = new Models.Enclosure()
            {
                Id = enclosureDao.Id,
                Name = enclosureDao.Name,
                Date = enclosureDao.Date,
                ProjectId = enclosureDao.ProjectId,
                Devices = devices,
                TotalPrice = enclosureDao.TotalPrice,
                EnclosureSpecs = _enclosureSpecs.GetEnclosureSpecsByEnclosureId(enclosureDao.Id),
            };

            return enclosure;
        }

        private static bool CheckIfEnclosureSpecsIsAppropriate(int enclosureId, int rows, int columns)
        {
            var allColumns = new List<int>();
            var allRows = new List<int>();
            var allWidths = new List<int>();
            var allHeights = new List<int>();
           
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string sql = 
                "SELECT d.*, ed.row, ed.[column] FROM Electric.Enclosure_Device as ed LEFT JOIN Electric.Device as d ON d.id = ed.deviceId WHERE enclosureId = @enclosureID";
            var devicesWithPosition = database.Query<DeviceDto>(sql, new {enclosureID = enclosureId}).ToList();
            
            if (devicesWithPosition.Count == 0)
            {
                return true;
            }

            devicesWithPosition.ForEach(dp =>
            {
                allColumns.Add(dp.Column);
                allRows.Add(dp.Row);
            });
            
            var maxColumn = allColumns.Max();
            var maxRow = allRows.Max();
            
            var devicesWithMaxColumn = devicesWithPosition.FindAll(d => d.Column == maxColumn);
            devicesWithMaxColumn.ForEach(d => allWidths.Add(d.Width));
            
            var devicesWithMaxRow = devicesWithPosition.FindAll(d => d.Row == maxRow);
            devicesWithMaxRow.ForEach(d => allHeights.Add(d.Height));
            
            var maxWidth = allWidths.Max();
            var maxHeight = allHeights.Max();

            return maxColumn <= columns && maxRow <= rows && (maxWidth + maxColumn - 1) <= columns && (maxHeight + maxRow - 1) <= rows;
        }
        
        private float CalculateTotalPrice(Models.Enclosure enclosure, List<DeviceDto>? deviceWithPosition)
        {
            var totalPrice = new float();
            var devices = _device.GetDevicesForEnclosure(enclosure.Id);
            if (deviceWithPosition == null)
            {
                devices.ForEach(el => totalPrice += el.Price);
            }
            else
            {
                deviceWithPosition.ForEach(el => totalPrice += el.Price);
            }
            
            return totalPrice;
        }

        private bool CheckIfRowsAndColumnsAreSuitableForEnclosure(int enclosureId, Enclosure_Device enclosureDevice)
        {
            var enclosure = GetEnclosureById(enclosureId);
            var device = _device.GetDeviceById(enclosureDevice.DeviceId);

            return AreRowsAndColumnsSuitableForEnclosureDimensions(enclosureDevice.Row, enclosureDevice.Column,
                enclosure.EnclosureSpecs.Rows, enclosure.EnclosureSpecs.Columns, device.Width, device.Height);
        }

        private static bool AreRowsAndColumnsSuitableForEnclosureDimensions(int deviceRow, int deviceColumn, int enclosureRows,
            int enclosureColumns, int deviceWidth, int deviceHeight)
        {
            return (deviceRow <= enclosureRows && deviceColumn <= enclosureColumns) && 
                   ((deviceRow + deviceHeight - 1) <= enclosureRows && 
                    (deviceColumn + deviceWidth - 1) <= enclosureColumns);
        }

        private bool CheckIfPositionIsAvailable(int enclosureId, Enclosure_Device enclosureDevice)
        {
            var device = _device.GetDeviceById(enclosureDevice.DeviceId);
            var existingEnclosureDevices = new List<DeviceDto>();

            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string sql = 
                "SELECT d.*, ed.row, ed.[column] FROM Electric.Enclosure_Device as ed LEFT JOIN Electric.Device as d ON d.id = ed.deviceId WHERE enclosureId = @enclosureID";
            var existingDevicesWithPosition = database.Query<DeviceDto>(sql, new {enclosureID = enclosureId}).ToList();

            existingDevicesWithPosition.ForEach(d =>
            {
                if (NoExistingDevicesWithSameRow(enclosureDevice.Row, d.Row,
                    device.Height, d.Height))
                {
                    return;
                }
                if (DoesDevicesColumnOverlapWithExistingDevicesColumn(enclosureDevice.Column, 
                    d.Column, device.Width, d.Width)) 
                {
                    existingEnclosureDevices.Add(d);
                }
            });

            return existingEnclosureDevices.Count == 0;
        }

        private static bool NoExistingDevicesWithSameRow(int deviceRow, int existingDeviceRow, int deviceHeight, int existingDeviceHeight)
        {
            return deviceRow != existingDeviceRow && deviceRow != existingDeviceRow + existingDeviceHeight - 1 &&
                   deviceRow + deviceHeight - 1 != existingDeviceRow;
        }
        
        private static bool DoesDevicesColumnOverlapWithExistingDevicesColumn(int deviceColumn, int existingDeviceColumn, int deviceWidth, int existingDeviceWidth)
        {
            return (deviceColumn >= existingDeviceColumn && deviceColumn <= (existingDeviceColumn + existingDeviceWidth - 1)) ||
                   (deviceColumn <= existingDeviceColumn && (deviceColumn + deviceWidth - 1) >= existingDeviceColumn);
        }
        
        private static bool DoesProjectExist(int projectId)
        {
            using IDbConnection database = new SqlConnection(_configuration.GetConnectionString("MyConnectionString"));
            const string sql = "SELECT * FROM Electric.Project WHERE id = @id";
            var project = database.QuerySingle<ProjectDao>(sql, new {id = projectId});

            return project != null;
        }
    }
}