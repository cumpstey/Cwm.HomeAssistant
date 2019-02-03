using System;
using System.Text.RegularExpressions;

namespace Cwm.HomeAssistant.Config.Services
{
   public abstract class ConfigTransformer
    {
        #region Helpers

        protected string FormatAsId(string name)
        {
            return Regex.Replace(new string(Array.FindAll(name.ToCharArray(), c => char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '_'))
                    .ToLower()
                    .Replace(" ", "_")
                    .TrimStart('_'),
                "_+",
                "_");
        }

        #endregion
    }
}
