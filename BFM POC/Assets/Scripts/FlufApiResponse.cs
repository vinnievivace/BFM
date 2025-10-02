using System;
using UnityEngine;

/// <summary>
/// Response class for Fluf API containing Fluf-specific fields
/// </summary>
[Serializable]
public class FlufApiResponse : BaseTokenResponse
{
    [Header("Fluf-Specific Fields")]
    public int fluf_id;
    public int tokenId;
    
    /// <summary>
    /// Gets the Fluf token ID
    /// </summary>
    /// <returns>Fluf token ID</returns>
    public override int GetTokenId()
    {
        return tokenId;
    }
    
    /// <summary>
    /// Gets the Fluf ID (different from token ID)
    /// </summary>
    /// <returns>Fluf ID</returns>
    public int GetFlufId()
    {
        return fluf_id;
    }
}
