using Cwm.HomeAssistant.Config.Exceptions;
using Cwm.HomeAssistant.Config.Initializtion;
using Cwm.HomeAssistant.Config.Models;
using Cwm.HomeAssistant.Config.Services;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Cwm.HomeAssistant.ConfigTransformer.Services
{
    public class MqttSensorConfigTransformerTests
    {
        #region General

        [Test]
        public void Missing_deviceId_throws()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                Name = "Test device",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "contact" } },
            };

            // Assert
            Assert.Throws<ValidationException>(() => transformer.TransformConfig(definition));
        }

        public void Unsupported_platform_throws()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test device",
                Platform = "unsupported",
                Sensors = new[] { new SensorDefinition { Type = "contact" } },
            };

            // Assert
            Assert.Throws<UnsupportedPlatformException>(() => transformer.TransformConfig(definition));
        }

        [Test]
        public void Configured_platform_prefix_is_used()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test sensor",
                Platform = "smartthings",
                Sensors = new[] { new SensorDefinition { Type = "temperature" } },
            };
            var expectedPartialConfig = @"
  state_topic: this/is/a/test/Test sensor/temperature
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.sensor"].Count, "Only one entity returned");

            var config = result["mqtt.sensor"].First();
            Assert.IsTrue(config.Entity.Contains(expectedPartialConfig), config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [TestCase("battery", "sensor", "Test multisensor battery", null)]
        [TestCase("button", "binary_sensor", "Test device", null)]
        [TestCase("contact", "binary_sensor", "Test device contact", "Test device")]
        [TestCase("motion", "binary_sensor", "Test device motion", null)]
        [TestCase("presence", "binary_sensor", "Test device", null)]
        [TestCase("temperature", "sensor", "Test device temperature", null)]
        public void Generated_name_is_as_expected(string sensorType, string entityType, string name, string friendlyName)
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test multisensor",
                Platform = "hubitat",
                Name = "Test device",
                Sensors = new[] { new SensorDefinition { Type = sensorType } },
            };
            var expectedPartialConfig = $@"
  name: {name}
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual($"mqtt.{entityType}", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result[$"mqtt.{entityType}"].Count, "Only one entity returned");

            var config = result[$"mqtt.{entityType}"].First();
            Assert.IsTrue(config.Entity.Contains(expectedPartialConfig), "Config declared as expected");

            if (friendlyName != null)
            {
                var expectedPartialCustomization = $@"
  friendly_name: {friendlyName}
".Trim();
                Assert.IsTrue(config.Customization.Contains(expectedPartialCustomization), "Customization declared as expected");
            }
        }

        [Test]
        public void Multiple_entities_are_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test multisensor",
                Platform = "hubitat",
                Name = "Test device",
                Sensors = new[] {
                    new SensorDefinition { Type = "battery" },
                    new SensorDefinition { Type = "motion" },
                    new SensorDefinition { Type = "temperature" },
                },
            };

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(2, result.Keys.Count, "Multiple entity types returned");
            Assert.Contains("mqtt.sensor", result.Keys, "Expected entity types returned");
            Assert.Contains("mqtt.binary_sensor", result.Keys, "Expected entity types returned");
            Assert.AreEqual(2, result["mqtt.sensor"].Count, "Expected number of entities returned");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Expected number of entities returned");
        }

        [Test]
        public void Generated_name_with_apostrophe_is_as_expected()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test user's button",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "button" } },
            };
            var expectedPartialConfig = $@"
  name: Test users button
".Trim();
            var expectedCustomization = $@"
binary_sensor.test_users_button:
  friendly_name: Test user's button
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.IsTrue(config.Entity.Contains(expectedPartialConfig), config.Entity, "Config declared as expected");

            Assert.AreEqual(expectedCustomization, config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Customizations_as_expected()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test device",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition {
                    Type = "battery",
                    Customize = new Dictionary<string, string>{ { "my_custom_attr", "The value" } },
                } },
            };
            var expectedCustomization = $@"
