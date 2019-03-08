namespace Cwm.HomeAssistant.Config.Services
{
    /// <summary>
    /// Class holding the entries in the Home Assistant config files representing
    /// a device.
    /// </summary>
    public class ConfigEntry
    {
        /// <summary>
        /// Configuration for the entity config file.
        /// </summary>
        public string Entity { get; set; }

        /// <summary>
        /// Configuration of the entity for the customization file.
        /// </summary>
        public string Customization { get; set; }
    }
}
