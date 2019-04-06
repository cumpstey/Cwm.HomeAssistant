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
    public class LovelaceConfigTransformer : ConfigTransformer
    {
        #region Methods

        /// <summary>
        /// Generates a low battery alert template sensor using all the
        /// battery sensors in th list of devices.
        /// </summary>
        /// <param name="definitions">List of device definitions</param>
        /// <returns>A collection of config entries with a single item</returns>
        public IReadOnlyCollection<LovelaceEntity> GenerateSensorEntityList(string sensorType, IEnumerable<DeviceDefinition> definitions)
        {
            var devices = definitions.Where(d => d.Sensors != null && d.Sensors.Any(s => s.Type == sensorType))
                                     .OrderBy(d => d.Name);

            var invalid = devices.Where(definition => string.IsNullOrWhiteSpace(definition.DeviceId));
            if (invalid.Any())
            {
                throw new ValidationException($"{invalid.Count()} definitions missing a device id.");
            }

            var entities = devices.Select(i => new LovelaceEntity
            {
                Entity = GetSensorEntityId(sensorType, i),
                Name = i.Name,
            }).ToArray();

            return  entities;
        }

        public string _GenerateSensorEntityList(string sensorType, IEnumerable<DeviceDefinition> definitions)
        {
            var devices = definitions.Where(d => d.Sensors != null && d.Sensors.Any(s => s.Type == sensorType))
                                     .OrderBy(d => d.Name);

            var invalid = devices.Where(definition => string.IsNullOrWhiteSpace(definition.DeviceId));
            if (invalid.Any())
            {
                throw new ValidationException($"{invalid.Count()} definitions missing a device id.");
            }

            var entities = devices.Select(i => $@"
- entity: {GetSensorEntityId(sensorType, i)}
  name: {i.Name}
".Trim());

            return string.Join(Environment.NewLine, entities);
        }

        #endregion
    }
}
