using UnityEngine;
using System.Collections.Generic;
using AudioSystem;
using System.Threading.Tasks;

/// <summary>
/// 인벤토리 전체를 관리하는 매니저
/// </summary>
public class InventoryManager : MonoBehaviour
{
    [Header("인벤토리 설정")]
    [SerializeField] private int maxSlots = 40;
    [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
    
    [Header("UI 참조")]
    [SerializeField] private InventoryUI inventoryUI;
    
    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;
    
    [Header("Firebase 연동")]
    [SerializeField] private bool enableFirebaseSync = true;
    [SerializeField] private bool autoSaveOnChange = true;
    [SerializeField] private bool autoLoadOnStart = true;
    
    // Firebase 참조
    private FirebaseInventoryManager firebaseInventoryManager;
    
    // 이벤트
    public System.Action OnInventoryChanged;
    public System.Action OnInventorySaved;
    public System.Action OnInventoryLoaded;
    public System.Action<string> OnFirebaseError;
    
    private async void Start()
    {
        Debug.Log("InventoryManager Start() 호출됨");
        InitializeInventory();
        
        // UI 자동 찾기
        if (inventoryUI == null)
        {
            inventoryUI = FindObjectOfType<InventoryUI>();
        }
        
        // Firebase 매니저 초기화
        InitializeFirebase();
        
        // Firebase 초기화를 기다린 후 인벤토리 불러오기
        if (enableFirebaseSync && autoLoadOnStart)
        {
            // Firebase 초기화 대기
            await System.Threading.Tasks.Task.Delay(2000);
            await LoadInventoryFromFirebase();
        }
        
        Debug.Log($"InventoryManager 초기화 완료 - 슬롯 수: {slots?.Count ?? 0}");
    }
    
    /// <summary>
    /// 인벤토리 초기화
    /// </summary>
    private void InitializeInventory()
    {
        slots.Clear();
        
        for (int i = 0; i < maxSlots; i++)
        {
            slots.Add(new InventorySlot());
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"인벤토리 초기화 완료: {maxSlots}개 슬롯");
        }
    }
    
