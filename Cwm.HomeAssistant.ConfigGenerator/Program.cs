using System.Threading.Tasks;
using Cwm.HomeAssistant.Config.Initialization;
using Cwm.HomeAssistant.Config.Services;

namespace Cwm.HomeAssistant.Config
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new AppSettingsReader().GenerateConfiguration();

            var filesystem = new Filesystem();
            var deviceTranslator = new DeviceTranslator();

            var mqttConfigGenerator = new MqttConfigGenerator(
                filesystem,
                new MqttActuatorConfigTransformer(configuration),
                new MqttSensorConfigTransformer(configuration, deviceTranslator),
                new TemplateSensorConfigTransformer(configuration, deviceTranslator));
            var lovelaceConfigGenerator = new LovelaceConfigGenerator(
                filesystem,
                new LovelaceConfigTransformer(deviceTranslator));

            Task.WaitAll(
                mqttConfigGenerator.GenerateConfigAsync(configuration.GetMqttDevicesFolder(), configuration.OutputFolder),
                lovelaceConfigGenerator.GenerateConfigAsync(configuration.GetMqttDevicesFolder(), configuration.GetLovelaceIncludesFolder())
            );
        }
    }
}
