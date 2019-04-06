namespace Cwm.HomeAssistant.Config.Services
{
    /// <summary>
    /// Interface defining the configuration needed to generate configuration files.
    /// </summary>
    public interface ITemplateSensorConfigTransformerConfiguration
    {
        int LowBatteryAlertThreshold { get; }
    }
}
