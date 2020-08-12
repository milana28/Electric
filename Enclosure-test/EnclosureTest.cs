using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Electric.Models;
using Xunit;
using Enclosure = Electric.Domain.Enclosure;

namespace Enclosure_test
{
    public class EnclosureTest
    {
        public static IEnumerable<object[]> GetDevicesTrue()
        {
            yield return new object[] { Devices, 6, 6 };
            yield return new object[] { Devices, 4, 6 };
        }
        
        public static IEnumerable<object[]> GetDevicesFalse()
        {
            yield return new object[] { Devices, 1, 4 };
            yield return new object[] { Devices, 4, 1 };
        }

        private static readonly List<DeviceDto> Devices = new List<DeviceDto>
        { 
            new DeviceDto()
                {
                    Name = "FI/LS 3LN",
                    Width = 3,
                    Height = 1,
                    Amperes = 10,
                    Price = 150,
                    Row = 1,
                    Column = 4
                },
                new DeviceDto()
                {
                    Name = "FI/LS 3LN",
                    Width = 3,
                    Height = 2,
                    Amperes = 16,
                    Price = 152,
                    Row = 1,
                    Column = 1
                }
            };

        [Theory]
        [InlineData(1, 3, 2, 1)]
        [InlineData(1, 3, 2, 4)]
        [InlineData(4, 3, 1, 1)]
        public void NoExistingDevicesWithSameRowTrue(int deviceRow, int existingDeviceRow, int deviceHeight, int existingDeviceHeight)
        {
            var result = Enclosure.NoExistingDevicesWithSameRow(deviceRow, existingDeviceRow, deviceHeight, existingDeviceHeight);
            Assert.True(result, "Can place device on position");
        }
        
        [Theory]
        [InlineData(2, 2, 2, 2)]
        [InlineData(1, 2, 2, 1)]
        [InlineData(3, 3, 1, 1)]
        public void NoExistingDevicesWithSameRowFalse(int deviceRow, int existingDeviceRow, int deviceHeight, int existingDeviceHeight)
        {
            var result = Enclosure.NoExistingDevicesWithSameRow(deviceRow, existingDeviceRow, deviceHeight, existingDeviceHeight);
            Assert.False(result, "Can't place device on position");
        }

        [Theory]
        [InlineData(3,4,4,6,3,1)]
        [InlineData(4,1,4,4,3,1)]
        [InlineData(1,4,2,6,3,2)]
        public void AreRowsAndColumnsSuitableForEnclosureDimensionsTrue(int deviceRow, int deviceColumn, int enclosureRows,
            int enclosureColumns, int deviceWidth, int deviceHeight)
        {
            var result = Enclosure.AreRowsAndColumnsSuitableForEnclosureDimensions(deviceRow, deviceColumn,
                enclosureRows, enclosureColumns, deviceWidth, deviceHeight);
            Assert.True(result, "Can place device on enclosure");
        }
        
        [Theory]
        [InlineData(3,4,4,6,3,3)]
        [InlineData(4,1,4,4,3,2)]
        [InlineData(1,4,2,4,3,2)]
        public void AreRowsAndColumnsSuitableForEnclosureDimensionsFalse(int deviceRow, int deviceColumn, int enclosureRows,
            int enclosureColumns, int deviceWidth, int deviceHeight)
        {
            var result = Enclosure.AreRowsAndColumnsSuitableForEnclosureDimensions(deviceRow, deviceColumn,
                enclosureRows, enclosureColumns, deviceWidth, deviceHeight);
            Assert.False(result, "Can't place device on enclosure");
        }

        [Theory]
        [InlineData(3,4,3,2)]
        [InlineData(4,2,3,3)]
        [InlineData(2,4,3,3)]
        public void DoesDevicesColumnOverlapWithExistingDevicesColumnTrue(int deviceColumn, int existingDeviceColumn,
            int deviceWidth, int existingDeviceWidth)
        {
            var result = Enclosure.DoesDevicesColumnOverlapWithExistingDevicesColumn(deviceColumn, existingDeviceColumn,
                deviceWidth, existingDeviceWidth);
            Assert.True(result, "Can't place device on enclosure");
        }
        
        [Theory]
        [InlineData(4,1,3,3)]
        [InlineData(2,5,3,3)]
        [InlineData(1,4,2,3)]
        public void DoesDevicesColumnOverlapWithExistingDevicesColumnFalse(int deviceColumn, int existingDeviceColumn,
            int deviceWidth, int existingDeviceWidth)
        {
            var result = Enclosure.DoesDevicesColumnOverlapWithExistingDevicesColumn(deviceColumn, existingDeviceColumn,
                deviceWidth, existingDeviceWidth);
            Assert.False(result, "Can place device on enclosure");
        }

      
        [Theory]
        [MemberData(nameof(GetDevicesTrue))]
        public void CheckIfEnclosureSpecsIsAppropriateTrue(List<DeviceDto> devicesWithPosition, int rows, int columns)
        {
            var result = Enclosure.CheckIfEnclosureSpecsIsAppropriate(devicesWithPosition, rows, columns);
            Assert.True(result, "Can change dimensions of enclosure");
        }
        
        [Theory]
        [MemberData(nameof(GetDevicesFalse))]
        public void CheckIfEnclosureSpecsIsAppropriateFalse(List<DeviceDto> devicesWithPosition, int rows, int columns)
        {
            var result = Enclosure.CheckIfEnclosureSpecsIsAppropriate(devicesWithPosition, rows, columns);
            Assert.False(result, "Can't change dimensions of enclosure");
        }
    }
}