sensor.test_device_battery:
  my_custom_attr: The value
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.sensor"].Count, "Only one entity returned");

            var config = result["mqtt.sensor"].First();
            //Assert.IsTrue(config.Entity.Contains(expectedPartialConfig), config.Entity, "Config declared as expected");

            Assert.AreEqual(expectedCustomization, config.Customization, "Customization declared as expected");
        }

        #endregion General

        #region Battery

        [Test]
        public void Battery_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test multisensor",
                Platform = "hubitat",
                Name = "Test device",
                Sensors = new[] { new SensorDefinition { Type = "battery" } },
            };
            var expectedConfig = @"
# Test multisensor battery, from hubitat via MQTT
- name: Test multisensor battery
  device_class: battery
  state_topic: hubitat/Test multisensor/battery
  value_template: >
    {{ value | int }}
  unit_of_measurement: '%'
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.sensor"].Count, "Only one entity returned");

            var config = result["mqtt.sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Button

        [TestCase("button")]
        [TestCase("1-button")]
        public void Button_sensor_config_is_generated(string sensorType)
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test button",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = sensorType } },
            };
            var expectedConfig = @"
# Test button, from hubitat via MQTT
- name: Test button
  state_topic: hubitat/Test button/1/push
  payload_on: pushed
  off_delay: 1
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Multiple_button_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test button",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "2-button" } },
            };
            var expectedConfig = new[] { @"
# Test button 1, from hubitat via MQTT
- name: Test button 1
  state_topic: hubitat/Test button/1/push
  payload_on: pushed
  off_delay: 1
".Trim(), @"
# Test button 2, from hubitat via MQTT
- name: Test button 2
  state_topic: hubitat/Test button/2/push
  payload_on: pushed
  off_delay: 1
".Trim() };

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(expectedConfig.Length, result["mqtt.binary_sensor"].Count, "Correct number of entities returned");

            var configs = result["mqtt.binary_sensor"].Select(i => i.Entity).ToArray();
            foreach (var expected in expectedConfig)
            {
                Assert.Contains(expected, configs, "Config declared as expected");
            }

            Assert.IsTrue(result["mqtt.binary_sensor"].All(i => string.IsNullOrEmpty(i.Customization)), "Customization declared as expected");
        }

        [Test]
        public void Multiple_button_sensor_config_is_generated_with_alternative_name()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test controller",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "2-button" } },
            };
            var expectedConfig = new[] { @"
# Test controller button 1, from hubitat via MQTT
- name: Test controller button 1
  state_topic: hubitat/Test controller/1/push
  payload_on: pushed
  off_delay: 1
".Trim(), @"
# Test controller button 2, from hubitat via MQTT
- name: Test controller button 2
  state_topic: hubitat/Test controller/2/push
  payload_on: pushed
  off_delay: 1
".Trim() };

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(expectedConfig.Length, result["mqtt.binary_sensor"].Count, "Correct number of entities returned");

            var configs = result["mqtt.binary_sensor"].Select(i => i.Entity).ToArray();
            foreach (var expected in expectedConfig)
            {
                Assert.Contains(expected, configs, "Config declared as expected");
            }

            Assert.IsTrue(result["mqtt.binary_sensor"].All(i => string.IsNullOrEmpty(i.Customization)), "Customization declared as expected");
        }

        [Test]
        public void Hold_button_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test button",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "hold-button" } },
            };
            var expectedConfig = @"
# Test button hold, from hubitat via MQTT
- name: Test button hold
  state_topic: hubitat/Test button/1/hold
  payload_on: held
  off_delay: 1
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [TestCase("hold-release-button")]
        [TestCase("1-hold-release-button")]
        public void Hold_release_button_sensor_config_is_generated(string sensorType)
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test button",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = sensorType } },
            };
            var expectedConfig = @"
