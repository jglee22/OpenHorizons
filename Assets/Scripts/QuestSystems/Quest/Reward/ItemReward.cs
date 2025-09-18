using UnityEngine;
using AudioSystem;

[CreateAssetMenu(menuName = "Quest/Reward/Item", fileName = "ItemReward_")]
public class ItemReward : Reward
{
    [Header("아이템 보상 설정")]
    [SerializeField] private string itemName = "Bronze Sword";
    [SerializeField] private string itemDescription = "퀘스트 보상으로 받은 검입니다.";
    [SerializeField] private ItemType itemType = ItemType.Weapon;
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private int itemCount = 1;
    
    public override void Give(Quest quest)
    {
        Debug.Log($"퀘스트 보상 지급: {itemName} x{itemCount}");
        
        // InventoryManager 찾기
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            // 아이템 생성
            Item rewardItem = new Item();
            rewardItem.itemName = itemName;
            rewardItem.description = itemDescription;
            rewardItem.itemType = itemType;
            rewardItem.icon = itemIcon;
            rewardItem.isStackable = false;
            rewardItem.maxStackSize = 1;
            rewardItem.isEquippable = true;
            rewardItem.isConsumable = false;
            rewardItem.value = 100f;
            
            // 인벤토리에 추가
            if (inventoryManager.AddItem(rewardItem, itemCount))
            {
                Debug.Log($"보상 아이템이 인벤토리에 추가되었습니다: {itemName}");
                
                // 아이템 획득 사운드 재생
                try
                {
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlaySFX("item_pickup");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"사운드 재생 실패: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("인벤토리 공간이 부족하여 보상을 지급할 수 없습니다!");
            }
        }
        else
        {
            Debug.LogError("InventoryManager를 찾을 수 없습니다!");
        }
    }
}
