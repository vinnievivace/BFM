using System;

namespace BFM.OpenAI
{
    /// <summary>
    /// Abstract base class for all OpenAI API data structures
    /// </summary>
    [System.Serializable]
    public abstract class BaseOpenAI
    {
        /// <summary>
        /// The model used for the operation
        /// </summary>
        public string model;
        
        /// <summary>
        /// Timestamp of when the operation occurred
        /// </summary>
        public long created;
        
        /// <summary>
        /// Checks if the model field is valid
        /// </summary>
        public virtual bool HasValidModel => !string.IsNullOrEmpty(model);
        
        /// <summary>
        /// Checks if the created timestamp is valid (greater than 0)
        /// </summary>
        public virtual bool HasValidTimestamp => created > 0;
        
        /// <summary>
        /// Gets a human-readable timestamp string
        /// </summary>
        public virtual string GetTimestampString()
        {
            if (created <= 0) return "Unknown";
            
            try
            {
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(created).DateTime;
                return dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            }
            catch
            {
                return "Invalid";
            }
        }
        
        /// <summary>
        /// Validates the basic properties of the OpenAI object
        /// </summary>
        /// <returns>True if the object is valid</returns>
        public virtual bool IsValid()
        {
            return HasValidModel && HasValidTimestamp;
        }
    }
}
