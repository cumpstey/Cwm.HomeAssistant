using Cwm.HomeAssistant.Config.Exceptions;
using Cwm.HomeAssistant.Config.Initializtion;
using Cwm.HomeAssistant.Config.Models;
using Cwm.HomeAssistant.Config.Services;
using NUnit.Framework;
using System.Linq;

namespace Cwm.HomeAssistant.ConfigTransformer.Services
{
    public class MqttActuatorConfigTransformerTests
    {
        #region General

        [Test]
        public void Missing_deviceId_throws()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                Name = "Test device",
                Platform = "hubitat",
                Actuators = new[] { new ActuatorDefinition { Type = "switch" } },
            };

            // Assert
            Assert.Throws<ValidationException>(() => transformer.TransformConfig(definition));
        }

        [TestCase("switch")]
        [TestCase("light")]
        [TestCase("heating")]
        public void Unsupported_platform_throws(string type)
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test device",
                Platform = "unsupported",
                Actuators = new[] { new ActuatorDefinition { Type = type } },
            };

            // Assert
            Assert.Throws<UnsupportedPlatformException>(() => transformer.TransformConfig(definition));
        }

        [Test]
        public void Configured_platform_prefix_is_used()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test switch",
                Platform = "smartthings",
                Actuators = new[] { new ActuatorDefinition { Type = "switch" } },
            };
            var expectedPartialConfig = @"
  state_topic: this/is/a/test/Test switch/switch
  command_topic: this/is/a/test/Test switch/switch
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("switch", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["switch"].Count, "Only one entity returned");

            var config = result["switch"].First();
            Assert.IsTrue(config.Entity.Contains(expectedPartialConfig), config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion General

        #region Switch

        [Test]
        public void Switch_config_is_generated()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test switch",
                Platform = "hubitat",
                Actuators = new[] { new ActuatorDefinition { Type = "switch" } },
            };
            var expectedConfig = @"
# Test switch, from hubitat via MQTT
- platform: mqtt
  name: Test switch
  retain: true
  state_topic: hubitat/Test switch/switch
  command_topic: hubitat/Test switch/switch
  payload_on: 'on'
  payload_off: 'off'
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("switch", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["switch"].Count, "Only one entity returned");

            var config = result["switch"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Switch_config_is_generated_for_Zipato()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "abcd",
                Name = "Test switch",
                Platform = "zipato",
                Actuators = new[] { new ActuatorDefinition { Type = "switch" } },
            };
            var expectedConfig = @"
# Test switch, from zipato via MQTT
- platform: mqtt
  name: Test switch
  retain: true
  state_topic: zipato/attributes/abcd/value
  command_topic: zipato/request/attributes/abcd/value
  value_template: ""{{value_json.value}}""
  state_on: true
  state_off: false
  payload_on: '{""value"": true}'
  payload_off: '{""value"": false}'
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("switch", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["switch"].Count, "Only one entity returned");

            var config = result["switch"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Switch_is_customized_with_icon()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test switch",
                Platform = "hubitat",
                Actuators = new[] { new ActuatorDefinition { Type = "switch", Icon = "mdi:my-icon" } },
            };
            var expectedConfig = @"
# Test switch, from hubitat via MQTT
- platform: mqtt
  name: Test switch
  retain: true
  state_topic: hubitat/Test switch/switch
  command_topic: hubitat/Test switch/switch
  payload_on: 'on'
  payload_off: 'off'
".Trim();
            var expectedCustomization = @"
switch.test_switch:
  icon: mdi:my-icon
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("switch", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["switch"].Count, "Only one entity returned");

            var config = result["switch"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.AreEqual(expectedCustomization, config.Customization, "Customization declared as expected");
        }

        #endregion Switch

        #region Light

        [Test]
        public void Light_config_is_generated()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test light",
                Platform = "hubitat",
                Actuators = new[] { new ActuatorDefinition { Type = "light" } },
            };
            var expectedConfig = @"
