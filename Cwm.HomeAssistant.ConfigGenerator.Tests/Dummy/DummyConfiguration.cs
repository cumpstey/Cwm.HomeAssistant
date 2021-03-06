﻿using System.Collections.Generic;
using Cwm.HomeAssistant.Config.Services;

namespace Cwm.HomeAssistant.Config.Initializtion
{
    public class DummyConfiguration : IMqttConfigGeneratorConfiguration,
                                      ITemplateSensorConfigTransformerConfiguration
    {
        #region Fields

        private static readonly IDictionary<string, string> DummyPlatforms = new Dictionary<string, string> {
            {"smartthings","this/is/a/test" }
        };

        #endregion

        #region Properties

        public int LowBatteryAlertThreshold { get; set; }

        #endregion

        #region Methods

        public string GetPlatformPrefix(string platform)
        {
            return DummyPlatforms.ContainsKey(platform)
                ? DummyPlatforms[platform]
                : platform;
        }

        #endregion
    }
}
