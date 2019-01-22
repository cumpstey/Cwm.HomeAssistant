using System;

namespace Cwm.HomeAssistant.Config.Exceptions
{
    public class UnrecognizedTypeException : Exception
    {
        #region Constructor

        public UnrecognizedTypeException(string type)
            : base($"'{type}' is not a supported type")
        {
            Type = type;
        }

        #endregion

        #region Properties

        public string Type { get; private set; }

        #endregion
    }
}
