using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Action/SurviveTime", fileName = "Survive Time Action")]
public class SurviveTimeAction : TaskAction
{
    [Header("Time Settings")]
    [SerializeField] public float timeInSeconds = 60f; // 생존해야 할 시간 (초)
    [SerializeField] private bool pauseOnGamePause = true; // 게임 일시정지 시 타이머 일시정지
    
    public override int Run(Task task, int currentSuccess, int successCount)
    {
        // 시간 기반 액션은 QuestSystem에서 시간 체크 후 호출
        // successCount는 경과 시간 (초)
        
        if (successCount > 0)
        {
            Debug.Log($"[SurviveTimeAction] 생존 시간 진행: {successCount}초");
            
            // 목표 시간 달성 시 완료
            if (currentSuccess + successCount >= timeInSeconds)
            {
                Debug.Log($"[SurviveTimeAction] 생존 시간 달성! ({timeInSeconds}초)");
                return (int)timeInSeconds; // 목표 시간 달성
            }
            
            return currentSuccess + successCount;
        }
        
        return currentSuccess;
    }
    
    public float TimeInSeconds => timeInSeconds;
    public bool PauseOnGamePause => pauseOnGamePause;
}
