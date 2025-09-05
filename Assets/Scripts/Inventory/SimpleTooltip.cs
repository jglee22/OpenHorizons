using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 가장 기본적인 아이템 툴팁
/// </summary>
public class SimpleTooltip : MonoBehaviour
{
    [Header("UI 요소")]
    public GameObject tooltipPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI descriptionText;
    
    private Canvas canvas;
    private RectTransform tooltipRectTransform;
    
    private void Start()
    {
        CreateTooltipUI();
    }
    
    private void CreateTooltipUI()
    {
        // Canvas 찾기
        canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // 툴팁 패널 생성
        tooltipPanel = new GameObject("TooltipPanel");
        tooltipPanel.transform.SetParent(canvas.transform, false);
        
        // 배경 이미지 추가
        Image background = tooltipPanel.AddComponent<Image>();
        background.color = new Color(0.1f, 0.1f, 0.1f, 0.95f);
        background.raycastTarget = false; // 마우스 이벤트 가로채기 방지
        
        // RectTransform 설정
        tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();
        tooltipRectTransform.anchorMin = new Vector2(0, 0);
        tooltipRectTransform.anchorMax = new Vector2(0, 0);
        tooltipRectTransform.sizeDelta = new Vector2(400, 400);
        
        // 아이템 이름 텍스트 (패널 상단에 배치)
        itemNameText = CreateText("ItemNameText", tooltipPanel.transform, new Vector2(0, 100), new Vector2(360, 50));
        itemNameText.fontSize = 24;
        itemNameText.fontStyle = FontStyles.Bold;
        itemNameText.color = Color.white;
        itemNameText.alignment = TextAlignmentOptions.Center;
        
        // 설명 텍스트 (패널 중앙에 배치)
        descriptionText = CreateText("DescriptionText", tooltipPanel.transform, new Vector2(0, 0), new Vector2(360, 100));
        descriptionText.fontSize = 18;
        descriptionText.color = Color.white;
        descriptionText.alignment = TextAlignmentOptions.Center;
        
        // 툴팁을 최상위로 올리기
        tooltipPanel.transform.SetAsLastSibling();
        
        // Canvas의 sortingOrder를 높여서 툴팁이 다른 UI 위에 표시되도록 설정
        if (canvas != null)
        {
            canvas.sortingOrder = 1000;
        }
        
        // 초기 상태는 숨김
        tooltipPanel.SetActive(false);
        
        Debug.Log("기본 툴팁 UI가 생성되었습니다. Canvas sortingOrder: " + (canvas != null ? canvas.sortingOrder.ToString() : "null"));
    }
    
    private TextMeshProUGUI CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "";
        text.alignment = TextAlignmentOptions.Center;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = anchoredPosition;
        textRect.sizeDelta = sizeDelta;
        
