using System;

namespace BFM.OpenAI
{
    /// <summary>
    /// Represents the response from OpenAI's image generation API
    /// </summary>
    [System.Serializable]
    public class OpenAIResponse : BaseOpenAI
    {
        /// <summary>
        /// Array of generated image data
        /// </summary>
        public OpenAIImageData[] data;
        
        /// <summary>
        /// Checks if the response has valid image data
        /// </summary>
        public bool HasValidData => data != null && data.Length > 0;
        
        /// <summary>
        /// Gets the number of images in the response
        /// </summary>
        public int ImageCount => data?.Length ?? 0;
        
        /// <summary>
        /// Gets the first image data if available
        /// </summary>
        /// <returns>First OpenAIImageData or null if none available</returns>
        public OpenAIImageData GetFirstImage()
        {
            return HasValidData ? data[0] : null;
        }
        
        /// <summary>
        /// Gets all valid image data
        /// </summary>
        /// <returns>Array of valid OpenAIImageData objects</returns>
        public OpenAIImageData[] GetValidImages()
        {
            if (!HasValidData) return new OpenAIImageData[0];
            
            var validImages = new System.Collections.Generic.List<OpenAIImageData>();
            foreach (var image in data)
            {
                if (image != null && image.IsValid())
                {
                    validImages.Add(image);
                }
            }
            
            return validImages.ToArray();
        }
        
        /// <summary>
        /// Validates the complete response
        /// </summary>
        /// <returns>True if the response is valid</returns>
        public override bool IsValid()
        {
            return base.IsValid() && HasValidData && GetValidImages().Length > 0;
        }
        
        /// <summary>
        /// Gets a summary of the response for logging
        /// </summary>
        /// <returns>String representation of the response</returns>
        public override string ToString()
        {
            return $"OpenAIResponse: {model}, {ImageCount} images, {GetTimestampString()}";
        }
    }
}
