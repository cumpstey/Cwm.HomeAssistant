using System.Collections.Generic;

namespace Cwm.HomeAssistant.Config.Models.Lovelace
{
    public class FoldableRow
    {
        #region Properties

        public string Type { get { return "custom:fold-entity-row"; } }

        public string Head { get; set; }

        public string[] Items { get; set; }

        #endregion
    }
}
