using UnityEngine;

/// <summary>
/// 퀘스트 UI 열기/닫기를 관리하는 매니저
/// </summary>
public class QuestUIManager : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private QuestView questView;
    
    [Header("키 설정")]
    [SerializeField] private KeyCode questToggleKey = KeyCode.Q; // Q키로 퀘스트 UI 토글
    
    private void Start()
    {
        // QuestView가 없으면 자동으로 찾기
        if (questView == null)
        {
            questView = FindObjectOfType<QuestView>();
        }
        
        if (questView == null)
        {
            Debug.LogError("[QuestUIManager] QuestView를 찾을 수 없습니다!");
        }
    }
    
    private void Update()
    {
        // J키로 퀘스트 UI 토글
        if (Input.GetKeyDown(questToggleKey))
        {
            ToggleQuestUI();
        }
    }
    
    /// <summary>
    /// 퀘스트 UI 토글 (열기/닫기)
    /// </summary>
    public void ToggleQuestUI()
    {
        if (questView != null)
        {
            bool isActive = questView.gameObject.activeInHierarchy;
            questView.gameObject.SetActive(!isActive);
            
            // UI가 열릴 때 게임 일시정지 (선택사항)
            if (!isActive)
            {
                Time.timeScale = 0f; // 게임 일시정지
                Cursor.lockState = CursorLockMode.None; // 마우스 커서 활성화
                Cursor.visible = true;
            }
            else
            {
                Time.timeScale = 1f; // 게임 재개
                Cursor.lockState = CursorLockMode.Locked; // 마우스 커서 숨김
                Cursor.visible = false;
            }
        }
    }
    
    /// <summary>
    /// 퀘스트 UI 열기
    /// </summary>
    public void OpenQuestUI()
    {
        if (questView != null && !questView.gameObject.activeInHierarchy)
        {
            questView.gameObject.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
    
    /// <summary>
    /// 퀘스트 UI 닫기
    /// </summary>
    public void CloseQuestUI()
    {
        if (questView != null && questView.gameObject.activeInHierarchy)
        {
            questView.gameObject.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
