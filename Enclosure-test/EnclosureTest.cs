using Electric.Domain;
using Xunit;

namespace Enclosure_test
{
    public class EnclosureTest
    {
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
    }
}
