using UnityEngine;
using AudioSystem;
using System.Collections;

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
        
        // InventoryManager 찾기 (여러 방법 시도)
        InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
        
        // InventoryManager가 없으면 잠시 대기 후 다시 시도
        if (inventoryManager == null)
        {
            Debug.LogWarning("[ItemReward] InventoryManager를 찾을 수 없습니다. 잠시 후 다시 시도합니다...");
            
            // 코루틴으로 지연 후 다시 시도
            if (QuestSystem.Instance != null)
            {
                QuestSystem.Instance.StartCoroutine(TryGiveRewardDelayed(quest));
                return;
            }
        }
        
        if (inventoryManager != null)
        {
            // 아이템 생성
            Item rewardItem = new Item();
            rewardItem.itemId = itemName.ToLower().Replace(" ", "_");
            rewardItem.itemName = itemName;
            rewardItem.description = itemDescription;
            rewardItem.itemType = itemType;
            
            // 아이콘 설정 - 허브 아이템 특별 처리
            if (itemIcon != null)
            {
                rewardItem.icon = itemIcon;
            }
            else if (itemName.Contains("허브") || itemName.Contains("herb") || itemName.ToLower().Contains("healing"))
            {
                // 허브 아이템 아이콘 로드
                Sprite herbIcon = Resources.Load<Sprite>("Healing_Herb");
                if (herbIcon != null)
                {
                    rewardItem.icon = herbIcon;
                    Debug.Log($"[ItemReward] 허브 아이콘 로드 성공: Healing_Herb");
                }
                else
                {
                    Debug.LogWarning($"[ItemReward] 허브 아이콘을 찾을 수 없습니다: Healing_Herb");
                }
            }
            
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
    
    /// <summary>
    /// 지연 후 보상 지급 재시도
    /// </summary>
    private IEnumerator TryGiveRewardDelayed(Quest quest)
    {
        // 최대 5초 동안 InventoryManager 대기
        float maxWaitTime = 5f;
        float elapsedTime = 0f;
        
        while (elapsedTime < maxWaitTime)
        {
            yield return new WaitForSeconds(0.5f);
            elapsedTime += 0.5f;
            
            InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
            if (inventoryManager != null)
            {
                Debug.Log("[ItemReward] InventoryManager를 찾았습니다. 보상을 지급합니다.");
                Give(quest);
                yield break;
            }
        }
        
        Debug.LogError("[ItemReward] 5초 후에도 InventoryManager를 찾을 수 없습니다. 보상 지급을 건너뜁니다.");
    }
}
