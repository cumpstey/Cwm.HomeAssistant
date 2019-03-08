using Cwm.HomeAssistant.Config.Services;
using System.Collections.Generic;

namespace Cwm.HomeAssistant.Config.Initialization
{
    /// <summary>
    /// Configuration for the config file generator.
    /// </summary>
    public class Configuration : IMqttConfigGeneratorConfiguration
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        /// <param name="sourceFolder">Folder containing source device definition files</param>
        /// <param name="outputFolder">Folder containing Home Assistant config files</param>
        /// <param name="platformPrefixes">MQTT topic prefixes for the supported platforms</param>
        public Configuration(string sourceFolder, string outputFolder, IReadOnlyDictionary<string, string> platformPrefixes)
        {
            SourceFolder = sourceFolder;
            OutputFolder = outputFolder;
            PlatformPrefixes = platformPrefixes;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Folder containing source device definition files.
        /// </summary>
        public string SourceFolder { get; private set; }

        /// <summary>
        /// Folder containing Home Assistant config files.
        /// </summary>
        public string OutputFolder { get; private set; }

        /// <summary>
        /// MQTT topic prefixes for the supported platforms.
        /// </summary>
        public IReadOnlyDictionary<string, string> PlatformPrefixes { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Find the MQTT topic prefix for the given platform.
        /// </summary>
        /// <param name="platform">Platform for which to find the topic prefix</param>
        /// <returns>Topic prefix</returns>
        public string GetPlatformPrefix(string platform)
        {
            return (PlatformPrefixes?.ContainsKey(platform)).GetValueOrDefault() ? PlatformPrefixes[platform] : platform;
        }

        #endregion
    }
}
