using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Action/KillEnemy", fileName = "Kill Enemy Action")]
public class KillEnemyAction : TaskAction
{
    [Header("Enemy Settings")]
    [SerializeField] public string enemyId = ""; // 적 ID
    [SerializeField] private bool requireSpecificEnemy = true; // 특정 적 타입 필요 여부
    [SerializeField] private int requiredKills = 1; // 필요한 처치 수
    
    public override int Run(Task task, int currentSuccess, int successCount)
    {
        // 적 처치 시 성공 카운트 증가
        int newSuccess = currentSuccess + successCount;
        
        Debug.Log($"[KillEnemyAction] 적 처치: {successCount}마리, 총 진행: {newSuccess}/{requiredKills}");
        
        return newSuccess;
    }
    
    public string EnemyId => enemyId;
    public bool RequireSpecificEnemy => requireSpecificEnemy;
    public int RequiredKills => requiredKills;
}
