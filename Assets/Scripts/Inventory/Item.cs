using UnityEngine;

/// <summary>
/// 인벤토리 아이템 데이터 구조
/// </summary>
[System.Serializable]
public class Item
{
    [Header("아이템 정보")]
    public string itemId;          // 아이템 고유 ID
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;
    public bool isStackable = true;
    public int maxStackSize = 99;
    
    [Header("아이템 속성")]
    public float value = 0f;
    public bool isConsumable = false;
    public bool isEquippable = false;
    
    [Header("아이템 효과")]
    public float healthRestore = 0f;
    public float staminaRestore = 0f;
    
    public Item()
    {
        itemId = "new_item";
        itemName = "새 아이템";
        description = "아이템 설명을 입력하세요.";
        itemType = ItemType.Misc;
    }
    
    public Item(string id, string name, string desc, ItemType type)
    {
        itemId = id;
        itemName = name;
        description = desc;
        itemType = type;
    }
    
    /// <summary>
    /// 아이템 복사본 생성
    /// </summary>
    public Item Clone()
    {
        Item newItem = new Item();
        newItem.itemId = this.itemId;
        newItem.itemName = this.itemName;
        newItem.description = this.description;
        newItem.icon = this.icon;
        newItem.itemType = this.itemType;
        newItem.isStackable = this.isStackable;
        newItem.maxStackSize = this.maxStackSize;
        newItem.value = this.value;
        newItem.isConsumable = this.isConsumable;
        newItem.isEquippable = this.isEquippable;
        newItem.healthRestore = this.healthRestore;
        newItem.staminaRestore = this.staminaRestore;
        return newItem;
    }
}

/// <summary>
/// 아이템 타입 열거형
/// </summary>
public enum ItemType
{
    Food,       // 음식
    Material,   // 재료
    Tool,       // 도구
    Weapon,     // 무기
    Armor,      // 방어구
    Treasure,   // 보물
    Misc        // 기타
}
