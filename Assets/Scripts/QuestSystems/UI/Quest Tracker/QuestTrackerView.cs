using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestTrackerView : MonoBehaviour
{
    [SerializeField]
    private QuestTracker questTrackerPrefab;
    [SerializeField]
    private CategoryColor[] categoryColors;

    private readonly Dictionary<string, QuestTracker> codeNameToTracker = new Dictionary<string, QuestTracker>();

    private void Start()
    {
        // Start에서도 한 번 초기화 (씬 진입 직후 케이스)
        SafeSubscribe();
        RefreshAllActive();
    }

    private void OnDestroy()
    {
        if (QuestSystem.Instance)
        {
            QuestSystem.Instance.onQuestRegistered -= CreateQuestTracker;
            QuestSystem.Instance.onQuestCompleted -= RemoveQuestTracker;
            QuestSystem.Instance.onQuestCanceled -= RemoveQuestTracker;
        }
    }

    private void OnEnable()
    {
        // 활성화 시마다 보장
        SafeSubscribe();
        RefreshAllActive();
    }

    private void SafeSubscribe()
    {
        if (QuestSystem.Instance == null) return;
        // 중복 구독 방지 위해 일단 해제 후 구독
        QuestSystem.Instance.onQuestRegistered -= CreateQuestTracker;
        QuestSystem.Instance.onQuestCompleted -= RemoveQuestTracker;
        QuestSystem.Instance.onQuestCanceled -= RemoveQuestTracker;

        QuestSystem.Instance.onQuestRegistered += CreateQuestTracker;
        QuestSystem.Instance.onQuestCompleted += RemoveQuestTracker;
        QuestSystem.Instance.onQuestCanceled += RemoveQuestTracker;
        Debug.Log("[QuestTrackerView] Subscribed quest events");
    }

    private void RefreshAllActive()
    {
        if (QuestSystem.Instance == null) return;
        Debug.Log($"[QuestTrackerView] RefreshAllActive count={QuestSystem.Instance.ActiveQuests.Count}");
        foreach (var quest in QuestSystem.Instance.ActiveQuests)
            CreateQuestTracker(quest);
    }

    // 외부에서 강제로 트래커를 다시 만들고 싶을 때 사용
    public void ForceRefresh()
    {
        // 기존 트래커 제거 후 다시 생성
        foreach (var pair in codeNameToTracker)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }
        codeNameToTracker.Clear();
        RefreshAllActive();
    }

    private void CreateQuestTracker(Quest quest)
    {
        if (quest == null)
        {
            Debug.LogWarning("[QuestTrackerView] CreateQuestTracker called with null quest");
            return;
        }
        if (questTrackerPrefab == null)
        {
            Debug.LogError("[QuestTrackerView] questTrackerPrefab is not assigned");
            return;
        }
        if (string.IsNullOrEmpty(quest.CodeName))
        {
            Debug.LogWarning("[QuestTrackerView] Quest CodeName is empty; tracker may collide");
        }
        if (codeNameToTracker.ContainsKey(quest.CodeName))
        {
            Debug.Log($"[QuestTrackerView] Tracker already exists for {quest.CodeName}");
            return; // 중복 방지
        }

        var categoryColor = categoryColors.FirstOrDefault(x => x.category == quest.Category);
        var color = categoryColor.category == null ? Color.white : categoryColor.color;
        var tracker = Instantiate(questTrackerPrefab, transform);
        tracker.Setup(quest, color);
        codeNameToTracker[quest.CodeName] = tracker;
        Debug.Log($"[QuestTrackerView] Tracker created: {quest.DisplayName} ({quest.CodeName})");
    }

    private void RemoveQuestTracker(Quest quest)
    {
        if (quest == null) return;
        if (!codeNameToTracker.TryGetValue(quest.CodeName, out var tracker)) return;
        if (tracker != null)
            Destroy(tracker.gameObject);
        codeNameToTracker.Remove(quest.CodeName);
    }

    [System.Serializable]
    private struct CategoryColor
    {
        public Category category;
        public Color color;
    }
}