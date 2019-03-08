using Cwm.HomeAssistant.Config.Initialization;
using Cwm.HomeAssistant.Config.Services;
using System.IO;

namespace Cwm.HomeAssistant.Config
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new AppSettingsReader().GenerateConfiguration();

            //var mqttDeviceInputFile = Path.Combine(configuration.SourceFolder, "mqtt-devices.json");
            //var mqttActuatorInputFile = Path.Combine(configuration.SourceFolder, "mqtt-actuators.json");
            //var mqttSensorInputFile = Path.Combine(configuration.SourceFolder, "mqtt-sensors.json");

            var mqttConfigGenerator = new MqttConfigGenerator(
                new MqttActuatorConfigTransformer(configuration),
                new MqttSensorConfigTransformer(configuration));
            //mqttConfigGenerator.TransformActuatorConfig(mqttDeviceInputFile, configuration.OutputFolder);
            mqttConfigGenerator.GenerateConfig(configuration.SourceFolder, configuration.OutputFolder);
        }
    }
}
