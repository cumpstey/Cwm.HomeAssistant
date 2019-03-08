using System.Configuration;
using System.Linq;

namespace Cwm.HomeAssistant.Config.Initialization
{
    /// <summary>
    /// Contains a method for reading application settings into the <see cref="Configuration"/> class.
    /// </summary>
    public class AppSettingsReader
    {
        #region Fields

        private const string PlatformPrefixKey = "PlatformPrefix:";

        #endregion

        #region Methods

        /// <summary>
        /// Read application settings from the app.config file into the <see cref="Configuration"/> class.
        /// </summary>
        /// <returns>Configuration</returns>
        public Configuration GenerateConfiguration()
        {
            var sourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
            var outputFolder = ConfigurationManager.AppSettings["OutputFolder"];

            var platformPrefixes = ConfigurationManager.AppSettings.AllKeys.Where(i => i.StartsWith(PlatformPrefixKey))
                .ToDictionary(key => key.Substring(PlatformPrefixKey.Length), value => ConfigurationManager.AppSettings[value]);

            return new Configuration(sourceFolder, outputFolder, platformPrefixes);
        }

        #endregion
    }
}
