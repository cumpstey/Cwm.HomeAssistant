namespace Cwm.HomeAssistant.Config.Models
{
    /// <summary>
    /// Class representing a sensor belonging to a device.
    /// </summary>
    /// <seealso cref="DeviceDefinition"/>
    /// <seealso cref="ActuatorDefinition"/>
    public class SensorDefinition
    {
        /// <summary>
        /// Type of the sensor.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Device class of the sensor.
        /// See the Home Assistant documentation: https://www.home-assistant.io/components/sensor/
        /// </summary>
        public string DeviceClass { get; set; }

        /// <summary>
        /// Icon which should be used for the entity in Home Assistant.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Condition to be used by the threshold sensor,
        /// in the format: "[<>] number", eg "> 5".
        /// </summary>
        public string OnCondition { get; set; }
    }
}
