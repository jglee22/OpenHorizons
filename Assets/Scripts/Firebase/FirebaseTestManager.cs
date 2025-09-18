using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Firebase 테스트 관리자
/// </summary>
public class FirebaseTestManager : MonoBehaviour
{
    [Header("테스트 설정")]
    public string testPlayerName = "TestPlayer";
    
    private FirebaseDataManager dataManager;
    
    void Start()
    {
        dataManager = FindObjectOfType<FirebaseDataManager>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            TestSavePlayerData();
        }
        
        if (Input.GetKeyDown(KeyCode.F6))
        {
            TestLoadPlayerData();
        }
        
        if (Input.GetKeyDown(KeyCode.F7))
        {
            TestSaveQuestData();
        }
        
        if (Input.GetKeyDown(KeyCode.F8))
        {
            TestUpdateLeaderboard();
        }
    }
    
    /// <summary>
    /// 플레이어 데이터 저장 테스트
    /// </summary>
    private async void TestSavePlayerData()
    {
        if (dataManager == null)
        {
            Debug.LogError("FirebaseDataManager를 찾을 수 없습니다.");
            return;
        }
        
        var playerData = new PlayerData
        {
            userId = "test_user_123",
            playerName = testPlayerName,
            health = 100f,
            position = new Vector3(10f, 0f, 5f),
            quests = new System.Collections.Generic.List<string> { "quest1", "quest2" },
            inventory = new System.Collections.Generic.List<InventoryItemData>
            {
                new InventoryItemData { itemId = "sword", quantity = 1 },
                new InventoryItemData { itemId = "potion", quantity = 3 }
            },
            settings = new PlayerSettings
            {
                volume = 0.8f,
                graphicsQuality = "High"
            }
        };
        
        bool success = await dataManager.SavePlayerData(playerData);
        Debug.Log($"플레이어 데이터 저장 테스트: {(success ? "성공" : "실패")}");
    }
    
    /// <summary>
    /// 플레이어 데이터 로드 테스트
    /// </summary>
    private async void TestLoadPlayerData()
    {
        if (dataManager == null)
        {
            Debug.LogError("FirebaseDataManager를 찾을 수 없습니다.");
            return;
        }
        
        var playerData = await dataManager.LoadPlayerData("test_user_123");
        if (playerData != null)
        {
            Debug.Log($"플레이어 데이터 로드 성공: {playerData.playerName}");
            Debug.Log($"체력: {playerData.health}, 위치: {playerData.position}");
        }
        else
        {
            Debug.LogWarning("플레이어 데이터 로드 실패");
        }
    }
    
    /// <summary>
    /// 퀘스트 데이터 저장 테스트
    /// </summary>
    private async void TestSaveQuestData()
    {
        if (dataManager == null)
        {
            Debug.LogError("FirebaseDataManager를 찾을 수 없습니다.");
            return;
        }
        
        var questData = new QuestData
        {
            questId = "test_quest_001",
            questName = "테스트 퀘스트",
            description = "Firebase 테스트를 위한 퀘스트입니다.",
            isCompleted = false,
            completedAt = System.DateTime.Now
        };
        
        bool success = await dataManager.SaveQuestData(questData);
        Debug.Log($"퀘스트 데이터 저장 테스트: {(success ? "성공" : "실패")}");
    }
    
    /// <summary>
    /// 리더보드 업데이트 테스트
    /// </summary>
    private async void TestUpdateLeaderboard()
    {
        if (dataManager == null)
        {
            Debug.LogError("FirebaseDataManager를 찾을 수 없습니다.");
            return;
        }
        
        int randomScore = Random.Range(100, 1000);
        bool success = await dataManager.UpdateLeaderboard(testPlayerName, randomScore);
        Debug.Log($"리더보드 업데이트 테스트: {(success ? "성공" : "실패")} (점수: {randomScore})");
    }
}