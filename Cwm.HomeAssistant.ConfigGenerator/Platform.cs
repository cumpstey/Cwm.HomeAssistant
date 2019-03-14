namespace Cwm.HomeAssistant.Config
{
    /// <summary>
    /// Contains string constants for the supported platforms.
    /// </summary>
    public static class Platform
    {
        /// <summary>
        /// Genius Hub: https://www.geniushub.co.uk/
        /// </summary>
        public const string Genius = "genius";

        /// <summary>
        /// Home Assistant - for sensors set internally by automations or scripts.
        /// </summary>
        public const string HomeAssistant = "homeassistant";

        /// <summary>
        /// Hubitat: https://hubitat.com/
        /// </summary>
        public const string Hubitat = "hubitat";

        /// <summary>
        /// SmartThings: https://www.smartthings.com/uk
        /// </summary>
        public const string SmartThings = "smartthings";

        /// <summary>
        /// Zipato: https://www.zipato.com/
        /// </summary>
        public const string Zipato = "zipato";
    }
}
