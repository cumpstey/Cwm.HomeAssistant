namespace Cwm.HomeAssistant.Config.Services
{
    /// <summary>
    /// Class holding all the properties of an entity required to
    /// generate the configuration for the Home Assistant config files.
    /// </summary>
    public class ActuatorConfig
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
        /// Type of the actuator.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Id of the device on the platform it's connected to.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Icon which should be used for the entity in Home Assistant.
        /// </summary>
        public string Icon { get; set; }
    }
}
