using System;

namespace Cwm.HomeAssistant.Config.Exceptions
{
    public class MissingParameterException : Exception
    {
        #region Constructor

        public MissingParameterException(string message)
            : base(message)
        {
        }

        #endregion
    }
}
