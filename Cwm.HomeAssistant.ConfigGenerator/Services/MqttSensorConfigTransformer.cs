using Cwm.HomeAssistant.Config.Exceptions;
using Cwm.HomeAssistant.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cwm.HomeAssistant.Config.Services
{
    public class MqttSensorConfigTransformer : ConfigTransformer
    {
        #region Fields

        private readonly IMqttConfigGeneratorConfiguration _configuration;

        #endregion

        #region Constructor

        public MqttSensorConfigTransformer(IMqttConfigGeneratorConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region Methods

        public KeyedCollection<ConfigEntry> TransformConfig(SensorDefinition definition)
        {
            var configs = new KeyedCollection<ConfigEntry>();
            foreach (var type in definition.Type)
            {
                // Figure out whether it should be a binary sensor
                var entityType = new[] { SensorType.Button, SensorType.Contact, SensorType.Motion, SensorType.Presence }.Contains(type)
                    ? "binary_sensor" : "sensor";

                // Generate a reasonably human-friendly name, depending on the type of sensor.
                var name = type == SensorType.Battery
                    ? $"{definition.DeviceId} battery"
                    : new[] { SensorType.Button, SensorType.Contact, SensorType.Presence }.Contains(type)
                        ? definition.Name
                        : $"{definition.Name} {type}";

                var config = FormatSensorDefinition(entityType, type, name, definition.Platform, definition.DeviceId, definition.DeviceClass, definition.Icons?.GetValueOrDefault(type), definition.ToDo);
                configs.Add(entityType, config);
            }

            return configs;
        }

        #endregion

        #region Helpers

        private ConfigEntry FormatSensorDefinition(string entityType, string sensorType, string name, string platform, string deviceId, string deviceClass, string icon, string toDo)
        {
            // TODO: Accept Genius sensors
            if (platform != Platform.Hubitat && platform != Platform.SmartThings)
            {
                throw new UnsupportedPlatformException(platform, $"sensor:{sensorType}");
            }

            var entity = new List<string>();
            var customization = new List<string>();

            entity.Add($"# {name}, from {platform} via MQTT");

            if (toDo != null)
            {
                entity.Add($"# TODO: {toDo}");
            }

            entity.Add("- platform: mqtt");
            entity.Add($"  name: {name}");
            entity.Add("  retain: true");

            if (icon != null)
            {
                customization.Add($"  icon: {icon}");
            }

            var prefix = _configuration.GetPlatformPrefix(platform);
            switch (sensorType)
            {
                case SensorType.Battery:
                    entity.Add("  device_class: battery");
                    entity.Add($"  state_topic: {prefix}/{deviceId}/battery");
                    entity.Add(@"  unit_of_measurement: '%'");
                    break;
                case SensorType.Button:
                    entity.Add($"  state_topic: {prefix}/{deviceId}/pushed");
                    entity.Add("  payload_on: 1");
                    entity.Add("  off_delay: 1");
                    break;
                case SensorType.Contact:
                    if (deviceClass != null)
                    {
                        entity.Add($"  device_class: {deviceClass}");
                    }

                    entity.Add($"  state_topic: {prefix}/{deviceId}/contact");
                    entity.Add("  payload_on: open");
                    entity.Add("  payload_off: closed");
                    break;
                case SensorType.Motion:
                    entity.Add("  device_class: motion");
                    entity.Add($"  state_topic: {prefix}/{deviceId}/motion");
                    entity.Add("  payload_on: active");
                    entity.Add("  payload_off: inactive");
                    break;
                case SensorType.Presence:
                    entity.Add("  device_class: presence");
                    entity.Add($"  state_topic: {prefix}/{deviceId}/presence");
                    entity.Add("  payload_on: present");
                    entity.Add("  payload_off: not present");
                    break;
                case SensorType.Temperature:
                    entity.Add("  device_class: temperature");
                    entity.Add($"  state_topic: {prefix}/{deviceId}/temperature");
                    entity.Add("  unit_of_measurement: °C");
                    entity.Add("  force_update: true");
                    break;
            }

            if (customization.Any())
            {
                customization.Insert(0, $@"""{entityType}.{FormatAsId(name)}"":");
            }

            return new ConfigEntry
            {
                Entity = string.Join(Environment.NewLine, entity),
                Customization = string.Join(Environment.NewLine, customization),
            };
        }

        #endregion
    }
}
