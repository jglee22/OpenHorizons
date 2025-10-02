using System;
using System.Collections.Generic;
using System.Linq;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

/// <summary>
/// 퀘스트 수락/진행/완료/취소를 Firebase Realtime Database에 기록하는 서비스
/// </summary>
public class FirebaseQuestService : MonoBehaviour
{
    private DatabaseReference Root => FirebaseDatabase.DefaultInstance.RootReference;
    private string Uid => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

    private void Awake()
    {
        // 중복 생성 방지
        var existing = FindObjectsOfType<FirebaseQuestService>();
        if (existing.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // QuestSystem 이벤트 구독
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.onQuestRegistered += OnQuestRegistered;
            QuestSystem.Instance.onQuestCompleted += OnQuestCompleted;
            QuestSystem.Instance.onQuestCanceled += OnQuestCanceled;

            // 이미 활성화된 퀘스트들의 진행도 변경도 추적 (씬 초기화 순서로 인해 OnQuestRegistered를 놓친 경우 대비)
            foreach (var q in QuestSystem.Instance.ActiveQuests)
            {
                // 중복 구독 방지 위해 먼저 해제 후 구독
                q.onTaskSuccessChanged -= OnTaskSuccessChanged;
                q.onTaskSuccessChanged += OnTaskSuccessChanged;
            }
        }
    }

    private void OnDisable()
    {
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.onQuestRegistered -= OnQuestRegistered;
            QuestSystem.Instance.onQuestCompleted -= OnQuestCompleted;
            QuestSystem.Instance.onQuestCanceled -= OnQuestCanceled;
        }
    }

    private void OnQuestRegistered(Quest quest)
    {
        // 진행 변경 구독
        quest.onTaskSuccessChanged += OnTaskSuccessChanged;

        // 최초 등록 로그
        _ = LogAccepted(quest);
    }

    private void OnQuestCompleted(Quest quest)
    {
        _ = LogCompleted(quest);
        quest.onTaskSuccessChanged -= OnTaskSuccessChanged;
    }

    private void OnQuestCanceled(Quest quest)
    {
        _ = LogCanceled(quest);
        quest.onTaskSuccessChanged -= OnTaskSuccessChanged;
    }

    private void OnTaskSuccessChanged(Quest quest, global::Task task, int currentSuccess, int prevSuccess)
    {
        _ = LogProgress(quest, task, currentSuccess);
    }

    // ========================= Firebase Writers ========================= //
    private async System.Threading.Tasks.Task LogAccepted(Quest quest)
    {
        if (Uid == null || quest == null) return;
        var now = DateTime.UtcNow.ToString("o");
        var path = $"users/{Uid}/quests/{quest.CodeName}";

        int required = 0;
        if (quest.CurrentTaskGroup != null && quest.CurrentTaskGroup.Tasks != null && quest.CurrentTaskGroup.Tasks.Count > 0)
        {
            required = quest.CurrentTaskGroup.Tasks[0].NeedSuccessToComplete;
        }

        var data = new Dictionary<string, object>
        {
            ["state"] = "accepted",
            ["progress"] = 0,
            ["required"] = required,
            ["taskIndex"] = 0,
            ["category"] = quest.Category?.DisplayName ?? string.Empty,
            ["acceptedAt"] = now,
            ["updatedAt"] = now,
        };
        await Root.Child(path).UpdateChildrenAsync(data);
    }

    /// <summary>
    /// 현재 활성 퀘스트들을 Firebase에 즉시 동기화(accepted 상태 기록)
    /// </summary>
    public async System.Threading.Tasks.Task SyncAllActiveNow()
    {
        if (Uid == null || QuestSystem.Instance == null) return;
        var active = QuestSystem.Instance.ActiveQuests;
        foreach (var quest in active)
        {
            try
            {
                await LogAccepted(quest);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FirebaseQuestService] SyncAllActiveNow 실패: {quest?.CodeName} - {e.Message}");
            }
        }
    }

    private async System.Threading.Tasks.Task LogProgress(Quest quest, global::Task task, int progress)
    {
        if (Uid == null || quest == null || task == null) return;

        var path = $"users/{Uid}/quests/{quest.CodeName}";
        var state = progress >= task.NeedSuccessToComplete ? "completed" : "in_progress";
        int taskIndex = 0;
        try
        {
            var tasks = quest.CurrentTaskGroup?.Tasks;
            if (tasks != null)
            {
                var idx = tasks.ToList().IndexOf(task);
                if (idx >= 0) taskIndex = idx;
            }
        }
        catch { /* 안전하게 무시 */ }
        var data = new Dictionary<string, object>
        {
            ["state"] = state,
            ["progress"] = progress,
            ["required"] = task.NeedSuccessToComplete,
            ["taskIndex"] = taskIndex,
            ["updatedAt"] = DateTime.UtcNow.ToString("o"),
            ["rewardClaimed"] = (state == "completed") ? (object)(quest != null && GetRewardClaimed(quest)) : (object)false
        };
        await Root.Child(path).UpdateChildrenAsync(data);
    }

    private async System.Threading.Tasks.Task LogCompleted(Quest quest)
    {
        if (Uid == null || quest == null) return;
        var now = DateTime.UtcNow.ToString("o");
        var data = new Dictionary<string, object>
        {
            ["state"] = "completed",
            ["completedAt"] = now,
            ["updatedAt"] = now,
            ["rewardClaimed"] = GetRewardClaimed(quest)
        };
        await Root.Child($"users/{Uid}/quests/{quest.CodeName}").UpdateChildrenAsync(data);
    }

    private static bool GetRewardClaimed(Quest quest)
    {
        // Quest 내부 저장 플래그를 ToSaveData/LoadFrom로 동기화하므로 여기서는 상태 유추: Complete이면 true 추정
        // 더 정확히 하려면 Quest에 공개 접근자 추가 가능
        return quest != null && quest.IsComplete;
    }

    private async System.Threading.Tasks.Task LogCanceled(Quest quest)
    {
        if (Uid == null || quest == null) return;
        var path = $"users/{Uid}/quests/{quest.CodeName}";
        await Root.Child(path + "/state").SetValueAsync("canceled");
        await Root.Child(path + "/updatedAt").SetValueAsync(DateTime.UtcNow.ToString("o"));
    }

    // 외부에서 호출: 특정 퀘스트 진행 데이터 삭제
    public async System.Threading.Tasks.Task DeleteQuestProgress(string questCodeName)
    {
        if (Uid == null || string.IsNullOrEmpty(questCodeName)) return;
        var path = $"users/{Uid}/quests/{questCodeName}";
        await Root.Child(path).RemoveValueAsync();
    }
}


