namespace Cwm.HomeAssistant.Config.Models
{
    /// <summary>
    /// Class representing an actuator belonging to a device.
    /// </summary>
    /// <seealso cref="DeviceDefinition"/>
    /// <seealso cref="SensorDefinition"/>
    public class ActuatorDefinition
    {
        /// <summary>
        /// Type of the actuator.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Icon which should be used for the entity in Home Assistant.
        /// </summary>
        public string Icon { get; set; }
    }
}
