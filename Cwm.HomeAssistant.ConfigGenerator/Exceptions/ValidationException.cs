using System;

namespace Cwm.HomeAssistant.Config.Exceptions
{
    /// <summary>
    /// Exception which should be thrown if the device definition file
    /// contains invalid definitions.
    /// </summary>
    public class ValidationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationException"/> class.
        /// </summary>
        /// <param name="message">Message describing the validation error</param>
        public ValidationException(string message)
            : base(message)
        {
        }
    }
}
