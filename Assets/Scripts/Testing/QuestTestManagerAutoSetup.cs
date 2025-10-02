using UnityEngine;

/// <summary>
/// QuestTestManager 자동 설정 스크립트
/// </summary>
public class QuestTestManagerAutoSetup : MonoBehaviour
{
    [Header("자동 설정")]
    [SerializeField] private bool autoSetupOnStart = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupQuestTestManager();
        }
    }
    
    private void SetupQuestTestManager()
    {
        // QuestTestManager가 이미 있는지 확인
        QuestTestManager existingManager = FindObjectOfType<QuestTestManager>();
        if (existingManager != null)
        {
            Debug.Log("[AutoSetup] QuestTestManager가 이미 존재합니다.");
            return;
        }
        
        // QuestTestManager 생성
        GameObject managerObj = new GameObject("QuestTestManager");
        QuestTestManager manager = managerObj.AddComponent<QuestTestManager>();
        
        Debug.Log("[AutoSetup] QuestTestManager가 자동으로 생성되었습니다!");
    }
    
    [ContextMenu("수동으로 QuestTestManager 생성")]
    public void ManualSetup()
    {
        SetupQuestTestManager();
    }
}
