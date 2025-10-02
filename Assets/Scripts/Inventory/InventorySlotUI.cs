using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 개별 인벤토리 슬롯 UI
/// </summary>
public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, 
    IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    [Header("UI 요소")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI quantityText;
    [SerializeField] private Image backgroundImage;
    
    [Header("설정")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    [SerializeField] private Color hoverColor = new Color(0.4f, 0.4f, 0.4f, 0.8f);
    [SerializeField] private Color selectedColor = new Color(0.6f, 0.6f, 0.2f, 0.8f);
    
    [Header("툴팁")]
    [SerializeField] private SimpleTooltip tooltip;
    
    // 툴팁 상태 추적
    private bool isTooltipShowing = false;
    private Coroutine hideTooltipCoroutine = null;
    

    
    private int slotIndex;
    private InventoryUI inventoryUI;
    private bool isSelected = false;
    

    
    // 드래그 앤 드롭 관련
    private static InventorySlotUI draggedSlot = null;
    private static GameObject dragIcon = null;
    private static Vector2 dragOffset;
    private bool isDragging = false;
    
    private void Awake()
    {
        // 컴포넌트 자동 할당
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        
        // 아이템 아이콘 생성
        CreateItemIcon();
        
        // 수량 텍스트 생성
        CreateQuantityText();
    }
    
    /// <summary>
    /// 슬롯 초기화
    /// </summary>
    public void Initialize(int index, InventoryUI ui)
    {
        slotIndex = index;
        inventoryUI = ui;
        
        // 초기 상태 설정
        UpdateSlot();
    }
    
    /// <summary>
    /// 아이템 아이콘 생성
    /// </summary>
    private void CreateItemIcon()
    {
        GameObject iconObj = new GameObject("ItemIcon");
        iconObj.transform.SetParent(transform, false);
        
        itemIcon = iconObj.AddComponent<Image>();
        itemIcon.raycastTarget = false;
        
        RectTransform iconRect = iconObj.GetComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.1f, 0.1f);
        iconRect.anchorMax = new Vector2(0.9f, 0.9f);
        iconRect.sizeDelta = Vector2.zero;
        iconRect.anchoredPosition = Vector2.zero;
        
        // 기본 아이콘 설정
        itemIcon.sprite = null;
        itemIcon.color = new Color(1, 1, 1, 0.5f);
    }
    
    /// <summary>
    /// 수량 텍스트 생성
    /// </summary>
    private void CreateQuantityText()
    {
        GameObject textObj = new GameObject("QuantityText");
        textObj.transform.SetParent(transform, false);
        
        quantityText = textObj.AddComponent<TextMeshProUGUI>();
        quantityText.fontSize = 12;
        quantityText.color = Color.white;
        quantityText.alignment = TextAlignmentOptions.BottomRight;
        quantityText.raycastTarget = false;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.7f, 0.1f);
        textRect.anchorMax = new Vector2(0.95f, 0.4f);
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        // 기본 텍스트 설정
        quantityText.text = "";
    }
    
    /// <summary>
    /// 슬롯 업데이트
    /// </summary>
    public void UpdateSlot()
    {
        if (inventoryUI == null || inventoryUI.inventoryManager == null) return;
        
        // 슬롯 데이터 가져오기 (InventoryManager에서 직접 접근)
        var inventoryManager = inventoryUI.inventoryManager;
        var slots = inventoryManager.GetType().GetField("slots", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (slots != null)
        {
            var slotsList = slots.GetValue(inventoryManager) as System.Collections.Generic.List<InventorySlot>;
            if (slotsList != null && slotIndex < slotsList.Count)
            {
                var slot = slotsList[slotIndex];
                UpdateSlotDisplay(slot);
            }
        }
    }
    
    /// <summary>
    /// 슬롯 표시 업데이트
    /// </summary>
    private void UpdateSlotDisplay(InventorySlot slot)
    {
        if (slot == null || !slot.isOccupied)
        {
            // 빈 슬롯
            if (itemIcon != null)
            {
                itemIcon.sprite = null;
                itemIcon.color = new Color(1, 1, 1, 0.1f);
            }
            
            if (quantityText != null)
            {
                quantityText.text = "";
            }
            
            // 배경 색상 강제 설정
            if (backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }
            else
            {
                Debug.LogWarning($"슬롯 {slotIndex} backgroundImage가 null입니다!");
            }
        }
        else
        {
            // 아이템이 있는 슬롯
            if (itemIcon != null)
            {
                itemIcon.sprite = slot.item.icon;
                itemIcon.color = slot.item.icon != null ? Color.white : new Color(1, 1, 1, 0.5f);
                
                // 허브 아이템 디버그 로그
                if (slot.item.itemName.Contains("허브") || slot.item.itemName.Contains("herb") || slot.item.itemName.ToLower().Contains("healing"))
                {
                    Debug.Log($"[InventorySlotUI] 허브 아이템 아이콘 설정: {slot.item.itemName}, 아이콘: {(slot.item.icon != null ? slot.item.icon.name : "null")}");
                }
            }
            
            if (quantityText != null)
            {
                quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
            }
            
            // 배경 색상
            if (backgroundImage != null)
            {
                backgroundImage.color = isSelected ? selectedColor : normalColor;
            }
        }
    }
    
    /// <summary>
    /// 슬롯 클릭 처리
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (inventoryUI == null) return;
        
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 왼쪽 클릭: 아이템 사용
            UseItem();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 오른쪽 클릭: 아이템 정보 표시
            ShowItemInfo();
        }
    }
    
    /// <summary>
    /// 마우스 진입 처리
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (backgroundImage != null && !isSelected)
        {
            backgroundImage.color = hoverColor;
        }
        
        // 툴팁 표시
        ShowTooltip();
    }
    
    /// <summary>
    /// 마우스 이탈 처리
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("OnPointerExit 호출됨");
        if (backgroundImage != null && !isSelected)
        {
            backgroundImage.color = normalColor;
        }
        
        // 툴팁 숨기기 코루틴 시작 (깜빡임 방지를 위해 짧은 지연)
        hideTooltipCoroutine = StartCoroutine(DelayedHideTooltip());
    }
    
    /// <summary>
    /// 아이템 사용
    /// </summary>
    private void UseItem()
    {
        if (inventoryUI != null)
        {
            inventoryUI.UseItem(slotIndex);
        }
    }
    
    /// <summary>
    /// 아이템 정보 표시
    /// </summary>
    private void ShowItemInfo()
    {
        // TODO: 아이템 정보 창 표시
        Debug.Log($"아이템 정보 표시: 슬롯 {slotIndex}");
    }
    
    /// <summary>
    /// 툴팁 표시
    /// </summary>
    private void ShowTooltip()
    {
        Debug.Log($"ShowTooltip 호출됨 - 슬롯 {slotIndex}");
        
        if (tooltip == null) 
        {
            Debug.LogWarning($"슬롯 {slotIndex}: tooltip이 null입니다!");
            return;
        }
        
        // 이미 툴팁이 표시 중이면 숨기기 코루틴 중단
        if (hideTooltipCoroutine != null)
        {
            StopCoroutine(hideTooltipCoroutine);
            hideTooltipCoroutine = null;
        }
        
        // 현재 슬롯의 아이템 정보 가져오기
        var slot = GetCurrentSlot();
        Debug.Log($"슬롯 {slotIndex} 데이터 - slot: {slot != null}, isOccupied: {slot?.isOccupied}, item: {slot?.item?.itemName}");
        
        if (slot != null && slot.isOccupied && slot.item != null)
        {
            // 마우스 위치 전달하여 툴팁 표시
            tooltip.ShowTooltip(slot.item, Input.mousePosition);
            isTooltipShowing = true;
        }
        else
        {
            Debug.LogWarning($"슬롯 {slotIndex}: 아이템 정보가 부족합니다!");
        }
    }
    
    /// <summary>
    /// 툴팁 숨기기
    /// </summary>
    private void HideTooltip()
    {
        if (tooltip != null)
        {
            tooltip.HideTooltip();
            isTooltipShowing = false;
        }
    }
    
    /// <summary>
    /// 지연된 툴팁 숨기기 (깜빡임 방지)
    /// </summary>
    private System.Collections.IEnumerator DelayedHideTooltip()
    {
        // 매우 짧은 지연으로 깜빡임 방지
        yield return new WaitForSeconds(0.02f);
        
        // 마우스가 실제로 슬롯 영역을 벗어났는지 한 번 더 확인
        if (IsMouseStillOverSlot())
        {
            Debug.Log($"슬롯 {slotIndex} - 마우스가 여전히 슬롯 위에 있음, 툴팁 유지");
            hideTooltipCoroutine = null;
            yield break;
        }
        
        // 툴팁 숨기기
        if (tooltip != null && isTooltipShowing)
        {
            Debug.Log($"슬롯 {slotIndex} - 지연 후 툴팁 숨김");
            HideTooltip();
        }
        
        // 코루틴 참조 초기화
        hideTooltipCoroutine = null;
    }
    
    /// <summary>
    /// 마우스가 여전히 슬롯 위에 있는지 확인
    /// </summary>
    private bool IsMouseStillOverSlot()
    {
        if (backgroundImage == null) return false;
        
        // 마우스 위치
        Vector3 mousePos = Input.mousePosition;
        
        // 슬롯의 스크린 좌표 계산
        Vector3[] corners = new Vector3[4];
        backgroundImage.rectTransform.GetWorldCorners(corners);
        
        // 슬롯의 경계 계산
        float minX = Mathf.Min(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float maxX = Mathf.Max(corners[0].x, corners[1].x, corners[2].x, corners[3].x);
        float minY = Mathf.Min(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        float maxY = Mathf.Max(corners[0].y, corners[1].y, corners[2].y, corners[3].y);
        
        // 마우스가 슬롯 영역 안에 있는지 확인
        bool isInside = mousePos.x >= minX && mousePos.x <= maxX && mousePos.y >= minY && mousePos.y <= maxY;
        
        Debug.Log($"슬롯 {slotIndex} - 마우스 위치 확인: ({mousePos.x:F1}, {mousePos.y:F1}), 슬롯 경계: X({minX:F1}~{maxX:F1}), Y({minY:F1}~{maxY:F1}), 안에 있음: {isInside}");
        
        return isInside;
    }
    
    /// <summary>
    /// 선택 상태 설정
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateSlot();
    }
    
    #region 드래그 앤 드롭
    
    /// <summary>
    /// 드래그 시작
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventoryUI == null || inventoryUI.inventoryManager == null) return;
        
        // 아이템이 있는 슬롯만 드래그 가능
        var slot = GetCurrentSlot();
        if (slot == null || !slot.isOccupied) return;
        
        // 드래그 상태 설정
        draggedSlot = this;
        isDragging = true;
        

        
        // 드래그 아이콘 생성
        CreateDragIcon(slot.item.icon);
        
        // 원본 아이콘 반투명하게
        if (itemIcon != null)
        {
            itemIcon.color = new Color(1, 1, 1, 0.3f);
        }
        
        // 드래그 오프셋 계산
        dragOffset = eventData.position - (Vector2)transform.position;
        
        Debug.Log($"드래그 시작: 슬롯 {slotIndex}, 아이템: {slot.item.itemName}");
        
        // 드래그 시작 시 툴팁 숨기기
        if (tooltip != null)
        {
            tooltip.HideTooltip();
        }
    }
    
    /// <summary>
    /// 드래그 중
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || dragIcon == null) return;
        
        // 드래그 아이콘 위치 업데이트
        dragIcon.transform.position = eventData.position - dragOffset;
    }
    
    /// <summary>
    /// 드래그 종료
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        
        // 드래그 상태 해제
        isDragging = false;
        
        // 원본 아이콘 복원
        if (itemIcon != null)
        {
            itemIcon.color = Color.white;
        }
        
        // 드래그 아이콘 제거
        if (dragIcon != null)
        {
            DestroyImmediate(dragIcon);
            dragIcon = null;
        }
        
        // 드래그된 슬롯 초기화
        if (draggedSlot == this)
        {
            draggedSlot = null;
        }
        
        // 강제로 UI 업데이트
        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }
        
        Debug.Log("드래그 종료 - UI 강제 업데이트 완료");
    }
    
    /// <summary>
    /// 드롭 처리
    /// </summary>
    public void OnDrop(PointerEventData eventData)
    {
        if (draggedSlot == null || draggedSlot == this) return;
        
        // 아이템 이동 시도
        if (TryMoveItem(draggedSlot.slotIndex, slotIndex))
        {
            Debug.Log($"아이템 이동 성공: {draggedSlot.slotIndex} → {slotIndex}");
            
            // UI 업데이트 - 모든 관련 슬롯 강제 업데이트
            draggedSlot.UpdateSlot();
            UpdateSlot();
            
            // 전체 인벤토리 UI 새로고침 (안전장치)
            if (inventoryUI != null)
            {
                inventoryUI.RefreshUI();
            }
            
            // 인벤토리 변경 이벤트 발생
            if (inventoryUI != null && inventoryUI.inventoryManager != null)
            {
                inventoryUI.inventoryManager.OnInventoryChanged?.Invoke();
            }
        }
        else
        {
            Debug.Log("아이템 이동 실패");
        }
    }
    
    /// <summary>
    /// 드래그 아이콘 생성
    /// </summary>
    private void CreateDragIcon(Sprite iconSprite)
    {
        if (dragIcon != null)
        {
            DestroyImmediate(dragIcon);
        }
        
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(transform.root, false);
        
        Image dragImage = dragIcon.AddComponent<Image>();
        dragImage.sprite = iconSprite;
        dragImage.raycastTarget = false;
        
        RectTransform dragRect = dragIcon.GetComponent<RectTransform>();
        dragRect.sizeDelta = new Vector2(50, 50);
        
        // Canvas에 추가
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            dragIcon.transform.SetParent(canvas.transform, false);
        }
        
        // 최상위 레이어로 설정 (Canvas의 sortingOrder 사용)
        Canvas parentCanvas = dragIcon.GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            parentCanvas.sortingOrder = 1000;
        }
    }
    
    /// <summary>
    /// 아이템 이동 시도
    /// </summary>
    private bool TryMoveItem(int fromIndex, int toIndex)
    {
        if (inventoryUI == null || inventoryUI.inventoryManager == null) return false;
        
        var inventoryManager = inventoryUI.inventoryManager;
        var slots = inventoryManager.GetType().GetField("slots", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (slots == null) return false;
        
        var slotsList = slots.GetValue(inventoryManager) as System.Collections.Generic.List<InventorySlot>;
        if (slotsList == null || fromIndex >= slotsList.Count || toIndex >= slotsList.Count) return false;
        
        var fromSlot = slotsList[fromIndex];
        var toSlot = slotsList[toIndex];
        
        if (fromSlot == null || !fromSlot.isOccupied) return false;
        
        // InventoryManager의 메서드를 사용하여 아이템 이동
        var inventoryManager2 = inventoryUI.inventoryManager;
        
        // 아이템 이동 로직
        if (!toSlot.isOccupied)
        {
            // 빈 슬롯으로 이동
            if (inventoryManager2.AddItemToSlot(fromSlot.item, fromSlot.quantity, toIndex))
            {
                if (inventoryManager2.RemoveItemFromSlot(fromIndex))
                {
                    // UI 즉시 업데이트 강제
                    if (inventoryUI != null)
                    {
                        // 개별 슬롯 먼저 업데이트
                        draggedSlot?.UpdateSlot();
                        UpdateSlot();
                        
                        // 전체 UI 새로고침
                        inventoryUI.RefreshUI();
                        
                        // 한 번 더 강제 업데이트
                        StartCoroutine(DelayedUpdate());
                    }
                    return true;
                }
            }
        }
        else if (fromSlot.item.itemName == toSlot.item.itemName && fromSlot.item.isStackable)
        {
            // 같은 아이템 스택
            int totalQuantity = fromSlot.quantity + toSlot.quantity;
            int maxStack = 99; // 최대 스택 크기
            
            if (totalQuantity <= maxStack)
            {
                // 전체 스택 이동
                if (inventoryManager2.AddItemToSlot(fromSlot.item, totalQuantity, toIndex))
                {
                    inventoryManager2.RemoveItemFromSlot(fromIndex);
                    return true;
                }
            }
            else
            {
                // 스택 분할
                if (inventoryManager2.AddItemToSlot(fromSlot.item, maxStack, toIndex))
                {
                    inventoryManager2.RemoveItemFromSlot(fromIndex);
                    inventoryManager2.AddItemToSlot(fromSlot.item, totalQuantity - maxStack, fromIndex);
                    return true;
                }
            }
        }
        else
        {
            // 아이템 교환 - 순서 중요!
            var fromItem = fromSlot.item;
            int fromQuantity = fromSlot.quantity;
            var toItem = toSlot.item;
            int toQuantity = toSlot.quantity;
            
            // 먼저 두 슬롯 모두 비우기
            if (inventoryManager2.RemoveItemFromSlot(fromIndex) && 
                inventoryManager2.RemoveItemFromSlot(toIndex))
            {
                // 그 다음 올바른 순서로 아이템 배치
                if (inventoryManager2.AddItemToSlot(fromItem, fromQuantity, toIndex) &&
                    inventoryManager2.AddItemToSlot(toItem, toQuantity, fromIndex))
                {
                    return true;
                }
                else
                {
                    // 실패 시 원래대로 복구
                    inventoryManager2.AddItemToSlot(fromItem, fromQuantity, fromIndex);
                    inventoryManager2.AddItemToSlot(toItem, toQuantity, toIndex);
                }
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 지연된 UI 업데이트 (안전장치)
    /// </summary>
    private System.Collections.IEnumerator DelayedUpdate()
    {
        yield return new WaitForEndOfFrame();
        
        // 한 번 더 강제 업데이트
        UpdateSlot();
        if (inventoryUI != null)
        {
            inventoryUI.RefreshUI();
        }
        
        Debug.Log($"슬롯 {slotIndex} 지연 업데이트 완료");
    }
    
    /// <summary>
    /// 현재 슬롯 데이터 가져오기
    /// </summary>
    private InventorySlot GetCurrentSlot()
    {
        if (inventoryUI == null || inventoryUI.inventoryManager == null) return null;
        
        var inventoryManager = inventoryUI.inventoryManager;
        var slots = inventoryManager.GetType().GetField("slots", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (slots == null) return null;
        
        var slotsList = slots.GetValue(inventoryManager) as System.Collections.Generic.List<InventorySlot>;
        if (slotsList == null || slotIndex >= slotsList.Count) return null;
        
        return slotsList[slotIndex];
    }
    
    #endregion
}
