using Cwm.HomeAssistant.Config.Exceptions;
using Cwm.HomeAssistant.Config.Initializtion;
using Cwm.HomeAssistant.Config.Models;
using Cwm.HomeAssistant.Config.Services;
using NUnit.Framework;
using System.Linq;

namespace Cwm.HomeAssistant.ConfigTransformer.Services
{
    public class LovelaceConfigTransformerTests
    {
        #region Sensor entity list

        [Test]
        public void Sensor_entity_list_config_is_generated()
        {
            // Arrange
            var transformer = new LovelaceConfigTransformer();
            var definitions = new[] {
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

            // Action
            var result = transformer.GenerateSensorEntityList("battery", definitions);

            // Assert
            Assert.AreEqual(2, result.Count(), "Expected number of entities");
            Assert.AreEqual("sensor.test_device_1_battery", result.ElementAt(0).Entity, "Expected entity listed first");
            Assert.AreEqual("Test device 1", result.ElementAt(0).Name, "Expected entity name");
            Assert.AreEqual("sensor.test_device_2_battery", result.ElementAt(1).Entity, "Expected entity listed second");
        }

        #endregion
    }
}
