using Cwm.HomeAssistant.Config.Services;
using System.Collections.Generic;
using System.Linq;

namespace Cwm.HomeAssistant.Config.Initialization
{
    public class Configuration : IMqttConfigGeneratorConfiguration
    {
        #region Constructor

        public Configuration(string sourceFolder, string outputFolder, IReadOnlyDictionary<string,string> platformPrefixes)
        {
            SourceFolder = sourceFolder;
            OutputFolder = outputFolder;
            PlatformPrefixes = platformPrefixes;
        }

        #endregion

        #region Properties

        public string SourceFolder { get; private set; }

        public string OutputFolder { get; private set; }

        public IReadOnlyDictionary<string, string> PlatformPrefixes { get; private set; }

        #endregion

        #region Methods

        public string GetPlatformPrefix(string platform)
        {
            return (PlatformPrefixes?.ContainsKey(platform)).GetValueOrDefault() ? PlatformPrefixes[platform] : platform;
        }

        #endregion
    }
}