    /// <summary>
    /// 아이템 추가
    /// </summary>
    public bool AddItem(Item item, int amount = 1)
    {
        if (item == null || amount <= 0) return false;
        
        // slots 리스트 null 체크
        if (slots == null)
        {
            Debug.LogError("slots 리스트가 초기화되지 않았습니다!");
            InitializeInventory();
        }
        
        // 먼저 기존 스택에 추가 시도
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && slots[i].item != null && 
                slots[i].item.itemName == item.itemName && 
                slots[i].HasSpace(amount))
            {
                if (slots[i].AddItem(item, amount))
                {
                    OnInventoryChangedInternal();
                    if (showDebugInfo)
                    {
                        Debug.Log($"아이템 추가됨: {item.itemName} x{amount} (슬롯 {i})");
                    }
                    return true;
                }
            }
        }
        
        // 빈 슬롯에 추가
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && !slots[i].isOccupied)
            {
                if (slots[i].AddItem(item, amount))
                {
                    OnInventoryChangedInternal();
                    if (showDebugInfo)
                    {
                        Debug.Log($"아이템 추가됨: {item.itemName} x{amount} (새 슬롯 {i})");
                    }
                    return true;
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"인벤토리 공간 부족: {item.itemName} x{amount}");
        }
        return false;
    }
    
    /// <summary>
    /// 아이템 제거
    /// </summary>
    public bool RemoveItem(string itemName, int amount = 1)
    {
        if (string.IsNullOrEmpty(itemName) || amount <= 0) return false;
        
        int remainingToRemove = amount;
        
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            if (slots[i] != null && slots[i].isOccupied && slots[i].item != null && slots[i].item.itemName == itemName)
            {
                int availableInSlot = slots[i].quantity;
                int toRemoveFromSlot = Mathf.Min(remainingToRemove, availableInSlot);
                
                if (slots[i].RemoveItem(toRemoveFromSlot))
                {
                    remainingToRemove -= toRemoveFromSlot;
                    
                    if (remainingToRemove <= 0)
                    {
                        OnInventoryChangedInternal();
                        if (showDebugInfo)
                        {
                            Debug.Log($"아이템 제거됨: {itemName} x{amount}");
                        }
                        return true;
                    }
                }
            }
        }
        
        if (showDebugInfo)
        {
            Debug.Log($"아이템 제거 실패: {itemName} x{amount} (수량 부족)");
        }
        return false;
    }
    
    /// <summary>
    /// 아이템 수량 확인
    /// </summary>
    public int GetItemCount(string itemName)
    {
        if (string.IsNullOrEmpty(itemName)) return 0;
        if (slots == null) return 0;
        
        int totalCount = 0;
        foreach (var slot in slots)
        {
            if (slot != null && slot.isOccupied && slot.item != null && slot.item.itemName == itemName)
            {
                totalCount += slot.quantity;
            }
        }
        
        return totalCount;
    }
    
    /// <summary>
    /// 아이템 보유 여부 확인
    /// </summary>
    public bool HasItem(string itemName, int amount = 1)
    {
        return GetItemCount(itemName) >= amount;
    }
    
    /// <summary>
    /// 빈 슬롯 개수
    /// </summary>
    public int GetEmptySlotCount()
    {
        if (slots == null) return 0;
        
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot != null && !slot.isOccupied) count++;
        }
        return count;
    }
    
    /// <summary>
    /// 인벤토리 공간 확인
    /// </summary>
    public bool HasSpace(int amount = 1)
    {
        return GetEmptySlotCount() >= amount;
    }
    
    /// <summary>
    /// 특정 슬롯의 아이템 사용
    /// </summary>
    public bool UseItem(int slotIndex)
    {
        Debug.Log($"UseItem 호출됨 - 슬롯 인덱스: {slotIndex}");
        
        // null 체크 추가
        if (slots == null)
        {
            Debug.LogError("slots 리스트가 초기화되지 않았습니다!");
            InitializeInventory();
            return false;
        }
        
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning($"잘못된 슬롯 인덱스: {slotIndex} (범위: 0-{slots.Count - 1})");
            return false;
        }
        
        var slot = slots[slotIndex];
        if (slot == null)
        {
            Debug.LogError($"슬롯 {slotIndex}가 null입니다!");
            return false;
        }
        
        Debug.Log($"슬롯 {slotIndex} 상태 - isOccupied: {slot.isOccupied}, item: {(slot.item != null ? slot.item.itemName : "null")}");
        
        if (!slot.isOccupied) return false;
        
        // slot.item null 체크 추가
        if (slot.item == null)
        {
            Debug.LogError($"슬롯 {slotIndex}의 아이템이 null입니다!");
            return false;
        }
        
        if (slot.item.isConsumable)
        {
            // 아이템 이름을 미리 저장 (RemoveItem 호출 후 변경될 수 있음)
            string itemName = slot.item.itemName;
            Debug.Log($"소비 가능한 아이템 발견: {itemName}");
            
            // 소비 가능한 아이템인 경우 수량 감소
            if (slot.RemoveItem(1))
            {
                OnInventoryChangedInternal();
                if (showDebugInfo)
                {
                    Debug.Log($"아이템 사용됨: {itemName}");
                }
                return true;
            }
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"사용할 수 없는 아이템: {slot.item.itemName}");
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// 인벤토리 정보 출력 (디버그용)
    /// </summary>
    [ContextMenu("인벤토리 정보 출력")]
    public void PrintInventoryInfo()
    {
        Debug.Log("=== 인벤토리 정보 ===");
        Debug.Log($"총 슬롯: {slots.Count}");
        Debug.Log($"빈 슬롯: {GetEmptySlotCount()}");
        Debug.Log($"사용 중 슬롯: {slots.Count - GetEmptySlotCount()}");
        
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && slots[i].isOccupied)
            {
                Debug.Log($"슬롯 {i}: {slots[i]}");
            }
        }
    }
    
    /// <summary>
    /// 테스트 아이템 추가
    /// </summary>
    [ContextMenu("테스트 아이템 추가")]
    public void AddTestItems()
    {
        // 테스트 아이템들 추가
        AddItem(new Item("나무 열매", "맛있는 나무 열매입니다.", ItemType.Food), 5);
        AddItem(new Item("돌", "건축에 사용할 수 있는 돌입니다.", ItemType.Material), 10);
        AddItem(new Item("나무 막대", "다양한 용도로 사용할 수 있습니다.", ItemType.Tool), 3);
        
        Debug.Log("테스트 아이템이 추가되었습니다.");
    }
    
    /// <summary>
    /// 특정 슬롯에서 아이템 제거
    /// </summary>
    public bool RemoveItemFromSlot(int slotIndex)
    {
        if (slots == null)
        {
            Debug.LogError("slots 리스트가 초기화되지 않았습니다!");
            InitializeInventory();
            return false;
        }
        
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning($"잘못된 슬롯 인덱스: {slotIndex}");
            return false;
        }
        
        var slot = slots[slotIndex];
        if (slot == null)
        {
            Debug.LogError($"슬롯 {slotIndex}가 null입니다!");
            return false;
        }
        
        if (!slot.isOccupied) return false;
        
        slot.ClearSlot();
        OnInventoryChangedInternal();
        
        return true;
    }
    
    /// <summary>
    /// 특정 슬롯에 아이템 추가
    /// </summary>
    public bool AddItemToSlot(Item item, int quantity, int slotIndex)
    {
        if (slots == null)
        {
            Debug.LogError("slots 리스트가 초기화되지 않았습니다!");
            InitializeInventory();
            return false;
        }
        
        if (slotIndex < 0 || slotIndex >= slots.Count)
        {
            Debug.LogWarning($"잘못된 슬롯 인덱스: {slotIndex}");
            return false;
        }
        
        var slot = slots[slotIndex];
        if (slot == null)
        {
            Debug.LogError($"슬롯 {slotIndex}가 null입니다!");
            return false;
        }
        
        if (slot.isOccupied)
        {
            Debug.LogWarning($"슬롯 {slotIndex}가 이미 사용 중입니다!");
            return false;
        }
        
        bool result = slot.AddItem(item, quantity);
        if (result)
        {
            OnInventoryChangedInternal();
        }
        
        return result;
    }
    
    #region Firebase 연동
    
    /// <summary>
    /// Firebase 매니저 초기화
    /// </summary>
    private void InitializeFirebase()
    {
        if (!enableFirebaseSync) return;
        
        // FirebaseInventoryManager 찾기 또는 생성
        firebaseInventoryManager = FindObjectOfType<FirebaseInventoryManager>();
        if (firebaseInventoryManager == null)
        {
            GameObject firebaseObj = new GameObject("FirebaseInventoryManager");
            firebaseInventoryManager = firebaseObj.AddComponent<FirebaseInventoryManager>();
        }
        
        // 이벤트 구독
        firebaseInventoryManager.OnInventoryLoaded += OnFirebaseInventoryLoaded;
        firebaseInventoryManager.OnInventorySaved += OnFirebaseInventorySaved;
        firebaseInventoryManager.OnError += OnFirebaseInventoryError;
        
        if (showDebugInfo)
            Debug.Log("[InventoryManager] Firebase 연동 초기화 완료");
    }
    
    /// <summary>
    /// Firebase에서 인벤토리 불러오기
    /// </summary>
    public async Task<bool> LoadInventoryFromFirebase()
    {
        if (!enableFirebaseSync)
        {
            if (showDebugInfo)
                Debug.LogWarning("[InventoryManager] Firebase 연동이 비활성화되어 있습니다.");
            return false;
        }
        
        if (firebaseInventoryManager == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("[InventoryManager] FirebaseInventoryManager가 초기화되지 않았습니다.");
            return false;
        }
        
        // Firebase Auth 상태 확인
        if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("[InventoryManager] 로그인된 사용자가 없습니다.");
            return false;
        }
        
        try
        {
            if (showDebugInfo)
                Debug.Log("[InventoryManager] Firebase에서 인벤토리 불러오기 시작...");
            
            var loadedSlots = await firebaseInventoryManager.LoadInventory();
            
            if (loadedSlots != null && loadedSlots.Count > 0)
            {
                // 기존 인벤토리 초기화
                InitializeInventory();
                
                // 불러온 데이터로 인벤토리 복원
                for (int i = 0; i < Mathf.Min(loadedSlots.Count, slots.Count); i++)
                {
                    if (loadedSlots[i].isOccupied)
                    {
                        slots[i] = loadedSlots[i];
                    }
                }
                
                OnInventoryChanged?.Invoke();
                OnInventoryLoaded?.Invoke();
                
                if (showDebugInfo)
                    Debug.Log($"[InventoryManager] Firebase에서 인벤토리 불러오기 완료: {loadedSlots.Count}개 슬롯");
                
                return true;
            }
            else
            {
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] 저장된 인벤토리가 없습니다.");
                return false;
            }
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
                Debug.LogError($"[InventoryManager] Firebase 인벤토리 불러오기 실패: {e.Message}");
            
            OnFirebaseError?.Invoke($"인벤토리 불러오기 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Firebase에 인벤토리 저장
    /// </summary>
    public async Task<bool> SaveInventoryToFirebase()
    {
        if (!enableFirebaseSync || firebaseInventoryManager == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("[InventoryManager] Firebase 연동이 비활성화되어 있습니다.");
            return false;
        }
        
        try
        {
            if (showDebugInfo)
                Debug.Log("[InventoryManager] Firebase에 인벤토리 저장 시작...");
            
            bool success = await firebaseInventoryManager.SaveInventory(slots);
            
            if (success)
            {
                OnInventorySaved?.Invoke();
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] Firebase에 인벤토리 저장 완료");
            }
            
            return success;
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
                Debug.LogError($"[InventoryManager] Firebase 인벤토리 저장 실패: {e.Message}");
            
            OnFirebaseError?.Invoke($"인벤토리 저장 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Firebase 이벤트 핸들러들
    /// </summary>
    private void OnFirebaseInventoryLoaded(List<InventorySlot> loadedSlots)
    {
        if (showDebugInfo)
            Debug.Log($"[InventoryManager] Firebase에서 인벤토리 로드됨: {loadedSlots.Count}개 슬롯");
    }
    
    private void OnFirebaseInventorySaved(bool success)
    {
        if (showDebugInfo)
            Debug.Log($"[InventoryManager] Firebase 인벤토리 저장 결과: {success}");
    }
    
    private void OnFirebaseInventoryError(string error)
    {
        if (showDebugInfo)
            Debug.LogError($"[InventoryManager] Firebase 오류: {error}");
        
        OnFirebaseError?.Invoke(error);
    }
    
    /// <summary>
    /// 인벤토리 변경 시 자동 저장
    /// </summary>
    private async void OnInventoryChangedInternal()
    {
        OnInventoryChanged?.Invoke();
        
        // 자동 저장 활성화 시 Firebase에 저장
        if (enableFirebaseSync && autoSaveOnChange && firebaseInventoryManager != null)
        {
            await SaveInventoryToFirebase();
        }
    }
    
    #endregion
    
    #region 기존 메서드 수정 (자동 저장 연동)
    
    /// <summary>
    /// 아이템 추가 (자동 저장 연동)
    /// </summary>
    public bool AddItemWithAutoSave(Item item, int amount = 1)
    {
        bool result = AddItem(item, amount);
        if (result)
        {
            OnInventoryChangedInternal();
        }
        return result;
    }
    
    /// <summary>
    /// 아이템 제거 (자동 저장 연동)
    /// </summary>
    public bool RemoveItemWithAutoSave(string itemName, int amount = 1)
    {
        bool result = RemoveItem(itemName, amount);
        if (result)
        {
            OnInventoryChangedInternal();
        }
        return result;
    }
    
    /// <summary>
    /// 아이템 사용 (자동 저장 연동)
    /// </summary>
    public bool UseItemWithAutoSave(int slotIndex)
    {
        bool result = UseItem(slotIndex);
        if (result)
        {
            OnInventoryChangedInternal();
        }
        return result;
    }
    
    #endregion
    
    #region 수동 저장/불러오기 메서드
    
    /// <summary>
    /// 수동으로 인벤토리 저장
    /// </summary>
    [ContextMenu("Firebase에 인벤토리 저장")]
    public async void SaveInventoryManually()
    {
        await SaveInventoryToFirebase();
    }
    
    /// <summary>
    /// 수동으로 인벤토리 불러오기
    /// </summary>
    [ContextMenu("Firebase에서 인벤토리 불러오기")]
    public async void LoadInventoryManually()
    {
        await LoadInventoryFromFirebase();
    }
    
    /// <summary>
    /// Firebase 인벤토리 데이터 삭제
    /// </summary>
    [ContextMenu("Firebase 인벤토리 데이터 삭제")]
    public async void DeleteFirebaseInventory()
    {
        if (firebaseInventoryManager != null)
        {
            await firebaseInventoryManager.DeleteInventory();
            if (showDebugInfo)
                Debug.Log("[InventoryManager] Firebase 인벤토리 데이터가 삭제되었습니다.");
        }
    }
    
    #endregion
}
