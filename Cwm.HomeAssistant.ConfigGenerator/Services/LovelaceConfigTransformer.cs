using Cwm.HomeAssistant.Config.Exceptions;
using Cwm.HomeAssistant.Config.Models;
using Cwm.HomeAssistant.Config.Models.Lovelace;
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
        #region Fields

        private DeviceTranslator _deviceTranslator;

        #endregion

        #region Constructor

        public LovelaceConfigTransformer(DeviceTranslator deviceTranslator)
        {
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
        public IReadOnlyCollection<LovelaceEntity> GenerateSensorEntityList(string sensorType, IEnumerable<DeviceDefinition> devices)
        {
            var sensorDevices = devices.Where(d => d.Sensors != null && d.Sensors.Any(s => s.Type == sensorType))
                                       .OrderBy(d => d.Name);

            var invalid = sensorDevices.Where(definition => string.IsNullOrWhiteSpace(definition.DeviceId));
            if (invalid.Any())
            {
                throw new ValidationException($"{invalid.Count()} definitions missing a device id.");
            }

            var entities = sensorDevices.Select(i => new LovelaceEntity
            {
                Entity = GetSensorEntityId(sensorType, i),
                Name = i.Name,
            }).ToArray();

            return  entities;
        }

        public IReadOnlyCollection<FoldableRow> GenerateButtonList(IEnumerable<DeviceDefinition> devices)
        {
            var model = new List<FoldableRow>();
            foreach (var device in devices)
            {
                var buttons = _deviceTranslator.TranslateButtonDefinition(device);
                if (!buttons.Any()) continue;

                model.Add(new FoldableRow
                {
                    Entity = GetButtonActivityEntityId(device),
                    Entities = buttons.Select(i => {
                        string name;
                        switch (i.Item2)
                        {
                            case ButtonType.Hold:
                            case ButtonType.HoldAndRelease:
                                name = i.Item1.HasValue ? $"Button {i.Item1} hold" : "Hold";
                                break;
                            default:
                                name = i.Item1.HasValue ? $"Button {i.Item1}" : "Push";
                                break;

                        }

                        return new LovelaceEntity
                        {
                            Entity = GetButtonEntityId(i.Item1, i.Item2, device),
                            Name = name,
                        };
                    }).OrderBy(i => i.Entity)
                      .ToArray(),
                });
            }

            return model;
        }

        #endregion
    }
}
