using System;
using System.Text.RegularExpressions;

namespace Cwm.HomeAssistant.Config.Services
{
    /// <summary>
    /// Base class for service classes providing functionality to generate configuration files.
    /// </summary>
    public abstract class ConfigTransformer
    {
        #region Helpers

        /// <summary>
        /// Format an entity's name as an id.
        /// This is, roughly, lower case with underscores, but I've not found the exact rules
        /// so it may require tweaking.
        /// </summary>
        /// <param name="name">Name of the entity</param>
        /// <returns>Id which and entity of the given name will have</returns>
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
