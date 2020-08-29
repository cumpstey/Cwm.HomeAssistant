using System.Collections.Generic;

namespace Cwm.HomeAssistant.Config.Services
{
    public class SensorConfig
    {
        /// <summary>
        /// Name of the entity in Home Assistant.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Platform which the device is connected to.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Type of the sensor.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Device class of the entity in Home Assistant.
        /// See the documentation: https://www.home-assistant.io/components/sensor/
        /// </summary>
        public string DeviceClass { get; set; }

        /// <summary>
        /// Id of the device on the platform it's connected to.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Name of the device within Home Assistant.
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Icon which should be used for the entity in Home Assistant.
        /// </summary>
        public string Icon { get; set; }

        public string ThresholdAttribute { get; set; }

        public string ThresholdOnCondition { get; set; }

        public Dictionary<string,string> Customize { get; set; }
    }
}
