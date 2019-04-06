using System.Collections.Generic;
using System.Threading.Tasks;
using Cwm.HomeAssistant.Config.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cwm.HomeAssistant.Config.Services
{
    public abstract class ConfigGenerator
    {
        #region Constructor

        protected ConfigGenerator(IFileProvider fileProvider)
        {
            FileProvider = fileProvider;
        }

        #endregion

        #region Properties

        protected IFileProvider FileProvider { get; private set; }

        #endregion

        #region Methods

        protected async Task<IReadOnlyCollection<DeviceDefinition>> GetDeviceDefinitionsAsync(string sourceDirectory)
        {
            var files = FileProvider.EnumerateFiles(sourceDirectory, "*.yaml");
            var definitions = new List<DeviceDefinition>();
            foreach (var file in files)
            {
                var fileContent = await FileProvider.ReadFileAsync(file);

                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(new CamelCaseNamingConvention())
                    .Build();
                var fileDefinitions = deserializer.Deserialize<DeviceDefinition[]>(fileContent);

                //var fileDefinitions = JsonConvert.DeserializeObject<IReadOnlyList<DeviceDefinition>>(fileContent);

                definitions.AddRange(fileDefinitions);
            }

            return definitions;
        }

        #endregion
    }
}
