using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Action/CollectItem", fileName = "Collect Item Action")]
public class CollectItemAction : TaskAction
{
    [Header("Collection Settings")]
    [SerializeField] private bool consumeItem = true; // 아이템을 소모할지 여부
    [SerializeField] private int requiredAmount = 1; // 필요한 아이템 개수
    [SerializeField] public string itemId = ""; // 아이템 ID
    
    public override int Run(Task task, int currentSuccess, int successCount)
    {
        // 아이템 수집 시 성공 카운트 증가
        int newSuccess = currentSuccess + successCount;
        
        // 아이템 소모 처리 (InventoryManager에서 처리)
        if (consumeItem && successCount > 0)
        {
            // 실제 아이템 소모는 QuestSystem에서 처리
            Debug.Log($"[CollectItemAction] 아이템 수집: {successCount}개, 총 진행: {newSuccess}/{requiredAmount}");
        }
        
        return newSuccess;
    }
}
