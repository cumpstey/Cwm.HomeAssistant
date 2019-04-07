using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Cwm.HomeAssistant.Config.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Cwm.HomeAssistant.Config.Services
{
    public abstract class ConfigGenerator
    {
        #region Constructor

        protected ConfigGenerator(IFilesystem filesystem)
        {
            Filesystem = filesystem;
        }

        #endregion

        #region Properties

        protected IFilesystem Filesystem { get; private set; }

        #endregion

        #region Methods

        protected async Task<IReadOnlyCollection<DeviceDefinition>> GetDeviceDefinitionsAsync(string sourceDirectory)
        {
            var files = Filesystem.EnumerateFiles(sourceDirectory, "*.yaml");
            var definitions = new List<DeviceDefinition>();
            foreach (var file in files)
            {
                var fileContent = await Filesystem.ReadFileAsync(file);

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
