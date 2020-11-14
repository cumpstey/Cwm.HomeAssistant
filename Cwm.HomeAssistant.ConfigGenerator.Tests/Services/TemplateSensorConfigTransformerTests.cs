using Cwm.HomeAssistant.Config.Exceptions;
using Cwm.HomeAssistant.Config.Initializtion;
using Cwm.HomeAssistant.Config.Models;
using Cwm.HomeAssistant.Config.Services;
using NUnit.Framework;
using System.Linq;

namespace Cwm.HomeAssistant.ConfigTransformer.Services
{
    public class TemplateSensorConfigTransformerTests
    {
        #region Low battery alert

        [Test]
        public void Low_battery_alert_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new TemplateSensorConfigTransformer(new DummyConfiguration { LowBatteryAlertThreshold = 14 }, new DeviceTranslator());
            var definitions = new [] {
                new DeviceDefinition
                {
                    DeviceId = "Test device 1",
                    Platform = "hubitat",
                    Sensors = new[] {
                        new SensorDefinition { Type = "battery" }
                    },
                },
                new DeviceDefinition
                {
                    DeviceId = "Test device 2",
                    Platform = "hubitat",
                    Sensors = new[] {
                        new SensorDefinition { Type = "battery" },
                        new SensorDefinition { Type = "motion" },
                    },
                },
                new DeviceDefinition
                {
                    DeviceId = "Test device 3",
                    Platform = "hubitat",
                    Sensors = new[] {
                        new SensorDefinition { Type = "motion" }
                    },
                },
            };
            var expectedConfig = @"
# Low battery alert
- platform: template
  sensors:
    low_battery_alert:
      friendly_name: Low battery alert
      value_template: >
        {% set battery_threshold = 14 %}
        {{ states('sensor.test_device_1_battery') | float <= battery_threshold
        or states('sensor.test_device_2_battery') | float <= battery_threshold
        }}
      attribute_templates:
        devicesHtml: >
          {% set battery_threshold = 50 %}
          <ul>
          {% if states('sensor.test_device_1_battery') | float <= battery_threshold %}<li>{{states.sensor.test_device_1_battery.attributes.friendly_name}}</li>{% endif %}
          {% if states('sensor.test_device_2_battery') | float <= battery_threshold %}<li>{{states.sensor.test_device_2_battery.attributes.friendly_name}}</li>{% endif %}
          <ul>
".Trim();

            // Action
            var result = transformer.GetLowBatteryAlertSensor(definitions);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Device offline alert

        [Test]
        public void Device_offline_alert_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new TemplateSensorConfigTransformer(new DummyConfiguration { }, new DeviceTranslator());
            var definitions = new[] {
                new DeviceDefinition
                {
                    DeviceId = "Test device 1",
                    Platform = "hubitat",
                    Sensors = new[] {
                        new SensorDefinition { Type = "connectivity" }
                    },
                },
                new DeviceDefinition
                {
                    DeviceId = "Test device 2",
                    Platform = "hubitat",
                    Sensors = new[] {
                        new SensorDefinition { Type = "connectivity" },
                        new SensorDefinition { Type = "motion" },
                    },
                },
                new DeviceDefinition
                {
                    DeviceId = "Test device 3",
                    Platform = "hubitat",
                    Sensors = new[] {
                        new SensorDefinition { Type = "motion" }
                    },
                },
            };
            var expectedConfig = @"
# Device offline alert
- platform: template
  sensors:
    device_offline_alert:
      friendly_name: Device offline alert
      value_template: >
        {{ is_state('binary_sensor.test_device_1_connectivity', 'off')
        or is_state('binary_sensor.test_device_2_connectivity', 'off')
        }}
      attribute_templates:
        devicesHtml: >
          <ul>
          {% if is_state('binary_sensor.test_device_1_connectivity', 'off') %}<li>{{states.binary_sensor.test_device_1_connectivity.attributes.friendly_name}}</li>{% endif %}
          {% if is_state('binary_sensor.test_device_2_connectivity', 'off') %}<li>{{states.binary_sensor.test_device_2_connectivity.attributes.friendly_name}}</li>{% endif %}
          <ul>
".Trim();

            // Action
            var result = transformer.GetDeviceOfflineAlertSensor(definitions);

            // Assert
            Assert.AreEqual(1, result.Keys.Count, "One entity type returned");
            Assert.AreEqual("binary_sensor", result.Keys.First(), "The type of the entity returned is correct");
            Assert.AreEqual(1, result["binary_sensor"].Count, "Only one entity returned");

            var config = result["binary_sensor"].First();
            Assert.AreEqual(expectedConfig, config.Entity, "Config declared as expected");
            Assert.IsEmpty(config.Customization, "Customization declared as expected");
        }

        #endregion

        #region Button activity

        [Test]
        public void Button_activity_sensor_config_is_generated()
        {
            // Arrange
            var transformer = new TemplateSensorConfigTransformer(new DummyConfiguration { LowBatteryAlertThreshold = 14 }, new DeviceTranslator());
            var definition = new DeviceDefinition
            {
                DeviceId = "My button",
                Platform = "hubitat",
                Sensors = new[] {
                    new SensorDefinition { Type = "button" },
                    new SensorDefinition { Type = "hold-button" },
                },
            };
            var expectedConfig = @"
# My button activity
- platform: template
  sensors:
    my_button_active:
      friendly_name: My button
      value_template: >
        {{ is_state('binary_sensor.my_button', 'on')
        or is_state('binary_sensor.my_button_hold', 'on')
        }}
".Trim();

            // Action
            var result = transformer.GetButtonActivitySensor(definition);

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