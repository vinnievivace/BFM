using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.IO;
using EmberAI;
using EmberAI.Attributes;
using EmberAI.Core.Util;

public class BunnyFUKManager : EmberBehaviour
{
    #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

    #endregion

    #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

    public enum Mode
    {
        Fluflet,
        Beast,
        Freak,
        Morph
    }

    #endregion

    #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

    private string endpoint = "https://api.openai.com/v1/images/edits";
    private const string flufAPIURL = "https://api.fluf.world/api/token/";
    private const string partyBearAPIURL = "https://api.partybear.xyz/api/token/";
    
    [BoxGroup("OpenAI"), PlayerPref]
    public string ChatGPT_API_Key = "";
    
    [BoxGroup("Generation Prompt"), TextArea(3, 5)]
    public string FlufletPrompt = "A baby Fluflet rabbit that combines traits from these two parent Fluf rabbits. " +
                                     "Create a cute, cartoon-like style baby that inherits characteristics from both parents.";
    
    [BoxGroup("Generation Prompt"), TextArea(3, 5)]
    public string BeastPrompt = "A baby Fluflet rabbit that combines traits from these two parent Fluf rabbits. " +
                                  "Create a cute, cartoon-like style baby that inherits characteristics from both parents.";

    [BoxGroup("Generation Prompt"), TextArea(3, 5)]
    public string InterSpeciesPrompt = "A baby Fluflet rabbit that combines traits from these two parent creatures - one Fluf rabbit and one PartyBear. " +
                                      "Create a cute, cartoon-like style baby that inherits characteristics from both species, blending rabbit and bear features harmoniously.";
    
    [BoxGroup("Generation Prompt"), TextArea(3, 5)]
    public string MorphPrompt = "A baby Fluflet rabbit that combines traits from this Fluf parent and the provided reference image. " +
                               "Create a cute, cartoon-like style baby that inherits characteristics from both the Fluf parent and the reference image, blending their features harmoniously. " +
                               "Note: The reference image must be in PNG, JPG, JPEG, or WEBP format for best results.";
    
    [BoxGroup("UI Components")]
    public Image Fluf1Image, Fluf2Image, FlufletImage;
    
    [BoxGroup("UI Components")]
    public TMP_InputField token1Text, token2Text;

    [BoxGroup("UI Components")] 
    public TMP_Text statusTXT, outputTXT;

    [BoxGroup("UI Components")] public Button flufletButton, beastButton, randomButton;
    
    [BoxGroup("Mode")]
    public Mode ActiveMode = Mode.Fluflet;
    
    [BoxGroup("Cache")]
    private string previousToken1 = "";
    private string previousToken2 = "";
    
    #endregion

    #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

    #endregion

    #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

    #region Static .................................................................................................

    #endregion

    #region Inspector ..............................................................................................

    #endregion

    #region Initialization .........................................................................................

    #endregion

    #region MonoBehaviours .........................................................................................

    protected override void OnStart()
    {
        base.OnStart();
        
        Application.runInBackground = true;
    
        if (flufletButton != null) flufletButton.onClick.AddListener(OnFlufletButtonClicked);
        if(beastButton != null) beastButton.onClick.AddListener(OnBeastButtonClicked);
        if(randomButton != null) randomButton.onClick.AddListener(RandomizeTokens);
        
        // Add input field listeners for real-time mode detection
        if (token1Text != null) token1Text.onValueChanged.AddListener(OnTokenInputChanged);
        if (token2Text != null) token2Text.onValueChanged.AddListener(OnTokenInputChanged);
        
        RandomizeTokens();
        
    }

    #endregion

    #region General ................................................................................................
    
    /// <summary>
    /// Randomizes both token input fields with random numbers
    /// </summary>
    private void RandomizeTokens()
    {
        if (token1Text != null) token1Text.text = MathUtil.GetRandomNumber(0,9999).ToString();
        if (token2Text != null) token2Text.text = MathUtil.GetRandomNumber(0,9999).ToString();
    }
    
    /// <summary>
    /// Centralized mode detection - determines the current mode based on input values
    /// </summary>
    /// <returns>The detected mode</returns>
    private Mode DetectCurrentMode()
    {
        string token1 = token1Text != null ? token1Text.text : "";
        string token2 = token2Text != null ? token2Text.text : "";
        
        // Check for InterSpecies (PartyBear) mode first
        if (token1.StartsWith("PB:") || token2.StartsWith("PB:"))
        {
            return Mode.Freak;
        }
        
        // Check for Morph mode (standard token + URL)
        if (!token1.StartsWith("PB:") && 
            !token1.StartsWith("http") && 
            !token1.StartsWith("ipfs") &&
            (token2.StartsWith("http") || token2.StartsWith("ipfs")))
        {
            return Mode.Morph;
        }
        
        // Default to Fluflet mode
        return Mode.Fluflet;
    }
    
