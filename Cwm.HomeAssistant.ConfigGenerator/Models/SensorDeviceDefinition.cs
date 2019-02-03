using System.Collections.Generic;

namespace Cwm.HomeAssistant.Config.Models
{
    public class SensorDeviceDefinition
    {
        #region Fields

        private string _name;

        #endregion

        #region Properties

        public string DeviceId { get; set; }

        public string Platform { get; set; }

        public string Name
        {
            get { return _name ?? DeviceId; }
            set { _name = value; }
        }

        public SensorDefinition[] Sensors { get; set; }

        #endregion
    }
}
