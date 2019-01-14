namespace Cwm.HomeAssistant.Config.Services
{
    public interface IMqttConfigGeneratorConfiguration
    {
        string GetPlatformPrefix(string platform);
    }
}
