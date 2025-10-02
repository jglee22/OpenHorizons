using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Action/ReachLocation", fileName = "Reach Location Action")]
public class ReachLocationAction : TaskAction
{
    [Header("Location Settings")]
    [SerializeField] public Vector3 targetPosition;
    [SerializeField] private float reachDistance = 5f; // 도달 거리
    [SerializeField] private bool requireExactPosition = false; // 정확한 위치 필요 여부
    [SerializeField] public string locationName = "목표 지점"; // 위치 이름
    
    public override int Run(Task task, int currentSuccess, int successCount)
    {
        // 위치 도달은 QuestSystem에서 위치 체크 후 호출
        // successCount는 1 (도달) 또는 0 (미도달)
        if (successCount > 0)
        {
            Debug.Log($"[ReachLocationAction] 목표 위치 도달 완료!");
            return 1; // 위치 도달은 한 번만 성공
        }
        
        return currentSuccess;
    }
    
    public Vector3 TargetPosition => targetPosition;
    public float ReachDistance => reachDistance;
    public bool RequireExactPosition => requireExactPosition;
}
