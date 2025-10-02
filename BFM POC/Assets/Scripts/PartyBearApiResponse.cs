using System;
using UnityEngine;

/// <summary>
/// Response class for PartyBear API containing PartyBear-specific fields
/// </summary>
[Serializable]
public class PartyBearApiResponse : BaseTokenResponse
{
    [Header("PartyBear-Specific Fields")]
    public int tokenId;
    public string folder;
    public string uuid;
    
    /// <summary>
    /// Gets the PartyBear token ID
    /// </summary>
    /// <returns>PartyBear token ID</returns>
    public override int GetTokenId()
    {
        return tokenId;
    }
    
    /// <summary>
    /// Gets the folder ID
    /// </summary>
    /// <returns>Folder ID</returns>
    public int GetFolder()
    {
        return int.Parse(folder);
    }
    
    /// <summary>
    /// Gets the UUID
    /// </summary>
    /// <returns>UUID string</returns>
    public string GetUuid()
    {
        return uuid;
    }
}
