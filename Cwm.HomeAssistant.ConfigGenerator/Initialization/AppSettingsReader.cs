using System.Configuration;
using System.Linq;

namespace Cwm.HomeAssistant.Config.Initialization
{
    public class AppSettingsReader
    {
        #region Fields

        private const string PlatformPrefixKey = "PlatformPrefix:";

        #endregion

        #region Methods

        public Configuration GenerateConfiguration()
        {
            var sourceFolder = ConfigurationManager.AppSettings["SourceFolder"];
            var outputFolder = ConfigurationManager.AppSettings["OutputFolder"];

            var platformPrefixes = ConfigurationManager.AppSettings.AllKeys.Where(i => i.StartsWith(PlatformPrefixKey))
                .ToDictionary(key => key.Substring(PlatformPrefixKey.Length), value => value);

            return new Configuration(sourceFolder, outputFolder, platformPrefixes);
        }

        #endregion
    }
}
