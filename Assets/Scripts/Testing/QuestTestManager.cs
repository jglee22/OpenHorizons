using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 퀘스트 시스템 테스트 매니저
/// </summary>
public class QuestTestManager : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private Button registerQuestButton;
    [SerializeField] private Button completeQuestButton;
    [SerializeField] private Button testKillEnemyButton;
    [SerializeField] private Button testCollectItemButton;
    [SerializeField] private Text questStatusText;
    
    [Header("테스트 설정")]
    [SerializeField] private Quest testQuest;
    [SerializeField] private GameObject testEnemyPrefab;
    [SerializeField] private GameObject testItemPrefab;
    
    private Quest activeTestQuest;
    
    private void Start()
    {
        SetupUI();
        UpdateQuestStatus();
    }
    
    private void SetupUI()
    {
        if (registerQuestButton != null)
            registerQuestButton.onClick.AddListener(RegisterTestQuest);
            
        if (completeQuestButton != null)
            completeQuestButton.onClick.AddListener(CompleteTestQuest);
            
        if (testKillEnemyButton != null)
            testKillEnemyButton.onClick.AddListener(TestKillEnemy);
            
        if (testCollectItemButton != null)
            testCollectItemButton.onClick.AddListener(TestCollectItem);
    }
    
    private void RegisterTestQuest()
    {
        if (testQuest == null)
        {
            Debug.LogWarning("테스트 퀘스트가 설정되지 않았습니다!");
            return;
        }
        
        if (QuestSystem.Instance != null)
        {
            activeTestQuest = QuestSystem.Instance.Register(testQuest);
            Debug.Log($"퀘스트 등록: {activeTestQuest.DisplayName}");
            UpdateQuestStatus();
        }
        else
        {
            Debug.LogError("QuestSystem을 찾을 수 없습니다!");
        }
    }
    
    private void CompleteTestQuest()
    {
        if (activeTestQuest != null && QuestSystem.Instance != null)
        {
            QuestSystem.Instance.CompleteWaitingQuests();
            Debug.Log("대기 중인 퀘스트 완료 시도");
            UpdateQuestStatus();
        }
    }
    
    private void TestKillEnemy()
    {
        if (testEnemyPrefab != null)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 5f;
            spawnPos.y = transform.position.y;
            
            GameObject enemy = Instantiate(testEnemyPrefab, spawnPos, Quaternion.identity);
            Debug.Log("테스트 적 생성: " + enemy.name);
        }
        else
        {
            Debug.LogWarning("테스트 적 프리팹이 설정되지 않았습니다!");
        }
    }
    
    private void TestCollectItem()
    {
        if (testItemPrefab != null)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 3f;
            spawnPos.y = transform.position.y;
            
            GameObject item = Instantiate(testItemPrefab, spawnPos, Quaternion.identity);
            Debug.Log("테스트 아이템 생성: " + item.name);
        }
        else
        {
            Debug.LogWarning("테스트 아이템 프리팹이 설정되지 않았습니다!");
        }
    }
    
    private void UpdateQuestStatus()
    {
        if (questStatusText == null || QuestSystem.Instance == null) return;
        
        string status = "=== 퀘스트 상태 ===\n";
        status += $"활성 퀘스트: {QuestSystem.Instance.ActiveQuests.Count}\n";
        status += $"완료 퀘스트: {QuestSystem.Instance.CompletedQuests.Count}\n";
        status += $"활성 업적: {QuestSystem.Instance.ActiveAchievements.Count}\n";
        status += $"완료 업적: {QuestSystem.Instance.CompletedAchievements.Count}\n";
        
        if (activeTestQuest != null)
        {
            status += $"\n테스트 퀘스트: {activeTestQuest.DisplayName}\n";
            status += $"상태: {activeTestQuest.State}\n";
            status += $"완료 가능: {activeTestQuest.IsComplatable}\n";
        }
        
        questStatusText.text = status;
    }
    
    private void Update()
    {
        // 퀘스트 관리
        if (Input.GetKeyDown(KeyCode.Q))
        {
            RegisterTestQuest();
        }
        
        if (Input.GetKeyDown(KeyCode.E))
        {
            CompleteTestQuest();
        }
        
        // 게임 액션 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestKillEnemy();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestCollectItem();
        }
        
        // 퀘스트 상태 업데이트
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UpdateQuestStatus();
        }
    }
    
    // private void OnGUI()
    // {
    //     GUILayout.BeginArea(new Rect(10, 10, 300, 250));
    //     GUILayout.Label("=== 퀘스트 테스트 ===");
    //     GUILayout.Space(5);
        
    //     GUILayout.Label("=== 퀘스트 관리 ===");
    //     GUILayout.Label("Q: 퀘스트 등록");
    //     GUILayout.Label("E: 퀘스트 완료");
    //     GUILayout.Label("Tab: 상태 새로고침");
    //     GUILayout.Space(5);
        
    //     GUILayout.Label("=== 게임 액션 테스트 ===");
    //     GUILayout.Label("1: 적 생성");
    //     GUILayout.Label("2: 아이템 생성");
    //     GUILayout.Space(5);
        
    //     GUILayout.Label("=== 기존 게임 기능 ===");
    //     GUILayout.Label("마우스 좌클릭: 공격");
    //     GUILayout.Label("WASD: 이동");
    //     GUILayout.Label("Space: 점프");
    //     GUILayout.Label("E: 상호작용");
        
    //     GUILayout.EndArea();
    // }
}
