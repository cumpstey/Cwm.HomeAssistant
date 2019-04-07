using Cwm.HomeAssistant.Config.Models;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Cwm.HomeAssistant.Config.Services
{
    /// <summary>
    /// Base class for service classes providing functionality to generate configuration files.
    /// </summary>
    public abstract class ConfigTransformer
    {
        #region Helpers

        /// <summary>
        /// Format an entity's name as an id.
        /// This is, roughly, lower case with underscores, but I've not found the exact rules
        /// so it may require tweaking.
        /// </summary>
        /// <param name="name">Name of the entity</param>
        /// <returns>Id which and entity of the given name will have</returns>
        protected string FormatAsId(string name)
        {
            return Regex.Replace(new string(Array.FindAll(name.ToCharArray(), c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '_'))
                    .ToLower()
                    .Replace(" ", "_")
                    .TrimStart('_'),
                "_+",
                "_");
        }

        /// <summary>
        /// Generates a reasonably human-friendly name, depending on the
        /// type of sensor.
        /// </summary>
        /// <param name="sensorType">Sensor type</param>
        /// <param name="definition">Device definition</param>
        /// <returns>The name of the entity to be used in Home Assistant</returns>
        protected string GetSensorName(string sensorType, DeviceDefinition definition)
        {
            var name = sensorType == SensorType.Battery
                        ? $"{definition.DeviceId} battery"
                    : sensorType == SensorType.PowerCycle
                        ? $"{definition.Name} cycle"
                    : new[] { SensorType.Button, SensorType.Contact, SensorType.Moisture, SensorType.Presence }.Contains(sensorType)
                        ? definition.Name
                    : $"{definition.Name} {sensorType}";
            return name;
        }

        /// <summary>
        /// Generates the Home Assistant entity id for the sensor.
        /// </summary>
        /// <param name="sensorType">Sensor type</param>
        /// <param name="definition">Device definition</param>
        /// <returns>The id of the entity as used in Home Assistant</returns>
        protected string GetSensorEntityId(string sensorType, DeviceDefinition definition) {
            var name = GetSensorName(sensorType, definition);
            var id = $"{GetSensorEntityType(sensorType)}.{FormatAsId(name)}";
            return id;
        }

        protected string GetButtonEntityId(int? buttonNumber, ButtonType buttonType, DeviceDefinition definition)
        {
            var name = GetSensorName(SensorType.Button, definition);
            var id = $"{GetSensorEntityType(SensorType.Button)}.{FormatAsId(name)}";

            if (buttonNumber.HasValue)
            {
                id += $"_{buttonNumber}";
            }

            if (buttonType == ButtonType.Hold || buttonType == ButtonType.HoldAndRelease)
            {
                id += "_hold";
            }

            return id;
        }

        protected string GetButtonActivitySensorId(DeviceDefinition definition)
        {
            var name = GetSensorName(SensorType.Button, definition);
            var id = FormatAsId($"{name} active");
            return id;
        }

        protected string GetButtonActivityEntityId(DeviceDefinition definition)
        {
            var sensorId = GetButtonActivitySensorId(definition);
            var id = $"{EntityType.BinarySensor}.{sensorId}";
            return id;
        }

        /// <summary>
        /// Returns the entity type for a given sensor type.
        /// </summary>
        /// <param name="sensorType">Sensor type</param>
        /// <returns>Entity type, either <see cref="EntityType.BinarySensor"/> or <see cref="EntityType.Sensor"/></returns>
        protected string GetSensorEntityType(string sensorType)
        {
            switch (sensorType)
            {
                case SensorType.Button:
                case SensorType.Contact:
                case SensorType.Moisture:
                case SensorType.Motion:
                case SensorType.Presence:
                    return EntityType.BinarySensor;
                default:
                    return EntityType.Sensor;
            }
        }

        #endregion
    }
}
