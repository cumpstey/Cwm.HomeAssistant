using System.Collections.Generic;

namespace Cwm.HomeAssistant.Config.Models
{
    public class SensorConfig
    {
        #region Properties

        public string Name { get; set; }

        public string Platform { get; set; }

        public string Type { get; set; }

        public string DeviceClass { get; set; }
  
        public string DeviceId { get; set; }

        public string Icon { get; set; }

        public string ThresholdAttribute { get; set; }

        public string ThresholdOnCondition { get; set; }

        #endregion
    }
}