# Test light, from hubitat via MQTT
- platform: mqtt
  name: Test light
  retain: true
  state_topic: hubitat/Test light/switch
  command_topic: hubitat/Test light/switch
  payload_on: 'on'
  payload_off: 'off'
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("light", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["light"].Count, "Only one entity returned");

            var config = result["light"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Light_is_customized_with_icon()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test light",
                Platform = "hubitat",
                Actuators = new[] { new ActuatorDefinition { Type = "light", Icon = "mdi:my-icon" } },
            };
            var expectedConfig = @"
# Test light, from hubitat via MQTT
- platform: mqtt
  name: Test light
  retain: true
  state_topic: hubitat/Test light/switch
  command_topic: hubitat/Test light/switch
  payload_on: 'on'
  payload_off: 'off'
".Trim();
            var expectedCustomization = @"
light.test_light:
  icon: mdi:my-icon
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("light", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["light"].Count, "Only one entity returned");

            var config = result["light"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.AreEqual(expectedCustomization, config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Dimmable_light_config_is_generated()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test light",
                Platform = "hubitat",
                Actuators = new[] { new ActuatorDefinition { Type = "dimmable-light" } },
            };
            var expectedConfig = @"
# Test light, from hubitat via MQTT
- platform: mqtt
  name: Test light
  retain: true
  state_topic: hubitat/Test light/switch
  command_topic: hubitat/Test light/switch
  payload_on: 'on'
  payload_off: 'off'
  brightness_state_topic: hubitat/Test light/level
  brightness_command_topic: hubitat/Test light/level
  brightness_scale: 99
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("light", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["light"].Count, "Only one entity returned");

            var config = result["light"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Rgbw_light_config_is_generated()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test light",
                Platform = "hubitat",
                Actuators = new[] { new ActuatorDefinition { Type = "rgbw-light" } },
            };
            var expectedConfig = @"
# Test light, from hubitat via MQTT
- platform: mqtt
  name: Test light
  retain: true
  state_topic: hubitat/Test light/switch
  command_topic: hubitat/Test light/switch
  payload_on: 'on'
  payload_off: 'off'
  brightness_state_topic: hubitat/Test light/level
  brightness_command_topic: hubitat/Test light/level
  brightness_scale: 99
  hs_state_topic: hubitat/Test light/hs
  hs_command_topic: hubitat/Test light/hs
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("light", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["light"].Count, "Only one entity returned");

            var config = result["light"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion Switch

        #region Heating

        [Test]
        public void Heating_config_is_generated()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test room",
                Name = "Test heating",
                Platform = "genius",
                Actuators = new[] { new ActuatorDefinition { Type = "heating" } },
            };
            var expectedConfig = @"
# Test heating, from genius via MQTT
- platform: mqtt
  name: Test heating
  modes:
  - auto
  - heat
  current_temperature_topic: genius/Test room/temperature
  mode_state_topic: genius/Test room/thermostatMode
  temperature_state_topic: genius/Test room/heatingSetpoint
  mode_command_topic: genius/Test room/thermostatMode
  temperature_command_topic: genius/Test room/heatingSetpoint
  min_temp: 4
  max_temp: 28
  temp_step: 0.5
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("climate", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["climate"].Count, "Only one entity returned");

            var config = result["climate"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }
        
        [Test]
        public void Heating_is_customized_with_icon()
        {
            // Arrange
            var transformer = new MqttActuatorConfigTransformer(new DummyConfiguration());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test room",
                Name = "Test heating",
                Platform = "genius",
                Actuators = new[] { new ActuatorDefinition { Type = "heating", Icon = "mdi:my-icon" } },
            };
            var expectedConfig = @"
# Test heating, from genius via MQTT
- platform: mqtt
  name: Test heating
  modes:
  - auto
  - heat
  current_temperature_topic: genius/Test room/temperature
  mode_state_topic: genius/Test room/thermostatMode
  temperature_state_topic: genius/Test room/heatingSetpoint
  mode_command_topic: genius/Test room/thermostatMode
  temperature_command_topic: genius/Test room/heatingSetpoint
  min_temp: 4
  max_temp: 28
  temp_step: 0.5
".Trim();
            var expectedCustomization = @"
climate.test_heating:
  icon: mdi:my-icon
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("climate", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["climate"].Count, "Only one entity returned");

            var config = result["climate"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.AreEqual(expectedCustomization, config.Customization, "Customization declared as expected");
        }

        #endregion Switch
    }
}