namespace Cwm.HomeAssistant.Config.Exceptions
{
    /// <summary>
    /// Exception which should be thrown if the device definition file
    /// contains a reference to a platform which is not supported for
    /// the given actuator or sensor type.
    /// </summary>
    public class UnsupportedPlatformException : ValidationException
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsupportedPlatformException"/> class.
        /// </summary>
        /// <param name="platform">Platform which is not supported</param>
        /// <param name="type">Type of the actuator or sensor for which the platform is not supported</param>
        public UnsupportedPlatformException(string platform, string type)
            : base($"'{platform}' is not a supported platform for {type}")
        {
            Platform = platform;
            Type = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Platform which is not supported.
        /// </summary>
        public string Platform { get; private set; }

        /// <summary>
        /// Type of the actuator or sensor for which the platform is not supported.
        /// </summary>
        public string Type { get; private set; }

        #endregion
    }
}
