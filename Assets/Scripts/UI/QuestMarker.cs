using UnityEngine;

/// <summary>
/// 월드 상의 퀘스트 목표 마커
/// </summary>
public class QuestMarker : MonoBehaviour
{
    private Vector3 worldPosition;
    private string questName;
    private Task associatedTask;
    private Camera playerCamera;
    
    public void Initialize(Vector3 position, string quest, Task task)
    {
        worldPosition = position;
        questName = quest;
        associatedTask = task;
        playerCamera = Camera.main;
    }
    
    public void UpdatePosition(Camera camera)
    {
        if (camera == null) return;
        
        playerCamera = camera;
        
        // 월드 좌표를 스크린 좌표로 변환
        Vector3 screenPos = playerCamera.WorldToScreenPoint(worldPosition);
        
        // 카메라 뒤에 있으면 숨김
        if (screenPos.z < 0)
        {
            gameObject.SetActive(false);
            return;
        }
        
        gameObject.SetActive(true);
        
        // 스크린 좌표를 UI 좌표로 변환
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 uiPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.parent.GetComponent<RectTransform>(),
            screenPos,
            null,
            out uiPos
        );
        
        rectTransform.anchoredPosition = uiPos;
        
        // 거리에 따른 크기 조절
        float distance = Vector3.Distance(playerCamera.transform.position, worldPosition);
        float scale = Mathf.Clamp(1f - (distance / 100f), 0.3f, 1f);
        rectTransform.localScale = Vector3.one * scale;
        
        // 거리에 따른 투명도 조절
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        float alpha = Mathf.Clamp(1f - (distance / 50f), 0.3f, 1f);
        canvasGroup.alpha = alpha;
    }
    
    private void OnDestroy()
    {
        // 마커가 파괴될 때 정리
    }
}
