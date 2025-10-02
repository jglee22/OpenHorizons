using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Target/ItemTarget", fileName = "Item Target")]
public class ItemTarget : TaskTarget
{
    [Header("Item Settings")]
    [SerializeField] public string itemId = "";
    [SerializeField] public string itemName = "";
    [SerializeField] public int requiredAmount = 1;
    
    public override object Value => itemId;

    public override bool IsEqual(object target)
    {
        if (target is string targetId)
        {
            return targetId == itemId || targetId == itemName;
        }
        
        if (target is Item item)
        {
            return item.itemId == itemId || item.itemName == itemName;
        }
        
        return false;
    }
    
    public string ItemId => itemId;
    public string ItemName => itemName;
    public int RequiredAmount => requiredAmount;
}