# Test button hold, from hubitat via MQTT
- name: Test button hold
  state_topic: hubitat/Test button/1/hold
  payload_on: held
  payload_off: released
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Multiple_hold_release_button_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test button",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "2-hold-release-button" } },
            };
            var expectedConfig = new[] { @"
# Test button 1 hold, from hubitat via MQTT
- name: Test button 1 hold
  state_topic: hubitat/Test button/1/hold
  payload_on: held
  payload_off: released
".Trim(), @"
# Test button 2 hold, from hubitat via MQTT
- name: Test button 2 hold
  state_topic: hubitat/Test button/2/hold
  payload_on: held
  payload_off: released
".Trim() };

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(expectedConfig.Length, result["mqtt.binary_sensor"].Count, "Correct number of entities returned");

            var configs = result["mqtt.binary_sensor"].Select(i => i.Entity).ToArray();
            foreach (var expected in expectedConfig)
            {
                Assert.Contains(expected, configs, "Config declared as expected");
            }

            Assert.IsTrue(result["mqtt.binary_sensor"].All(i => string.IsNullOrEmpty(i.Customization)), "Customization declared as expected");
        }

        #endregion

        #region Contact

        [Test]
        public void Contact_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "contact" } },
            };
            var expectedConfig = @"
# Test contact, from hubitat via MQTT
- name: Test contact
  state_topic: hubitat/Test/contact
  payload_on: open
  payload_off: closed
".Trim();
            var expectedCustomization = @"
binary_sensor.test_contact:
  friendly_name: Test
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.AreEqual(expectedCustomization, config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Contact_sensor_config_is_generated_with_device_class()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test door",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "contact", DeviceClass = "door" } },
            };
            var expectedConfig = @"
# Test door contact, from hubitat via MQTT
- name: Test door contact
  device_class: door
  state_topic: hubitat/Test door/contact
  payload_on: open
  payload_off: closed
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
        }

        #endregion

        #region Illuminance

        [Test]
        public void Illuminance_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test multisensor",
                Platform = "hubitat",
                Name = "Test device",
                Sensors = new[] { new SensorDefinition { Type = "illuminance" } },
            };
            var expectedConfig = @"
# Test device illuminance, from hubitat via MQTT
- name: Test device illuminance
  device_class: illuminance
  state_topic: hubitat/Test multisensor/illuminance
  value_template: >
    {{ value | int }}
  unit_of_measurement: lux
  force_update: true
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.sensor"].Count, "Only one entity returned");

            var config = result["mqtt.sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Moisture

        [Test]
        public void Moisture_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test flood",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "moisture" } },
            };
            var expectedConfig = @"
# Test flood moisture, from hubitat via MQTT
- name: Test flood moisture
  device_class: moisture
  state_topic: hubitat/Test flood/water
  payload_on: wet
  payload_off: dry
".Trim();
            var expectedCustomization = @"
binary_sensor.test_flood_moisture:
  friendly_name: Test flood
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.AreEqual(expectedCustomization, config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Motion

        [Test]
        public void Motion_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test multisensor",
                Platform = "hubitat",
                Name = "Test device",
                Sensors = new[] { new SensorDefinition { Type = "motion" } },
            };
            var expectedConfig = @"
# Test device motion, from hubitat via MQTT
- name: Test device motion
  device_class: motion
  state_topic: hubitat/Test multisensor/motion
  payload_on: active
  payload_off: inactive
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Connectivity

        [Test]
        public void Connectivity_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test multisensor",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "connectivity" } },
            };
            var expectedConfig = @"
# Test multisensor connectivity, from hubitat via MQTT
- name: Test multisensor connectivity
  device_class: connectivity
  state_topic: hubitat/Test multisensor/activity
  payload_on: active
  payload_off: inactive
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Power

        [Test]
        public void Power_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test meter",
                Platform = "hubitat",
                Name = "Test device",
                Sensors = new[] { new SensorDefinition { Type = "power" } },
            };
            var expectedConfig = @"
# Test device power, from hubitat via MQTT
- name: Test device power
  state_topic: hubitat/Test meter/power
  value_template: >
    {{ value | float }}
  unit_of_measurement: W
  force_update: true
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.sensor"].Count, "Only one entity returned");

            var config = result["mqtt.sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Power_cycle_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test meter",
                Platform = "hubitat",
                Name = "Test device",
                Sensors = new[] { new SensorDefinition { Type = "power-cycle" } },
            };
            var expectedConfig = @"
