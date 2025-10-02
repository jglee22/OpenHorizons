using UnityEngine;

/// <summary>
/// 플레이어 입력을 처리하는 핸들러
/// </summary>
public class PlayerInputHandler : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private QuestView questView;
    
    [Header("키 설정")]
    [SerializeField] private KeyCode questKey = KeyCode.Q;
    [SerializeField] private KeyCode inventoryKey = KeyCode.I;
    
    private void Start()
    {
        // QuestView가 없으면 자동으로 찾기
        if (questView == null)
        {
            questView = FindObjectOfType<QuestView>();
        }
    }
    
    private void Update()
    {
        HandleUIInput();
    }
    
    private void HandleUIInput()
    {
        // 퀘스트 UI 토글
        if (Input.GetKeyDown(questKey))
        {
            if (questView != null)
            {
                questView.ToggleQuestUI();
            }
        }
        
        // 인벤토리 UI 토글 (예시)
        if (Input.GetKeyDown(inventoryKey))
        {
            // 인벤토리 UI 토글 로직
            Debug.Log("인벤토리 UI 토글");
        }
    }
}
