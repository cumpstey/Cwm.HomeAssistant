using Cwm.HomeAssistant.Config.Exceptions;
using Cwm.HomeAssistant.Config.Initializtion;
using Cwm.HomeAssistant.Config.Models;
using Cwm.HomeAssistant.Config.Services;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Cwm.HomeAssistant.ConfigTransformer.Services
{
    public class LovelaceConfigGeneratorTests
    {
        #region Battery entities

        [Test]
        public void Battery_entities_file_is_generated()
        {
            // Arrange
            var filesystem = new DummyFilesystem();
            var transformer = new LovelaceConfigTransformer();
            var generator = new LovelaceConfigGenerator(filesystem, transformer);

            filesystem.WriteFileAsync(@"Z:\source\devices.yaml", @"
# My test device
- deviceId: Test device
  platform: hubitat
  sensors:
  - type: battery
".Trim());

            var expectedConfig = @"
- entity: sensor.test_device_battery
  name: Test device
".Trim();

            // Action
            Task.WaitAll(generator.GenerateConfigAsync(@"Z:\source", @"Z:\output"));
            var generatedConfig = filesystem.ReadFileAsync(@"Z:\output\battery-entities.yaml").Result;

            // Assert
            Assert.AreEqual(expectedConfig, generatedConfig.Trim(), "Config declared as expected");
        }

        [Test]
        public void Battery_entities_existing_entries_are_retained()
        {
            // Arrange
            var filesystem = new DummyFilesystem();
            var transformer = new LovelaceConfigTransformer();
            var generator = new LovelaceConfigGenerator(filesystem, transformer);

            filesystem.WriteFileAsync(@"Z:\output\battery-entities.yaml", @"
- entity: sensor.my_existing_device_battery
  name: My existing device
");

            filesystem.WriteFileAsync(@"Z:\source\devices.yaml", @"
# My test device
- deviceId: Test device
  platform: hubitat
  sensors:
  - type: battery
".Trim());

            var expectedConfig = @"
- entity: sensor.my_existing_device_battery
  name: My existing device
- entity: sensor.test_device_battery
  name: Test device
".Trim();

            // Action
            Task.WaitAll(generator.GenerateConfigAsync(@"Z:\source", @"Z:\output"));
            var generatedConfig = filesystem.ReadFileAsync(@"Z:\output\battery-entities.yaml").Result;

            // Assert
            Assert.AreEqual(expectedConfig, generatedConfig.Trim(), "Config declared as expected");
        }

        #endregion
    }
}