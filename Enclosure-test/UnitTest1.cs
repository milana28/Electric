using Electric.Domain;
using Xunit;

namespace Enclosure_test
{
    public class UnitTest1
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
    }
}
