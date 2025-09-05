using UnityEngine;
using System.Collections.Generic;
using AudioSystem;

/// <summary>
/// 인벤토리 전체를 관리하는 매니저
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("인벤토리 설정")]
    [SerializeField] private int maxSlots = 40;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    
    [Header("UI 참조")]
    [SerializeField] private InventoryUI inventoryUI;
    
    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;
    
    // 이벤트
    public System.Action OnInventoryChanged;
    
    private void Start()
    {
        Debug.Log("InventoryManager Start() 호출됨");
        InitializeInventory();
        
        // UI 자동 찾기
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<InventoryUI>();
        }
        
        Debug.Log($"InventoryManager 초기화 완료 - 슬롯 수: {slots?.Count ?? 0}");
    }
    
    /// <summary>
    /// 인벤토리 초기화
    /// </summary>
    private void InitializeInventory()
    {
        slots.Clear();
        
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot());
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"인벤토리 초기화 완료: {maxSlots}개 슬롯");
        }
    }
    
    /// <summary>
    /// 아이템 추가
    /// </summary>
    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        
        // slots 리스트 null 체크
        if (slots == null)
        {
            Debug.LogError("slots 리스트가 초기화되지 않았습니다!");
            InitializeInventory();
        }
        
        // 먼저 기존 스택에 추가 시도
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && slots[i].item != null && 
                slots[i].item.itemName == item.itemName && 
                slots[i].HasSpace(amount))
            {
                if (slots[i].AddItem(item, amount))
                {
                    OnInventoryChanged?.Invoke();
                    if (showDebugInfo)
                    {
                        Debug.Log($"아이템 추가됨: {item.itemName} x{amount} (슬롯 {i})");
                    }
                    return true;
                }
            }
        }
        
        // 빈 슬롯에 추가
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && !slots[i].isOccupied)
            {
                if (slots[i].AddItem(item, amount))
                {
                    OnInventoryChanged?.Invoke();
                    if (showDebugInfo)
                    {
                        Debug.Log($"아이템 추가됨: {item.itemName} x{amount} (새 슬롯 {i})");
                    }
                    return true;
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"인벤토리 공간 부족: {item.itemName} x{amount}");
        }
        return false;
    }
    
    /// <summary>
    /// 아이템 제거
    /// </summary>
    public bool RemoveItem(string itemName, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemName) || amount <= 0) return false;
        
        int remainingToRemove = amount;
        
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i] != null && slots[i].isOccupied && slots[i].item != null && slots[i].item.itemName == itemName)
            {
                int availableInSlot = slots[i].quantity;
                int toRemoveFromSlot = Mathf.Min(remainingToRemove, availableInSlot);
                
                if (slots[i].RemoveItem(toRemoveFromSlot))
                {
                    remainingToRemove -= toRemoveFromSlot;
                    
                    if (remainingToRemove <= 0)
                    {
                        OnInventoryChanged?.Invoke();
                        if (showDebugInfo)
                        {
                            Debug.Log($"아이템 제거됨: {itemName} x{amount}");
                        }
                        return true;
                    }
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"아이템 제거 실패: {itemName} x{amount} (수량 부족)");
        }
        return false;
    }
    
    /// <summary>
    /// 아이템 수량 확인
    /// </summary>
    public int GetItemCount(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return 0;
        if (slots == null) return 0;
        
        int totalCount = 0;
        foreach (var slot in slots)
        {
            if (slot != null && slot.isOccupied && slot.item != null && slot.item.itemName == itemName)
            {
                totalCount += slot.quantity;
            }
        }
        
        return totalCount;
    }
    
    /// <summary>
    /// 아이템 보유 여부 확인
    /// </summary>
    public bool HasItem(string itemName, int amount = 1)
    {
        return GetItemCount(itemName) >= amount;
    }
    
    /// <summary>
    /// 빈 슬롯 개수
    /// </summary>
    public int GetEmptySlotCount()
    {
        if (slots == null) return 0;
        
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot != null && !slot.isOccupied) count++;
        }
        return count;
    }
    
    /// <summary>
    /// 인벤토리 공간 확인
    /// </summary>
    public bool HasSpace(int amount = 1)
    {
        return GetEmptySlotCount() >= amount;
    }
    
    /// <summary>
    /// 특정 슬롯의 아이템 사용
    /// </summary>
    public bool UseItem(int slotIndex)
    {
        Debug.Log($"UseItem 호출됨 - 슬롯 인덱스: {slotIndex}");
        
        // null 체크 추가
        if (slots == null)
        {
            Debug.LogError("slots 리스트가 초기화되지 않았습니다!");
            InitializeInventory();
            return false;
        }
        
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning($"잘못된 슬롯 인덱스: {slotIndex} (범위: 0-{slots.Count - 1})");
            return false;
        }
        
        var slot = slots[slotIndex];
        if (slot == null)
        {
            Debug.LogError($"슬롯 {slotIndex}가 null입니다!");
            return false;
        }
        
        Debug.Log($"슬롯 {slotIndex} 상태 - isOccupied: {slot.isOccupied}, item: {(slot.item != null ? slot.item.itemName : "null")}");
        
        if (!slot.isOccupied) return false;
        
        // slot.item null 체크 추가
        if (slot.item == null)
        {
            Debug.LogError($"슬롯 {slotIndex}의 아이템이 null입니다!");
            return false;
        }
        
        if (slot.item.isConsumable)
        {
            // 아이템 이름을 미리 저장 (RemoveItem 호출 후 변경될 수 있음)
            string itemName = slot.item.itemName;
            Debug.Log($"소비 가능한 아이템 발견: {itemName}");
            
            // 소비 가능한 아이템인 경우 수량 감소
            if (slot.RemoveItem(1))
            {
                OnInventoryChanged?.Invoke();
                if (showDebugInfo)
                {
                    Debug.Log($"아이템 사용됨: {itemName}");
                }
                return true;
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"사용할 수 없는 아이템: {slot.item.itemName}");
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 인벤토리 정보 출력 (디버그용)
    /// </summary>
    [ContextMenu("인벤토리 정보 출력")]
    public void PrintInventoryInfo()
    {
        Debug.Log("=== 인벤토리 정보 ===");
        Debug.Log($"총 슬롯: {slots.Count}");
        Debug.Log($"빈 슬롯: {GetEmptySlotCount()}");
        Debug.Log($"사용 중 슬롯: {slots.Count - GetEmptySlotCount()}");
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && slots[i].isOccupied)
            {
                Debug.Log($"슬롯 {i}: {slots[i]}");
            }
        }
    }
    
    /// <summary>
    /// 테스트 아이템 추가
    /// </summary>
    [ContextMenu("테스트 아이템 추가")]
    public void AddTestItems()
    {
        // 테스트 아이템들 추가
        AddItem(new Item("나무 열매", "맛있는 나무 열매입니다.", ItemType.Food), 5);
        AddItem(new Item("돌", "건축에 사용할 수 있는 돌입니다.", ItemType.Material), 10);
        AddItem(new Item("나무 막대", "다양한 용도로 사용할 수 있습니다.", ItemType.Tool), 3);
        
        Debug.Log("테스트 아이템이 추가되었습니다.");
    }
    
    /// <summary>
    /// 특정 슬롯에서 아이템 제거
    /// </summary>
    public bool RemoveItemFromSlot(int slotIndex)
    {
        if (slots == null)
        {
            Debug.LogError("slots 리스트가 초기화되지 않았습니다!");
            InitializeInventory();
            return false;
        }
        
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning($"잘못된 슬롯 인덱스: {slotIndex}");
            return false;
        }
        
        var slot = slots[slotIndex];
        if (slot == null)
        {
            Debug.LogError($"슬롯 {slotIndex}가 null입니다!");
            return false;
        }
        
        if (!slot.isOccupied) return false;
        
        slot.ClearSlot();
        OnInventoryChanged?.Invoke();
        
        return true;
    }
    
    /// <summary>
    /// 특정 슬롯에 아이템 추가
    /// </summary>
    public bool AddItemToSlot(Item item, int quantity, int slotIndex)
    {
        if (slots == null)
        {
            Debug.LogError("slots 리스트가 초기화되지 않았습니다!");
            InitializeInventory();
            return false;
        }
        
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning($"잘못된 슬롯 인덱스: {slotIndex}");
            return false;
        }
        
        var slot = slots[slotIndex];
        if (slot == null)
        {
            Debug.LogError($"슬롯 {slotIndex}가 null입니다!");
            return false;
        }
        
        if (slot.isOccupied)
        {
            Debug.LogWarning($"슬롯 {slotIndex}가 이미 사용 중입니다!");
            return false;
        }
        
        bool result = slot.AddItem(item, quantity);
        if (result)
        {
            OnInventoryChanged?.Invoke();
        }
        
        return result;
    }
}