        return text;
    }
    
    public void ShowTooltip(Item item, Vector2 mousePosition)
    {
        Debug.Log($"ShowTooltip 호출됨 - 아이템: {item?.itemName}, 마우스 위치: {mousePosition}");
        
        if (item == null) 
        {
            Debug.LogWarning("아이템이 null입니다!");
            return;
        }
        
        if (tooltipPanel == null)
        {
            Debug.LogError("tooltipPanel이 null입니다!");
            return;
        }
        
        // 아이템 정보 설정
        itemNameText.text = item.itemName;
        descriptionText.text = item.description;
        
        Debug.Log($"아이템 정보 설정 완료 - 이름: {itemNameText.text}, 설명: {descriptionText.text}");
        
        // 위치 설정
        SetPosition(mousePosition);
        
        // 툴팁 표시
        tooltipPanel.SetActive(true);
        
        Debug.Log("툴팁이 표시되었습니다!");
    }
    
    public void HideTooltip()
    {
        Debug.Log("HideTooltip 호출됨");
        
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
            Debug.Log("툴팁이 숨겨졌습니다!");
        }
        else
        {
            Debug.LogWarning("tooltipPanel이 null입니다!");
        }
    }
    
    private void SetPosition(Vector2 mousePosition)
    {
        if (tooltipRectTransform == null) 
        {
            Debug.LogError("tooltipRectTransform이 null입니다!");
            return;
        }
        
        if (canvas == null)
        {
            Debug.LogError("canvas가 null입니다!");
            return;
        }
        
        // Canvas의 RenderMode에 따라 다른 좌표 변환 방식 사용
        Vector2 position;
        
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // ScreenSpaceOverlay 모드: 마우스 위치를 직접 사용
            // 큰 툴팁이 마우스 커서를 가리지 않도록 오프셋 조정
            position = mousePosition + new Vector2(50f, 50f);
            
            // 로그 제거 (깜빡임 방지)
            // Debug.Log($"ScreenSpaceOverlay 모드 - 마우스: {mousePosition}, 직접 사용 위치: {position}");
        }
        else
        {
            // ScreenSpaceCamera 또는 WorldSpace 모드: 좌표 변환 사용
            Vector2 localPoint;
            bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(), mousePosition, null, out localPoint);
            
            if (!success)
            {
                Debug.LogWarning("마우스 위치를 캔버스 좌표로 변환할 수 없습니다!");
                // 기본 위치로 설정
                tooltipRectTransform.anchoredPosition = new Vector2(100, 100);
                return;
            }
            
            position = localPoint + new Vector2(50f, 50f);
            
            Debug.Log($"좌표 변환 모드 - 마우스: {mousePosition}, 변환된 좌표: {localPoint}, 최종 위치: {position}");
        }
        
        tooltipRectTransform.anchoredPosition = position;
        
        // 툴팁이 화면 밖으로 나가지 않도록 조정
        EnsureTooltipOnScreen();
    }
    
    /// <summary>
    /// 툴팁이 화면 안에 있는지 확인하고 조정
    /// </summary>
    private void EnsureTooltipOnScreen()
    {
        if (tooltipRectTransform == null || canvas == null) return;
        
        // 현재 위치
        Vector2 currentPos = tooltipRectTransform.anchoredPosition;
        
        // 화면 크기 (Canvas 기준)
        Vector2 screenSize;
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // ScreenSpaceOverlay: 화면 해상도 사용
            screenSize = new Vector2(Screen.width, Screen.height);
        }
        else
        {
            // 다른 모드: Canvas 크기 사용
            screenSize = canvas.GetComponent<RectTransform>().sizeDelta;
        }
        
        Vector2 tooltipSize = tooltipRectTransform.sizeDelta;
        
        // 로그 제거 (깜빡임 방지)
        // Debug.Log($"화면 크기: {screenSize}, 툴팁 크기: {tooltipSize}, 현재 위치: {currentPos}");
        
        // 오른쪽 경계 확인
        if (currentPos.x + tooltipSize.x > screenSize.x)
        {
            currentPos.x = screenSize.x - tooltipSize.x - 10;
            // Debug.Log($"오른쪽 경계 조정: {currentPos.x}");
        }
        
        // 위쪽 경계 확인
        if (currentPos.y + tooltipSize.y > screenSize.y)
        {
            currentPos.y = screenSize.y - tooltipSize.y - 10;
            // Debug.Log($"위쪽 경계 조정: {currentPos.y}");
        }
        
        // 왼쪽 경계 확인
        if (currentPos.x < 0)
        {
            currentPos.x = 10;
            // Debug.Log($"왼쪽 경계 조정: {currentPos.x}");
        }
        
        // 아래쪽 경계 확인
        if (currentPos.y < 0)
        {
            currentPos.y = 10;
            // Debug.Log($"아래쪽 경계 조정: {currentPos.y}");
        }
        
        tooltipRectTransform.anchoredPosition = currentPos;
        
        // Debug.Log($"툴팁 화면 경계 조정 완료 - 최종 위치: {currentPos}");
    }
}
