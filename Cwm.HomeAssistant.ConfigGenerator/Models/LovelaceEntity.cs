namespace Cwm.HomeAssistant.Config.Models
{
    /// <summary>
    /// Class representing the configuration of an entity as declared
    /// in Lovelace configuration files.
    /// </summary>
    public class LovelaceEntity
    {
        /// <summary>
        /// Allows the implicit casting of a string to a <see cref="LovelaceEntity"/>
        /// to enable deserialization of the yaml config files, in which an entity can
        /// be represented by an object or an id string.
        /// </summary>
        /// <param name="entity">The string which is assumed to be the entity id</param>
        public static implicit operator LovelaceEntity(string entity)
        {
            return new LovelaceEntity { Entity = entity };
        }

        /// <summary>
        /// Entity id.
        /// </summary>
        public string Entity { get; set; }

        /// <summary>
        /// Custom card type.
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Overrides the friendly name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Overrides the icon or entity picture.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Show additional info. Valid values: `entity-id` and `last-changed`.
        /// </summary>
        public string SecondaryInfo { get; set; }

        /// <summary>
        /// How the state should be formatted. Currently only used for timestamp
        /// sensors. Valid values: `relative`, `total`, `date`, `time` and `datetime`.
        /// </summary>
        public string Format { get; set; }
    }
}
