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

    #endregion

    #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

    [BoxGroup("OpenAI"), PlayerPref]
    public string ChatGPT_API_Key = "";
    
    [Header("Generation Prompt")]
    [TextArea(3, 5)]
    public string GenerationPrompt = "A baby Fluflet rabbit that combines traits from these two parent Fluf rabbits. Create a cute, cartoon-like style baby that inherits characteristics from both parents.";
    
    [Header("UI Components")]
    public Image Fluf1Image;
    public Image Fluf2Image;
    public Image FlufletImage;
    public TMP_InputField token1Text;
    public TMP_InputField token2Text;
    public Button generateButton;
    
    private string endpoint = "https://api.openai.com/v1/images/edits";
    
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
    
        if (generateButton != null)
            generateButton.onClick.AddListener(OnGenerateButtonClicked);
        
        if (token1Text != null) token1Text.text = MathUtil.GetRandomNumber(0,9999).ToString();
        if (token2Text != null) token2Text.text = MathUtil.GetRandomNumber(0,9999).ToString();
        
    }

    #endregion

    #region General ................................................................................................

    #endregion

    #region Event Handlers .........................................................................................

    #endregion

    #endregion
    
    
    
    
    
    public void OnGenerateButtonClicked()
    {
        StartCoroutine(GenerateFlufletCoroutine());
    }
    
    private IEnumerator GenerateFlufletCoroutine()
    {
        Log("Starting Fluflet generation process...");

        // Step 1: Get Fluf1 metadata and image
        Log($"Fetching Fluf1 (Token ID: {token1Text.text}) metadata...");
        string fluf1ImageUrl = null;
        yield return StartCoroutine(GetFlufImageUrl(token1Text.text, (url) => fluf1ImageUrl = url));
        
        if (string.IsNullOrEmpty(fluf1ImageUrl))
        {
            LogError($"Failed to get Fluf1 image URL for token {token1Text.text}!");
            yield break;
        }

        Log("Downloading Fluf1 image...");
        Texture2D fluf1Texture = null;
        yield return StartCoroutine(DownloadImage(fluf1ImageUrl, (texture) => fluf1Texture = texture));
        
        if (fluf1Texture == null)
        {
            LogError("Failed to download Fluf1 image!");
            yield break;
        }

        // Load Fluf1 image into UI Image component
        if (Fluf1Image != null)
        {
            Sprite fluf1Sprite = Sprite.Create(fluf1Texture, new Rect(0, 0, fluf1Texture.width, fluf1Texture.height), new Vector2(0.5f, 0.5f));
            Fluf1Image.sprite = fluf1Sprite;
            Log("Fluf1 image loaded into UI Image component");
        }

        // Step 2: Get Fluf2 metadata and image
        Log($"Fetching Fluf2 (Token ID: {token2Text.text}) metadata...");
        string fluf2ImageUrl = null;
        yield return StartCoroutine(GetFlufImageUrl(token2Text.text, (url) => fluf2ImageUrl = url));
        
        if (string.IsNullOrEmpty(fluf2ImageUrl))
        {
            LogError($"Failed to get Fluf2 image URL for token {token2Text.text}!");
            yield break;
        }

        Log("Downloading Fluf2 image...");
        Texture2D fluf2Texture = null;
        yield return StartCoroutine(DownloadImage(fluf2ImageUrl, (texture) => fluf2Texture = texture));
        
        if (fluf2Texture == null)
        {
            LogError("Failed to download Fluf2 image!");
            yield break;
        }

        // Load Fluf2 image into UI Image component
        if (Fluf2Image != null)
        {
            Sprite fluf2Sprite = Sprite.Create(fluf2Texture, new Rect(0, 0, fluf2Texture.width, fluf2Texture.height), new Vector2(0.5f, 0.5f));
            Fluf2Image.sprite = fluf2Sprite;
            Log("Fluf2 image loaded into UI Image component");
        }

        // Step 3: Generate Fluflet using OpenAI images/edits endpoint
        Log("Generating Fluflet image with OpenAI images/edits...");
        yield return StartCoroutine(GenerateImageWithOpenAI(fluf1Texture, fluf2Texture, GenerationPrompt));

        Log("Fluflet generation process completed!");
    }
    
    private IEnumerator GetFlufImageUrl(string tokenID, System.Action<string> callback)
    {
        string apiUrl = $"https://api.fluf.world/api/token/{tokenID}";
        
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Log($"Fluf API Response: {responseText}");
                
                // Parse JSON response to get transparent_image
                var response = JsonUtility.FromJson<FlufApiResponse>(responseText);
                if (response != null && !string.IsNullOrEmpty(response.transparent_image))
                {
                    string httpUrl = ConvertIPFSUrlToHttp(response.transparent_image);
                    Log($"Converted IPFS URL to HTTP: {httpUrl}");
                    callback(httpUrl);
                }
                else
                {
                    LogError("Failed to parse transparent_image from Fluf API response");
                    callback(null);
                }
            }
            else
            {
                LogError($"Failed to fetch Fluf metadata: {request.error}");
                callback(null);
            }
        }
    }
    
    private string ConvertIPFSUrlToHttp(string ipfsUrl)
    {
        if (ipfsUrl.StartsWith("ipfs://"))
        {
            string hash = ipfsUrl.Replace("ipfs://", "");
            return $"https://gateway.pinata.cloud/ipfs/{hash}";
        }
        return ipfsUrl;
    }
    
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
    
    private IEnumerator GenerateImageWithOpenAI(Texture2D fluf1Texture, Texture2D fluf2Texture, string prompt)
    {
        Log("Creating OpenAI images/edits request...");
        
        // Convert textures to PNG bytes
        byte[] fluf1Bytes = fluf1Texture.EncodeToPNG();
        byte[] fluf2Bytes = fluf2Texture.EncodeToPNG();
        
        // Create WWWForm for multipart form data
        WWWForm form = new WWWForm();
        form.AddField("model", "gpt-image-1");
        form.AddField("prompt", prompt);
        form.AddField("size", "1024x1024");
        form.AddField("n", "1");
        
        // Add both parent images as binary data using array syntax
        form.AddBinaryData("image[]", fluf1Bytes, "fluf1.png", "image/png");
        form.AddBinaryData("image[]", fluf2Bytes, "fluf2.png", "image/png");
        
        Log($"Sending request to OpenAI images/edits endpoint...");
        Log($"Request size: {form.data.Length} bytes");
        
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
    
    private void LoadGeneratedImageIntoUI(Texture2D generatedTexture)
    {
        // Load into UI Image component
        if (FlufletImage != null)
        {
            Sprite generatedSprite = Sprite.Create(generatedTexture, new Rect(0, 0, generatedTexture.width, generatedTexture.height), new Vector2(0.5f, 0.5f));
            FlufletImage.sprite = generatedSprite;
            Log("Generated Fluflet image loaded into UI Image component");
        }
        
        // Save to file
        byte[] pngData = generatedTexture.EncodeToPNG();
        string savePath = Path.Combine(Application.dataPath, "GeneratedFluflet.png");
        File.WriteAllBytes(savePath, pngData);
        Log($"Generated Fluflet saved to: {savePath}");
    }
    
    private void Log(string message)
    {
        Debug.Log($"[POC] {message}");
    }
    
    private void LogError(string message)
    {
        Debug.LogError($"[POC] {message}");
    }
}

[System.Serializable]
public class FlufApiResponse
{
    public string transparent_image;
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