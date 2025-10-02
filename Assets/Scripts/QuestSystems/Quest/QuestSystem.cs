using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class QuestSystem : MonoBehaviour
{
    #region Save Path
    private const string kSaveRootPath = "questSystem";
    private const string kActiveQuestsSavePath = "activeQuests";
    private const string kCompletedQuestsSavePath = "completedQuests";
    private const string kActiveAchievementsSavePath = "activeAchievement";
    private const string kCompletedAchievementsSavePath = "completedAchievement";
    #endregion

    #region Events
    public delegate void QuestRegisteredHandler(Quest newQuest);
    public delegate void QuestCompletedHandler(Quest quest);
    public delegate void QuestCanceledHandler(Quest quest);
    #endregion

    private static QuestSystem instance;
    private static bool isApplicationQuitting;

    public static QuestSystem Instance
    {
        get
        {
            if (!isApplicationQuitting && instance == null)
            {
                instance = FindObjectOfType<QuestSystem>();
                if (instance == null)
                {
                    instance = new GameObject("Quest System").AddComponent<QuestSystem>();
                    DontDestroyOnLoad(instance.gameObject);
                }
            }
            return instance;
        }
    }

    private List<Quest> activeQuests = new List<Quest>();
    private List<Quest> completedQuests = new List<Quest>();

    private List<Quest> activeAchievements = new List<Quest>();
    private List<Quest> completedAchievements = new List<Quest>();

    private QuestDatabase questDatatabase;
    private QuestDatabase achievementDatabase;

    public event QuestRegisteredHandler onQuestRegistered;
    public event QuestCompletedHandler onQuestCompleted;
    public event QuestCanceledHandler onQuestCanceled;

    public event QuestRegisteredHandler onAchievementRegistered;
    public event QuestCompletedHandler onAchievementCompleted;

    public IReadOnlyList<Quest> ActiveQuests => activeQuests;
    public IReadOnlyList<Quest> CompletedQuests => completedQuests;
    public IReadOnlyList<Quest> ActiveAchievements => activeAchievements;
    public IReadOnlyList<Quest> CompletedAchievements => completedAchievements;

    private void Awake()
    {
        questDatatabase = Resources.Load<QuestDatabase>("QuestDatabase");
        achievementDatabase = Resources.Load<QuestDatabase>("AchievementDatabase");

        if (!Load())
        {
            foreach (var achievement in achievementDatabase.Quests)
                Register(achievement);
        }
    }

    private void OnApplicationQuit()
    {
        isApplicationQuitting = true;
        // 종료 시 퀘스트 진행 저장 (로컬 PlayerPrefs)
        try
        {
            Save();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[QuestSystem] 종료 저장 실패: {e.Message}");
        }
    }

    public Quest Register(Quest quest)
    {
        // 중복 등록 체크
        if (IsQuestAlreadyRegistered(quest.CodeName))
        {
            Debug.LogWarning($"[QuestSystem] 퀘스트가 이미 등록되어 있습니다: {quest.DisplayName} ({quest.CodeName})");
            return GetRegisteredQuest(quest.CodeName);
        }

        var newQuest = quest.Clone();

        if (newQuest is Achievement)
        {
            newQuest.onCompleted += OnAchievementCompleted;

            activeAchievements.Add(newQuest);

            newQuest.OnRegister();
            onAchievementRegistered?.Invoke(newQuest);
        }
        else
        {
            newQuest.onCompleted += OnQuestCompleted;
            newQuest.onCanceled += OnQuestCanceled;

            activeQuests.Add(newQuest);

            newQuest.OnRegister();
            onQuestRegistered?.Invoke(newQuest);
        }

        return newQuest;
    }

    /// <summary>
    /// 테스트용: 모든 퀘스트 상태를 초기화 (Active/Completed/업적 포함)하고 로컬 저장 데이터도 삭제
    /// </summary>
    public void ResetAllForTesting()
    {
        // 활성 퀘스트 취소 및 리스트 비우기
        var activeCopy = new List<Quest>(activeQuests);
        foreach (var q in activeCopy)
        {
            if (q != null)
                q.Cancel();
        }
        activeQuests.Clear();

        // 완료/업적 리스트 비우기
        completedQuests.Clear();
        activeAchievements.Clear();
        completedAchievements.Clear();

        // 로컬 저장 삭제
        if (PlayerPrefs.HasKey(kSaveRootPath))
            PlayerPrefs.DeleteKey(kSaveRootPath);

        Debug.Log("[QuestSystem] ResetAllForTesting: 모든 퀘스트 상태 초기화 완료");
    }
    
    /// <summary>
    /// 퀘스트가 이미 등록되어 있는지 확인
    /// </summary>
    private bool IsQuestAlreadyRegistered(string questCodeName)
    {
        bool isActive = activeQuests.Any(q => q.CodeName == questCodeName);
        bool isCompleted = completedQuests.Any(q => q.CodeName == questCodeName);
        bool isActiveAchievement = activeAchievements.Any(q => q.CodeName == questCodeName);
        bool isCompletedAchievement = completedAchievements.Any(q => q.CodeName == questCodeName);
        
        if (isActive || isCompleted || isActiveAchievement || isCompletedAchievement)
        {
            Debug.Log($"[QuestSystem] 퀘스트 중복 등록 방지: {questCodeName} (Active: {isActive}, Completed: {isCompleted}, ActiveAchievement: {isActiveAchievement}, CompletedAchievement: {isCompletedAchievement})");
        }
        
        return isActive || isCompleted || isActiveAchievement || isCompletedAchievement;
    }
    
    /// <summary>
    /// 등록된 퀘스트 가져오기
    /// </summary>
    private Quest GetRegisteredQuest(string questCodeName)
    {
        var quest = activeQuests.FirstOrDefault(q => q.CodeName == questCodeName);
        if (quest != null) return quest;
        
        quest = completedQuests.FirstOrDefault(q => q.CodeName == questCodeName);
        if (quest != null) return quest;
        
        quest = activeAchievements.FirstOrDefault(q => q.CodeName == questCodeName);
        if (quest != null) return quest;
        
        quest = completedAchievements.FirstOrDefault(q => q.CodeName == questCodeName);
        return quest;
    }

    public void ReceiveReport(string category, object target, int successCount)
    {
        Debug.Log($"[QuestSystem] 리포팅 시작 - 카테고리: {category}, 타겟: {target} ({target?.GetType()}), 성공수: {successCount}");
        Debug.Log($"[QuestSystem] 활성 퀘스트 수: {activeQuests.Count}");
        
        if (activeQuests.Count == 0)
        {
            Debug.LogWarning("[QuestSystem] 활성 퀘스트가 없습니다! F1키로 퀘스트를 등록하세요.");
        }
        
        ReceiveReport(activeQuests, category, target, successCount);
        Debug.Log("퀘스트 리포팅: " + category + " " + target + " " + successCount);    
        ReceiveReport(activeAchievements, category, target, successCount);
    }

    public void ReceiveReport(Category category, TaskTarget target, int successCount)
        => ReceiveReport(category.CodeName, target.Value, successCount);
    
    // 새로운 리포팅 메서드들 추가 - 모두 118라인을 통해 호출
    public void ReportItemCollected(string itemId, int amount = 1)
    {
        var collectionCategory = FindCategoryByCodeName("collection");
        if (collectionCategory != null)
        {
            var target = CreateStringTarget(itemId);
            ReceiveReport(collectionCategory, target, amount);
        }
    }
    
    public void ReportLocationReached(Vector3 position, string locationName = "")
    {
        var explorationCategory = FindCategoryByCodeName("exploration");
        if (explorationCategory != null)
        {
            var positionTarget = CreateLocationTarget(position, locationName);
            ReceiveReport(explorationCategory, positionTarget, 1);
            
            if (!string.IsNullOrEmpty(locationName))
            {
                var nameTarget = CreateStringTarget(locationName);
                ReceiveReport(explorationCategory, nameTarget, 1);
            }
        }
    }
    
    public void ReportNPCTalked(string npcId, string npcName = "")
    {
        var socialCategory = FindCategoryByCodeName("social");
        if (socialCategory != null)
        {
            var target = CreateStringTarget(npcId);
            ReceiveReport(socialCategory, target, 1);
            
            if (!string.IsNullOrEmpty(npcName))
            {
                var nameTarget = CreateStringTarget(npcName);
                ReceiveReport(socialCategory, nameTarget, 1);
            }
        }
    }
    
    public void ReportTimeElapsed(float timeInSeconds)
    {
        var survivalCategory = FindCategoryByCodeName("survival");
        if (survivalCategory != null)
        {
            var target = CreateStringTarget("Time");
            ReceiveReport(survivalCategory, target, Mathf.RoundToInt(timeInSeconds));
        }
    }
    
    public void ReportEnemyKilled(string enemyId, int amount = 1)
    {
        var combatCategory = FindCategoryByCodeName("combat");
        if (combatCategory != null)
        {
            var target = CreateStringTarget(enemyId);
            ReceiveReport(combatCategory, target, amount);
        }
    }
    
    // 헬퍼 메서드들
    private Category FindCategoryByCodeName(string codeName)
    {
        // QuestDatabase에서 카테고리 찾기
        var db = Resources.Load<QuestDatabase>("QuestDatabase");
        if (db?.Quests != null)
        {
            foreach (var quest in db.Quests)
            {
                if (quest.CurrentTaskGroup != null)
                {
                    foreach (var task in quest.CurrentTaskGroup.Tasks)
                    {
                        if (task.Category != null && 
                            string.Equals(task.Category.CodeName, codeName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return task.Category;
                        }
                    }
                }
            }
        }
        return null;
    }
    
    private StringTarget CreateStringTarget(string value)
    {
        var target = ScriptableObject.CreateInstance<StringTarget>();
        target.value = value;
        return target;
    }
    
    private LocationTarget CreateLocationTarget(Vector3 position, string locationName = "")
    {
        var target = ScriptableObject.CreateInstance<LocationTarget>();
        target.targetPosition = position;
        target.locationName = locationName;
        target.reachDistance = 5f;
        return target;
    }

    private void ReceiveReport(List<Quest> quests, string category, object target, int successCount)
    {
        foreach (var quest in quests.ToArray())
            quest.ReceiveReport(category, target, successCount);
    }

    public void CompleteWaitingQuests()
    {
        foreach (var quest in activeQuests.ToList())
        {
            if (quest.IsComplatable)
                quest.Complete();
        }
    }

    public void Cancel(Quest quest)
    {
        if (quest != null && activeQuests.Contains(quest))
        {
            quest.Cancel();
        }
    }

    public bool ContainsInActiveQuests(Quest quest) => activeQuests.Any(x => x.CodeName == quest.CodeName);

    public bool ContainsInCompleteQuests(Quest quest) => completedQuests.Any(x => x.CodeName == quest.CodeName);

    public bool ContainsInActiveAchievements(Quest quest) => activeAchievements.Any(x => x.CodeName == quest.CodeName);
    
    // 디버그용 메서드
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public void DebugActiveQuests()
    {
        Debug.Log("=== 활성 퀘스트 디버그 ===");
        Debug.Log($"활성 퀘스트 수: {activeQuests.Count}");
        
        foreach (var quest in activeQuests)
        {
            Debug.Log($"퀘스트: {quest.DisplayName} ({quest.CodeName})");
            Debug.Log($"  상태: {quest.State}");
            
            if (quest.CurrentTaskGroup != null)
            {
                foreach (var task in quest.CurrentTaskGroup.Tasks)
                {
                    Debug.Log($"    태스크: {task.Description}");
                    Debug.Log($"      카테고리: {task.Category?.CodeName} ({task.Category?.DisplayName})");
                    Debug.Log($"      진행도: {task.CurrentSuccess}/{task.NeedSuccessToComplete}");
                    Debug.Log($"      상태: {task.State}");
                    
                    // 타겟 정보
                    if (task.Action != null)
                    {
                        Debug.Log($"      액션: {task.Action.GetType().Name}");
                    }
                }
            }
        }
        Debug.Log("=== 디버그 끝 ===");
    }
    
    // 직접 테스트용 메서드
    public void TestEnemyKillReport()
    {
        Debug.Log("=== 적 처치 리포팅 테스트 ===");
        
        // 1. 카테고리 찾기 테스트
        var combatCategory = FindCategoryByCodeName("combat");
        Debug.Log($"Combat 카테고리 찾기: {(combatCategory != null ? combatCategory.CodeName : "null")}");
        
        // 2. 타겟 생성 테스트
        var target = CreateStringTarget("Grunt");
        Debug.Log($"StringTarget 생성: {target.value}");
        
        // 3. 직접 리포팅 테스트
        if (combatCategory != null)
        {
            Debug.Log("직접 리포팅 시도...");
            ReceiveReport(combatCategory, target, 1);
        }
        
        Debug.Log("=== 테스트 끝 ===");
    }

    public bool ContainsInCompletedAchievements(Quest quest) => completedAchievements.Any(x => x.CodeName == quest.CodeName);

    public void Save()
    {
        var root = new JObject();
        root.Add(kActiveQuestsSavePath, CreateSaveDatas(activeQuests));
        root.Add(kCompletedQuestsSavePath, CreateSaveDatas(completedQuests));
        root.Add(kActiveAchievementsSavePath, CreateSaveDatas(activeAchievements));
        root.Add(kCompletedAchievementsSavePath, CreateSaveDatas(completedAchievements));

        PlayerPrefs.SetString(kSaveRootPath, root.ToString());
        PlayerPrefs.Save();
    }

    public bool Load()
    {
        if (PlayerPrefs.HasKey(kSaveRootPath))
        {
            var root = JObject.Parse(PlayerPrefs.GetString(kSaveRootPath));

            LoadSaveDatas(root[kActiveQuestsSavePath], questDatatabase, LoadActiveQuest);
            LoadSaveDatas(root[kCompletedQuestsSavePath], questDatatabase, LoadCompletedQuest);

            LoadSaveDatas(root[kActiveAchievementsSavePath], achievementDatabase, LoadActiveQuest);
            LoadSaveDatas(root[kCompletedAchievementsSavePath], achievementDatabase, LoadCompletedQuest);

            return true;
        }
        else
            return false;
    }

    private JArray CreateSaveDatas(IReadOnlyList<Quest> quests)
    {
        var saveDatas = new JArray();
        foreach (var quest in quests)
        {
            if (quest.IsSavable)
                saveDatas.Add(JObject.FromObject(quest.ToSaveData()));
        }
        return saveDatas;
    }

    private void LoadSaveDatas(JToken datasToken, QuestDatabase database, System.Action<QuestSaveData, Quest> onSuccess)
    {
        var datas = datasToken as JArray;
        foreach (var data in datas)
        {
            var saveData = data.ToObject<QuestSaveData>();
            var quest = database.FindQuestBy(saveData.codeName);
            onSuccess.Invoke(saveData, quest);
        }
    }
    
    private void LoadActiveQuest(QuestSaveData saveData, Quest quest)
    {
        var newQuest = Register(quest);
        newQuest.LoadFrom(saveData);
    }

    private void LoadCompletedQuest(QuestSaveData saveData, Quest quest)
    {
        var newQuest = quest.Clone();
        newQuest.LoadFrom(saveData);

        if (newQuest is Achievement)
            completedAchievements.Add(newQuest);
        else
            completedQuests.Add(newQuest);
    }

    #region Callback
    private void OnQuestCompleted(Quest quest)
    {
        activeQuests.Remove(quest);
        completedQuests.Add(quest);

        onQuestCompleted?.Invoke(quest);
    }

    private void OnQuestCanceled(Quest quest)
    {
        activeQuests.Remove(quest);
        onQuestCanceled?.Invoke(quest);

        Destroy(quest, Time.deltaTime);
    }

    private void OnAchievementCompleted(Quest achievement)
    {
        activeAchievements.Remove(achievement);
        completedAchievements.Add(achievement);

        onAchievementCompleted?.Invoke(achievement);
    }
    #endregion
}
