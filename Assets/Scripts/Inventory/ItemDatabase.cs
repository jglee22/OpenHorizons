using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 아이템 데이터베이스 (ScriptableObject)
/// </summary>
[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Inventory/Item Database")]
public class ItemDatabase : ScriptableObject
{
    [Header("아이템 데이터")]
    [SerializeField] private List<ItemData> items = new List<ItemData>();
    
    /// <summary>
    /// 아이템 이름으로 아이템 데이터 찾기
    /// </summary>
    public ItemData GetItem(string itemName)
    {
        return items.FirstOrDefault(item => item.itemName == itemName);
    }
    
    /// <summary>
    /// 아이템 ID로 아이템 데이터 찾기
    /// </summary>
    public ItemData GetItemById(string itemId)
    {
        return items.FirstOrDefault(item => item.itemId == itemId);
    }
    
    /// <summary>
    /// 아이템 타입으로 아이템들 찾기
    /// </summary>
    public List<ItemData> GetItemsByType(ItemType itemType)
    {
        return items.Where(item => item.itemType == itemType).ToList();
    }
    
    /// <summary>
    /// 모든 아이템 반환
    /// </summary>
    public List<ItemData> GetAllItems()
    {
        return new List<ItemData>(items);
    }
    
    /// <summary>
    /// 아이템 존재 여부 확인
    /// </summary>
    public bool HasItem(string itemName)
    {
        return items.Any(item => item.itemName == itemName);
    }
    
    /// <summary>
    /// 아이템 추가 (에디터용)
    /// </summary>
    public void AddItem(ItemData itemData)
    {
        if (!items.Contains(itemData))
        {
            items.Add(itemData);
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
    
    /// <summary>
    /// 아이템 제거 (에디터용)
    /// </summary>
    public void RemoveItem(ItemData itemData)
    {
        if (items.Contains(itemData))
        {
            items.Remove(itemData);
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
    
    /// <summary>
    /// 데이터베이스 초기화 (에디터용)
    /// </summary>
    [ContextMenu("데이터베이스 초기화")]
    public void InitializeDatabase()
    {
        items.Clear();
        
        // 기본 아이템들 추가
        AddDefaultItems();
        
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }
    
    /// <summary>
    /// 폴더에서 모든 ItemData 자동 로드 (에디터용)
    /// </summary>
    [ContextMenu("폴더에서 아이템 자동 로드")]
    public void LoadItemsFromFolder()
    {
        #if UNITY_EDITOR
        items.Clear();
        
        // ScriptableObjects/Items 폴더에서 모든 ItemData 찾기
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/Scripts/Inventory/ScriptableObjects" });
        
        foreach (string guid in guids)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            ItemData itemData = UnityEditor.AssetDatabase.LoadAssetAtPath<ItemData>(path);
            
            if (itemData != null)
            {
                items.Add(itemData);
            }
        }
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[ItemDatabase] {items.Count}개의 아이템을 폴더에서 로드했습니다.");
        #endif
    }
    
    /// <summary>
    /// 중복 아이템 제거 (에디터용)
    /// </summary>
    [ContextMenu("중복 아이템 제거")]
    public void RemoveDuplicateItems()
    {
        #if UNITY_EDITOR
        var uniqueItems = new List<ItemData>();
        var seenIds = new HashSet<string>();
        
        foreach (var item in items)
        {
            if (item != null && !seenIds.Contains(item.itemId))
            {
                uniqueItems.Add(item);
                seenIds.Add(item.itemId);
            }
        }
        
        int removedCount = items.Count - uniqueItems.Count;
        items = uniqueItems;
        
        UnityEditor.EditorUtility.SetDirty(this);
        Debug.Log($"[ItemDatabase] {removedCount}개의 중복 아이템을 제거했습니다.");
        #endif
    }
    
    /// <summary>
    /// 기본 아이템들 추가
    /// </summary>
    private void AddDefaultItems()
    {
        // Bronze Sword 추가
        var bronzeSword = ScriptableObject.CreateInstance<ItemData>();
        bronzeSword.itemId = "bronze_sword";
        bronzeSword.itemName = "Bronze Sword";
        bronzeSword.description = "청동으로 만든 견고한 검입니다.";
        bronzeSword.itemType = ItemType.Weapon;
        bronzeSword.isStackable = false;
        bronzeSword.maxStackSize = 1;
        bronzeSword.value = 100f;
        bronzeSword.isConsumable = false;
        bronzeSword.isEquippable = true;
        bronzeSword.healthRestore = 0f;
        bronzeSword.staminaRestore = 0f;
        
        // 아이콘 로드
        bronzeSword.icon = Resources.Load<Sprite>("Bronze_Sword");
        
        items.Add(bronzeSword);
        
        Debug.Log($"[ItemDatabase] 기본 아이템 {items.Count}개 추가 완료");
    }
}
