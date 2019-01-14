using Cwm.HomeAssistant.Config.Services;

namespace Cwm.HomeAssistant.Config.Initializtion
{
    public class DummyConfiguration : IMqttConfigGeneratorConfiguration
    {
        public string GetPlatformPrefix(string platform)
        {
            return platform == "smartthings" ? "this/is/a/test" : platform;
        }
    }
}
