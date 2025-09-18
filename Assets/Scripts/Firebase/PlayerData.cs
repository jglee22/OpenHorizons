using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 플레이어 데이터 구조
/// </summary>
[System.Serializable]
public class PlayerData
{
    [Header("기본 정보")]
    public string userId;
    public string playerName;
    public System.DateTime lastPlayed;
    
    [Header("플레이어 상태")]
    public float health;
    public Vector3 position;
    public Vector3 rotation;
    
    [Header("게임 진행도")]
    public List<string> quests = new List<string>();
    public List<InventoryItemData> inventory = new List<InventoryItemData>();
    
    [Header("설정")]
    public PlayerSettings settings = new PlayerSettings();
    
    public PlayerData()
    {
        userId = System.Guid.NewGuid().ToString();
        playerName = "Player";
        lastPlayed = System.DateTime.Now;
        health = 100f;
        position = Vector3.zero;
        rotation = Vector3.zero;
        settings = new PlayerSettings();
    }
}

/// <summary>
/// 인벤토리 아이템 데이터
/// </summary>
[System.Serializable]
public class InventoryItemData
{
    public string itemId;
    public string itemName;
    public int quantity;
    public string description;
    
    public InventoryItemData()
    {
        itemId = "";
        itemName = "";
        quantity = 0;
        description = "";
    }
}

/// <summary>
/// 플레이어 설정
/// </summary>
[System.Serializable]
public class PlayerSettings
{
    public float volume = 1.0f;
    public string graphicsQuality = "Medium";
    public bool fullscreen = true;
    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
    
    public PlayerSettings()
    {
        volume = 1.0f;
        graphicsQuality = "Medium";
        fullscreen = true;
        resolutionWidth = 1920;
        resolutionHeight = 1080;
    }
}