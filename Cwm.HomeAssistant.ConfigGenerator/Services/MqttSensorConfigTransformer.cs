using Cwm.HomeAssistant.Config.Exceptions;
using Cwm.HomeAssistant.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
                if (type.EndsWith("button"))
                {
                    var config = ProcessButtonDefinition(type, definition);
                    foreach (var item in config)
                    {
                        configs.Add(EntityType.BinarySensor, item);
                    }
                }
                else if (type.EndsWith(SensorType.Threshold))
                {
                    var attribute = Regex.Replace(type, $"-?{SensorType.Threshold}$", string.Empty);

                    if (string.IsNullOrWhiteSpace(attribute))
                    {
                        throw new MissingParameterException("Threshold attribute is missing");
                    }

                    if (string.IsNullOrWhiteSpace(definition.OnCondition))
                    {
                        throw new MissingParameterException("Threshold on condition is missing");
                    }

                    var config = FormatSensorDefinition(EntityType.BinarySensor, new SensorConfig
                    {
                        Type = SensorType.Threshold,
                        Name = definition.Name,
                        Platform = definition.Platform,
                        DeviceId = definition.DeviceId,
                        DeviceClass = definition.DeviceClasses?.GetValueOrDefault(type),
                        Icon = definition.Icons?.GetValueOrDefault(type),
                        ToDo = definition.ToDo,
                        ThresholdAttribute = attribute,
                        ThresholdOnCondition = definition.OnCondition,
                    });
                    configs.Add(EntityType.BinarySensor, config);
                }
                else
                {
                    // Figure out whether it should be a binary sensor
                    var entityType = new[] { SensorType.Button, SensorType.Contact, SensorType.Motion, SensorType.Presence }.Contains(type)
                        ? EntityType.BinarySensor : EntityType.Sensor;

                    // Generate a reasonably human-friendly name, depending on the type of sensor.
                    var name = type == SensorType.Battery
                        ? $"{definition.DeviceId} battery"
                        : new[] { SensorType.Button, SensorType.Contact, SensorType.Presence }.Contains(type)
                            ? definition.Name
                            : $"{definition.Name} {type}";

                    var config = FormatSensorDefinition(entityType, new SensorConfig
                    {
                        Type = type,
                        Name = name,
                        Platform = definition.Platform,
                        DeviceId = definition.DeviceId,
                        DeviceClass = definition.DeviceClasses?.GetValueOrDefault(type),
                        Icon = definition.Icons?.GetValueOrDefault(type),
                        ToDo = definition.ToDo,
                    });
                    configs.Add(entityType, config);
                }
            }

            return configs;
        }

        #endregion

        #region Helpers

        private ConfigEntry FormatDefinition(Action<List<string>> addConfig, string entityType, SensorConfig sensor)
        {
            // TODO: Accept Genius sensors
            if (sensor.Platform != Platform.Hubitat && sensor.Platform != Platform.SmartThings)
            {
                throw new UnsupportedPlatformException(sensor.Platform, $"sensor:{sensor.Type}");
            }

            var entity = new List<string>();
            var customization = new List<string>();

            entity.Add($"# {sensor.Name}, from {sensor.Platform} via MQTT");

            if (sensor.ToDo != null)
            {
                entity.Add($"# TODO: {sensor.ToDo}");
            }

            entity.Add("- platform: mqtt");
            entity.Add($"  name: {sensor.Name}");
            entity.Add("  retain: true");

            if (sensor.Icon != null)
            {
                customization.Add($"  icon: {sensor.Icon}");
            }

            addConfig(entity);

            if (customization.Any())
            {
                customization.Insert(0, $@"""{entityType}.{FormatAsId(sensor.Name)}"":");
            }

            return new ConfigEntry
            {
                Entity = string.Join(Environment.NewLine, entity),
                Customization = string.Join(Environment.NewLine, customization),
            };
        }

        private IReadOnlyCollection<ConfigEntry> ProcessButtonDefinition(string type, SensorDefinition definition)
        {
            if (type == "hold-button")
            {
                void addConfig(List<string> entity)
                {
                    var prefix = _configuration.GetPlatformPrefix(definition.Platform);
                    entity.Add($"  state_topic: {prefix}/{definition.DeviceId}/held");
                    entity.Add("  payload_on: 1");
                    entity.Add("  off_delay: 1");
                }

                return new[] {
                    FormatDefinition(addConfig, EntityType.BinarySensor, new SensorConfig
                    {
                        Type = type,
                        Name = $"{definition.Name} (hold)",
                        Platform = definition.Platform,
                        DeviceId = definition.DeviceId,
                        DeviceClass = definition.DeviceClasses?.GetValueOrDefault(type),
                        Icon = definition.Icons?.GetValueOrDefault(type),
                        ToDo = definition.ToDo,
                    })
                };
            }
            else if (type == "hold-release-button")
            {
                // For a hold/release button, until I find a better way I'm using a contact sensor.
                void addConfig(List<string> entity)
                {
                    var prefix = _configuration.GetPlatformPrefix(definition.Platform);
                    entity.Add($"  state_topic: {prefix}/{definition.DeviceId}/contact");
                    entity.Add("  payload_on: closed");
                    entity.Add("  payload_off: open");
                }

                return new[] {
                    FormatDefinition(addConfig, EntityType.BinarySensor, new SensorConfig {
                        Type = type,
                        Name = $"{definition.Name} (hold)",
                        Platform = definition.Platform,
                        DeviceId = definition.DeviceId,
                        DeviceClass = definition.DeviceClasses?.GetValueOrDefault(type),
                        Icon = definition.Icons?.GetValueOrDefault(type),
                        ToDo = definition.ToDo,
                    })
                };
            }
            else if (type == "button" || Regex.IsMatch(type, @"\d+-button"))
            {
                var count = type == "button" ? 1 : int.Parse(type.Split('-').First());
                var configs = new List<ConfigEntry>();
                for (var i = 1; i <= count; i++)
                {
                    void addConfig(List<string> entity)
                    {
                        var prefix = _configuration.GetPlatformPrefix(definition.Platform);
                        entity.Add($"  state_topic: {prefix}/{definition.DeviceId}/pushed");
                        entity.Add($"  payload_on: {i}");
                        entity.Add("  off_delay: 1");
                    }

                    var name = count == 1 ? definition.Name : $"{definition.Name} {i}";
                    configs.Add(FormatDefinition(addConfig, EntityType.BinarySensor, new SensorConfig
                    {
                        Type = type,
                        Name = name,
                        Platform = definition.Platform,
                        DeviceId = definition.DeviceId,
                        DeviceClass = definition.DeviceClasses?.GetValueOrDefault(type),
                        Icon = definition.Icons?.GetValueOrDefault(type),
                        ToDo = definition.ToDo,
                    }));
                }

                return configs;
            }

            throw new UnrecognizedTypeException(type);
        }

        private ConfigEntry FormatSensorDefinition(string entityType, SensorConfig sensor)
        {
            Action<List<string>> addConfig = (entity) =>
            {
                var prefix = _configuration.GetPlatformPrefix(sensor.Platform);
                switch (sensor.Type)
                {
                    case SensorType.Battery:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "battery"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/battery");
                        entity.Add(@"  unit_of_measurement: '%'");
                        break;
                    case SensorType.Contact:
                        if (sensor.DeviceClass != null)
                        {
                            entity.Add($"  device_class: {sensor.DeviceClass}");
                        }

                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/contact");
                        entity.Add("  payload_on: open");
                        entity.Add("  payload_off: closed");
                        break;
                    case SensorType.Illuminance:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "illuminance"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/illuminance");
                        entity.Add("  unit_of_measurement: lux");
                        entity.Add("  force_update: true");
                        break;
                    case SensorType.Motion:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "motion"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/motion");
                        entity.Add("  payload_on: active");
                        entity.Add("  payload_off: inactive");
                        break;
                    case SensorType.Presence:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "presence"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/presence");
                        entity.Add("  payload_on: present");
                        entity.Add("  payload_off: not present");
                        break;
                    case SensorType.Temperature:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "temperature"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/temperature");
                        entity.Add("  unit_of_measurement: °C");
                        entity.Add("  force_update: true");
                        break;
                    case SensorType.Threshold:
                        if (sensor.DeviceClass != null)
                        {
                            entity.Add($"  device_class: {sensor.DeviceClass}");
                        }

                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/{sensor.ThresholdAttribute}");
                        entity.Add($"  value_template: '{{%if (value | float) {sensor.ThresholdOnCondition}-%}}on{{%-else-%}}off{{%-endif%}}'");
                        entity.Add($"  payload_on: 'on'");
                        entity.Add($"  payload_off: 'off'");
                        break;
                    default:
                        throw new UnrecognizedTypeException(sensor.Type);
                }
            };

            return FormatDefinition(addConfig, entityType, sensor);
        }

        #endregion
    }
}
