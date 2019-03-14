namespace Cwm.HomeAssistant.Config.Services
{
    public class TemplateSensorConfig
    {
        /// <summary>
        /// Name of the entity in Home Assistant.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Icon which should be used for the entity in Home Assistant.
        /// </summary>
        public string Icon { get; set; }

        public string ValueTemplate { get; set; }
    }
}
