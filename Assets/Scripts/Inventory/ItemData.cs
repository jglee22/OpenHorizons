using UnityEngine;

/// <summary>
/// 아이템 데이터 (ScriptableObject)
/// </summary>
[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] public string itemId;
    [SerializeField] public string itemName;
    [SerializeField] public string description;
    [SerializeField] public Sprite icon;
    [SerializeField] public ItemType itemType;
    
    [Header("스택 설정")]
    [SerializeField] public bool isStackable = true;
    [SerializeField] public int maxStackSize = 99;
    
    [Header("아이템 속성")]
    [SerializeField] public float value = 0f;
    [SerializeField] public bool isConsumable = false;
    [SerializeField] public bool isEquippable = false;
    
    [Header("아이템 효과")]
    [SerializeField] public float healthRestore = 0f;
    [SerializeField] public float staminaRestore = 0f;
    
    // 프로퍼티들 (직접 접근 가능하므로 제거)
    
    /// <summary>
    /// Item 클래스로 변환
    /// </summary>
    public Item ToItem()
    {
        return new Item
        {
            itemName = this.itemName,
            description = this.description,
            icon = this.icon,
            itemType = this.itemType,
            isStackable = this.isStackable,
            maxStackSize = this.maxStackSize,
            value = this.value,
            isConsumable = this.isConsumable,
            isEquippable = this.isEquippable,
            healthRestore = this.healthRestore,
            staminaRestore = this.staminaRestore
        };
    }
    
    /// <summary>
    /// ItemData 복사본 생성
    /// </summary>
    public ItemData Clone()
    {
        ItemData clone = ScriptableObject.CreateInstance<ItemData>();
        clone.itemId = this.itemId;
        clone.itemName = this.itemName;
        clone.description = this.description;
        clone.icon = this.icon;
        clone.itemType = this.itemType;
        clone.isStackable = this.isStackable;
        clone.maxStackSize = this.maxStackSize;
        clone.value = this.value;
        clone.isConsumable = this.isConsumable;
        clone.isEquippable = this.isEquippable;
        clone.healthRestore = this.healthRestore;
        clone.staminaRestore = this.staminaRestore;
        return clone;
    }
    
    /// <summary>
    /// 아이템 정보 문자열 반환
    /// </summary>
    public override string ToString()
    {
        return $"{itemName} ({itemType})";
    }
    
    /// <summary>
    /// 아이템이 같은지 확인
    /// </summary>
    public bool Equals(ItemData other)
    {
        if (other == null) return false;
        return itemId == other.itemId;
    }
    
    public override bool Equals(object obj)
    {
        return Equals(obj as ItemData);
    }
    
    public override int GetHashCode()
    {
        return itemId?.GetHashCode() ?? 0;
    }
}
