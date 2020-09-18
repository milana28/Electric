using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using Electric.Attributes;
using Electric.Exceptions;
using Electric.Models;
using Electric.Utils;
using Microsoft.EntityFrameworkCore;
using Xunit.Sdk;

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
        private readonly IEnclosureSpecs _enclosureSpecs;
        private readonly IDevice _device;
        private readonly IDbConnection _database;
        
        public Enclosure(IEnclosureSpecs enclosureSpecs, IDevice device, IDatabase database)
        {
            _enclosureSpecs = enclosureSpecs;
            _database = database.Get();
            _device = device;
        }
        
        public Models.Enclosure CreateEnclosure(EnclosureDao enclosure)
        {
            // if (!DoesProjectExist(enclosure.ProjectId))
            // {
            //     throw new ProjectNotFountException("Project does not exist!");
            // }

            var enclosureDao = new EnclosureDao()
            {
                Name = enclosure.Name,
                Date = DateTime.Now,
                ProjectId = enclosure.ProjectId,
            };

            const string insertQuery = "INSERT INTO Electric.Enclosure VALUES (@name, @date, @projectId, null) SELECT * FROM Electric.Enclosure WHERE id = SCOPE_IDENTITY()";
          
            return TransformDaoToBusinessLogicEnclosure(_database.QueryFirst<EnclosureDao>(insertQuery, enclosureDao));
        }
        public List<Models.Enclosure> GetEnclosures(int? projectId)
        {
            return projectId == null ? GetAll() : GetEnclosuresByProjectId(projectId);
        }
        public List<Models.Enclosure> GetAll()
        {
            var enclosureList = new List<Models.Enclosure>();
           
            var enclosures = _database.Query<EnclosureDao>("SELECT * FROM Electric.Enclosure").ToList();

            enclosures.ForEach(e => enclosureList.Add(TransformDaoToBusinessLogicEnclosure(e)));

            return enclosureList;
        }
        public Models.Enclosure GetEnclosureById(int id)
        {
            const string sql= "SELECT * FROM Electric.Enclosure WHERE id = @enclosureId";
            var enclosure = _database.QueryFirstOrDefault<EnclosureDao>(sql, new {enclosureId = id});
            
            if (enclosure ==  null)
            {
                throw new EnclosureNotFoundException("Enclosure does not exist!");
            }

            return TransformDaoToBusinessLogicEnclosure(enclosure);
        }
        public List<Models.Enclosure> GetEnclosuresByProjectId(int? id)
        {
            var enclosureList = new List<Models.Enclosure>();

            const string sql= "SELECT * FROM Electric.Enclosure WHERE projectId = @projectId";
            var enclosures = _database.Query<EnclosureDao>(sql, new {projectId = id}).ToList();

            enclosures.ForEach(e => enclosureList.Add(TransformDaoToBusinessLogicEnclosure(e)));

            return enclosureList;
        }
        public Models.Enclosure DeleteEnclosure(int id)
        {
            var enclosure = GetEnclosureById(id);
            if (enclosure ==  null)
            {
                throw new EnclosureNotFoundException("Enclosure does not exist!");
            }
            
            const string sql= "DELETE FROM Electric.Enclosure WHERE id = @enclosureId";
            _database.Execute(sql, new {enclosureId = id});

            return enclosure;
        }
        public Models.Enclosure AddNewDevice(int projectId, int enclosureId, Enclosure_Device enclosureDevice)
        {
            if (!CheckIfRowsAndColumnsAreSuitableForEnclosure(enclosureId, enclosureDevice))
            {
                return null;
            }
            
            const string sql = 
                "SELECT d.*, ed.row, ed.[column] FROM Electric.Enclosure_Device as ed LEFT JOIN Electric.Device as d ON d.id = ed.deviceId WHERE enclosureId = @enclosureID";
            var existingDevicesWithPosition = _database.Query<DeviceDto>(sql, new {enclosureID = enclosureId}).ToList();

            if (!CheckIfPositionIsAvailable(existingDevicesWithPosition, enclosureDevice))
            {
                return null;
            }
            
            var enclosure = GetEnclosureById(enclosureId);
            if (enclosure ==  null)
            {
                throw new EnclosureNotFoundException("Enclosure does not exist!");
            }

            const string insertEnclosureDevice = "INSERT INTO Electric.Enclosure_Device VALUES (@enclosureID, @deviceID, @row, @column)";
            _database.Execute(insertEnclosureDevice, 
                new {enclosureID = enclosureId, deviceID = enclosureDevice.DeviceId, row = enclosureDevice.Row, column = enclosureDevice.Column});

            var devices = _device.GetDevicesForEnclosure(enclosureId);

            Project.UpdateProjectDate(projectId);
            
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
            if (enclosure ==  null)
            {
                throw new EnclosureNotFoundException("Enclosure does not exist!");
            }
            
            const string enclosureDevice = "DELETE FROM Electric.Enclosure_Device WHERE deviceId = @deviceID AND enclosureId = @enclosureID";
            _database.Execute(enclosureDevice, new {enclosureID = enclosureId, deviceID = deviceId});

            var devices = _device.GetDevicesForEnclosure(enclosureId);
            
            Project.UpdateProjectDate(projectId);

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
            if (enclosure ==  null)
            {
                throw new EnclosureNotFoundException("Enclosure does not exist!");
            }

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
            const string updateEnclosure = "UPDATE Electric.Enclosure SET totalPrice = @totalPrice WHERE id = @enclosureID";
            _database.Execute(updateEnclosure, 
                new {enclosureID = enclosure.Id, totalPrice = CalculateTotalPrice(enclosure, null)});
        }
        public Models.Enclosure UpdateEnclosure(int enclosureId, string name, int rows, int columns)
        {
            const string sql = 
                "SELECT d.*, ed.row, ed.[column] FROM Electric.Enclosure_Device as ed LEFT JOIN Electric.Device as d ON d.id = ed.deviceId WHERE enclosureId = @enclosureID";
            var devicesWithPosition = _database.Query<DeviceDto>(sql, new {enclosureID = enclosureId}).ToList();
            
            if (!CheckIfEnclosureSpecsIsAppropriate(devicesWithPosition, rows, columns))
            {
                return null;
            }
            
            const string updateEnclosureSpecs = "UPDATE Electric.EnclosureSpecs SET rows = @numberOfRows, columns = @numberOfColumns WHERE enclosureId = @enclosureID";
            _database.Execute(updateEnclosureSpecs, new {enclosureID = enclosureId, numberOfRows = rows, numberOfColumns = columns});
      
            const string updateEnclosure = "UPDATE Electric.Enclosure SET name = @newName WHERE id = @enclosureID";
            _database.Execute(updateEnclosure, new {enclosureID = enclosureId, newName = name});
            
            var enclosure = GetEnclosureById(enclosureId);
            if (enclosure ==  null)
            {
                throw new EnclosureNotFoundException("Enclosure does not exist!");
            }
            
            Project.UpdateProjectDate(enclosure.ProjectId);

            return enclosure;
        }
        public static bool CheckIfEnclosureSpecsIsAppropriate(List<DeviceDto> devicesWithPosition, int rows, int columns)
        {
            var allColumns = new List<int>();
            var allRows = new List<int>();
            var allWidths = new List<int>();
            var allHeights = new List<int>();

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
        public static bool AreRowsAndColumnsSuitableForEnclosureDimensions(int deviceRow, int deviceColumn, int enclosureRows,
            int enclosureColumns, int deviceWidth, int deviceHeight)
        {
            return (deviceRow <= enclosureRows && deviceColumn <= enclosureColumns) && 
                   ((deviceRow + deviceHeight - 1) <= enclosureRows && 
                    (deviceColumn + deviceWidth - 1) <= enclosureColumns);
        }
        public static bool NoExistingDevicesWithSameRow(int deviceRow, int existingDeviceRow, int deviceHeight, int existingDeviceHeight)
        {
            return deviceRow != existingDeviceRow && deviceRow != existingDeviceRow + existingDeviceHeight - 1 &&
                   deviceRow + deviceHeight - 1 != existingDeviceRow;
        }
        public static bool DoesDevicesColumnOverlapWithExistingDevicesColumn(int deviceColumn, int existingDeviceColumn, int deviceWidth, int existingDeviceWidth)
        {
            return (deviceColumn >= existingDeviceColumn && deviceColumn <= (existingDeviceColumn + existingDeviceWidth - 1)) ||
                   (deviceColumn <= existingDeviceColumn && (deviceColumn + deviceWidth - 1) >= existingDeviceColumn);
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
        private bool CheckIfRowsAndColumnsAreSuitableForEnclosure(int enclosureId, Enclosure_Device enclosureDevice)
        {
            var enclosure = GetEnclosureById(enclosureId);
            var device = _device.GetDeviceById(enclosureDevice.DeviceId);

            return AreRowsAndColumnsSuitableForEnclosureDimensions(enclosureDevice.Row, enclosureDevice.Column,
                enclosure.EnclosureSpecs.Rows, enclosure.EnclosureSpecs.Columns, device.Width, device.Height);
        }
        private bool CheckIfPositionIsAvailable(List<DeviceDto> existingDevicesWithPosition, Enclosure_Device enclosureDevice)
        {
            var device = _device.GetDeviceById(enclosureDevice.DeviceId);
            var existingEnclosureDevices = new List<DeviceDto>();

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
        private bool DoesProjectExist(int projectId)
        {
            const string sql = "SELECT * FROM Electric.Project WHERE id = @id";
            var project = _database.QueryFirstOrDefault<ProjectDao>(sql, new {id = projectId});
            
            return project != null;
        }
        private ProjectDao GetProject(int projectId)
        {
            const string sql = "SELECT * FROM Electric.Project WHERE id = @id";
            return _database.QueryFirstOrDefault<ProjectDao>(sql, new {id = projectId});
        }
    }
}