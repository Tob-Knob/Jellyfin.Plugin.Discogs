using System;

namespace Jellyfin.Plugin.Discogs.Exceptions
{
    /// <summary>
    /// Invalid configuration.
    /// </summary>
    public class InvalidConfigurationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
        /// </summary>
        /// <param name="fieldName">Field that is invalid.</param>
        /// <param name="message">Message describing the error.</param>
        public InvalidConfigurationException(string fieldName, string message) : base(message)
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidConfigurationException"/> class.
        /// </summary>
        /// <param name="fieldName">Field that is invalid.</param>
        /// <param name="message">Message describing the error.</param>
        /// <param name="innerException">Inner exception.</param>
        public InvalidConfigurationException(string fieldName, string message, Exception innerException) : base(message, innerException)
        {
            FieldName = fieldName;
        }

        /// <summary>
        /// Gets the field that is invalid.
        /// </summary>
        public string FieldName { get; }
    }
}
