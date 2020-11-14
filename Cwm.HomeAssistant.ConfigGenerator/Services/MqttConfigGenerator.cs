using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Cwm.HomeAssistant.Config.Services
{
    /// <summary>
    /// Class providing functionality to update Home Assistant configuration files.
    /// </summary>
    public class MqttConfigGenerator : ConfigGenerator
    {
        #region Fields

        /// <summary>
        /// String showing the beginning of the section of the config file which will be updated.
        /// </summary>
        const string SectionStartFormat = "### MQTT {0} ###";

        /// <summary>
        /// String showing the end of the section of the config file which will be updated.
        /// </summary>
        const string SectionEndFormat = "### end MQTT {0} ###";

        private readonly MqttActuatorConfigTransformer _actuatorTransformer;

        private readonly MqttSensorConfigTransformer _sensorTransformer;

        private readonly TemplateSensorConfigTransformer _templateSensorTransformer;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttConfigGenerator"/> class.
        /// </summary>
        /// <param name="actuatorTransformer"></param>
        /// <param name="sensorTransformer"></param>
        public MqttConfigGenerator(IFilesystem filesystem,
                                   MqttActuatorConfigTransformer actuatorTransformer,
                                   MqttSensorConfigTransformer sensorTransformer,
                                   TemplateSensorConfigTransformer templateSensorTransformer)
            : base(filesystem)
        {
            _actuatorTransformer = actuatorTransformer ?? throw new ArgumentNullException(nameof(actuatorTransformer));
            _sensorTransformer = sensorTransformer ?? throw new ArgumentNullException(nameof(sensorTransformer));
            _templateSensorTransformer = templateSensorTransformer ?? throw new ArgumentNullException(nameof(templateSensorTransformer));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads all device definition files from the source directory,
        /// generates Home Assistant entity configurations, and writes
        /// these to the config files in the output directory.
        /// </summary>
        /// <param name="sourceDirectory">Directory containig device definition files</param>
        /// <param name="outputDirectory">Directory containing Home Assistant config files</param>
        public async Task GenerateConfigAsync(string sourceDirectory, string outputDirectory)
        {
            var definitions = await GetDeviceDefinitionsAsync(sourceDirectory);

            var configs = new KeyedCollection<ConfigEntry>();
            foreach (var definition in definitions)
            {
                configs.Add(_actuatorTransformer.TransformConfig(definition));
                configs.Add(_sensorTransformer.TransformConfig(definition));
                configs.Add(_templateSensorTransformer.GetButtonActivitySensor(definition));
            }

            configs.Add(_templateSensorTransformer.GetLowBatteryAlertSensor(definitions));
            configs.Add(_templateSensorTransformer.GetDeviceOfflineAlertSensor(definitions));

            foreach (var key in configs.Keys)
            {
                await WriteToConfigFileAsync(key, configs[key].Select(i => i.Entity).ToArray(), Path.Combine(outputDirectory, $"{key}.yaml"));
                await WriteToConfigFileAsync(key, configs[key].Select(i => i.Customization).Where(i => i.Any()).ToArray(), Path.Combine(outputDirectory, $"customize.yaml"));
            }
        }

        #endregion

        #region Helpers

        private async Task WriteToConfigFileAsync(string type, string[] entries, string filePath)
        {
            if (!entries.Any())
            {
                return;
            }

            var sectionStart = string.Format(SectionStartFormat, type);
            var sectionEnd = string.Format(SectionEndFormat, type);

            var fileContent = (Filesystem.FileExists(filePath) ? await Filesystem.ReadFileAsync(filePath) : string.Empty).Trim();
            var startIndex = fileContent.IndexOf(sectionStart);
            var endIndex = fileContent.IndexOf(sectionEnd);

            if (startIndex > -1 && endIndex > startIndex)
            {
                // Remove existing section from file
                fileContent = fileContent.Remove(startIndex, endIndex + sectionEnd.Length - startIndex);
            }

            var newContent = string.Join(Environment.NewLine + Environment.NewLine,
                                sectionStart,
                                string.Join(Environment.NewLine + Environment.NewLine, entries),
                                sectionEnd);

            if (startIndex > -1)
            {
                fileContent = fileContent.Insert(startIndex, newContent);
            }
            else
            {
                fileContent = fileContent + Environment.NewLine + Environment.NewLine + newContent;
            }

            await Filesystem.WriteFileAsync(filePath, fileContent + Environment.NewLine);
        }

        #endregion
    }
}
