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
            var transformer = new LovelaceConfigTransformer(new DeviceTranslator());
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
            var transformer = new LovelaceConfigTransformer(new DeviceTranslator());
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

        #region Button entities

        [Test]
        public void Button_entities_file_is_generated()
        {
            // Arrange
            var filesystem = new DummyFilesystem();
            var transformer = new LovelaceConfigTransformer(new DeviceTranslator());
            var generator = new LovelaceConfigGenerator(filesystem, transformer);

            filesystem.WriteFileAsync(@"Z:\source\devices.yaml", @"
# My test button
- deviceId: Test button
  platform: hubitat
  sensors:
  - type: button
  - type: hold-button
".Trim());

            var expectedConfig = @"
- type: custom:fold-entity-row
  entity: binary_sensor.test_button_active
  entities:
  - entity: binary_sensor.test_button
    name: Push
  - entity: binary_sensor.test_button_hold
    name: Hold
".Trim();

            // Action
            Task.WaitAll(generator.GenerateConfigAsync(@"Z:\source", @"Z:\output"));
            var generatedConfig = filesystem.ReadFileAsync(@"Z:\output\button-entities.yaml").Result;

            // Assert
            Assert.AreEqual(expectedConfig, generatedConfig.Trim(), "Config declared as expected");
        }

        [Test]
        public void Button_entities_with_multiple_buttons()
        {
            // Arrange
            var filesystem = new DummyFilesystem();
            var transformer = new LovelaceConfigTransformer(new DeviceTranslator());
            var generator = new LovelaceConfigGenerator(filesystem, transformer);

            filesystem.WriteFileAsync(@"Z:\source\devices.yaml", @"
# My test button
- deviceId: Test button
  platform: hubitat
  sensors:
  - type: 3-button
".Trim());

            var expectedConfig = @"
- type: custom:fold-entity-row
  entity: binary_sensor.test_button_active
  entities:
  - entity: binary_sensor.test_button_1
    name: Button 1
  - entity: binary_sensor.test_button_2
    name: Button 2
  - entity: binary_sensor.test_button_3
    name: Button 3
".Trim();

            // Action
            Task.WaitAll(generator.GenerateConfigAsync(@"Z:\source", @"Z:\output"));
            var generatedConfig = filesystem.ReadFileAsync(@"Z:\output\button-entities.yaml").Result;

            // Assert
            Assert.AreEqual(expectedConfig, generatedConfig.Trim(), "Config declared as expected");
        }

        [Test]
        public void Button_entities_with_multiple_buttons_with_alternative_name()
        {
            // Arrange
            var filesystem = new DummyFilesystem();
            var transformer = new LovelaceConfigTransformer(new DeviceTranslator());
            var generator = new LovelaceConfigGenerator(filesystem, transformer);

            filesystem.WriteFileAsync(@"Z:\source\devices.yaml", @"
# My test button
- deviceId: Test controller
  platform: hubitat
  sensors:
  - type: 3-button
".Trim());

            var expectedConfig = @"
- type: custom:fold-entity-row
  entity: binary_sensor.test_controller_active
  entities:
  - entity: binary_sensor.test_controller_button_1
    name: Button 1
  - entity: binary_sensor.test_controller_button_2
    name: Button 2
  - entity: binary_sensor.test_controller_button_3
    name: Button 3
".Trim();

            // Action
            Task.WaitAll(generator.GenerateConfigAsync(@"Z:\source", @"Z:\output"));
            var generatedConfig = filesystem.ReadFileAsync(@"Z:\output\button-entities.yaml").Result;

            // Assert
            Assert.AreEqual(expectedConfig, generatedConfig.Trim(), "Config declared as expected");
        }

        [Test]
        public void Button_entities_with_multiple_hold_buttons()
        {
            // Arrange
            var filesystem = new DummyFilesystem();
            var transformer = new LovelaceConfigTransformer(new DeviceTranslator());
            var generator = new LovelaceConfigGenerator(filesystem, transformer);

            filesystem.WriteFileAsync(@"Z:\source\devices.yaml", @"
# My test button
- deviceId: Test button
  platform: hubitat
  sensors:
  - type: 3-button
  - type: 3-hold-release-button
".Trim());

            var expectedConfig = @"
- type: custom:fold-entity-row
  entity: binary_sensor.test_button_active
  entities:
  - entity: binary_sensor.test_button_1
    name: Button 1
  - entity: binary_sensor.test_button_1_hold
    name: Button 1 hold
  - entity: binary_sensor.test_button_2
    name: Button 2
  - entity: binary_sensor.test_button_2_hold
    name: Button 2 hold
  - entity: binary_sensor.test_button_3
    name: Button 3
  - entity: binary_sensor.test_button_3_hold
    name: Button 3 hold
".Trim();

            // Action
            Task.WaitAll(generator.GenerateConfigAsync(@"Z:\source", @"Z:\output"));
            var generatedConfig = filesystem.ReadFileAsync(@"Z:\output\button-entities.yaml").Result;

            // Assert
            Assert.AreEqual(expectedConfig, generatedConfig.Trim(), "Config declared as expected");
        }

        #endregion

        #region Offline entities

        [Test]
        public void Connectivity_entities_file_is_generated()
        {
            // Arrange
            var filesystem = new DummyFilesystem();
            var transformer = new LovelaceConfigTransformer(new DeviceTranslator());
            var generator = new LovelaceConfigGenerator(filesystem, transformer);

            filesystem.WriteFileAsync(@"Z:\source\devices.yaml", @"
# My test device
- deviceId: Test device
  platform: hubitat
  sensors:
  - type: connectivity
".Trim());

            var expectedConfig = @"
- entity: binary_sensor.test_device_connectivity
  name: Test device
".Trim();

            // Action
            Task.WaitAll(generator.GenerateConfigAsync(@"Z:\source", @"Z:\output"));
            var generatedConfig = filesystem.ReadFileAsync(@"Z:\output\connectivity-entities.yaml").Result;

            // Assert
            Assert.AreEqual(expectedConfig, generatedConfig.Trim(), "Config declared as expected");
        }

        [Test]
        public void Connectivity_entities_existing_entries_are_retained()
        {
            // Arrange
            var filesystem = new DummyFilesystem();
            var transformer = new LovelaceConfigTransformer(new DeviceTranslator());
            var generator = new LovelaceConfigGenerator(filesystem, transformer);

            filesystem.WriteFileAsync(@"Z:\output\connectivity-entities.yaml", @"
- entity: binary_sensor.my_existing_device_connectivity
  name: My existing device
");

            filesystem.WriteFileAsync(@"Z:\source\devices.yaml", @"
# My test device
- deviceId: Test device
  platform: hubitat
  sensors:
  - type: connectivity
".Trim());

            var expectedConfig = @"
- entity: binary_sensor.my_existing_device_connectivity
  name: My existing device
- entity: binary_sensor.test_device_connectivity
  name: Test device
".Trim();

            // Action
            Task.WaitAll(generator.GenerateConfigAsync(@"Z:\source", @"Z:\output"));
            var generatedConfig = filesystem.ReadFileAsync(@"Z:\output\connectivity-entities.yaml").Result;

            // Assert
            Assert.AreEqual(expectedConfig, generatedConfig.Trim(), "Config declared as expected");
        }

        #endregion
    }
}