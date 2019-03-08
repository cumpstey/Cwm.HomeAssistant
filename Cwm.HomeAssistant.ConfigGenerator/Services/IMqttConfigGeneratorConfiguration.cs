namespace Cwm.HomeAssistant.Config.Services
{
    /// <summary>
    /// Interface defining the configuration needed to generate configuration files.
    /// </summary>
    public interface IMqttConfigGeneratorConfiguration
    {
        /// <summary>
        /// Find the MQTT topic prefix for the given platform.
        /// </summary>
        /// <param name="platform">Platform for which to find the topic prefix</param>
        /// <returns>Topic prefix</returns>
        string GetPlatformPrefix(string platform);
    }
}
