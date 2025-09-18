using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Analytics;
using Firebase.Crashlytics;
using System.Collections;

/// <summary>
/// Firebase 통합 관리자
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    [Header("Firebase 설정")]
    public bool enableDebugLogs = true;
    
    // Firebase 인스턴스들
    public static FirebaseManager Instance { get; private set; }
    public FirebaseAuth Auth { get; private set; }
    public DatabaseReference Database { get; private set; }
    public StorageReference Storage { get; private set; }
    
    // 초기화 상태
    public bool IsInitialized { get; private set; } = false;
    
    private void Awake()
    {
        // Singleton 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFirebase();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeFirebase()
    {
        Debug.Log("Firebase 초기화 시작...");
        
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase 초기화 성공!");
                
                // 각 서비스 초기화
                Auth = FirebaseAuth.DefaultInstance;
                Database = FirebaseDatabase.DefaultInstance.RootReference;
                Storage = FirebaseStorage.DefaultInstance.RootReference;
                
                // Crashlytics 초기화
                Crashlytics.ReportUncaughtExceptionsAsFatal = true;
                
                IsInitialized = true;
                OnFirebaseInitialized();
            }
            else
            {
                Debug.LogError($"Firebase 초기화 실패: {task.Result}");
            }
        });
    }
    
    private void OnFirebaseInitialized()
    {
        Debug.Log("Firebase 모든 서비스 초기화 완료!");
        
        // 기존 시스템들과 연동
        ConnectWithExistingSystems();
    }
    
    private void ConnectWithExistingSystems()
    {
        // 퀘스트 시스템과 연동
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.onQuestCompleted += OnQuestCompleted;
        }
        
        // 인벤토리 시스템과 연동
        var inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += OnInventoryChanged;
        }
        
        // 전투 시스템과 연동 (기존 시스템에 이벤트가 없으므로 주기적으로 체크)
        StartCoroutine(MonitorPlayerData());
    }
    
    // 이벤트 핸들러들
    private void OnQuestCompleted(Quest quest)
    {
        if (enableDebugLogs)
            Debug.Log($"퀘스트 완료: {quest.DisplayName}");
        
        // Analytics에 퀘스트 완료 이벤트 전송
        FirebaseAnalytics.LogEvent("quest_completed", new Parameter("quest_name", quest.DisplayName));
    }
    
    private void OnInventoryChanged()
    {
        if (enableDebugLogs)
            Debug.Log("인벤토리 변경됨");
        
        // Analytics에 인벤토리 변경 이벤트 전송
        FirebaseAnalytics.LogEvent("inventory_changed");
    }
    
    // 플레이어 데이터 모니터링 (이벤트가 없으므로 주기적으로 체크)
    private System.Collections.IEnumerator MonitorPlayerData()
    {
        var playerCombat = FindObjectOfType<PlayerCombatController>();
        if (playerCombat == null)
        {
            Debug.LogWarning("PlayerCombatController를 찾을 수 없습니다. 모니터링을 중단합니다.");
            yield break;
        }
        
        float lastHealth = -1f;
        Vector3 lastPosition = Vector3.zero;
        int checkCount = 0;
        const int maxChecks = 100; // 최대 100번만 체크 (안전장치)
        
        while (playerCombat != null && checkCount < maxChecks)
        {
            // 체력 변경 체크
            if (Mathf.Abs(playerCombat.currentHealth - lastHealth) > 0.1f)
            {
                lastHealth = playerCombat.currentHealth;
                OnPlayerDataChanged();
            }
            
            // 위치 변경 체크
            if (Vector3.Distance(playerCombat.transform.position, lastPosition) > 1f)
            {
                lastPosition = playerCombat.transform.position;
                OnPlayerDataChanged();
            }
            
            checkCount++;
            yield return new WaitForSeconds(2f); // 2초마다 체크 (더 안전하게)
        }
        
        if (checkCount >= maxChecks)
        {
            Debug.Log("플레이어 데이터 모니터링이 최대 체크 횟수에 도달했습니다.");
        }
    }
    
    private void OnPlayerDataChanged()
    {
        if (enableDebugLogs)
            Debug.Log("플레이어 데이터 변경됨");
        
        // Analytics에 플레이어 데이터 변경 이벤트 전송
        FirebaseAnalytics.LogEvent("player_data_changed");
    }
}