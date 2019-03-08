using Cwm.HomeAssistant.Config.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cwm.HomeAssistant.Config.Services
{
    /// <summary>
    /// Class providing functionality to update Home Assistant configuration files.
    /// </summary>
    public class MqttConfigGenerator
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

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MqttConfigGenerator"/> class.
        /// </summary>
        /// <param name="actuatorTransformer"></param>
        /// <param name="sensorTransformer"></param>
        public MqttConfigGenerator(MqttActuatorConfigTransformer actuatorTransformer, MqttSensorConfigTransformer sensorTransformer)
        {
            _actuatorTransformer = actuatorTransformer;
            _sensorTransformer = sensorTransformer;
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
        public void GenerateConfig(string sourceDirectory, string outputDirectory)
        {
            var files = Directory.EnumerateFiles(sourceDirectory, "*.yaml");
            var definitions = new List<DeviceDefinition>();
            foreach (var file in files)
            {
                var fileContent = File.ReadAllText(file);

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(new CamelCaseNamingConvention())
                    .Build();
                var fileDefinitions = deserializer.Deserialize<DeviceDefinition[]>(fileContent);

                //var fileDefinitions = JsonConvert.DeserializeObject<IReadOnlyList<DeviceDefinition>>(fileContent);

                definitions.AddRange(fileDefinitions);
            }

            var configs = new KeyedCollection<ConfigEntry>();
            foreach (var definition in definitions)
            {
                configs.Add(_actuatorTransformer.TransformConfig(definition));
                configs.Add(_sensorTransformer.TransformConfig(definition));
            }

            foreach (var key in configs.Keys)
            {
                WriteToConfigFile(key, configs[key].Select(i => i.Entity).ToArray(), Path.Join(outputDirectory, $"{key}.yaml"));
                WriteToConfigFile(key, configs[key].Select(i => i.Customization).Where(i => i.Any()).ToArray(), Path.Join(outputDirectory, $"customize.yaml"));
            }
        }

        #endregion

        #region Helpers

        private void WriteToConfigFile(string type, string[] entries, string filePath)
        {
            if(!entries.Any())
            {
                return;
            }

            var sectionStart = string.Format(SectionStartFormat, type);
            var sectionEnd = string.Format(SectionEndFormat, type);

            var fileContent = (File.Exists(filePath) ? File.ReadAllText(filePath) : string.Empty).Trim();
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

            File.WriteAllText(filePath, fileContent + Environment.NewLine);
        }

        #endregion
    }
}
