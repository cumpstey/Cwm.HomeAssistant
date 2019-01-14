using Cwm.HomeAssistant.Config.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cwm.HomeAssistant.Config.Services
{
    public class MqttConfigGenerator
    {
        #region Fields

        const string SectionStartFormat = "### MQTT {0} ###";

        const string SectionEndFormat = "### end MQTT {0} ###";

        private readonly MqttActuatorConfigTransformer _actuatorTransformer;

        private readonly MqttSensorConfigTransformer _sensorTransformer;

        #endregion

        #region Constructor

        public MqttConfigGenerator(MqttActuatorConfigTransformer actuatorTransformer, MqttSensorConfigTransformer sensorTransformer)
        {
            _actuatorTransformer = actuatorTransformer;
            _sensorTransformer = sensorTransformer;
        }

        #endregion

        #region Methods

        public void TransformActuatorConfig(string inputFile, string outputDirectory)
        {
            var definitionJson = File.ReadAllText(inputFile);
            var definitions = JsonConvert.DeserializeObject<IReadOnlyList<ActuatorDefinition>>(definitionJson);

            var configs = new KeyedCollection<ConfigEntry>();
            foreach (var definition in definitions)
            {
                configs.Add(_actuatorTransformer.TransformConfig(definition));
            }

            foreach (var key in configs.Keys)
            {
                WriteToConfigFile(key, configs[key].Select(i => i.Entity).ToArray(), Path.Join(outputDirectory, $"{key}.yaml"));
                WriteToConfigFile(key, configs[key].Select(i => i.Customization).Where(i => i.Any()).ToArray(), Path.Join(outputDirectory, $"customize.yaml"));
            }
        }

        public void TransformSensorConfig(string inputFile, string outputDirectory)
        {
            var definitionJson = File.ReadAllText(inputFile);
            var definitions = JsonConvert.DeserializeObject<IReadOnlyList<SensorDefinition>>(definitionJson);

            var configs = new KeyedCollection<ConfigEntry>();
            foreach(var definition in definitions)
            {
                configs.Add(_sensorTransformer.TransformConfig(definition));
            }

            foreach(var key in configs.Keys)
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
