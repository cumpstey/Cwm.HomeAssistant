using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cwm.HomeAssistant.Config.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cwm.HomeAssistant.Config.Services
{
    public class DeviceTranslator
    {
        #region Methods

        public IReadOnlyCollection<Tuple<int?, ButtonType>> TranslateButtonDefinition(DeviceDefinition device)
        {
            var buttons = new List<Tuple<int?, ButtonType>>();
            buttons.AddRange(GetButtonList(ButtonType.Push, device));
            buttons.AddRange(GetButtonList(ButtonType.Hold, device));
            buttons.AddRange(GetButtonList(ButtonType.HoldAndRelease, device));
            return buttons;
        }

        #endregion

        #region Helpers

        private IReadOnlyCollection<Tuple<int?, ButtonType>> GetButtonList(ButtonType buttonType, DeviceDefinition device)
        {
            if (device.Sensors == null)
            {
                return new Tuple<int?, ButtonType>[0];
            }

            var buttonTypeName = buttonType.GetAttribute<DisplayAttribute>()?.Name ?? buttonType.ToString();

            var buttons = new List<Tuple<int?, ButtonType>>();
            foreach (var sensor in device.Sensors)
            {
                // If there's only a single button we don't need to number it.
                if (sensor.Type == buttonTypeName || sensor.Type == $"1-{buttonTypeName}")
                {
                    buttons.Add(new Tuple<int?, ButtonType>(null, buttonType));
                }
                else if (Regex.IsMatch(sensor.Type, $@"\d+-{buttonTypeName}"))
                {
                    var count = int.Parse(sensor.Type.Split('-').First());
                    for (var i = 1; i <= count; i++)
                    {
                        buttons.Add(new Tuple<int?, ButtonType>(i, buttonType));
                    }
                }
            }

            return buttons;
        }

        #endregion
    }
}
