using System;

namespace BFM.OpenAI
{
    /// <summary>
    /// Represents individual image data from OpenAI's image generation API response
    /// </summary>
    [System.Serializable]
    public class OpenAIImageData : BaseOpenAI
    {
        /// <summary>
        /// URL to the generated image (if provided by OpenAI)
        /// </summary>
        public string url;
        
        /// <summary>
        /// Base64 encoded image data (if provided by OpenAI)
        /// </summary>
        public string b64_json;
        
        /// <summary>
        /// Revision number of the generated image
        /// </summary>
        public string revised_prompt;
        
        /// <summary>
        /// Checks if this image data has a valid URL
        /// </summary>
        public bool HasUrl => !string.IsNullOrEmpty(url);
        
        /// <summary>
        /// Checks if this image data has valid base64 data
        /// </summary>
        public bool HasBase64Data => !string.IsNullOrEmpty(b64_json);
        
        /// <summary>
        /// Checks if this image data is valid (has either URL or base64 data)
        /// </summary>
        public override bool IsValid()
        {
            return HasUrl || HasBase64Data;
        }
        
        /// <summary>
        /// Gets the size of the base64 data in bytes
        /// </summary>
        /// <returns>Size in bytes, or -1 if not available</returns>
        public int GetBase64Size()
        {
            if (!HasBase64Data) return -1;
            
            try
            {
                return System.Convert.FromBase64String(b64_json).Length;
            }
            catch
            {
                return -1;
            }
        }
        
        /// <summary>
        /// Gets a human-readable size string for the base64 data
        /// </summary>
        /// <returns>Size string (e.g., "1.2 MB") or "Unknown"</returns>
        public string GetSizeString()
        {
            int sizeBytes = GetBase64Size();
            if (sizeBytes <= 0) return "Unknown";
            
            if (sizeBytes < 1024)
                return $"{sizeBytes} B";
            else if (sizeBytes < 1024 * 1024)
                return $"{sizeBytes / 1024.0:F1} KB";
            else
                return $"{sizeBytes / (1024.0 * 1024.0):F1} MB";
        }
        
        /// <summary>
        /// Gets a summary of the image data for logging
        /// </summary>
        /// <returns>String representation of the image data</returns>
        public override string ToString()
        {
            string dataType = HasUrl ? "URL" : (HasBase64Data ? $"Base64 ({GetSizeString()})" : "None");
            return $"OpenAIImageData: {dataType}";
        }
    }
}
