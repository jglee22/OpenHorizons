using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Action/TalkToNPC", fileName = "Talk To NPC Action")]
public class TalkToNPCAction : TaskAction
{
    [Header("NPC Settings")]
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private bool requireSpecificDialogue = false; // 특정 대화 필요 여부
    [SerializeField] private string requiredDialogueKey = ""; // 필요한 대화 키
    [SerializeField] public string npcId = ""; // NPC ID
    
    public override int Run(Task task, int currentSuccess, int successCount)
    {
        // NPC와 대화 시 성공 카운트 증가
        if (successCount > 0)
        {
            Debug.Log($"[TalkToNPCAction] {npcName}과 대화 완료!");
            return 1; // 대화는 한 번만 성공
        }
        
        return currentSuccess;
    }
    
    public string NPCName => npcName;
    public bool RequireSpecificDialogue => requireSpecificDialogue;
    public string RequiredDialogueKey => requiredDialogueKey;
}
