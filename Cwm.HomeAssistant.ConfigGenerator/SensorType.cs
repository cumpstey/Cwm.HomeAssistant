namespace Cwm.HomeAssistant.Config
{
    /// <summary>
    /// Contains string constants for the supported sensor types.
    /// </summary>
    public static class SensorType
    {
        /// <summary>
        /// Battery sensor.
        /// </summary>
        public const string Battery = "battery";

        /// <summary>
        /// Button.
        /// </summary>
        public const string Button = "button";

        /// <summary>
        /// Contact sensor.
        /// </summary>
        public const string Contact = "contact";

        /// <summary>
        /// Illuminance sensor.
        /// </summary>
        public const string Illuminance = "illuminance";

        /// <summary>
        /// Moisture sensor.
        /// </summary>
        public const string Moisture = "moisture";

        /// <summary>
        /// Motion sensor.
        /// </summary>
        public const string Motion = "motion";

        /// <summary>
        /// Power sensor.
        /// </summary>
        public const string Power = "power";

        /// <summary>
        /// Cycle sensor for washing machine etc based on power usage.
        /// </summary>
        public const string PowerCycle = "power-cycle";

        /// <summary>
        /// Binary sensor for washing machine etc based on power cycle.
        /// </summary>
        public const string PowerCycleOnOff = "power-cycle-onoff";

        /// <summary>
        /// Presence sensor.
        /// </summary>
        public const string Presence = "presence";

        /// <summary>
        /// Scene trigger.
        /// </summary>
        public const string Scene = "scene";

        /// <summary>
        /// Temperature sensor.
        /// </summary>
        public const string Temperature = "temperature";

        /// <summary>
        /// Binary sensor triggered by a value from an analogue sensor crossin a threshold.
        /// </summary>
        public const string Threshold = "threshold";
    }
}
