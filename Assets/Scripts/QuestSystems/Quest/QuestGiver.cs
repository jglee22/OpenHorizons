using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestGiver : MonoBehaviour, IInteractable
{
    [Header("Quest Settings")]
    [SerializeField]
    private Quest[] quests;
    
    [Header("Interaction")]
    [SerializeField]
    private float interactionRange = 3f;
    [SerializeField]
    private string interactionPrompt = "대화";
    
    [Header("Dialogue")]
    [SerializeField]
    private string[] dialogueLines = {
        "안녕하세요! 도움이 필요한 일이 있습니다.",
        "이 퀘스트를 완료해주시겠어요?",
        "감사합니다! 퀘스트를 수락해주셨네요."
    };
    
    [Header("UI")]
    [SerializeField]
    private GameObject dialogueUI;
    [SerializeField]
    private TMPro.TextMeshProUGUI dialogueText;
    
    private int currentDialogueIndex = 0;
    private bool isShowingDialogue = false;
    private float lastInteractionTime = 0f;
    private float dialogueTimeout = 2f; // 2초 타임아웃
    private float maxDialogueDistance = 5f; // 최대 대화 거리
    
    private QuestSystem questSystem;
    private bool canGiveQuest = true;
    private bool canCompleteQuest = false;
    
    public bool CanInteract => true; // 임시로 항상 true로 설정
    
    private void Start()
    {
        questSystem = QuestSystem.Instance;
        
        // 초기 상태 설정(자동 등록 없음, 상호작용 시 수락)
        UpdateQuestState();
    }
    
    private void Update()
    {
        if (isShowingDialogue)
        {
            CheckDialogueConditions();
        }
    }
    
    void UpdateQuestState()
    {
        Debug.Log($"[QuestGiver] UpdateQuestState 시작 - questSystem: {questSystem != null}, quests: {quests?.Length ?? 0}");
        
        if (questSystem == null || quests == null || quests.Length == 0) 
        {
            Debug.LogWarning($"[QuestGiver] 초기화 실패 - questSystem: {questSystem != null}, quests: {quests?.Length ?? 0}");
            return;
        }
        
        // 첫 번째 퀘스트 기준으로 상태 결정
        var quest = quests[0];
        Debug.Log($"[QuestGiver] 퀘스트 확인: {quest?.DisplayName}, CodeName: {quest?.CodeName}");
        
        // 활성/완료 상태를 코드네임 기준으로 판단
        bool isActive = questSystem.ContainsInActiveQuests(quest);
        bool isCompleted = questSystem.ContainsInCompleteQuests(quest);
        
        Debug.Log($"[QuestGiver] 퀘스트 상태 - Active: {isActive}, Completed: {isCompleted}");
        
        if (isActive)
        {
            // 이미 수락한 퀘스트인 경우
            if (isCompleted)
            {
                canGiveQuest = false;
                canCompleteQuest = true;
                Debug.Log($"[QuestGiver] 완료된 퀘스트 - canGiveQuest: false, canCompleteQuest: true");
            }
            else
            {
                canGiveQuest = false;
                canCompleteQuest = false; // 진행 중인 퀘스트는 완료 불가
                Debug.Log($"[QuestGiver] 진행 중인 퀘스트 - canGiveQuest: false, canCompleteQuest: false");
            }
        }
        else
        {
            // 아직 수락하지 않은 퀘스트
            canGiveQuest = true;
            canCompleteQuest = false;
            Debug.Log($"[QuestGiver] 새로운 퀘스트 - canGiveQuest: true, canCompleteQuest: false");
        }
        
        Debug.Log($"[QuestGiver] 최종 상태 - CanInteract: {CanInteract}");
    }
    
    public void Interact()
    {
        Debug.Log($"[QuestGiver] Interact() 호출됨 - CanInteract: {CanInteract}, quests: {quests?.Length ?? 0}");
        
        if (!CanInteract || quests == null || quests.Length == 0) 
        {
            Debug.LogWarning($"[QuestGiver] 상호작용 불가 - CanInteract: {CanInteract}, quests: {quests?.Length ?? 0}");
            return;
        }
        
        var quest = quests[0]; // 첫 번째 퀘스트 사용
        Debug.Log($"[QuestGiver] 퀘스트 처리 시작: {quest?.DisplayName}");
        
        if (canGiveQuest)
        {
            Debug.Log($"[QuestGiver] 퀘스트 수락 시도");
            GiveQuest(quest);
        }
        else if (canCompleteQuest)
        {
            Debug.Log($"[QuestGiver] 퀘스트 완료 시도");
            CompleteQuest(quest);
        }
        else
        {
            Debug.Log($"[QuestGiver] 대화만 표시");
            StartDialogueSequence(); // 다이얼로그 시퀀스 시작
        }
    }
    
    public string GetInteractionPrompt()
    {
        if (canGiveQuest)
            return "대화";
        else if (canCompleteQuest)
            return "퀘스트 완료";
        else
            return "대화";
    }
    
    public bool IsInRange(Vector3 playerPosition)
    {
        return Vector3.Distance(transform.position, playerPosition) <= interactionRange;
    }
    
    void GiveQuest(Quest quest)
    {
        if (questSystem == null || quest == null) return;
        
        // 퀘스트 수락(Register는 중복 시 기존 인스턴스를 반환)
        var registered = questSystem.Register(quest);
        
        if (registered != null)
        {
            Debug.Log($"퀘스트 수락: {quest.DisplayName}");
            
            // 대화 표시
            ShowDialogue(2); // 감사 메시지
            
            // 상태 업데이트
            UpdateQuestState();
        }
        else
        {
            Debug.LogWarning($"퀘스트 수락 실패: {quest.DisplayName}");
        }
    }
    
    void CompleteQuest(Quest quest)
    {
        if (questSystem == null || quest == null) return;
        
        // 활성 목록에서 해당 퀘스트 인스턴스 탐색 후 완료 조건 시 완료 처리
        var active = questSystem.ActiveQuests.FirstOrDefault(q => q.CodeName == quest.CodeName);
        if (active != null)
        {
            if (active.IsComplatable)
            {
                active.Complete();
            }
            Debug.Log($"퀘스트 완료: {quest.DisplayName}");
            
            // 보상 지급 (퀘스트 시스템에서 자동 처리)
            ShowDialogue(0); // 완료 메시지
            
            // 상태 업데이트
            UpdateQuestState();
        }
        else
        {
            Debug.LogWarning($"퀘스트 완료 실패: {quest.DisplayName}");
        }
    }
    
    void StartDialogueSequence()
    {
        if (isShowingDialogue) return;
        
        currentDialogueIndex = 0;
        isShowingDialogue = true;
        lastInteractionTime = Time.time;
        ShowDialogue(currentDialogueIndex);
        currentDialogueIndex++;
    }
    
    public void NextDialogue()
    {
        if (!isShowingDialogue) return;
        
        lastInteractionTime = Time.time; // 상호작용 시간 업데이트
        
        if (currentDialogueIndex >= dialogueLines.Length)
        {
            // 모든 다이얼로그 완료 - 2초 후 UI 숨김
            isShowingDialogue = false;
            Invoke(nameof(HideDialogue), 2f);
            return;
        }
        
        ShowDialogue(currentDialogueIndex);
        currentDialogueIndex++;
    }
    
    void ShowDialogue(int dialogueIndex)
    {
        if (dialogueIndex < 0 || dialogueIndex >= dialogueLines.Length) return;
        
        string dialogue = dialogueLines[dialogueIndex];
        Debug.Log($"[{gameObject.name}] {dialogue}");
        
        // UI에 다이얼로그 표시
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(true);
        }
        
        if (dialogueText != null)
        {
            dialogueText.text = dialogue;
        }
    }
    
    void HideDialogue()
    {
        if (dialogueUI != null)
        {
            dialogueUI.SetActive(false);
        }
    }
    
    public bool IsShowingDialogue()
    {
        return isShowingDialogue;
    }
    
    void CheckDialogueConditions()
    {
        // 1. 타임아웃 체크 (2초 이상 입력 없음)
        if (Time.time - lastInteractionTime > dialogueTimeout)
        {
            Debug.Log($"[QuestGiver] 다이얼로그 타임아웃 - UI 숨김");
            ForceHideDialogue();
            return;
        }
        
        // 2. 거리 체크 (플레이어가 너무 멀어짐)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance > maxDialogueDistance)
            {
                Debug.Log($"[QuestGiver] 플레이어 거리 초과 - UI 숨김 (거리: {distance:F1})");
                ForceHideDialogue();
                return;
            }
        }
    }
    
    void ForceHideDialogue()
    {
        isShowingDialogue = false;
        currentDialogueIndex = 0;
        HideDialogue();
    }
    
    // 퀘스트 상태가 변경될 때 호출 (외부에서)
    public void RefreshQuestState()
    {
        UpdateQuestState();
    }
    
    void OnDrawGizmosSelected()
    {
        // 상호작용 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
