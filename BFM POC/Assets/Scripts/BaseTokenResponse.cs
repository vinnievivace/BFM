using System;
using UnityEngine;

/// <summary>
/// Abstract base class for token API responses containing shared fields
/// </summary>
[Serializable]
public abstract class BaseTokenResponse
{
    [Header("Shared Fields")]
    public string image;
    public string transparent_image;
    public string animation_url;
    public string gif;
    public string description;
    public string name;
    public string owner;
    public int trait_count;
    public string image_small;
    public string image_medium;
    public string image_large;
    
    [Header("Attributes")]
    public TokenAttribute[] attributes;
    
    /// <summary>
    /// Gets the primary image URL (transparent_image if available, otherwise image)
    /// </summary>
    /// <returns>Primary image URL</returns>
    public virtual string GetPrimaryImageUrl()
    {
        return !string.IsNullOrEmpty(transparent_image) ? transparent_image : image;
    }
    
    /// <summary>
    /// Gets the token ID (implemented by derived classes)
    /// </summary>
    /// <returns>Token ID</returns>
    public abstract int GetTokenId();
}

/// <summary>
/// Represents a token attribute
/// </summary>
[Serializable]
public class TokenAttribute
{
    public string trait_type;
    public string value;
}
