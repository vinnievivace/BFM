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
        Freak
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
    /// Checks if either token has PB: prefix indicating interSpecies generation
    /// </summary>
    /// <returns>True if interSpecies generation should be used</returns>
    private bool IsInterSpeciesGeneration()
    {
        bool isInterSpecies = token1Text.text.StartsWith("PB:") || token2Text.text.StartsWith("PB:");
        
        if (isInterSpecies)
        {
            ActiveMode = Mode.Freak;
            UpdateButtonLabels();
        }
        
        return isInterSpecies;
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
    /// Checks current input values and updates mode accordingly
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
        
        bool hasPB = currentToken1.StartsWith("PB:") || currentToken2.StartsWith("PB:");
        
        if (hasPB && ActiveMode != Mode.Freak)
        {
            ActiveMode = Mode.Freak;
            UpdateButtonLabels();
        }
        else if (!hasPB && ActiveMode == Mode.Freak)
        {
            // Reset to default mode when no PB: prefix is found
            ActiveMode = Mode.Fluflet;
            UpdateButtonLabels();
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
        
        // Check if this is an interSpecies generation (PB: prefix)
        bool isInterSpecies = IsInterSpeciesGeneration();
        if (isInterSpecies)
        {
            Log("InterSpecies generation detected - using InterSpeciesPrompt");
            prompt = InterSpeciesPrompt;
            outputTXT.text = $"{ActiveMode}: {token1Text.text} x {token2Text.text}";
        }
        
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

        // Step 2: Get Parent2 metadata and image
        Log($"Fetching Parent2 (Token ID: {token2Text.text}) metadata...");
        string parent2ImageUrl = null;
        yield return StartCoroutine(GetTokenImageUrl(token2Text.text, (url) => parent2ImageUrl = url));
        
        if (string.IsNullOrEmpty(parent2ImageUrl))
        {
            LogError($"Failed to get Parent2 image URL for token {token2Text.text}!");
            yield break;
        }

        Log("Downloading Parent2 image...");
        Texture2D parent2Texture = null;
        yield return StartCoroutine(DownloadImageWithCache(parent2ImageUrl, token2Text.text, (texture) => parent2Texture = texture));
        
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
    /// Gets the result file path for a generated image
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
        
        string fileName = $"{ActiveMode}_{token1Text.text}x{token2Text.text}.png";
        return Path.Combine(folder, fileName);
    }
    
    /// <summary>
    /// Downloads an image from a URL and returns it as a Texture2D
    /// </summary>
    /// <param name="url">URL to download from</param>
    /// <param name="callback">Callback with the downloaded texture</param>
    private IEnumerator DownloadImage(string url, System.Action<Texture2D> callback)
    {
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
                callback(null);
            }
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
    /// Handles Fluflet button click - generates a Fluflet using FlufletPrompt
    /// </summary>
    private void OnFlufletButtonClicked()
    {
        ActiveMode = Mode.Fluflet;
        UpdateButtonLabels();
        outputTXT.text = $"{ActiveMode}: {token1Text.text} x {token2Text.text}";
        
        StartCoroutine(GenerateFlufletCoroutine(FlufletPrompt));
    }

    /// <summary>
    /// Handles Beast button click - generates a Beast using BeastPrompt
    /// </summary>
    private void OnBeastButtonClicked()
    {
        ActiveMode = Mode.Beast;
        UpdateButtonLabels();
        outputTXT.text = $"{ActiveMode}: {token1Text.text} x {token2Text.text}";
        
        StartCoroutine(GenerateFlufletCoroutine(BeastPrompt));
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