    /// <summary>
    /// Gets the appropriate prompt for the current mode
    /// </summary>
    /// <param name="mode">The mode to get prompt for</param>
    /// <returns>The prompt string</returns>
    private string GetPromptForMode(Mode mode)
    {
        switch (mode)
        {
            case Mode.Fluflet:
                return FlufletPrompt;
            case Mode.Beast:
                return BeastPrompt;
            case Mode.Freak:
                return InterSpeciesPrompt;
            case Mode.Morph:
                return MorphPrompt;
            default:
                return FlufletPrompt;
        }
    }
    
    /// <summary>
    /// Gets the display name for output text based on mode and inputs
    /// </summary>
    /// <param name="mode">The current mode</param>
    /// <returns>Formatted display string</returns>
    private string GetOutputDisplayText(Mode mode)
    {
        string token1 = token1Text != null ? token1Text.text : "";
        string token2 = token2Text != null ? token2Text.text : "";
        
        switch (mode)
        {
            case Mode.Morph:
                string displayName = GetFilenameFromUrl(token2);
                string result = $"{mode}: {token1} x {displayName}";
                Log($"GetOutputDisplayText - token2: {token2}, displayName: {displayName}, result: {result}");
                return result;
            default:
                return $"{mode}: {token1} x {token2}";
        }
    }
    
    /// <summary>
    /// Gets the filename for saving based on mode and inputs
    /// </summary>
    /// <param name="mode">The current mode</param>
    /// <returns>Filename for saving (without extension)</returns>
    private string GetSaveFilename(Mode mode)
    {
        string token1 = token1Text != null ? token1Text.text : "";
        string token2 = token2Text != null ? token2Text.text : "";
        
        switch (mode)
        {
            case Mode.Morph:
                string filename = GetSafeFilenameFromUrl(token2);
                return $"{token1}x{filename}";
            default:
                return $"{mode}_{token1}x{token2}";
        }
    }
    
    /// <summary>
    /// Gets a safe filename from URL that can be used in file system (max 15 chars)
    /// </summary>
    /// <param name="url">URL to extract safe filename from</param>
    /// <returns>Safe filename for file system use</returns>
    private string GetSafeFilenameFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return "unknown";
            
        try
        {
            // Try to extract filename from URL
            int lastSlash = url.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < url.Length - 1)
            {
                string filename = url.Substring(lastSlash + 1);
                
                // Remove query parameters if present
                int questionMark = filename.IndexOf('?');
                if (questionMark > 0)
                {
                    filename = filename.Substring(0, questionMark);
                }
                
                // Remove file extension
                int dotIndex = filename.LastIndexOf('.');
                if (dotIndex > 0)
                {
                    filename = filename.Substring(0, dotIndex);
                }
                
                // Sanitize filename for file system use
                filename = SanitizeFilename(filename);
                
                // Truncate to 15 characters max
                if (filename.Length > 15)
                {
                    filename = filename.Substring(0, 15);
                }
                
                return filename;
            }
            
            // Fallback: use domain name
            if (url.Contains("://"))
            {
                int domainStart = url.IndexOf("://") + 3;
                int domainEnd = url.IndexOf('/', domainStart);
                if (domainEnd > domainStart)
                {
                    string domain = url.Substring(domainStart, domainEnd - domainStart);
                    domain = SanitizeFilename(domain);
                    return domain.Length > 15 ? domain.Substring(0, 15) : domain;
                }
            }
            
