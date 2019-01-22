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

        public void Unsupported_platform_throws(string type)
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test device",
                Type = new[] { "contact" },
                Platform = "unsupported",
            };

            // Assert
            Assert.Throws<UnsupportedPlatformException>(() => transformer.TransformConfig(definition));
        }

        [Test]
        public void Configured_platform_prefix_is_used()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test sensor",
                Type = new[] { "temperature" },
                Platform = "smartthings",
            };
            var expectedPartialConfig = @"
  state_topic: this/is/a/test/Test sensor/temperature
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["sensor"].Count, "Only one entity returned");

            var config = result["sensor"].First();
            Assert.IsTrue(config.Entity.Contains(expectedPartialConfig), config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [TestCase("battery", "sensor", "Test multisensor battery")]
        [TestCase("button", "binary_sensor", "Test device")]
        [TestCase("contact", "binary_sensor", "Test device")]
        [TestCase("motion", "binary_sensor", "Test device motion")]
        [TestCase("presence", "binary_sensor", "Test device")]
        [TestCase("temperature", "sensor", "Test device temperature")]
        public void Generated_name_is_as_expected(string sensorType, string entityType, string name)
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test device",
                Type = new[] { sensorType },
                Platform = "hubitat",
                DeviceId = "Test multisensor",
            };
            var expectedPartialConfig = $@"
  name: {name}
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual(entityType, result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result[entityType].Count, "Only one entity returned");

            var config = result[entityType].First();
            Assert.IsTrue(config.Entity.Contains(expectedPartialConfig), config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Multiple_entities_are_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test device",
                Type = new[] { "battery", "motion", "temperature" },
                Platform = "hubitat",
                DeviceId = "Test multisensor",
            };

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(2, result.Keys.Count, "Multiple entity types returned");
            Assert.Contains("sensor", result.Keys, "Expected entity types returned");
            Assert.Contains("binary_sensor", result.Keys, "Expected entity types returned");
            Assert.AreEqual(2, result["sensor"].Count, "Expected number of entities returned");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Expected number of entities returned");
        }

        #endregion General

        #region Battery

        [Test]
        public void Battery_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test device",
                Type = new[] { "battery" },
                Platform = "hubitat",
                DeviceId = "Test multisensor",
            };
            var expectedConfig = @"
# Test multisensor battery, from hubitat via MQTT
- platform: mqtt
  name: Test multisensor battery
  retain: true
  device_class: battery
  state_topic: hubitat/Test multisensor/battery
  unit_of_measurement: '%'
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["sensor"].Count, "Only one entity returned");

            var config = result["sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Button

        [TestCase("button")]
        [TestCase("1-button")]
        public void Button_sensor_config_is_generated(string declaration)
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test button",
                Type = new[] { declaration },
                Platform = "hubitat",
            };
            var expectedConfig = @"
# Test button, from hubitat via MQTT
- platform: mqtt
  name: Test button
  retain: true
  state_topic: hubitat/Test button/pushed
  payload_on: 1
  off_delay: 1
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Multiple_button_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test button",
                Type = new[] { "2-button" },
                Platform = "hubitat",
            };
            var expectedConfig = new[] { @"
# Test button 1, from hubitat via MQTT
- platform: mqtt
  name: Test button 1
  retain: true
  state_topic: hubitat/Test button/pushed
  payload_on: 1
  off_delay: 1
".Trim(), @"
# Test button 2, from hubitat via MQTT
- platform: mqtt
  name: Test button 2
  retain: true
  state_topic: hubitat/Test button/pushed
  payload_on: 2
  off_delay: 1
".Trim() };

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(expectedConfig.Length, result["binary_sensor"].Count, "Correct number of entities returned");

            var configs = result["binary_sensor"].Select(i => i.Entity).ToArray();
            foreach (var expected in expectedConfig)
            {
                Assert.Contains(expected, configs, "Config declared as expected");
            }

            Assert.IsTrue(result["binary_sensor"].All(i => string.IsNullOrEmpty(i.Customization)), "Customization declared as expected");
        }

        [Test]
        public void Hold_button_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test button",
                Type = new[] { "hold-button" },
                Platform = "hubitat",
            };
            var expectedConfig = @"
