using System;

namespace BFM.OpenAI
{
    /// <summary>
    /// Represents a request to OpenAI's image generation API
    /// </summary>
    [System.Serializable]
    public class OpenAIRequest : BaseOpenAI
    {
        /// <summary>
        /// The prompt describing the image to generate
        /// </summary>
        public string prompt;
        
        /// <summary>
        /// The size of the generated image (e.g., "1024x1024")
        /// </summary>
        public string size;
        
        /// <summary>
        /// Number of images to generate (1-10)
        /// </summary>
        public int n;
        
        /// <summary>
        /// Background style for the image (e.g., "transparent")
        /// </summary>
        public string background;
        
        /// <summary>
        /// Output format for the image (e.g., "png", "jpeg")
        /// </summary>
        public string output_format;
        
        /// <summary>
        /// Quality setting for the image generation
        /// </summary>
        public string quality;
        
        /// <summary>
        /// Style setting for the image generation
        /// </summary>
        public string style;
        
        /// <summary>
        /// User identifier for tracking purposes
        /// </summary>
        public string user;
        
        /// <summary>
        /// Default constructor with common values
        /// </summary>
        public OpenAIRequest()
        {
            model = "gpt-image-1";
            size = "1024x1024";
            n = 1;
            background = "transparent";
            output_format = "png";
            quality = "standard";
            style = "vivid";
        }
        
        /// <summary>
        /// Constructor with custom prompt
        /// </summary>
        /// <param name="prompt">The generation prompt</param>
        public OpenAIRequest(string prompt) : this()
        {
            this.prompt = prompt;
        }
        
        /// <summary>
        /// Checks if the prompt is valid
        /// </summary>
        public bool HasValidPrompt => !string.IsNullOrEmpty(prompt) && prompt.Length > 0;
        
        /// <summary>
        /// Checks if the size is valid
        /// </summary>
        public bool HasValidSize => !string.IsNullOrEmpty(size) && size.Contains("x");
        
        /// <summary>
        /// Checks if the number of images is valid
        /// </summary>
        public bool HasValidCount => n >= 1 && n <= 10;
        
        /// <summary>
        /// Checks if the output format is valid
        /// </summary>
        public bool HasValidOutputFormat => !string.IsNullOrEmpty(output_format);
        
        /// <summary>
        /// Gets the width from the size string
        /// </summary>
        /// <returns>Width in pixels, or -1 if invalid</returns>
        public int GetWidth()
        {
            if (!HasValidSize) return -1;
            
            try
            {
                var parts = size.Split('x');
                if (parts.Length >= 1 && int.TryParse(parts[0], out int width))
                    return width;
            }
            catch { }
            
            return -1;
        }
        
        /// <summary>
        /// Gets the height from the size string
        /// </summary>
        /// <returns>Height in pixels, or -1 if invalid</returns>
        public int GetHeight()
        {
            if (!HasValidSize) return -1;
            
            try
            {
                var parts = size.Split('x');
                if (parts.Length >= 2 && int.TryParse(parts[1], out int height))
                    return height;
            }
            catch { }
            
            return -1;
        }
        
        /// <summary>
        /// Validates the complete request
        /// </summary>
        /// <returns>True if the request is valid</returns>
        public override bool IsValid()
        {
            return base.IsValid() && 
                   HasValidPrompt && 
                   HasValidSize && 
                   HasValidCount && 
                   HasValidOutputFormat;
        }
        
        /// <summary>
        /// Gets a summary of the request for logging
        /// </summary>
        /// <returns>String representation of the request</returns>
        public override string ToString()
        {
            return $"OpenAIRequest: {model}, {size}, {n} images, {output_format}";
        }
    }
}
