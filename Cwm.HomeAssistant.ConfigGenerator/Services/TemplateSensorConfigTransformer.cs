﻿using Cwm.HomeAssistant.Config.Exceptions;
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

        private readonly DeviceTranslator _deviceTranslator;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateSensorConfigTransformer"/> class.
        /// </summary>
        /// <param name="configuration">Required configuration</param>
        public TemplateSensorConfigTransformer(ITemplateSensorConfigTransformerConfiguration configuration,
                                               DeviceTranslator deviceTranslator)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _deviceTranslator = deviceTranslator ?? throw new ArgumentNullException(nameof(deviceTranslator));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generates a low battery alert template sensor using all the
        /// battery sensors in th list of devices.
        /// </summary>
        /// <param name="devices">List of device definitions</param>
        /// <returns>A collection of config entries with a single item</returns>
        public KeyedCollection<ConfigEntry> GetLowBatteryAlertSensor(IEnumerable<DeviceDefinition> devices)
        {
            var batteryDevices = devices.Where(d => d.Sensors != null && d.Sensors.Any(s => s.Type == SensorType.Battery));

            var invalid = batteryDevices.Where(d => string.IsNullOrWhiteSpace(d.DeviceId));
            if (invalid.Any())
            {
                throw new ValidationException($"{invalid.Count()} definitions missing a device id.");
            }

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
                { EntityType.BinarySensor, new ConfigEntry { Entity = entity, Customization = string.Empty } }
            };
        }

        public KeyedCollection<ConfigEntry> GetButtonActivitySensor(DeviceDefinition device)
        {
            var buttons = _deviceTranslator.TranslateButtonDefinition(device);
            if (!buttons.Any()) return new KeyedCollection<ConfigEntry>();


            var lines = buttons.Select(i => $"is_state('{GetButtonEntityId(i.Item1,i.Item2,device)}', 'on')");
            var entity = $@"
# {device.Name} activity
- platform: template
  sensors:
    {GetButtonActivitySensorId(device)}:
      friendly_name: {device.Name}
      value_template: >
        {{{{ {string.Join($"{Environment.NewLine}        or ", lines)}
        }}}}
".Trim();

            return new KeyedCollection<ConfigEntry>() {
                { EntityType.BinarySensor, new ConfigEntry { Entity = entity, Customization = string.Empty } }
            };
        }
        #endregion
    }
}
