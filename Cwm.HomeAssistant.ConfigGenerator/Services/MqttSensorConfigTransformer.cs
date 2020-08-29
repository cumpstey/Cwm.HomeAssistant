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
    public class MqttSensorConfigTransformer : ConfigTransformer
    {
        #region Fields

        private readonly IMqttConfigGeneratorConfiguration _configuration;

        private readonly DeviceTranslator _deviceTranslator;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttSensorConfigTransformer"/> class.
        /// </summary>
        /// <param name="configuration">Required configuration</param>
        public MqttSensorConfigTransformer(IMqttConfigGeneratorConfiguration configuration, DeviceTranslator deviceTranslator)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _deviceTranslator = deviceTranslator ?? throw new ArgumentNullException(nameof(deviceTranslator));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Generate Home Assistant configuration file entries for the sensors in the provided device.
        /// </summary>
        /// <param name="definition">Device properties</param>
        /// <returns>Configuration file entries covering all sensors defined in the device</returns>
        public KeyedCollection<ConfigEntry> TransformConfig(DeviceDefinition definition)
        {
            if (definition.Sensors == null)
            {
                return new KeyedCollection<ConfigEntry>();
            }

            if (string.IsNullOrWhiteSpace(definition.DeviceId))
            {
                throw new ValidationException($"{nameof(definition.DeviceId)} requires a value.");
            }

            var configs = new KeyedCollection<ConfigEntry>();
            foreach (var sensor in definition.Sensors)
            {
                if (sensor.Type.EndsWith("button"))
                {
                    // Buttons processed separately below.
                }
                else if (sensor.Type.EndsWith(SensorType.Threshold))
                {
                    var attribute = Regex.Replace(sensor.Type, $"-?{SensorType.Threshold}$", string.Empty);

                    if (string.IsNullOrWhiteSpace(attribute))
                    {
                        throw new ValidationException("Threshold attribute is missing");
                    }

                    if (string.IsNullOrWhiteSpace(sensor.OnCondition))
                    {
                        throw new ValidationException("Threshold on condition is missing");
                    }

                    var config = FormatSensorDefinition(EntityType.BinarySensor, new SensorConfig
                    {
                        Type = SensorType.Threshold,
                        Name = definition.Name,
                        Platform = definition.Platform,
                        DeviceId = definition.DeviceId,
                        DeviceName = definition.Name,
                        DeviceClass = sensor.DeviceClass,
                        Icon = sensor.Icon,
                        ThresholdAttribute = attribute,
                        ThresholdOnCondition = sensor.OnCondition,
                        Customize = sensor.Customize,
                    });
                    configs.Add(EntityType.BinarySensor, config);
                }
                else if (sensor.Type == SensorType.PowerCycleOnOff)
                {
                    if (!definition.Sensors.Any(i => i.Type == SensorType.PowerCycle))
                    {
                        throw new ValidationException("Can't have a power cycle on-off sensor without a power cycle sensor.");
                    }

                    var config = FormatTemplateDefinition(EntityType.BinarySensor, new TemplateSensorConfig
                    {
                        Name = definition.Name,
                        Icon = sensor.Icon,
                        ValueTemplate = $"states.{GetSensorEntityId(SensorType.PowerCycle, definition)}.state not in ['unknown','off']",
                    });
                    configs.Add(EntityType.BinarySensor, config);
                }
                else
                {
                    // Figure out whether it should be a binary sensor
                    var entityType = GetSensorEntityType(sensor.Type);

                    // Generate a reasonably human-friendly name, depending on the type of sensor.
                    var name = GetSensorName(sensor.Type, definition);

                    // Identify sensors which are set by Home Assistant
                    var platform = new[] { SensorType.PowerCycle }.Contains(sensor.Type)
                        ? Platform.HomeAssistant
                        : definition.Platform;

                    var config = FormatSensorDefinition(entityType, new SensorConfig
                    {
                        Type = sensor.Type,
                        Name = name,
                        Platform = platform,
                        DeviceId = definition.DeviceId,
                        DeviceName = definition.Name,
                        DeviceClass = sensor.DeviceClass,
                        Icon = sensor.Icon,
                        Customize = sensor.Customize,
                    });
                    configs.Add(entityType, config);
                }
            }

            var buttonConfigs = ProcessButtonDefinition(definition);
            configs.AddMany(EntityType.BinarySensor, buttonConfigs);

            return configs;
        }

        #endregion

        #region Helpers

        private ConfigEntry FormatDefinition(Action<List<string>> addConfig, string entityType, SensorConfig sensor)
        {
            // TODO: Accept Genius sensors
            if (!new[] { Platform.HomeAssistant, Platform.Hubitat, Platform.SmartThings }.Contains(sensor.Platform))
            {
                throw new UnsupportedPlatformException(sensor.Platform, $"sensor:{sensor.Type}");
            }

            var entity = new List<string>();
            var customization = new List<string>();

            entity.Add($"# {sensor.Name}, from {sensor.Platform} via MQTT");
            entity.Add("- platform: mqtt");

            if (sensor.Name.Contains("'"))
            {
                entity.Add($"  name: {sensor.Name.Replace("'", string.Empty)}");
                customization.Add($"  friendly_name: {sensor.Name}");
            }
            else
            {
                entity.Add($"  name: {sensor.Name}");
            }

            if (sensor.Icon != null)
            {
                customization.Add($"  icon: {sensor.Icon}");
            }

            if (sensor.Customize != null)
            {
                foreach (var key in sensor.Customize.Keys)
                {
                    customization.Add($"  {key}: {sensor.Customize[key]}");
                }
            }

            addConfig(entity);

            if (customization.Any())
            {
                customization.Insert(0, $@"{entityType}.{FormatAsId(sensor.Name)}:");
            }

            return new ConfigEntry
            {
                Entity = string.Join(Environment.NewLine, entity),
                Customization = string.Join(Environment.NewLine, customization),
            };
        }

        private ConfigEntry FormatTemplateDefinition(string entityType, TemplateSensorConfig sensor)
        {
            var entity = new List<string>();
            var customization = new List<string>();

            entity.Add($"# {sensor.Name}");
            entity.Add("- platform: template");
            entity.Add("  sensors:");
            entity.Add($"    {FormatAsId(sensor.Name)}:");
            entity.Add($"      value_template: >");
            entity.Add($"        {{{{{sensor.ValueTemplate}}}}}");

            //entity.Add($"      friendly_name:{FormatAsId(sensor.Name)}:");

            customization.Add($"  friendly_name: {sensor.Name}");

            if (sensor.Icon != null)
            {
                customization.Add($"  icon: {sensor.Icon}");
            }

            if (customization.Any())
            {
                customization.Insert(0, $@"{entityType}.{FormatAsId(sensor.Name)}:");
            }

            return new ConfigEntry
            {
                Entity = string.Join(Environment.NewLine, entity),
                Customization = string.Join(Environment.NewLine, customization),
            };
        }

        private IReadOnlyCollection<ConfigEntry> ProcessButtonDefinition(DeviceDefinition definition)
        {
            var configs = new List<ConfigEntry>();

            var buttons = _deviceTranslator.TranslateButtonDefinition(definition);
            if (!buttons.Any())
            {
                return configs;
            }

            foreach (var button in buttons)
            {
                var buttonNumber = button.Item1 ?? 1;
                if (button.Item2 == ButtonType.Push)
                {
                    void addConfig(List<string> entity)
                    {
                        var prefix = _configuration.GetPlatformPrefix(definition.Platform);
                        entity.Add($"  state_topic: {prefix}/{definition.DeviceId}/{buttonNumber}/push");
                        entity.Add($"  payload_on: pushed");
                        entity.Add("  off_delay: 1");
                    }

                    var name = button.Item1.HasValue ? $"{definition.Name} {button.Item1}" : definition.Name;
                    configs.Add(FormatDefinition(addConfig, EntityType.BinarySensor, new SensorConfig
                    {
                        Name = name,
                        Platform = definition.Platform,
                        DeviceId = definition.DeviceId,
                        DeviceName = definition.Name,
                        //DeviceClass = sensor.DeviceClass,
                        //Icon = sensor.Icon,
                    }));
                }
                else if (button.Item2 == ButtonType.Hold)
                {
                    void addConfig(List<string> entity)
                    {
                        var prefix = _configuration.GetPlatformPrefix(definition.Platform);
                        entity.Add($"  state_topic: {prefix}/{definition.DeviceId}/{buttonNumber}/hold");
                        entity.Add("  payload_on: held");
                        entity.Add("  off_delay: 1");
                    }

                    var name = button.Item1.HasValue ? $"{definition.Name} {button.Item1} hold" : $"{definition.Name} hold";
                    configs.Add(FormatDefinition(addConfig, EntityType.BinarySensor, new SensorConfig
                    {
                        Name = name,
                        Platform = definition.Platform,
                        DeviceId = definition.DeviceId,
                        DeviceName = definition.Name,
                    }));
                }
                else if (button.Item2 == ButtonType.HoldAndRelease)
                {
                    void addConfig(List<string> entity)
                    {
                        var prefix = _configuration.GetPlatformPrefix(definition.Platform);
                        entity.Add($"  state_topic: {prefix}/{definition.DeviceId}/{buttonNumber}/hold");
                        entity.Add("  payload_on: held");
                        entity.Add("  payload_off: released");
                    }

                    var name = button.Item1.HasValue ? $"{definition.Name} {button.Item1} hold" : $"{definition.Name} hold";
                    configs.Add(FormatDefinition(addConfig, EntityType.BinarySensor, new SensorConfig
                    {
                        Name = name,
                        Platform = definition.Platform,
                        DeviceId = definition.DeviceId,
                        DeviceName = definition.Name,
                    }));
                }
            }

            return configs;
        }

        private ConfigEntry FormatSensorDefinition(string entityType, SensorConfig sensor)
        {
            void addConfig(List<string> entity)
            {
                var prefix = _configuration.GetPlatformPrefix(sensor.Platform);
                switch (sensor.Type)
                {
                    case SensorType.Battery:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "battery"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/battery");
                        entity.Add($"  value_template: >{Environment.NewLine}    {{{{ value | int }}}}");
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
                        entity.Add($"  value_template: >{Environment.NewLine}    {{{{ value | int }}}}");
                        entity.Add("  unit_of_measurement: lux");
                        entity.Add("  force_update: true");
                        break;
                    case SensorType.Moisture:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "moisture"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/water");
                        entity.Add("  payload_on: wet");
                        entity.Add("  payload_off: dry");
                        break;
                    case SensorType.Motion:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "motion"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/motion");
                        entity.Add("  payload_on: active");
                        entity.Add("  payload_off: inactive");
                        break;
                    case SensorType.Offline:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "problem"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/activity");
                        entity.Add("  payload_on: inactive");
                        entity.Add("  payload_off: active");
                        break;
                    case SensorType.Power:
                        if (sensor.DeviceClass != null)
                        {
                            entity.Add($"  device_class: {sensor.DeviceClass}");
                        }

                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/power");
                        entity.Add($"  value_template: >{Environment.NewLine}    {{{{ value | float }}}}");
                        entity.Add("  unit_of_measurement: W");
                        entity.Add("  force_update: true");
                        break;
                    case SensorType.PowerCycle:
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceName}/cycle");
                        break;
                    case SensorType.Presence:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "presence"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/presence");
                        entity.Add("  payload_on: present");
                        entity.Add("  payload_off: not present");
                        break;
                    case SensorType.Scene:
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/scene");
                        entity.Add("  force_update: true");
                        break;
                    case SensorType.Temperature:
                        entity.Add($"  device_class: {sensor.DeviceClass ?? "temperature"}");
                        entity.Add($"  state_topic: {prefix}/{sensor.DeviceId}/temperature");
                        entity.Add($"  value_template: >{Environment.NewLine}    {{{{ value | float }}}}");
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
            }

            return FormatDefinition(addConfig, entityType, sensor);
        }

        #endregion
    }
}
