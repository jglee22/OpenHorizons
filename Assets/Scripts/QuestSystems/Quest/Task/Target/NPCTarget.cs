using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Target/NPCTarget", fileName = "NPC Target")]
public class NPCTarget : TaskTarget
{
    [Header("NPC Settings")]
    [SerializeField] public string npcId = "";
    [SerializeField] public string npcName = "";
    [SerializeField] public string npcTag = "";
    
    public override object Value => npcId;

    public override bool IsEqual(object target)
    {
        if (target is string targetId)
        {
            return targetId == npcId || targetId == npcName;
        }
        
        if (target is GameObject npcObject)
        {
            if (!string.IsNullOrEmpty(npcTag) && npcObject.CompareTag(npcTag))
                return true;
                
            if (npcObject.name == npcName)
                return true;
        }
        
        return false;
    }
    
    public string NPCId => npcId;
    public string NPCName => npcName;
    public string NPCTag => npcTag;
}
