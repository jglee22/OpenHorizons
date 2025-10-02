using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 개별 퀘스트 안내 아이템
/// </summary>
public class QuestGuidanceItem : MonoBehaviour
{
    public Quest Quest { get; private set; }
    
    private Text titleText;
    private Text descriptionText;
    private Text progressText;
    
    public void Initialize(Quest quest, Text title, Text description, Text progress)
    {
        Quest = quest;
        titleText = title;
        descriptionText = description;
        progressText = progress;
        
        // 퀘스트 이벤트 구독
        Quest.onTaskSuccessChanged += OnTaskSuccessChanged;
        
        UpdateDisplay();
    }
    
    private void OnDestroy()
    {
        if (Quest != null)
        {
            Quest.onTaskSuccessChanged -= OnTaskSuccessChanged;
        }
    }
    
    private void OnTaskSuccessChanged(Quest quest, Task task, int currentSuccess, int prevSuccess)
    {
        UpdateDisplay();
    }
    
    private void UpdateDisplay()
    {
        if (Quest == null) return;
        
        // 제목 업데이트
        titleText.text = Quest.DisplayName;
        
        // 설명 업데이트
        descriptionText.text = Quest.Description;
        
        // 진행 상황 업데이트
        UpdateProgressText();
    }
    
    private void UpdateProgressText()
    {
        if (Quest.CurrentTaskGroup == null)
        {
            progressText.text = "진행 상황 없음";
            return;
        }
        
        string progressInfo = "";
        
        foreach (var task in Quest.CurrentTaskGroup.Tasks)
        {
            if (task.Action != null)
            {
                string taskDescription = GetTaskDescription(task);
                string progress = $"{task.CurrentSuccess}/{task.NeedSuccessToComplete}";
                
                if (task.IsComplete)
                {
                    progressInfo += $"✓ {taskDescription} (완료)\n";
                }
                else
                {
                    progressInfo += $"○ {taskDescription}: {progress}\n";
                }
            }
        }
        
        progressText.text = progressInfo.TrimEnd('\n');
    }
    
    private string GetTaskDescription(Task task)
    {
        if (task.Action is KillEnemyAction killAction)
        {
            return $"적 처치 ({killAction.enemyId})";
        }
        else if (task.Action is CollectItemAction collectAction)
        {
            return $"아이템 수집 ({collectAction.itemId})";
        }
        else if (task.Action is ReachLocationAction locationAction)
        {
            return $"위치 도달 ({locationAction.locationName})";
        }
        else if (task.Action is TalkToNPCAction npcAction)
        {
            return $"NPC 대화 ({npcAction.npcId})";
        }
        else if (task.Action is SurviveTimeAction timeAction)
        {
            return $"생존 시간 ({timeAction.timeInSeconds}초)";
        }
        
        return "알 수 없는 작업";
    }
}
