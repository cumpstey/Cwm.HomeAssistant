namespace Cwm.HomeAssistant.Config.Models
{
    public class ActuatorDefinition
    {
        #region Fields

        private string _deviceId;

        #endregion

        #region Properties

        public string Name { get; set; }

        public string Platform { get; set; }

        public string DeviceId
        {
            get { return _deviceId ?? Name; }
            set { _deviceId = value; }
        }

        public string Type { get; set; }

        public string Icon { get; set; }
     
        #endregion
    }
}
