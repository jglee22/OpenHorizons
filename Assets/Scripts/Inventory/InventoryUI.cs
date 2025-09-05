using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using AudioSystem;

/// <summary>
/// 인벤토리 UI 관리
/// </summary>
public class InventoryUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject slotPrefab;
    
    [Header("설정")]
    [SerializeField] private KeyCode toggleKey = KeyCode.I;
    [SerializeField] private bool startHidden = true;
    
    [Header("참조")]
    [SerializeField] public InventoryManager inventoryManager;
    [SerializeField] private SimpleTooltip tooltip;
    
    private List<InventorySlotUI> slotUIs = new List<InventorySlotUI>();
    private bool isVisible = false;
    
    private void Start()
    {
        // 참조 자동 찾기
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }
        
        // UI 초기화
        InitializeUI();
        
        // 초기 상태 설정
        if (startHidden)
        {
            SetVisible(false);
        }
        
        // 이벤트 구독
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged += RefreshUI;
        }
    }
    
    private void Update()
    {
        // 토글 키 입력 처리
        if (Input.GetKeyDown(toggleKey))
        {
            ToggleInventory();
        }
    }
    
    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        if (inventoryPanel == null)
        {
            CreateInventoryPanel();
        }
        
        if (slotsParent == null)
        {
            CreateSlotsParent();
        }
        
        CreateSlots();
    }
    
    /// <summary>
    /// 인벤토리 패널 생성
    /// </summary>
    private void CreateInventoryPanel()
    {
        // Canvas 찾기 또는 생성
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // 인벤토리 패널 생성
        inventoryPanel = new GameObject("InventoryPanel");
        inventoryPanel.transform.SetParent(canvas.transform, false);
        
        // 배경 이미지
        Image panelImage = inventoryPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.9f);
        
        // RectTransform 설정
        RectTransform panelRect = inventoryPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(600, 400);
        panelRect.anchoredPosition = Vector2.zero;
        
        // 제목 텍스트
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(inventoryPanel.transform, false);
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "인벤토리";
        titleText.fontSize = 24;
        titleText.color = Color.white;
        titleText.alignment = TextAlignmentOptions.Center;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.sizeDelta = new Vector2(0, 50);
        titleRect.anchoredPosition = new Vector2(0, -25);
        
        Debug.Log("인벤토리 패널이 자동 생성되었습니다.");
    }
    
    /// <summary>
    /// 슬롯 부모 생성 (스크롤뷰 기반)
    /// </summary>
    private void CreateSlotsParent()
    {
        // 스크롤뷰 생성
        GameObject scrollViewObj = new GameObject("ScrollView");
        scrollViewObj.transform.SetParent(inventoryPanel.transform, false);
        
        ScrollRect scrollRect = scrollViewObj.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.scrollSensitivity = 10f;
        
        // 스크롤뷰 RectTransform 설정
        RectTransform scrollViewRect = scrollViewObj.GetComponent<RectTransform>();
        scrollViewRect.anchorMin = new Vector2(0, 0);
        scrollViewRect.anchorMax = new Vector2(1, 1);
        scrollViewRect.sizeDelta = new Vector2(-20, -80);
        scrollViewRect.anchoredPosition = new Vector2(0, -20);
        
        // Viewport 생성
        GameObject viewportObj = new GameObject("Viewport");
        viewportObj.transform.SetParent(scrollViewObj.transform, false);
        
        RectTransform viewportRect = viewportObj.GetComponent<RectTransform>();
        viewportRect.anchorMin = Vector2.zero;
        viewportRect.anchorMax = Vector2.one;
        viewportRect.sizeDelta = Vector2.zero;
        viewportRect.anchoredPosition = Vector2.zero;
        
        // Viewport에 마스크 추가
        Mask viewportMask = viewportObj.AddComponent<Mask>();
        Image viewportImage = viewportObj.AddComponent<Image>();
        viewportImage.color = new Color(0, 0, 0, 0.1f);
        
        // Content (실제 슬롯들이 들어갈 곳) 생성
        GameObject contentObj = new GameObject("Content");
        contentObj.transform.SetParent(viewportObj.transform, false);
        
        // Grid Layout Group 추가
        GridLayoutGroup gridLayout = contentObj.AddComponent<GridLayoutGroup>();
        gridLayout.cellSize = new Vector2(60, 60);
        gridLayout.spacing = new Vector2(5, 5);
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = 8;
        gridLayout.childAlignment = TextAnchor.UpperCenter;
        
        // Content RectTransform 설정
        RectTransform contentRect = contentObj.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.anchoredPosition = Vector2.zero;
        
        // Content의 크기를 자동으로 조정
        ContentSizeFitter contentFitter = contentObj.AddComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        // 스크롤뷰 연결
        scrollRect.viewport = viewportRect;
        scrollRect.content = contentRect;
        
        slotsParent = contentObj.transform;
        
        Debug.Log("스크롤뷰 기반 슬롯 컨테이너가 생성되었습니다.");
    }
    
    /// <summary>
    /// 슬롯들 생성
    /// </summary>
    private void CreateSlots()
    {
        if (inventoryManager == null) return;
        
        // 툴팁 자동 생성
        if (tooltip == null)
        {
            CreateTooltip();
        }
        
        // 기존 슬롯들 제거
        foreach (var slotUI in slotUIs)
        {
            if (slotUI != null)
            {
                DestroyImmediate(slotUI.gameObject);
            }
        }
        slotUIs.Clear();
        
        // 새 슬롯들 생성 (더 많은 슬롯으로 스크롤 효과 확인)
        for (int i = 0; i < 40; i++) // 40개 슬롯 (5줄)
        {
            GameObject slotObj = new GameObject($"Slot_{i}");
            slotObj.transform.SetParent(slotsParent, false);
            
            // 슬롯 배경
            Image slotImage = slotObj.AddComponent<Image>();
            slotImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // 테두리
            slotImage.type = Image.Type.Sliced;
            
            // 슬롯 UI 컴포넌트 추가
            InventorySlotUI slotUI = slotObj.AddComponent<InventorySlotUI>();
            
            // 툴팁 자동 연결
            var tooltipField = slotUI.GetType().GetField("tooltip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (tooltipField != null && tooltip != null)
            {
                tooltipField.SetValue(slotUI, tooltip);
                Debug.Log($"슬롯 {i}에 툴팁 연결 완료");
            }
            else
            {
                Debug.LogWarning($"슬롯 {i}에 툴팁 연결 실패 - tooltipField: {tooltipField != null}, tooltip: {tooltip != null}");
            }
            
            slotUI.Initialize(i, this);
            slotUIs.Add(slotUI);
        }
        
        Debug.Log($"{slotUIs.Count}개의 인벤토리 슬롯이 생성되었습니다. (스크롤 가능)");
    }
    
    /// <summary>
    /// UI 새로고침
    /// </summary>
    public void RefreshUI()
    {
        if (inventoryManager == null) return;
        
        // 각 슬롯 UI 업데이트
        for (int i = 0; i < slotUIs.Count; i++)
        {
            if (slotUIs[i] != null)
            {
                slotUIs[i].UpdateSlot();
            }
        }
    }
    
    /// <summary>
    /// 인벤토리 토글
    /// </summary>
    public void ToggleInventory()
    {
        SetVisible(!isVisible);
        
        // 인벤토리 열기/닫기 사운드 재생
        if (AudioManager.Instance != null)
        {
            if (!isVisible)
            {
                AudioManager.Instance.PlayUISFX("inventory_open");
            }
            else
            {
                AudioManager.Instance.PlayUISFX("inventory_close");
            }
        }
    }
    
    /// <summary>
    /// 가시성 설정
    /// </summary>
    public void SetVisible(bool visible)
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(visible);
            isVisible = visible;
            
            if (visible)
            {
                RefreshUI();
            }
            else
            {
                // 인벤토리를 닫을 때 모든 슬롯의 툴팁 숨기기
                HideAllTooltips();
            }
        }
    }
    
    /// <summary>
    /// 특정 슬롯의 아이템 사용
    /// </summary>
    public void UseItem(int slotIndex)
    {
        if (inventoryManager != null)
        {
            inventoryManager.UseItem(slotIndex);
        }
    }
    
    /// <summary>
    /// 인벤토리가 열려있는지 확인
    /// </summary>
    public bool IsInventoryOpen()
    {
        return isVisible;
    }
    
    /// <summary>
    /// 모든 슬롯의 툴팁 숨기기
    /// </summary>
    private void HideAllTooltips()
    {
        if (tooltip != null)
        {
            tooltip.HideTooltip();
        }
        
        // 모든 슬롯의 툴팁 상태 초기화
        foreach (var slotUI in slotUIs)
        {
            if (slotUI != null)
            {
                // reflection을 사용하여 isTooltipShowing 필드에 접근
                var isTooltipShowingField = slotUI.GetType().GetField("isTooltipShowing", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (isTooltipShowingField != null)
                {
                    isTooltipShowingField.SetValue(slotUI, false);
                }
            }
        }
        
        Debug.Log("모든 툴팁이 숨겨졌습니다.");
    }
    
    /// <summary>
    /// 툴팁 생성
    /// </summary>
    private void CreateTooltip()
    {
        // Canvas 찾기
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;
        
        // 툴팁 오브젝트 생성
        GameObject tooltipObj = new GameObject("SimpleTooltip");
        tooltipObj.transform.SetParent(canvas.transform, false);
        
        // SimpleTooltip 컴포넌트 추가
        tooltip = tooltipObj.AddComponent<SimpleTooltip>();
        
        Debug.Log("기본 툴팁이 자동으로 생성되었습니다.");
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (inventoryManager != null)
        {
            inventoryManager.OnInventoryChanged -= RefreshUI;
        }
    }
}
