using Cwm.HomeAssistant.Config.Exceptions;
using Cwm.HomeAssistant.Config.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Cwm.HomeAssistant.Config.Services
{
    public class MqttActuatorConfigTransformer : ConfigTransformer
    {
        #region Fields

        private readonly IMqttConfigGeneratorConfiguration _configuration;

        #endregion

        #region Constructor

        public MqttActuatorConfigTransformer(IMqttConfigGeneratorConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region Methods

        public KeyedCollection<ConfigEntry> TransformConfig(ActuatorDefinition definition)
        {
            var configs = new KeyedCollection<ConfigEntry>();
            if (definition.Type == "light" || definition.Type.EndsWith("-light"))
            {
                var config = FormatLightDefinition(definition);
                configs.Add("light", config);
            }
            else if (definition.Type == "switch")
            {
                var config = FormatSwitchDefinition(definition);
                configs.Add("switch", config);
            }
            else if (definition.Type == "heating")
            {
                var config = FormatHeatingDefinition(definition);
                configs.Add("climate", config);
            }

            return configs;
        }

        #endregion

        #region Helpers

        private ConfigEntry FormatLightDefinition(ActuatorDefinition definition)
        {
            var entity = new List<string>();
            var customization = new List<string>();

            entity.Add($"# {definition.Name}, from {definition.Platform} via MQTT");

            if (definition.ToDo != null)
            {
                entity.Add($"# TODO: {definition.ToDo}");
            }

            entity.Add("- platform: mqtt");
            entity.Add($"  name: {definition.Name}");
            entity.Add("  retain: true");

            if (definition.Icon != null)
            {
                customization.Add($"  icon: {definition.Icon}");
            }

            var prefix = _configuration.GetPlatformPrefix(definition.Platform);
            switch (definition.Platform)
            {
                case "smartthings":
                case "hubitat":
                    entity.Add($"  state_topic: {prefix}/{definition.DeviceId}/switch");
                    entity.Add($"  command_topic: {prefix}/{definition.DeviceId}/switch");
                    entity.Add(@"  payload_on: 'on'");
                    entity.Add(@"  payload_off: 'off'");

                    if (definition.Type == "dimmable-light" || definition.Type == "rgbw-light")
                    {
                        entity.Add($"  brightness_state_topic: {prefix}/{definition.DeviceId}/level");
                        entity.Add($"  brightness_command_topic: {prefix}/{definition.DeviceId}/level");
                        entity.Add("  brightness_scale: 99");
                    }

                    if (definition.Type == "rgbw-light")
                    {
                        entity.Add($"  hs_state_topic: {prefix}/{definition.DeviceId}/hs");
                        entity.Add($"  hs_command_topic: {prefix}/{definition.DeviceId}/hs");
                    }

                    break;
                default:
                    throw new UnsupportedPlatformException(definition.Platform, definition.Type);
            }

            if (customization.Any())
            {
                customization.Insert(0, $@"""light.{FormatAsId(definition.Name)}"":");
            }

            return new ConfigEntry
            {
                Entity = string.Join(Environment.NewLine, entity),
                Customization = string.Join(Environment.NewLine, customization),
            };
        }

        private ConfigEntry FormatSwitchDefinition(ActuatorDefinition definition)
        {
            var entity = new List<string>();
            var customization = new List<string>();

            entity.Add($"# {definition.Name}, from {definition.Platform} via MQTT");

            if (definition.ToDo != null)
            {
                entity.Add($"# TODO: {definition.ToDo}");
            }

            entity.Add("- platform: mqtt");
            entity.Add($"  name: {definition.Name}");
            entity.Add("  retain: true");

            if (definition.Icon != null)
            {
                customization.Add($"  icon: {definition.Icon}");
            }

            var prefix = _configuration.GetPlatformPrefix(definition.Platform);
            switch (definition.Platform)
            {
                case "zipato":
                    entity.Add($"  state_topic: {prefix}/attributes/{definition.DeviceId}/value");
                    entity.Add($"  command_topic: {prefix}/request/attributes/{definition.DeviceId}/value");
                    entity.Add(@"  value_template: ""{{value_json.value}}""");
                    entity.Add("  state_on: true");
                    entity.Add("  state_off: false");
                    entity.Add(@"  payload_on: '{""value"": true}'");
                    entity.Add(@"  payload_off: '{""value"": false}'");
                    break;
                case "genius":
                case "smartthings":
                case "hubitat":
                    entity.Add($"  state_topic: {prefix}/{definition.DeviceId}/switch");
                    entity.Add($"  command_topic: {prefix}/{definition.DeviceId}/switch");
                    entity.Add(@"  payload_on: 'on'");
                    entity.Add(@"  payload_off: 'off'");
                    break;
                default:
                    throw new UnsupportedPlatformException(definition.Platform, definition.Type);
            }

            if (customization.Any())
            {
                customization.Insert(0, $@"""switch.{FormatAsId(definition.Name)}"":");
            }

            return new ConfigEntry
            {
                Entity = string.Join(Environment.NewLine, entity),
                Customization = string.Join(Environment.NewLine, customization),
            };
        }

        private ConfigEntry FormatHeatingDefinition(ActuatorDefinition definition)
        {
            var entity = new List<string>();
            var customization = new List<string>();

            entity.Add($"# {definition.Name}, from {definition.Platform} via MQTT");

            if (definition.ToDo != null)
            {
                entity.Add($"# TODO: {definition.ToDo}");
            }

            entity.Add("- platform: mqtt");
            entity.Add($"  name: {definition.Name}");
            entity.Add("  retain: true");

            if (definition.Icon != null)
            {
                customization.Add($"  icon: {definition.Icon}");
            }

            var prefix = _configuration.GetPlatformPrefix(definition.Platform);
            switch (definition.Platform)
            {
                case "genius":
                    entity.Add("  modes:");
                    entity.Add("    - auto");
                    entity.Add("    - heat");
                    entity.Add($"  current_temperature_topic: {prefix}/{definition.DeviceId}/temperature");
                    entity.Add($"  mode_state_topic: {prefix}/{definition.DeviceId}/thermostatMode");
                    entity.Add($"  temperature_state_topic: {prefix}/{definition.DeviceId}/heatingSetpoint");
                    entity.Add($"  mode_command_topic: {prefix}/{definition.DeviceId}/thermostatMode");
                    entity.Add($"  temperature_command_topic: {prefix}/{definition.DeviceId}/heatingSetpoint");
                    entity.Add("  min_temp: 4");
                    entity.Add("  max_temp: 28");
                    entity.Add("  temp_step: 0.5");
                    break;
                default:
                    throw new UnsupportedPlatformException(definition.Platform, definition.Type);
            }

            if (customization.Any())
            {
                customization.Insert(0, $@"""climate.{FormatAsId(definition.Name)}"":");
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
