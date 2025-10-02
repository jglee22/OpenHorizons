using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;

/// <summary>
/// 로그인 이후 Firebase에 저장된 퀘스트 등록/진행 상태를 로컬 QuestSystem으로 복원합니다.
/// </summary>
public class FirebaseQuestSync : MonoBehaviour
{
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private float maxWaitSeconds = 10.0f; // 의존성 준비 대기 최대 시간(증가)

    private DatabaseReference Root => FirebaseDatabase.DefaultInstance.RootReference;
    private string Uid => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (!loadOnStart) return;
        StartCoroutine(RestoreWhenReady());
    }

    private System.Collections.IEnumerator RestoreWhenReady()
    {
        float waited = 0f;
        // Firebase Auth, QuestSystem, QuestDatabase 준비까지 대기 (가능한 한 빨리)
        while ((FirebaseAuth.DefaultInstance.CurrentUser == null || QuestSystem.Instance == null || Resources.Load<QuestDatabase>("QuestDatabase") == null) && waited < maxWaitSeconds)
        {
            waited += Time.deltaTime;
            yield return null;
        }
        var _ = LoadAndRestoreQuests();
    }

    public async System.Threading.Tasks.Task LoadAndRestoreQuests()
    {
        // 로그인 대기 재시도
        float waited = 0f;
        while (Uid == null && waited < maxWaitSeconds)
        {
            await System.Threading.Tasks.Task.Delay(200);
            waited += 0.2f;
        }
        if (Uid == null)
        {
            Debug.Log("[FirebaseQuestSync] 로그인 유저 없음 - 복원 생략");
            return;
        }
        if (QuestSystem.Instance == null)
        {
            Debug.LogWarning("[FirebaseQuestSync] QuestSystem 인스턴스 없음");
            return;
        }

        var snapshot = await Root.Child($"users/{Uid}/quests").GetValueAsync();
        if (snapshot == null || !snapshot.Exists)
        {
            Debug.Log("[FirebaseQuestSync] 저장된 퀘스트 진행 없음");
            return;
        }

        var db = Resources.Load<QuestDatabase>("QuestDatabase");
        if (db == null)
        {
            Debug.LogWarning("[FirebaseQuestSync] Resources/QuestDatabase를 찾지 못했습니다.");
            return;
        }

        foreach (var child in snapshot.Children)
        {
            string questCode = child.Key;
            var stateToken = child.Child("state");
            var progressToken = child.Child("progress");
            var requiredToken = child.Child("required");
            var taskIndexToken = child.Child("taskIndex");
            var rewardClaimedToken = child.Child("rewardClaimed");

            string state = stateToken?.Value?.ToString() ?? "accepted";
            int progress = ParseInt(progressToken?.Value);
            int required = ParseInt(requiredToken?.Value);
            int taskIndex = ParseInt(taskIndexToken?.Value);
            bool rewardClaimed = ParseBool(rewardClaimedToken?.Value);

            Quest sourceQuest = db.FindQuestBy(questCode);
            if (sourceQuest == null)
            {
                Debug.LogWarning($"[FirebaseQuestSync] QuestDatabase에서 찾을 수 없음: {questCode}");
                continue;
            }

            // 이미 활성/완료 목록에 있는지 검사
            if (!QuestSystem.Instance.ContainsInActiveQuests(sourceQuest) &&
                !QuestSystem.Instance.ContainsInCompleteQuests(sourceQuest))
            {
                // 등록
                var active = QuestSystem.Instance.Register(sourceQuest);
                // 보상 수령 상태를 먼저 반영하고 진행 복원
                active.MarkRewardClaimed(rewardClaimed);
                RestoreProgress(active, taskIndex, progress, required, state, rewardClaimed);
            }
            else
            {
                // 이미 존재 - 가능한 경우 진행 갱신
                var activeList = QuestSystem.Instance.ActiveQuests;
                foreach (var q in activeList)
                {
                    if (q.CodeName == questCode)
                    {
                        q.MarkRewardClaimed(rewardClaimed);
                        RestoreProgress(q, taskIndex, progress, required, state, rewardClaimed);
                        break;
                    }
                }
            }
        }
    }

    private static int ParseInt(object value)
    {
        if (value == null) return 0;
        int parsed;
        return int.TryParse(value.ToString(), out parsed) ? parsed : 0;
    }

    private static bool ParseBool(object value)
    {
        if (value == null) return false;
        bool parsed;
        if (bool.TryParse(value.ToString(), out parsed)) return parsed;
        int asInt;
        if (int.TryParse(value.ToString(), out asInt)) return asInt != 0;
        return false;
    }

    private void RestoreProgress(Quest quest, int taskIndex, int progress, int required, string state, bool rewardAlreadyClaimed)
    {
        if (quest == null) return;

        // Task 그룹/인덱스 방어적 처리
        taskIndex = Mathf.Clamp(taskIndex, 0, Mathf.Max(0, quest.CurrentTaskGroup.Tasks.Count - 1));

        // 현재 그룹 시작은 Register 시점에 호출됨. 진행 수 복원
        var task = quest.CurrentTaskGroup.Tasks[taskIndex];
        if (required > 0)
        {
            // 필요 수가 변경되었으면 반영 (선택)
            // Task.NeedSuccessToComplete는 private set이므로 변경하지 않음
        }
        task.CurrentSuccess = Mathf.Clamp(progress, 0, task.NeedSuccessToComplete);

        if (state == "completed")
        {
            // Complete 내부에서 rewardClaimed를 검사하므로 안전하게 호출
            if (!quest.IsComplete)
                quest.Complete();
        }
    }
}


