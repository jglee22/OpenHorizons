using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 퀘스트 시스템 테스트 매니저
/// </summary>
public class QuestTestManager : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private Quest testQuest;
    [SerializeField] private string testQuestCodeName = ""; // Resources의 QuestDatabase에서 코드네임으로 자동 조회
    [SerializeField] private GameObject testEnemyPrefab;
    [SerializeField] private GameObject testItemPrefab;
    [SerializeField] private Vector3 testLocation = new Vector3(10, 0, 10);
    [SerializeField] private string testNPCName = "TestNPC";
    
    private Quest activeTestQuest;
    private List<Quest> availableQuests = new List<Quest>();
    
    private void Start()
    {
        Debug.Log("[QuestTestManager] Start() 호출됨");
        ResolveTestQuest();
        LoadAvailableQuests();
        //InitializeQuestGuidance();
        
        // 위치 도달 체크 초기화
        lastPosition = transform.position;
        
        Debug.Log("[QuestTestManager] 초기화 완료");
    }

    private void ResolveTestQuest()
    {
        if (testQuest != null) return;

        var db = Resources.Load<QuestDatabase>("QuestDatabase");
        if (db == null)
        {
            Debug.LogWarning("[QuestTestManager] Resources/QuestDatabase를 찾지 못했습니다. testQuest를 수동 지정하세요.");
            return;
        }

        if (!string.IsNullOrEmpty(testQuestCodeName))
        {
            var found = db.FindQuestBy(testQuestCodeName);
            if (found != null)
            {
                testQuest = found;
                Debug.Log($"[QuestTestManager] 코드네임으로 테스트 퀘스트 설정: {testQuestCodeName}");
                return;
            }
        }

        // 코드네임이 없거나 못 찾았으면 첫 번째 퀘스트 사용
        if (db.Quests != null && db.Quests.Count > 0)
        {
            testQuest = db.Quests[0];
            Debug.Log($"[QuestTestManager] 첫 번째 퀘스트를 테스트 대상으로 설정: {testQuest.DisplayName}");
        }
        else
        {
            Debug.LogWarning("[QuestTestManager] QuestDatabase에 퀘스트가 없습니다. 위자드로 생성 후 DB에 등록하세요.");
        }
    }
    
    
    
    
    
    
    
    
    private void LoadAvailableQuests()
    {
        availableQuests.Clear();
        
        var db = Resources.Load<QuestDatabase>("QuestDatabase");
        if (db != null && db.Quests != null)
        {
            availableQuests.AddRange(db.Quests);
            Debug.Log($"[QuestTestManager] 사용 가능한 퀘스트 {availableQuests.Count}개 로드됨");
        }
        else
        {
            Debug.LogWarning("[QuestTestManager] QuestDatabase를 찾을 수 없습니다.");
        }
    }
    
    
    
    
    private void InitializeQuestGuidance()
    {
        // QuestGuidanceUI가 없으면 생성
        QuestGuidanceUI guidanceUI = FindObjectOfType<QuestGuidanceUI>();
        if (guidanceUI == null)
        {
            GameObject guidanceObj = new GameObject("QuestGuidanceUI");
            guidanceObj.AddComponent<QuestGuidanceUI>();
            Debug.Log("[QuestTestManager] QuestGuidanceUI 생성됨");
        }
    }
    
    private void RegisterTestQuest()
    {
        if (testQuest == null)
        {
            Debug.LogWarning("테스트 퀘스트가 설정되지 않았습니다. 자동 조회를 시도합니다.");
            ResolveTestQuest();
            if (testQuest == null)
            {
                Debug.LogWarning("자동 조회 실패: testQuest를 인스펙터에서 직접 지정하거나 코드네임을 입력하세요.");
                return;
            }
        }
        
        if (QuestSystem.Instance != null)
        {
            activeTestQuest = QuestSystem.Instance.Register(testQuest);
            Debug.Log($"퀘스트 등록: {activeTestQuest.DisplayName}");
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
        }
    }

    private async void ResetQuestProgress()
    {
        if (string.IsNullOrEmpty(testQuestCodeName) && testQuest != null)
            testQuestCodeName = testQuest.CodeName;

        var svc = FindObjectOfType<FirebaseQuestService>();
        if (svc == null)
        {
            Debug.LogWarning("FirebaseQuestService를 찾을 수 없습니다.");
            return;
        }

        await svc.DeleteQuestProgress(testQuestCodeName);
        Debug.Log("파이어베이스 진행 데이터 삭제 완료. 다시 등록해보세요.");
    }
    
    private void TestKillEnemy()
    {
        if (testEnemyPrefab != null)
        {
            // NavMesh 위에 적을 생성하도록 수정
            Vector3 spawnPos = FindNavMeshPosition();
            
            GameObject enemy = Instantiate(testEnemyPrefab, spawnPos, Quaternion.identity);
            enemy.name = "Grunt";
            Debug.Log("테스트 적 생성: " + enemy.name + " at " + spawnPos);
            
            // 적이 죽으면 EnemyHealth에서 자동으로 퀘스트 리포팅됨
            Debug.Log("[QuestTestManager] Grunt 적을 생성했습니다. 적을 처치하면 퀘스트가 자동으로 카운트됩니다.");
        }
        else
        {
            Debug.LogWarning("테스트 적 프리팹이 설정되지 않았습니다!");
        }
    }
    
    private Vector3 FindNavMeshPosition()
    {
        // 플레이어 주변에서 NavMesh 위의 위치를 찾기
        Vector3 playerPos = transform.position;
        
        // 여러 위치를 시도해서 NavMesh 위의 위치 찾기
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPos = playerPos + Random.insideUnitSphere * 8f;
            randomPos.y = playerPos.y + 2f; // 약간 위에서 시작
            
            // NavMesh 위의 위치 찾기
            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(randomPos, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
            {
                Debug.Log($"[QuestTestManager] NavMesh 위치 발견: {hit.position}");
                return hit.position;
            }
        }
        
        // NavMesh를 찾지 못하면 플레이어 앞쪽에 생성
        Vector3 fallbackPos = playerPos + transform.forward * 5f;
        Debug.LogWarning($"[QuestTestManager] NavMesh를 찾지 못해 대체 위치 사용: {fallbackPos}");
        return fallbackPos;
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
        
        // 아이템 수집 리포팅
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.ReportItemCollected("herb_001", 1);
            Debug.Log("[QuestTestManager] 아이템 수집 리포팅: herb_001");
        }
    }
    
    private void TestReachLocation()
    {
        Debug.Log($"목표 위치로 이동: {testLocation}");
        
        // 위치 도달 리포팅
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.ReportLocationReached(testLocation, "TestLocation");
        }
    }
    
    [Header("위치 도달 설정")]
    [SerializeField] private float checkInterval = 0.5f; // 0.5초마다 체크
    [SerializeField] private float reachDistance = 5f; // 도달 거리
    private float lastCheckTime = 0f;
    private Vector3 lastPosition;
    private bool hasMoved = false;
    
    // 실제 위치 기반 테스트 (주기적 체크)
    private void CheckLocationReached()
    {
        if (QuestSystem.Instance == null) return;
        
        // 이동 감지
        Vector3 currentPos = transform.position;
        if (Vector3.Distance(currentPos, lastPosition) > 0.1f)
        {
            hasMoved = true;
            lastPosition = currentPos;
        }
        
        // 이동했거나 주기적 체크 시간이 되었을 때만 체크
        if (!hasMoved && Time.time - lastCheckTime < checkInterval) return;
        
        lastCheckTime = Time.time;
        hasMoved = false;
        
        // 플레이어 오브젝트 찾기 (한 번만)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            player = GameObject.Find("Player");
        }
        if (player == null)
        {
            player = GameObject.Find("First Person Player");
        }
        
        if (player == null)
        {
            Debug.LogWarning("[QuestTestManager] 플레이어 오브젝트를 찾을 수 없습니다!");
            return;
        }
        
        Vector3 playerPos = player.transform.position;
        
        // 활성 퀘스트에서 지역 이동 퀘스트 찾기 (스냅샷 순회로 컬렉션 수정 예외 방지)
        int locationQuestCount = 0;
        var activeQuestsSnapshot = new List<Quest>(QuestSystem.Instance.ActiveQuests);
        foreach (var quest in activeQuestsSnapshot)
        {
            if (quest?.CurrentTaskGroup == null) continue;

            var tasksSnapshot = quest.CurrentTaskGroup.Tasks?.ToList();
            if (tasksSnapshot == null) continue;

            foreach (var task in tasksSnapshot)
            {
                if (task?.Category == null || !task.Category.CodeName.ToLower().Contains("exploration"))
                    continue;

                locationQuestCount++;

                var targetsSnapshot = task.targets != null ? task.targets.ToArray() : null;
                if (targetsSnapshot == null) continue;

                foreach (var target in targetsSnapshot)
                {
                    if (target is LocationTarget locationTarget)
                    {
                        Vector3 targetPos = locationTarget.targetPosition;
                        float distance = Vector3.Distance(playerPos, targetPos);

                        // 디버그 정보 출력 (첫 번째만)
                        if (Time.time - lastCheckTime < 0.1f)
                        {
                            Debug.Log($"[QuestTestManager] 위치 체크 - 퀘스트: {quest.CodeName}, 현재: {playerPos}, 목표: {targetPos}, 거리: {distance:F1}m");
                        }

                        if (distance <= locationTarget.reachDistance)
                        {
                            QuestSystem.Instance.ReportLocationReached(targetPos, locationTarget.locationName);
                            Debug.Log($"[QuestTestManager] 위치 도달 감지 - 퀘스트: {quest.CodeName}, 위치: {targetPos}, 거리: {distance:F1}m");
                            break; // 이 태스크의 추가 열거 중단 (컬렉션 변경 직후 안전)
                        }
                    }
                }
            }
        }
        
        // 디버그 정보 (첫 번째만)
        if (Time.time - lastCheckTime < 0.1f && locationQuestCount > 0)
        {
            Debug.Log($"[QuestTestManager] 활성 지역 이동 퀘스트 수: {locationQuestCount}개");
        }
    }
    
    private void TestTalkToNPC()
    {
        Debug.Log($"NPC와 대화: {testNPCName}");
        
        // NPC 대화 리포팅
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.ReportNPCTalked("TestNPC", testNPCName);
        }
    }
    
    private void TestSurviveTime()
    {
        Debug.Log("생존 시간 테스트 시작 (60초)");
        
        // 시간 경과 리포팅 (1초씩)
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.ReportTimeElapsed(60f);
        }
    }
    
    
    private void DebugQuestSystemStatus()
    {
        if (QuestSystem.Instance == null)
        {
            Debug.LogError("[QuestTestManager] QuestSystem.Instance가 null입니다!");
            return;
        }
        
        Debug.Log("=== 퀘스트 시스템 디버그 정보 ===");
        Debug.Log($"활성 퀘스트 수: {QuestSystem.Instance.ActiveQuests.Count}");
        
        foreach (var quest in QuestSystem.Instance.ActiveQuests)
        {
            Debug.Log($"퀘스트: {quest.DisplayName} ({quest.CodeName})");
            Debug.Log($"  상태: {quest.State}");
            
            if (quest.CurrentTaskGroup != null)
            {
                Debug.Log($"  현재 태스크 그룹의 태스크 수: {quest.CurrentTaskGroup.Tasks.Count}");
                
                foreach (var task in quest.CurrentTaskGroup.Tasks)
                {
                    Debug.Log($"    태스크: {task.Description}");
                    Debug.Log($"      카테고리: {task.Category?.CodeName}");
                    Debug.Log($"      진행도: {task.CurrentSuccess}/{task.NeedSuccessToComplete}");
                    Debug.Log($"      상태: {task.State}");
                    
                    // 타겟 정보 출력
                    if (task.Action != null)
                    {
                        Debug.Log($"      액션 타입: {task.Action.GetType().Name}");
                    }
                }
            }
        }
        
        Debug.Log("=== 디버그 정보 끝 ===");
    }
    
    private void Update()
    {
        // 게임 액션 테스트 (숫자키 그룹)
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            TestKillEnemy();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            TestCollectItem();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            TestReachLocation();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            TestTalkToNPC();
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            TestSurviveTime();
        }
        
        // 6키: 인벤토리 초기화
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            ClearInventory();
        }
        
        // 7키: 중복 아이템 제거
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            RemoveDuplicateItems();
        }
        
        // 키패드 9: 모든 퀘스트 등록
        if (Input.GetKeyDown(KeyCode.Keypad9))
        {
            RegisterAllQuests();
            // 등록 직후 Firebase 저장 강제
            var svc = FindObjectOfType<FirebaseQuestService>();
            if (svc != null)
            {
                _ = svc.SyncAllActiveNow();
                Debug.Log("[QuestTestManager] 모든 퀘스트 등록 후 Firebase 즉시 동기화");
            }
        }
        
        // 키패드 8: 활성 퀘스트 모두 취소
        if (Input.GetKeyDown(KeyCode.Keypad8))
        {
            CancelAllActiveQuests();
        }
        
        // 키패드 0: 모든 퀘스트 진행 초기화(Firebase 삭제) 후 재등록
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            _ = ResetAllQuestsAndReregister();
        }
        
        // 위치 도달 자동 체크 (매 프레임)
        CheckLocationReached();
    }

    private void RegisterAllQuests()
    {
        var db = Resources.Load<QuestDatabase>("QuestDatabase");
        if (db == null)
        {
            Debug.LogWarning("[QuestTestManager] Resources/QuestDatabase를 찾지 못했습니다.");
            return;
        }

        if (QuestSystem.Instance == null)
        {
            Debug.LogError("[QuestTestManager] QuestSystem.Instance가 null입니다!");
            return;
        }

        int registered = 0;
        foreach (var quest in db.Quests)
        {
            if (quest == null) continue;
            // 중복 등록 방지 로직은 QuestSystem.Register 내부에서 처리됨
            var result = QuestSystem.Instance.Register(quest);
            if (result != null)
            {
                registered++;
            }
        }
        Debug.Log($"[QuestTestManager] 모든 퀘스트 등록 완료: {registered}개");
    }

    private void CancelAllActiveQuests()
    {
        if (QuestSystem.Instance == null)
        {
            Debug.LogError("[QuestTestManager] QuestSystem.Instance가 null입니다!");
            return;
        }
        
        // 수정 중 컬렉션 변경을 피하기 위해 복사본에서 순회
        var activeCopy = new List<Quest>(QuestSystem.Instance.ActiveQuests);
        int canceled = 0;
        foreach (var quest in activeCopy)
        {
            if (quest != null && quest.IsCancelable)
            {
                quest.Cancel();
                canceled++;
            }
        }
        Debug.Log($"[QuestTestManager] 활성 퀘스트 취소 완료: {canceled}개");
    }

    private async System.Threading.Tasks.Task ResetAllQuestsAndReregister()
    {
        var db = Resources.Load<QuestDatabase>("QuestDatabase");
        if (db == null)
        {
            Debug.LogWarning("[QuestTestManager] Resources/QuestDatabase를 찾지 못했습니다.");
            return;
        }

        var svc = FindObjectOfType<FirebaseQuestService>();
        if (svc == null)
        {
            Debug.LogWarning("[QuestTestManager] FirebaseQuestService를 찾지 못했습니다.");
        }

        // Firebase 진행 데이터 일괄 삭제
        int deleted = 0;
        if (svc != null)
        {
            foreach (var q in db.Quests)
            {
                if (q == null) continue;
                await svc.DeleteQuestProgress(q.CodeName);
                deleted++;
            }
        }
        Debug.Log($"[QuestTestManager] Firebase 진행 데이터 삭제: {deleted}개");

        // 로컬 전부 리셋 (Active/Completed/업적/PlayerPrefs)
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.ResetAllForTesting();
        }

        // 재등록 + Firebase 즉시 동기화
        RegisterAllQuests();
        if (svc != null)
        {
            await svc.SyncAllActiveNow();
        }
        Debug.Log("[QuestTestManager] 초기화 후 재등록 및 Firebase 동기화 완료");
    }
    
    private void ClearInventory()
    {
        Debug.Log("=== 인벤토리 초기화 ===");
        
        var inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager를 찾을 수 없습니다!");
            return;
        }
        
        inventoryManager.ClearAllItems();
        Debug.Log("인벤토리가 초기화되었습니다!");
    }
    
    private void RemoveDuplicateItems()
    {
        Debug.Log("=== 중복 아이템 제거 ===");
        
        var inventoryManager = FindObjectOfType<InventoryManager>();
        if (inventoryManager == null)
        {
            Debug.LogWarning("InventoryManager를 찾을 수 없습니다!");
            return;
        }
        
        inventoryManager.RemoveDuplicateItems();
        Debug.Log("중복 아이템 제거 완료!");
    }
}
