using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cwm.HomeAssistant.Config.Models;
using Cwm.HomeAssistant.Config.Models.Lovelace;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cwm.HomeAssistant.Config.Services
{
    /// <summary>
    /// Class providing functionality to update Home Assistant
    /// Lovelace configuration files.
    /// </summary>
    public class LovelaceConfigGenerator : ConfigGenerator
    {
        #region Fields

        private readonly LovelaceConfigTransformer _transformer;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="LovelaceConfigGenerator"/> class.
        /// </summary>
        /// <param name="transformer"></param>
        public LovelaceConfigGenerator(IFilesystem filesystem, LovelaceConfigTransformer transformer)
            : base(filesystem)
        {
            _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
        }

        #endregion

        #region Methods

        /// <summary>
        /// Reads all device definition files from the source directory,
        /// generates entity lists for battery and button sensors, which
        /// can be included in Lovelace configuration, and writes
        /// these to the config files in the output directory.
        /// </summary>
        /// <param name="sourceDirectory">Directory containig device definition files</param>
        /// <param name="outputDirectory">Directory containing Lovelace files</param>
        public async Task GenerateConfigAsync(string sourceDirectory, string outputDirectory)
        {
            var devices = await GetDeviceDefinitionsAsync(sourceDirectory);

            await UpdateEntitiesConfigAsync(SensorType.Battery, outputDirectory, devices);
            await UpdateButtonsConfigAsync(outputDirectory, devices);
        }

        #endregion

        #region Helpers

        private async Task UpdateEntitiesConfigAsync(string sensorType, string configDirectory, IEnumerable<DeviceDefinition> definitions)
        {
            var file = Path.Combine(configDirectory, $"{sensorType}-entities.yaml");

            // Deserialize the existing Lovelace config file.
            LovelaceEntity[] existingConfig = null;
            if (Filesystem.FileExists(file))
            {
                var fileContent = await Filesystem.ReadFileAsync(file);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(new UnderscoredNamingConvention())
                    .Build();
                existingConfig = deserializer.Deserialize<LovelaceEntity[]>(fileContent);
            }

            // Generate the list of entities required by the list of devices.
            var entities = _transformer.GenerateSensorEntityList(sensorType, definitions);

            // Generate the list of entites to be included in the Lovelace config,
            // by adding in any which don't already exist.
            var config = existingConfig?.ToList() ?? new List<LovelaceEntity>();
            foreach (var entity in entities)
            {
                // If entity exists in the list already, leave it unchanged.
                // If it doesn't, add it in.
                if (!config.Any(i => i.Entity == entity.Entity))
                {
                    config.Add(entity);
                }
            }

            // Serialize the list of entities to yaml.
            var serializer = new SerializerBuilder()
                .WithNamingConvention(new UnderscoredNamingConvention())
                .Build();
            var output = serializer.Serialize(config);

            await Filesystem.WriteFileAsync(file, output);
        }

        private async Task UpdateButtonsConfigAsync(string configDirectory, IEnumerable<DeviceDefinition> devices)
        {
            var file = Path.Combine(configDirectory, $"button-entities.yaml");

            // Generate the list of entities required by the list of devices.
            var model = _transformer.GenerateButtonList(devices);

            // Serialize the list of entities to yaml.
            var serializer = new SerializerBuilder()
                .WithNamingConvention(new UnderscoredNamingConvention())
                .Build();
            var output = serializer.Serialize(model);

            await Filesystem.WriteFileAsync(file, output);
        }

        #endregion
    }
}
