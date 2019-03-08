namespace Cwm.HomeAssistant.Config.Exceptions
{
    /// <summary>
    /// Exception which should be thrown if the device definition file
    /// contains a reference to an actuator or sensor type which is not
    /// supported.
    /// </summary>
    public class UnrecognizedTypeException : ValidationException
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="UnrecognizedTypeException"/> class.
        /// </summary>
        /// <param name="type">Type of the actuator or sensor which is not supported</param>
        public UnrecognizedTypeException(string type)
            : base($"'{type}' is not a supported type")
        {
            Type = type;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Type of the actuator or sensor which is not supported.
        /// </summary>
        public string Type { get; private set; }

        #endregion
    }
}
