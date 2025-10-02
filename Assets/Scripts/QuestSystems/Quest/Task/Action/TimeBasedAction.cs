using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Task/Action/TimeBased", fileName = "Time Based Action")]
public class TimeBasedAction : TaskAction
{
    [Header("Time Settings")]
    [SerializeField] private float requiredTime = 60f; // 필요한 시간 (초)
    [SerializeField] private bool countDown = false; // 카운트다운 방식
    [SerializeField] private bool pauseOnGamePause = true; // 게임 일시정지 시 타이머 일시정지
    
    private float elapsedTime = 0f;
    private bool isActive = false;
    
    public override int Run(Task task, int currentSuccess, int successCount)
    {
        // 시간 기반 액션은 QuestSystem에서 시간 체크 후 호출
        // successCount는 경과 시간 (초)
        
        if (isActive)
        {
            elapsedTime += successCount;
            
            if (countDown)
            {
                float remainingTime = requiredTime - elapsedTime;
                Debug.Log($"[TimeBasedAction] 남은 시간: {remainingTime:F1}초");
                
                if (remainingTime <= 0)
                {
                    Debug.Log($"[TimeBasedAction] 시간 완료!");
                    isActive = false;
                    return 1; // 시간 완료
                }
            }
            else
            {
                Debug.Log($"[TimeBasedAction] 경과 시간: {elapsedTime:F1}/{requiredTime}초");
                
                if (elapsedTime >= requiredTime)
                {
                    Debug.Log($"[TimeBasedAction] 목표 시간 달성!");
                    isActive = false;
                    return 1; // 시간 달성
                }
            }
        }
        
        return currentSuccess;
    }
    
    public void StartTimer()
    {
        isActive = true;
        elapsedTime = 0f;
        Debug.Log($"[TimeBasedAction] 타이머 시작: {requiredTime}초");
    }
    
    public void StopTimer()
    {
        isActive = false;
        Debug.Log($"[TimeBasedAction] 타이머 중지");
    }
    
    public float RequiredTime => requiredTime;
    public float ElapsedTime => elapsedTime;
    public bool IsActive => isActive;
}
