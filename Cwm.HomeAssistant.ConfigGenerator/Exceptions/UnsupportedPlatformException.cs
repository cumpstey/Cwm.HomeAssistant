using System;

namespace Cwm.HomeAssistant.Config.Exceptions
{
    public class UnsupportedPlatformException : Exception
    {
        #region Constructor

        public UnsupportedPlatformException(string platform, string type)
            : base($"'{platform}' is not a supported platform for {type}")
        {
            Platform = platform;
            Type = type;
        }

        #endregion

        #region Properties

        public string Platform { get; private set; }

        public string Type { get; private set; }

        #endregion
    }
}