# Test device cycle, from homeassistant via MQTT
- name: Test device cycle
  state_topic: homeassistant/Test device/cycle
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.sensor"].Count, "Only one entity returned");

            var config = result["mqtt.sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Missing_power_cycle_sensor_throws()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test meter",
                Platform = "hubitat",
                Name = "Test device",
                Sensors = new[] { new SensorDefinition { Type = "power-cycle-onoff" } },
            };

            // Assert
            Assert.Throws<ValidationException>(() => transformer.TransformConfig(definition));
        }

        [Test]
        public void Power_cycle_onoff_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test meter",
                Platform = "hubitat",
                Name = "Test device",
                Sensors = new[] {
                    new SensorDefinition { Type = "power-cycle" },
                    new SensorDefinition { Type = "power-cycle-onoff" },
                },
            };
            var expectedConfig = @"
# Test device
- platform: template
  sensors:
    test_device:
      value_template: >
        {{states('sensor.test_device_cycle') not in ['unknown','off']}}
".Trim();
            var expectedCustomization = @"
binary_sensor.test_device:
  friendly_name: Test device
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            // We don't care about the sensor here - that's tested elsewhere. Just look at the binary_sensor.
            Assert.IsTrue(result.ContainsKey("binary_sensor"), "Expected entity type is returned");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.AreEqual(expectedCustomization, config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Presence

        [Test]
        public void Presence_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test presence",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "presence" } },
            };
            var expectedConfig = @"
# Test presence, from hubitat via MQTT
- name: Test presence
  device_class: presence
  state_topic: hubitat/Test presence/presence
  payload_on: present
  payload_off: not present
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Scene

        [Test]
        public void Scene_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test device",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "scene" } },
            };
            var expectedConfig = @"
# Test device scene, from hubitat via MQTT
- name: Test device scene
  state_topic: hubitat/Test device/scene
  force_update: true
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.sensor"].Count, "Only one entity returned");

            var config = result["mqtt.sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Smoke

        [Test]
        public void Smoke_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test smoke",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "smoke" } },
            };
            var expectedConfig = @"
# Test smoke, from hubitat via MQTT
- name: Test smoke
  device_class: smoke
  state_topic: hubitat/Test smoke/smoke
  value_template: >
    {%if value == 'clear'-%}clear{%-else-%}smoke{%-endif%}
  payload_on: smoke
  payload_off: clear
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Temperature

        [Test]
        public void Temperature_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test multisensor",
                Platform = "hubitat",
                Name = "Test device",
                Sensors = new[] { new SensorDefinition { Type = "temperature" } },
            };
            var expectedConfig = @"
# Test device temperature, from hubitat via MQTT
- name: Test device temperature
  device_class: temperature
  state_topic: hubitat/Test multisensor/temperature
  value_template: >
    {{ value | float }}
  unit_of_measurement: �C
  force_update: true
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.sensor"].Count, "Only one entity returned");

            var config = result["mqtt.sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Threshold

        [Test]
        public void Missing_threshold_attribute_throws()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test meter",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "threshold", OnCondition = "> 5" } },
            };

            // Action
            Assert.Throws<ValidationException>(() => transformer.TransformConfig(definition));
        }

        [Test]
        public void Missing_threshold_condition_throws()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test meter",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "power-threshold" } },
            };

            // Action
            Assert.Throws<ValidationException>(() => transformer.TransformConfig(definition));
        }

        [Test]
        public void Threshold_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration(), new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "Test meter",
                Platform = "hubitat",
                Sensors = new[] { new SensorDefinition { Type = "power-threshold", OnCondition = "< 5" } },
            };
            var expectedConfig = @"
# Test meter, from hubitat via MQTT
- name: Test meter
  state_topic: hubitat/Test meter/power
  value_template: '{%if (value | float) < 5-%}on{%-else-%}off{%-endif%}'
  payload_on: 'on'
  payload_off: 'off'
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("mqtt.binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["mqtt.binary_sensor"].Count, "Only one entity returned");

            var config = result["mqtt.binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion
    }
}