using UnityEngine;

/// <summary>
/// 인벤토리 슬롯 (아이템 + 수량)
/// </summary>
[System.Serializable]
public class InventorySlot
{
    [Header("슬롯 정보")]
    public Item item;
    public int quantity;
    public bool isOccupied => item != null && quantity > 0;
    
    public InventorySlot()
    {
        item = null;
        quantity = 0;
    }
    
    /// <summary>
    /// 슬롯에 아이템 추가
    /// </summary>
    public bool AddItem(Item newItem, int amount = 1)
    {
        if (newItem == null || amount <= 0) return false;
        
        // 빈 슬롯이거나 같은 아이템인 경우
        if (!isOccupied || (item.itemName == newItem.itemName && item.isStackable))
        {
            if (!isOccupied)
            {
                item = newItem.Clone();
                quantity = amount;
            }
            else
            {
                // 스택 가능한 아이템인 경우 수량만 증가
                int newQuantity = quantity + amount;
                if (newQuantity <= item.maxStackSize)
                {
                    quantity = newQuantity;
                }
                else
                {
                    // 최대 스택 크기를 초과하는 경우
                    quantity = item.maxStackSize;
                    return false; // 추가 실패
                }
            }
            return true;
        }
        
        return false; // 다른 아이템이 있어서 추가 불가
    }
    
    /// <summary>
    /// 슬롯에서 아이템 제거
    /// </summary>
    public bool RemoveItem(int amount = 1)
    {
        if (!isOccupied || amount <= 0) return false;
        
        if (quantity <= amount)
        {
            // 모든 아이템 제거
            ClearSlot();
        }
        else
        {
            // 일부 아이템만 제거
            quantity -= amount;
        }
        
        return true;
    }
    
    /// <summary>
    /// 슬롯 비우기
    /// </summary>
    public void ClearSlot()
    {
        item = null;
        quantity = 0;
    }
    
    /// <summary>
    /// 슬롯에 공간이 있는지 확인
    /// </summary>
    public bool HasSpace(int amount = 1)
    {
        if (!isOccupied) return true;
        return item.isStackable && (quantity + amount) <= item.maxStackSize;
    }
    
    /// <summary>
    /// 슬롯 정보 문자열 반환
    /// </summary>
    public override string ToString()
    {
        if (!isOccupied) return "빈 슬롯";
        return $"{item.itemName} x{quantity}";
    }
}
