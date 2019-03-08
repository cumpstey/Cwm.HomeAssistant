using System.Collections.Generic;

namespace Cwm.HomeAssistant.Config.Models
{
    /// <summary>
    /// Class representing a device, which may contain
    /// several actuators and/or sensors.
    /// </summary>
    public class DeviceDefinition
    {
        #region Fields

        private string _name;

        #endregion

        #region Properties

        /// <summary>
        /// Id of the device on the platform it's connected to.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Platform which the device is connected to.
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// Friendly name of the device, if different from the device id.
        /// </summary>
        public string Name
        {
            get { return _name ?? DeviceId; }
            set { _name = value; }
        }

        /// <summary>
        /// Actuators which the device contains.
        /// </summary>
        public ActuatorDefinition[] Actuators { get; set; }

        /// <summary>
        /// Sensors which the device contains.
        /// </summary>
        public SensorDefinition[] Sensors { get; set; }

        #endregion
    }
}
