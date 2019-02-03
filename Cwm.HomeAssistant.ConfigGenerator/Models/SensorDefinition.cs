using System.Collections.Generic;

namespace Cwm.HomeAssistant.Config.Models
{
    public class SensorDefinition
    {
        #region Properties

        public string Type { get; set; }

        public string DeviceClass { get; set; }

        public string Icon { get; set; }

        public string OnCondition { get; set; }

        #endregion
    }
}
