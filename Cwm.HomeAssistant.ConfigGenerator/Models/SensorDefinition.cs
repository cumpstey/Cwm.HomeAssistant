using System.Collections.Generic;

namespace Cwm.HomeAssistant.Config.Models
{
    public class SensorDefinition
    {
        #region Fields

        private string _deviceId;

        #endregion

        #region Properties

        public string Name { get; set; }

        public string Platform { get; set; }

        public string[] Type { get; set; }

        public Dictionary<string, string> DeviceClasses { get; set; }

        public string ToDo { get; set; }

        public string DeviceId
        {
            get { return _deviceId ?? Name; }
            set { _deviceId = value; }
        }

        public Dictionary<string, string> Icons { get; set; }

        public string OnCondition { get; set; }

        #endregion
    }
}
