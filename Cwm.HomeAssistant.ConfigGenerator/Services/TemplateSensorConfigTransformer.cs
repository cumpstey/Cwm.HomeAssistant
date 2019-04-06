using Cwm.HomeAssistant.Config.Exceptions;
using Cwm.HomeAssistant.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cwm.HomeAssistant.Config.Services
{
    /// <summary>
    /// Class providing functionality to generate Home Assistant configuration for
    /// sensor devices.
    /// </summary>
    public class TemplateSensorConfigTransformer : ConfigTransformer
    {
        #region Fields

        private readonly ITemplateSensorConfigTransformerConfiguration _configuration;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateSensorConfigTransformer"/> class.
        /// </summary>
        /// <param name="configuration">Required configuration</param>
        public TemplateSensorConfigTransformer(ITemplateSensorConfigTransformerConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generates a low battery alert template sensor using all the
        /// battery sensors in th list of devices.
        /// </summary>
        /// <param name="definitions">List of device definitions</param>
        /// <returns>A collection of config entries with a single item</returns>
        public KeyedCollection<ConfigEntry> GetLowBatteryAlertSensor(IEnumerable<DeviceDefinition> definitions)
        {
            var batteryDevices = definitions.Where(d => d.Sensors != null && d.Sensors.Any(s => s.Type == SensorType.Battery));

            var invalid = batteryDevices.Where(definition => string.IsNullOrWhiteSpace(definition.DeviceId));
            if (invalid.Any())
            {
                throw new ValidationException($"{invalid.Count()} definitions missing a device id.");
            }

            var customization = new List<string>();

            var lines = batteryDevices.Select(i => $"states('{GetSensorEntityId(SensorType.Battery, i)}') | float < battery_threshold");
            var entity = $@"
# Low battery alert
- platform: template
  sensors:
    low_battery_alert:
      friendly_name: Low battery alert
      value_template: >
        {{% set battery_threshold = {_configuration.LowBatteryAlertThreshold} %}}
        {{{{ {string.Join($"{Environment.NewLine}        or ", lines)}
        }}}}
".Trim();

            return new KeyedCollection<ConfigEntry>() {
                { EntityType.BinarySensor, new ConfigEntry
                    {
                        Entity = string.Join(Environment.NewLine, entity),
                        Customization = string.Join(Environment.NewLine, customization),
                    } }
            };
        }

        #endregion
    }
}
