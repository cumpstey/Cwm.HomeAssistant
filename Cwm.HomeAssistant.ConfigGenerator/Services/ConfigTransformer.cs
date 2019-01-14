using System;

namespace Cwm.HomeAssistant.Config.Services
{
   public abstract class ConfigTransformer
    {
        #region Helpers

        protected string FormatAsId(string name)
        {
            return new string(Array.FindAll(name.ToCharArray(), c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '_'))
                .ToLower()
                .Replace(" ", "_");
        }

        #endregion
    }
}