            // Ultimate fallback
            return "morph";
        }
        catch
        {
            // Ultimate fallback
            return "morph";
        }
    }
    
    /// <summary>
    /// Sanitizes a string to be safe for use as a filename
    /// </summary>
    /// <param name="filename">Filename to sanitize</param>
    /// <returns>Sanitized filename</returns>
    private string SanitizeFilename(string filename)
    {
        if (string.IsNullOrEmpty(filename))
            return "unknown";
            
        // Replace invalid characters with underscores
        char[] invalidChars = Path.GetInvalidFileNameChars();
        foreach (char c in invalidChars)
        {
            filename = filename.Replace(c, '_');
        }
        
        // Replace common URL characters that cause issues
        filename = filename.Replace(":", "_")
                          .Replace("/", "_")
                          .Replace("\\", "_")
                          .Replace("?", "_")
                          .Replace("*", "_")
                          .Replace("<", "_")
                          .Replace(">", "_")
                          .Replace("|", "_");
        
        // Remove multiple consecutive underscores
        while (filename.Contains("__"))
        {
            filename = filename.Replace("__", "_");
        }
        
        // Remove leading/trailing underscores
        filename = filename.Trim('_');
        
        // Ensure it's not empty after sanitization
        if (string.IsNullOrEmpty(filename))
            return "unknown";
            
        return filename;
    }
    
    
    /// <summary>
    /// Updates button labels based on the current ActiveMode
    /// </summary>
    private void UpdateButtonLabels()
    {
        if (ActiveMode == Mode.Freak)
        {
            if (flufletButton != null)
            {
                var flufletText = flufletButton.GetComponentInChildren<TMP_Text>();
                if (flufletText != null) flufletText.text = "FREAK";
            }
            if (beastButton != null)
            {
                var beastText = beastButton.GetComponentInChildren<TMP_Text>();
                if (beastText != null) beastText.text = "FREAK";
            }
        }
        else if (ActiveMode == Mode.Morph)
        {
            if (flufletButton != null)
            {
                var flufletText = flufletButton.GetComponentInChildren<TMP_Text>();
                if (flufletText != null) flufletText.text = "MORPH";
            }
            if (beastButton != null)
            {
                var beastText = beastButton.GetComponentInChildren<TMP_Text>();
                if (beastText != null) beastText.text = "MORPH";
            }
        }
        else
        {
            if (flufletButton != null)
            {
                var flufletText = flufletButton.GetComponentInChildren<TMP_Text>();
                if (flufletText != null) flufletText.text = "Fluflet";
            }
            if (beastButton != null)
            {
                var beastText = beastButton.GetComponentInChildren<TMP_Text>();
                if (beastText != null) beastText.text = "Beast";
            }
        }
    }
    
    /// <summary>
    /// Clears the parent images from the UI
    /// </summary>
    private void ClearParentImages()
    {
        if (Fluf1Image != null)
        {
            Fluf1Image.sprite = null;
            Fluf1Image.enabled = false;
        }
        
        if (Fluf2Image != null)
        {
            Fluf2Image.sprite = null;
            Fluf2Image.enabled = false;
        }
    }
    
    /// <summary>
    /// Applies Morph mode scaling to Fluf2Image (scaled down to 0.5)
    /// </summary>
    private void ApplyMorphImageStyling()
    {
        if (Fluf2Image != null)
        {
            // Scale down to 0.5
            Fluf2Image.transform.localScale = Vector3.one * 0.5f;
        }
    }
    
    /// <summary>
    /// Resets Fluf2Image scaling to default (normal scale 1.0)
    /// </summary>
    private void ResetImageStyling()
    {
        if (Fluf2Image != null)
        {
            // Reset to normal scale
            Fluf2Image.transform.localScale = Vector3.one;
        }
    }
    
    /// <summary>
    /// Checks current input values and updates mode accordingly using centralized detection
    /// </summary>
    private void CheckAndUpdateMode()
    {
        string currentToken1 = token1Text != null ? token1Text.text : "";
        string currentToken2 = token2Text != null ? token2Text.text : "";
        
        // Check if token IDs have changed
        bool token1Changed = currentToken1 != previousToken1;
        bool token2Changed = currentToken2 != previousToken2;
        
        if (token1Changed || token2Changed)
        {
            ClearParentImages();
            previousToken1 = currentToken1;
            previousToken2 = currentToken2;
        }
        
        // Use centralized mode detection
        Mode detectedMode = DetectCurrentMode();
        
        // Only update if mode has changed
        if (detectedMode != ActiveMode)
        {
            // Reset image styling when switching away from Morph
            if (ActiveMode == Mode.Morph)
            {
                ResetImageStyling();
            }
            
            ActiveMode = detectedMode;
            UpdateButtonLabels();
            
            // Apply Morph styling if switching to Morph mode
            if (ActiveMode == Mode.Morph)
            {
                ApplyMorphImageStyling();
            }
        }
    }
    
    /// <summary>
    /// Gets the display name for the current mode
    /// </summary>
    /// <returns>Display name for the mode</returns>
    private string GetModeDisplayName()
    {
        switch (ActiveMode)
        {
            case Mode.Fluflet:
            case Mode.Beast:
                return "Flufs";
            case Mode.Freak:
                return "Freaks";
            case Mode.Morph:
                return "Morphs";
            default:
                return "Flufs";
        }
    }
    
    /// <summary>
    /// Loads a texture into a UI Image component
    /// </summary>
    /// <param name="texture">The texture to load</param>
    /// <param name="imageComponent">The UI Image component to load into</param>
    /// <param name="logMessage">Message to log when successful</param>
    private void LoadTextureIntoUI(Texture2D texture, Image imageComponent, string logMessage)
    {
        if (imageComponent != null && texture != null)
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            imageComponent.sprite = sprite;
            imageComponent.enabled = true;
            Log(logMessage);
        }
    }
    
    private IEnumerator GenerateFlufletCoroutine(string prompt)
    {
        Log($"Starting {ActiveMode} generation process...");
        
        // Clear the previous Fluflet image
        if (FlufletImage != null)
        {
            FlufletImage.sprite = null;
            FlufletImage.enabled = false;
        }
        
        // Use the centralized mode detection and prompt system
        Mode detectedMode = DetectCurrentMode();
        ActiveMode = detectedMode;
        prompt = GetPromptForMode(ActiveMode);
        
        Log($"{ActiveMode} generation detected - using {ActiveMode} prompt");
        // Output text already set by button handler
        
        // Step 1: Get Parent1 metadata and image
        Log($"Fetching Parent1 (Token ID: {token1Text.text}) metadata...");
        string parent1ImageUrl = null;
        yield return StartCoroutine(GetTokenImageUrl(token1Text.text, (url) => parent1ImageUrl = url));
        
        if (string.IsNullOrEmpty(parent1ImageUrl))
        {
            LogError($"Failed to get Parent1 image URL for token {token1Text.text}!");
            yield break;
        }

        Log("Downloading Parent1 image...");
        Texture2D parent1Texture = null;
        yield return StartCoroutine(DownloadImageWithCache(parent1ImageUrl, token1Text.text, (texture) => parent1Texture = texture));
        
        if (parent1Texture == null)
        {
            LogError("Failed to download Parent1 image!");
            yield break;
        }

        // Load Parent1 image into UI Image component
        LoadTextureIntoUI(parent1Texture, Fluf1Image, "Parent1 image loaded into UI Image component");

        // Step 2: Get Parent2 image (different logic for Morph mode)
        Texture2D parent2Texture = null;
        
        if (ActiveMode == Mode.Morph)
        {
            // For Morph mode, token2Text is already a URL
            Log($"Downloading Parent2 image from URL: {token2Text.text}");
            
            // Check format before attempting download
            if (!IsSupportedImageFormat(token2Text.text))
            {
                LogError($"Morph mode requires PNG, JPG, JPEG, or WEBP images. The provided URL appears to be a GIF or unsupported format: {token2Text.text}");
                yield break;
            }
            
            yield return StartCoroutine(DownloadImageWithMorphCache(token2Text.text, (texture) => parent2Texture = texture));
        }
        else
        {
            // For other modes, fetch metadata first
            Log($"Fetching Parent2 (Token ID: {token2Text.text}) metadata...");
            string parent2ImageUrl = null;
            yield return StartCoroutine(GetTokenImageUrl(token2Text.text, (url) => parent2ImageUrl = url));
            
            if (string.IsNullOrEmpty(parent2ImageUrl))
            {
                LogError($"Failed to get Parent2 image URL for token {token2Text.text}!");
                yield break;
            }

            Log("Downloading Parent2 image...");
            yield return StartCoroutine(DownloadImageWithCache(parent2ImageUrl, token2Text.text, (texture) => parent2Texture = texture));
        }
        
        if (parent2Texture == null)
        {
            LogError("Failed to download Parent2 image!");
            yield break;
        }

        // Load Parent2 image into UI Image component
        LoadTextureIntoUI(parent2Texture, Fluf2Image, "Parent2 image loaded into UI Image component");

        // Step 3: Generate Fluflet using OpenAI images/edits endpoint
        Log($"Generating {ActiveMode} image with OpenAI images/edits...");
        yield return StartCoroutine(GenerateImageWithOpenAI(parent1Texture, parent2Texture, prompt));

        Log($"{ActiveMode} generation process completed!");
    }
    
    #endregion
    
    #region API Methods ...........................................................................................
    
    /// <summary>
    /// Generic method to get image URL for any token type (Fluf or PartyBear)
    /// </summary>
    /// <param name="tokenID">Token ID with optional PB: prefix</param>
    /// <param name="callback">Callback with the image URL</param>
    private IEnumerator GetTokenImageUrl(string tokenID, System.Action<string> callback)
    {
        // Determine API URL based on token prefix
        string apiUrl;
        string apiName;
        
        if (tokenID.StartsWith("PB:"))
        {
            string cleanTokenID = tokenID.Replace("PB:", "");
            apiUrl = $"{partyBearAPIURL}{cleanTokenID}";
            apiName = "PartyBear";
        }
        else
        {
            apiUrl = $"{flufAPIURL}{tokenID}";
            apiName = "Fluf";
        }
        
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Log($"{apiName} API Response: {responseText}");
                
                // Parse JSON response based on API type
                string imageUrl = null;
                
                if (tokenID.StartsWith("PB:"))
                {
                    var response = JsonUtility.FromJson<PartyBearApiResponse>(responseText);
                    if (response != null)
                    {
                        imageUrl = response.GetPrimaryImageUrl();
                    }
                }
                else
                {
                    var response = JsonUtility.FromJson<FlufApiResponse>(responseText);
                    if (response != null)
                    {
                        imageUrl = response.GetPrimaryImageUrl();
                    }
                }
                
                if (!string.IsNullOrEmpty(imageUrl))
                {
                    string httpUrl = ConvertIPFSUrlToHttp(imageUrl);
                    Log($"Converted IPFS URL to HTTP: {httpUrl}");
                    callback(httpUrl);
                }
                else
                {
                    LogError($"Failed to parse image from {apiName} API response");
                    callback(null);
                }
            }
            else
            {
                LogError($"Failed to fetch {apiName} metadata: {request.error}");
                callback(null);
            }
        }
    }
    
    #endregion
    
    #region Utility Methods ........................................................................................
    
    /// <summary>
    /// Converts IPFS URLs to HTTP URLs using Pinata gateway
    /// </summary>
    /// <param name="ipfsUrl">IPFS URL to convert</param>
    /// <returns>HTTP URL</returns>
    private string ConvertIPFSUrlToHttp(string ipfsUrl)
    {
        if (ipfsUrl.StartsWith("ipfs://"))
        {
            string hash = ipfsUrl.Replace("ipfs://", "");
            return $"https://gateway.pinata.cloud/ipfs/{hash}";
        }
        return ipfsUrl;
    }
    
    /// <summary>
    /// Gets the cache file path for a token image
    /// </summary>
    /// <param name="tokenID">Token ID with optional PB: prefix</param>
    /// <returns>Cache file path</returns>
    private string GetCacheFilePath(string tokenID)
    {
        string folder;
        string fileName;
        
        if (tokenID.StartsWith("PB:"))
        {
            string cleanTokenID = tokenID.Replace("PB:", "");
            folder = Path.Combine(Application.streamingAssetsPath, "PartyBear");
            fileName = $"PartyBear_{cleanTokenID}_transparent.png";
        }
        else
        {
            folder = Path.Combine(Application.streamingAssetsPath, "Fluf");
            fileName = $"Fluf_{tokenID}_transparent.png";
        }
        
        // Ensure directory exists
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        return Path.Combine(folder, fileName);
    }
    
    /// <summary>
    /// Gets the result file path for a generated image using centralized naming
    /// </summary>
    /// <returns>Result file path</returns>
    private string GetResultFilePath()
    {
        string folder = Path.Combine(Application.streamingAssetsPath, ActiveMode.ToString());
        
        // Ensure directory exists
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        string fileName = GetSaveFilename(ActiveMode) + ".png";
        return Path.Combine(folder, fileName);
    }
    
    /// <summary>
    /// Downloads an image from a URL and returns it as a Texture2D
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="callback">Callback with the downloaded texture</param>
    private IEnumerator DownloadImage(string url, System.Action<Texture2D> callback)
    {
        // Check if the URL points to a supported image format
        if (!IsSupportedImageFormat(url))
        {
            LogError($"Unsupported image format for URL: {url}. Supported formats: PNG, JPG, JPEG, WEBP");
            callback(null);
            yield break;
        }
        
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                Log($"Successfully downloaded image: {url}");
                callback(texture);
            }
            else
            {
                LogError($"Failed to download image from {url}: {request.error}");
                if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    LogError($"Download Handler Error: {request.downloadHandler.text}");
                }
                callback(null);
            }
        }
    }
    
    /// <summary>
    /// Checks if the URL points to a supported image format
    /// </summary>
    /// <param name="url">URL to check</param>
    /// <returns>True if the format is supported</returns>
    private bool IsSupportedImageFormat(string url)
    {
        string lowerUrl = url.ToLower();
        
        // Check for explicit file extensions
        if (lowerUrl.EndsWith(".png") || 
            lowerUrl.EndsWith(".jpg") || 
            lowerUrl.EndsWith(".jpeg") || 
            lowerUrl.EndsWith(".webp"))
        {
            return true;
        }
        
        // Check for URLs with query parameters
        if (lowerUrl.Contains(".png?") ||
            lowerUrl.Contains(".jpg?") ||
            lowerUrl.Contains(".jpeg?") ||
            lowerUrl.Contains(".webp?"))
        {
            return true;
        }
        
        // Check for common image service patterns that don't have explicit extensions
        // YouTube thumbnails, Imgur, etc.
        if (lowerUrl.Contains("ytimg.com") ||           // YouTube thumbnails
            lowerUrl.Contains("imgur.com") ||           // Imgur
            lowerUrl.Contains("i.redd.it") ||           // Reddit images
            lowerUrl.Contains("cdn.discordapp.com") ||  // Discord
            lowerUrl.Contains("media.tenor.com") ||     // Tenor GIFs (though we don't support GIF)
            lowerUrl.Contains("pinata.cloud") ||        // IPFS gateways
            lowerUrl.Contains("cloudinary.com") ||      // Cloudinary
            lowerUrl.Contains("amazonaws.com"))         // AWS S3
        {
            return true;
        }
        
        // Check for common image filename patterns
        if (lowerUrl.Contains("image") || 
            lowerUrl.Contains("img") || 
            lowerUrl.Contains("photo") ||
            lowerUrl.Contains("picture") ||
            lowerUrl.Contains("avatar") ||
            lowerUrl.Contains("thumbnail"))
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Extracts the filename from a URL for cleaner logging (truncated to 15 chars)
    /// </summary>
    /// <param name="url">URL to extract filename from</param>
    /// <returns>Short filename for display</returns>
    private string GetFilenameFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return "Unknown";
            
        try
        {
            // Try to extract filename from URL
            int lastSlash = url.LastIndexOf('/');
            if (lastSlash >= 0 && lastSlash < url.Length - 1)
            {
                string filename = url.Substring(lastSlash + 1);
                
                // Remove query parameters if present
                int questionMark = filename.IndexOf('?');
                if (questionMark > 0)
                {
                    filename = filename.Substring(0, questionMark);
                }
                
                // Remove file extension for display purposes
                int dotIndex = filename.LastIndexOf('.');
                if (dotIndex > 0)
                {
                    filename = filename.Substring(0, dotIndex);
                }
                
                // Truncate to 15 characters max
                if (filename.Length > 15)
                {
                    filename = filename.Substring(0, 15);
                }
                
                return filename;
            }
            
            // Fallback: use domain name or truncate URL
            if (url.Contains("://"))
            {
                int domainStart = url.IndexOf("://") + 3;
                int domainEnd = url.IndexOf('/', domainStart);
                if (domainEnd > domainStart)
                {
                    string domain = url.Substring(domainStart, domainEnd - domainStart);
                    return domain.Length > 15 ? domain.Substring(0, 15) : domain;
                }
            }
            
            // Ultimate fallback: truncate URL
            return url.Length > 15 ? url.Substring(0, 15) : url;
        }
        catch
        {
            // Ultimate fallback
            return "image";
        }
    }
    
    /// <summary>
    /// Downloads an image with caching support
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="tokenID">Token ID for cache naming</param>
    /// <param name="callback">Callback with the downloaded texture</param>
    private IEnumerator DownloadImageWithCache(string url, string tokenID, System.Action<Texture2D> callback)
    {
        string cachePath = GetCacheFilePath(tokenID);
        
        // Check if image is already cached
        if (File.Exists(cachePath))
        {
            yield return StartCoroutine(LoadImageFromFile(cachePath, callback));
            yield break;
        }
        
        // Download and cache the image
        yield return StartCoroutine(DownloadImage(url, (texture) =>
        {
            if (texture != null)
            {
                // Save to cache
                byte[] pngData = texture.EncodeToPNG();
                File.WriteAllBytes(cachePath, pngData);
                callback(texture);
            }
            else
            {
                callback(null);
            }
        }));
    }
    
    /// <summary>
    /// Downloads an image for Morph mode without caching the source (only used for generation)
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="callback">Callback with the downloaded texture</param>
    private IEnumerator DownloadImageWithMorphCache(string url, System.Action<Texture2D> callback)
    {
        // Convert IPFS URL to HTTP if needed
        string httpUrl = ConvertIPFSUrlToHttp(url);
        
        // Download the image directly without caching (source images aren't saved for Morph mode)
        yield return StartCoroutine(DownloadImage(httpUrl, callback));
    }
    
    /// <summary>
    /// Loads an image from a local file
    /// </summary>
    /// <param name="filePath">Path to the image file</param>
    /// <param name="callback">Callback with the loaded texture</param>
    private IEnumerator LoadImageFromFile(string filePath, System.Action<Texture2D> callback)
    {
        try
        {
            byte[] imageData = File.ReadAllBytes(filePath);
            Texture2D texture = new Texture2D(2, 2);
            
            if (texture.LoadImage(imageData))
            {
                callback(texture);
            }
            else
            {
                LogError($"Failed to load image from file: {filePath}");
                callback(null);
            }
        }
        catch (System.Exception e)
        {
            LogError($"Error loading image from file {filePath}: {e.Message}");
            callback(null);
        }
        
        yield return null;
    }
    
    #endregion
    
    #region OpenAI Generation ......................................................................................
    
    /// <summary>
    /// Generates a new image using OpenAI's images/edits endpoint with two parent images
    /// </summary>
    /// <param name="parent1Texture">First parent image texture</param>
    /// <param name="parent2Texture">Second parent image texture</param>
    /// <param name="prompt">Generation prompt</param>
    private IEnumerator GenerateImageWithOpenAI(Texture2D parent1Texture, Texture2D parent2Texture, string prompt)
    {
        Log("Creating OpenAI images/edits request...");
        
        // Convert textures to PNG bytes
        byte[] parent1Bytes = parent1Texture.EncodeToPNG();
        byte[] parent2Bytes = parent2Texture.EncodeToPNG();
        
        // Create WWWForm for multipart form data
        WWWForm form = new WWWForm();
        form.AddField("model", "gpt-image-1");
        form.AddField("prompt", prompt);
        form.AddField("size", "1024x1024");
        form.AddField("n", "1");
        form.AddField("background", "transparent");
        form.AddField("output_format", "png");
        
        // Add both parent images as binary data using array syntax
        form.AddBinaryData("image[]", parent1Bytes, "parent1.png", "image/png");
        form.AddBinaryData("image[]", parent2Bytes, "parent2.png", "image/png");
        
        Log($"One moment while the {GetModeDisplayName()} do their thing...");
        
        using (UnityWebRequest request = UnityWebRequest.Post(endpoint, form))
        {
            request.SetRequestHeader("Authorization", $"Bearer {ChatGPT_API_Key}");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Log($"OpenAI Response: {responseText}");
                
                // Parse response to get image data
                var response = JsonUtility.FromJson<OpenAIResponse>(responseText);
                if (response != null && response.data != null && response.data.Length > 0)
                {
                    var imageData = response.data[0];
                    
                    if (!string.IsNullOrEmpty(imageData.url))
                    {
                        // If we get a URL, download it
                        Log($"Generated image URL: {imageData.url}");
                        yield return StartCoroutine(DownloadGeneratedImage(imageData.url));
                    }
                    else if (!string.IsNullOrEmpty(imageData.b64_json))
                    {
                        // If we get base64 data, decode it directly
                        Log("Generated image as base64 data");
                        yield return StartCoroutine(LoadBase64Image(imageData.b64_json));
                    }
                    else
                    {
                        LogError("No image URL or base64 data found in response");
                    }
                }
                else
                {
                    LogError("Failed to parse OpenAI response or no image data found");
                }
            }
            else
            {
                LogError($"OpenAI API Error: {request.error}");
                LogError($"Response: {request.downloadHandler.text}");
            }
        }
    }
    
    /// <summary>
    /// Downloads a generated image from OpenAI's URL
    /// </summary>
    /// <param name="imageUrl">URL of the generated image</param>
    private IEnumerator DownloadGeneratedImage(string imageUrl)
    {
        Log($"Downloading generated image from: {imageUrl}");
        
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                Texture2D generatedTexture = DownloadHandlerTexture.GetContent(request);
                LoadGeneratedImageIntoUI(generatedTexture);
            }
            else
            {
                LogError($"Failed to download generated image: {request.error}");
            }
        }
    }
    
    /// <summary>
    /// Loads a base64 encoded image into a texture
    /// </summary>
    /// <param name="base64Data">Base64 encoded image data</param>
    private IEnumerator LoadBase64Image(string base64Data)
    {
        Log("Loading base64 image data...");
        
        try
        {
            // Remove data URL prefix if present
            if (base64Data.StartsWith("data:image/"))
            {
                int commaIndex = base64Data.IndexOf(',');
                if (commaIndex > 0)
                {
                    base64Data = base64Data.Substring(commaIndex + 1);
                }
            }
            
            // Decode base64 to bytes
            byte[] imageBytes = System.Convert.FromBase64String(base64Data);
            
            // Create texture from bytes
            Texture2D generatedTexture = new Texture2D(2, 2);
            if (generatedTexture.LoadImage(imageBytes))
            {
                LoadGeneratedImageIntoUI(generatedTexture);
            }
            else
            {
                LogError("Failed to load base64 image data into texture");
            }
        }
        catch (System.Exception e)
        {
            LogError($"Error processing base64 image data: {e.Message}");
        }
        
        yield return null;
    }
    
    /// <summary>
    /// Loads a generated texture into the UI and saves it to file
    /// </summary>
    /// <param name="generatedTexture">The generated texture to load</param>
    private void LoadGeneratedImageIntoUI(Texture2D generatedTexture)
    {
        // Load into UI Image component
        if (FlufletImage != null)
        {
            Sprite generatedSprite = Sprite.Create(generatedTexture, new Rect(0, 0, generatedTexture.width, generatedTexture.height), new Vector2(0.5f, 0.5f));
            FlufletImage.sprite = generatedSprite;
            FlufletImage.enabled = true;
            Log($"{ActiveMode} Generation Complete.");
        }
        
        // Save to file with new naming convention
        byte[] pngData = generatedTexture.EncodeToPNG();
        string savePath = GetResultFilePath();
        File.WriteAllBytes(savePath, pngData);
    }
    
    #endregion
    
    #region Logging .................................................................................................
    
    /// <summary>
    /// Logs a message to both Debug.Log and UI status text
    /// </summary>
    /// <param name="message">Message to log</param>
    private void Log(string message)
    {
        Debug.Log($"[POC] {message}");
        
        statusTXT.text = message;
    }
    
    /// <summary>
    /// Logs an error message to both Debug.LogError and UI status text
    /// </summary>
    /// <param name="message">Error message to log</param>
    private void LogError(string message)
    {
        Debug.LogError($"[POC] {message}");
        
        statusTXT.text = "ERROR: " + message;
    }
    
    #endregion

    #region Event Handlers .........................................................................................

    /// <summary>
    /// Handles input field value changes for real-time mode detection
    /// </summary>
    /// <param name="value">The new input value</param>
    private void OnTokenInputChanged(string value)
    {
        CheckAndUpdateMode();
    }

    /// <summary>
    /// Handles Fluflet button click - generates using appropriate mode and prompt
    /// </summary>
    private void OnFlufletButtonClicked()
    {
        Mode detectedMode = DetectCurrentMode();
        
        // Override detected mode if it's Beast mode (button-specific behavior)
        if (detectedMode == Mode.Fluflet)
        {
            ActiveMode = Mode.Fluflet;
        }
        else
        {
            ActiveMode = detectedMode;
        }
        
        UpdateButtonLabels();
        string outputText = GetOutputDisplayText(ActiveMode);
        outputTXT.text = outputText;
        Log($"Fluflet button handler set outputTXT to: {outputText}");
        
        string prompt = GetPromptForMode(ActiveMode);
        StartCoroutine(GenerateFlufletCoroutine(prompt));
    }

    /// <summary>
    /// Handles Beast button click - generates using appropriate mode and prompt
    /// </summary>
    private void OnBeastButtonClicked()
    {
        Mode detectedMode = DetectCurrentMode();
        
        // Override detected mode if it's Fluflet mode (button-specific behavior)
        if (detectedMode == Mode.Fluflet)
        {
            ActiveMode = Mode.Beast;
        }
        else
        {
            ActiveMode = detectedMode;
        }
        
        UpdateButtonLabels();
        outputTXT.text = GetOutputDisplayText(ActiveMode);
        
        string prompt = GetPromptForMode(ActiveMode);
        StartCoroutine(GenerateFlufletCoroutine(prompt));
    }
    
    #endregion

    #endregion
}

[System.Serializable]
public class OpenAIResponse
{
    public OpenAIImageData[] data;
}

[System.Serializable]
public class OpenAIImageData
{
    public string url;
    public string b64_json;
}