# Test button (hold), from hubitat via MQTT
- platform: mqtt
  name: Test button (hold)
  retain: true
  state_topic: hubitat/Test button/held
  payload_on: 1
  off_delay: 1
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Hold_release_button_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test button",
                Type = new[] { "hold-release-button" },
                Platform = "hubitat",
            };
            var expectedConfig = @"
# Test button (hold), from hubitat via MQTT
- platform: mqtt
  name: Test button (hold)
  retain: true
  state_topic: hubitat/Test button/contact
  payload_on: closed
  payload_off: open
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Contact

        [Test]
        public void Contact_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test contact",
                Type = new[] { "contact" },
                Platform = "hubitat",
            };
            var expectedConfig = @"
# Test contact, from hubitat via MQTT
- platform: mqtt
  name: Test contact
  retain: true
  state_topic: hubitat/Test contact/contact
  payload_on: open
  payload_off: closed
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        [Test]
        public void Contact_sensor_config_is_generated_with_device_class()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test door",
                Type = new[] { "contact" },
                Platform = "hubitat",
                DeviceClasses = new Dictionary<string, string> { { "contact", "door" } },
            };
            var expectedConfig = @"
# Test door, from hubitat via MQTT
- platform: mqtt
  name: Test door
  retain: true
  device_class: door
  state_topic: hubitat/Test door/contact
  payload_on: open
  payload_off: closed
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Illuminance

        [Test]
        public void Illuminance_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test device",
                Type = new[] { "illuminance" },
                Platform = "hubitat",
                DeviceId = "Test multisensor",
            };
            var expectedConfig = @"
# Test device illuminance, from hubitat via MQTT
- platform: mqtt
  name: Test device illuminance
  retain: true
  device_class: illuminance
  state_topic: hubitat/Test multisensor/illuminance
  unit_of_measurement: lux
  force_update: true
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["sensor"].Count, "Only one entity returned");

            var config = result["sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Motion

        [Test]
        public void Motion_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test device",
                Type = new[] { "motion" },
                Platform = "hubitat",
                DeviceId = "Test multisensor",
            };
            var expectedConfig = @"
# Test device motion, from hubitat via MQTT
- platform: mqtt
  name: Test device motion
  retain: true
  device_class: motion
  state_topic: hubitat/Test multisensor/motion
  payload_on: active
  payload_off: inactive
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Presence

        [Test]
        public void Presence_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test presence",
                Type = new[] { "presence" },
                Platform = "hubitat",
            };
            var expectedConfig = @"
# Test presence, from hubitat via MQTT
- platform: mqtt
  name: Test presence
  retain: true
  device_class: presence
  state_topic: hubitat/Test presence/presence
  payload_on: present
  payload_off: not present
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Temperature

        [Test]
        public void Temperature_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test device",
                Type = new[] { "temperature" },
                Platform = "hubitat",
                DeviceId = "Test multisensor",
            };
            var expectedConfig = @"
# Test device temperature, from hubitat via MQTT
- platform: mqtt
  name: Test device temperature
  retain: true
  device_class: temperature
  state_topic: hubitat/Test multisensor/temperature
  unit_of_measurement: �C
  force_update: true
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["sensor"].Count, "Only one entity returned");

            var config = result["sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Threshold

        [Test]
        public void Missing_threshold_attribute_throws()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test meter",
                Type = new[] { "threshold" },
                Platform = "hubitat",
                OnCondition = "> 5",
            };

            // Action
            Assert.Throws<MissingParameterException>(() => transformer.TransformConfig(definition));
        }

        [Test]
        public void Missing_threshold_condition_throws()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test meter",
                Type = new[] { "power-threshold" },
                Platform = "hubitat",
            };

            // Action
            Assert.Throws<MissingParameterException>(() => transformer.TransformConfig(definition));
        }

        [Test]
        public void Threshold_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new MqttSensorConfigTransformer(new DummyConfiguration());
            var definition = new SensorDefinition
            {
                Name = "Test meter",
                Type = new[] { "power-threshold" },
                Platform = "hubitat",
                OnCondition = "< 5"
            };
            var expectedConfig = @"
# Test meter, from hubitat via MQTT
- platform: mqtt
  name: Test meter
  retain: true
  state_topic: hubitat/Test meter/power
  value_template: '{%if (value | float) < 5-%}on{%-else-%}off{%-endif%}'
  payload_on: 'on'
  payload_off: 'off'
".Trim();

            // Action
            var result = transformer.TransformConfig(definition);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion
    }
}