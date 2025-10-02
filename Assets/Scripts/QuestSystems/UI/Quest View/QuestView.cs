using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestView : MonoBehaviour
{
    [SerializeField]
    private QuestListViewController questListViewController;
    [SerializeField]
    private QuestDetailView questDetailView;

    private void Start()
    {
        // 퀘스트 UI를 시작 시 비활성화
        gameObject.SetActive(false);
        
        var questSystem = QuestSystem.Instance;

        foreach (var quest in questSystem.ActiveQuests)
            AddQuestToActiveListView(quest);

        foreach (var quest in questSystem.CompletedQuests)
            AddQuestToCompletedListView(quest);

        questSystem.onQuestRegistered += AddQuestToActiveListView;
        questSystem.onQuestCompleted += RemoveQuestFromActiveListView;
        questSystem.onQuestCompleted += AddQuestToCompletedListView;
        questSystem.onQuestCompleted += HideDetailIfQuestCanceled;
        questSystem.onQuestCanceled += HideDetailIfQuestCanceled;
        questSystem.onQuestCanceled += RemoveQuestFromActiveListView;

        foreach (var tab in questListViewController.Tabs)
            tab.onValueChanged.AddListener(HideDetail);
    }

    private void OnDestroy()
    {
        var questSystem = QuestSystem.Instance;
        if (questSystem)
        {
            questSystem.onQuestRegistered -= AddQuestToActiveListView;
            questSystem.onQuestCompleted -= RemoveQuestFromActiveListView;
            questSystem.onQuestCompleted -= AddQuestToCompletedListView;
            questSystem.onQuestCompleted -= HideDetailIfQuestCanceled;
            questSystem.onQuestCanceled -= HideDetailIfQuestCanceled;
            questSystem.onQuestCanceled -= RemoveQuestFromActiveListView;
        }
    }

    private void OnEnable()
    {
        if (questDetailView.Target != null)
            questDetailView.Show(questDetailView.Target);
    }

    private void Update()
    {
        // ESC키로 퀘스트 UI 닫기 (활성화된 상태에서만)
        if (Input.GetKeyDown(KeyCode.Escape))
            CloseQuestUI();
    }
    
    /// <summary>
    /// 퀘스트 UI 열기
    /// </summary>
    public void OpenQuestUI()
    {
        gameObject.SetActive(true);
        Time.timeScale = 0f; // 게임 일시정지
        Cursor.lockState = CursorLockMode.None; // 마우스 커서 활성화
        Cursor.visible = true;
    }
    
    /// <summary>
    /// 퀘스트 UI 닫기
    /// </summary>
    public void CloseQuestUI()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f; // 게임 재개
        Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 숨김
        Cursor.visible = false;
    }
    
    /// <summary>
    /// 퀘스트 UI 토글 (열기/닫기)
    /// </summary>
    public void ToggleQuestUI()
    {
        if (gameObject.activeInHierarchy)
            CloseQuestUI();
        else
            OpenQuestUI();
    }

    private void ShowDetail(bool isOn, Quest quest)
    {
        if (isOn)
            questDetailView.Show(quest);
    }

    private void HideDetail(bool isOn)
    {
        questDetailView.Hide();
    }

    private void AddQuestToActiveListView(Quest quest)
        => questListViewController.AddQuestToActiveListView(quest, isOn => ShowDetail(isOn, quest));

    private void AddQuestToCompletedListView(Quest quest)
        => questListViewController.AddQuestToCompletedListView(quest, isOn => ShowDetail(isOn, quest));

    private void HideDetailIfQuestCanceled(Quest quest)
    {
        if (questDetailView.Target == quest)
            questDetailView.Hide();
    }

    private void RemoveQuestFromActiveListView(Quest quest)
    {
        questListViewController.RemoveQuestFromActiveListView(quest);
        if (questDetailView.Target == quest)
            questDetailView.Hide();
    }
}
