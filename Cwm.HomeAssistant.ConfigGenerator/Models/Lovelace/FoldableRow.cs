using System.Collections.Generic;

namespace Cwm.HomeAssistant.Config.Models.Lovelace
{
    public class FoldableRow
    {
        #region Properties

        public string Type { get { return "custom:fold-entity-brick-dev"; } }

        public string Entity { get; set; }

        public LovelaceEntity[] Entities { get; set; }

        #endregion
    }
}
