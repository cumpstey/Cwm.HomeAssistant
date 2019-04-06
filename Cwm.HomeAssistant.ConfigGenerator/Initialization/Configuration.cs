using Cwm.HomeAssistant.Config.Services;
using System.Collections.Generic;
using System.IO;

namespace Cwm.HomeAssistant.Config.Initialization
{
    /// <summary>
    /// Configuration for the config file generator.
    /// </summary>
    public class Configuration : IMqttConfigGeneratorConfiguration,
                                 ITemplateSensorConfigTransformerConfiguration
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

            LowBatteryAlertThreshold = 15;
            MqttDevicesFolderName = "mqtt";
            LovelaceIncludesFolderName = @"lovelace\includes";
        }

        #endregion

        #region Properties

        /// <summary>
        /// Folder containing source files.
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

        /// <summary>
        /// Threshold below which a battery level is alerted as low.
        /// </summary>
        public int LowBatteryAlertThreshold { get; set; }

        /// <summary>
        /// Name of the folder in which the device definition files are
        /// stored, relative to <see cref="SourceFolder"/>
        /// </summary>
        public string MqttDevicesFolderName { get; set; }

        /// <summary>
        /// Name of the folder in which the Lovelace config files should be
        /// generated, relative to <see cref="OutputFolder"/>.
        /// </summary>
        public string LovelaceIncludesFolderName { get; set; }

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

        /// <summary>
        /// Folder containing source device definition files.
        /// </summary>
        public string GetMqttDevicesFolder()
        {
            return Path.Combine(SourceFolder, MqttDevicesFolderName);
        }

        /// <summary>
        /// Folder containing Home Assistant Lovelace config files.
        /// </summary>
        public string GetLovelaceIncludesFolder()
        {
            return Path.Combine(OutputFolder, LovelaceIncludesFolderName);
        }

        #endregion
    }
}
