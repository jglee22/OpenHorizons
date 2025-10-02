using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AudioSystem;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase;
using Firebase.Database;
using System.Linq;

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
    [SerializeField] private bool enableFirebaseSync = true; // Firebase 다시 활성화
    [SerializeField] private bool autoSaveOnChange = true;
    [SerializeField] private bool autoLoadOnStart = true;
    
    [Header("로컬 저장")]
    [SerializeField] private bool enableLocalSave = true;
    
    // 외부 접근을 위한 프로퍼티
    public bool AutoSaveOnChange => autoSaveOnChange;
    public bool EnableFirebaseSync => enableFirebaseSync;
    public bool EnableLocalSave => enableLocalSave;
    
    // 인벤토리 정리 메서드들
    public void ClearAllItems()
    {
        int itemCount = slots.Count(slot => slot.item != null && slot.quantity > 0);
        foreach (var slot in slots)
        {
            slot.ClearSlot();
        }
        
        if (autoSaveOnChange)
        {
            SaveInventoryToLocal();
            SaveInventoryToFirebase();
        }
        
        Debug.Log($"[InventoryManager] 모든 아이템이 초기화되었습니다. ({itemCount}개 제거)");
    }
    
    public void RemoveDuplicateItems()
    {
        var seenItems = new HashSet<string>();
        int removedCount = 0;
        
        foreach (var slot in slots)
        {
            if (slot.item != null && slot.quantity > 0)
            {
                string itemKey = $"{slot.item.itemName}_{slot.item.itemType}";
                if (!seenItems.Contains(itemKey))
                {
                    seenItems.Add(itemKey);
                }
                else
                {
                    slot.ClearSlot();
                    removedCount++;
                }
            }
        }
        
        if (autoSaveOnChange)
        {
            SaveInventoryToLocal();
            SaveInventoryToFirebase();
        }
        
        Debug.Log($"[InventoryManager] {removedCount}개의 중복 아이템이 제거되었습니다.");
    }
    
    public void RemoveItemByName(string itemName)
    {
        int removedCount = 0;
        foreach (var slot in slots)
        {
            if (slot.item != null && slot.item.itemName == itemName)
            {
                slot.ClearSlot();
                removedCount++;
            }
        }
        
        if (autoSaveOnChange)
        {
            SaveInventoryToLocal();
            SaveInventoryToFirebase();
        }
        
        Debug.Log($"[InventoryManager] '{itemName}' {removedCount}개가 제거되었습니다.");
    }
    [SerializeField] private string localSaveKey = "InventoryData";
    
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
        
        // Firebase 매니저 초기화 (비동기)
        StartCoroutine(InitializeFirebaseWhenReady());
        
        // 인벤토리 로드 (Firebase 우선, 실패 시 로컬)
        if (autoLoadOnStart)
        {
            // Firebase 인증 상태 확인 후 로드
            if (enableFirebaseSync && FirebaseAuth.DefaultInstance != null && FirebaseAuth.DefaultInstance.CurrentUser != null)
            {
                // Firebase 인증이 완료된 상태에서 로드
                try
                {
                    bool loaded = await LoadInventoryFromFirebase();
                    if (loaded && showDebugInfo)
                        Debug.Log("[InventoryManager] Firebase에서 인벤토리 로드 완료");
                }
                catch (System.Exception e)
                {
                    if (showDebugInfo)
                        Debug.LogWarning($"[InventoryManager] Firebase 로드 실패, 로컬로 전환: {e.Message}");
                    
                    // Firebase 실패 시 로컬 로드
                    if (enableLocalSave)
                    {
                        LoadInventoryFromLocal();
                    }
                }
            }
            else
            {
                // Firebase 인증이 안된 상태에서는 로컬만 로드
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] Firebase 인증 대기 중, 로컬 로드 시도");
                
                if (enableLocalSave)
                {
                    LoadInventoryFromLocal();
                }
            }
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
        AddItem(new Item("wooden_berry", "나무 열매", "맛있는 나무 열매입니다.", ItemType.Food), 5);
        AddItem(new Item("stone", "돌", "건축에 사용할 수 있는 돌입니다.", ItemType.Material), 10);
        AddItem(new Item("wooden_stick", "나무 막대", "다양한 용도로 사용할 수 있습니다.", ItemType.Tool), 3);
        
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
    
    #region 로컬 저장
    
    /// <summary>
    /// 로컬에 인벤토리 저장
    /// </summary>
    public bool SaveInventoryToLocal()
    {
        if (!enableLocalSave) return false;
        
        try
        {
            // 인벤토리 데이터를 JSON으로 직렬화
            var inventoryData = new LocalInventoryData();
            inventoryData.slots = new List<LocalInventorySlot>();
            
            foreach (var slot in slots)
            {
                var localSlot = new LocalInventorySlot();
                localSlot.isOccupied = slot.isOccupied;
                localSlot.quantity = slot.quantity;
                
                if (slot.isOccupied && slot.item != null)
                {
                    localSlot.item = new LocalItemData
                    {
                        itemId = slot.item.itemId,
                        itemName = slot.item.itemName,
                        description = slot.item.description,
                        itemType = slot.item.itemType.ToString(),
                        isStackable = slot.item.isStackable,
                        maxStackSize = slot.item.maxStackSize,
                        value = slot.item.value,
                        isConsumable = slot.item.isConsumable,
                        isEquippable = slot.item.isEquippable,
                        healthRestore = slot.item.healthRestore,
                        staminaRestore = slot.item.staminaRestore
                    };
                }
                
                inventoryData.slots.Add(localSlot);
            }
            
            string jsonData = JsonUtility.ToJson(inventoryData, true);
            PlayerPrefs.SetString(localSaveKey, jsonData);
            PlayerPrefs.Save();
            
            if (showDebugInfo)
                Debug.Log("[InventoryManager] 로컬에 인벤토리 저장 완료");
            
            return true;
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
                Debug.LogError($"[InventoryManager] 로컬 저장 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 로컬에서 인벤토리 불러오기
    /// </summary>
    public bool LoadInventoryFromLocal()
    {
        if (!enableLocalSave) return false;
        
        try
        {
            if (!PlayerPrefs.HasKey(localSaveKey))
            {
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] 로컬에 저장된 인벤토리가 없습니다.");
                return false;
            }
            
            string jsonData = PlayerPrefs.GetString(localSaveKey);
            var inventoryData = JsonUtility.FromJson<LocalInventoryData>(jsonData);
            
            if (inventoryData == null || inventoryData.slots == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning("[InventoryManager] 로컬 인벤토리 데이터가 손상되었습니다.");
                return false;
            }
            
            // 기존 인벤토리 초기화
            InitializeInventory();
            
            // 불러온 데이터로 인벤토리 복원
            for (int i = 0; i < Mathf.Min(inventoryData.slots.Count, slots.Count); i++)
            {
                var localSlot = inventoryData.slots[i];
                if (localSlot.isOccupied && localSlot.item != null)
                {
                    // Item 객체 생성
                    var item = new Item();
                    item.itemId = localSlot.item.itemId;
                    item.itemName = localSlot.item.itemName;
                    item.description = localSlot.item.description;
                    item.itemType = (ItemType)System.Enum.Parse(typeof(ItemType), localSlot.item.itemType);
                    item.isStackable = localSlot.item.isStackable;
                    item.maxStackSize = localSlot.item.maxStackSize;
                    item.value = localSlot.item.value;
                    item.isConsumable = localSlot.item.isConsumable;
                    item.isEquippable = localSlot.item.isEquippable;
                    item.healthRestore = localSlot.item.healthRestore;
                    item.staminaRestore = localSlot.item.staminaRestore;
                    
                    // 아이콘 설정 (허브 아이템 특별 처리)
                    if (item.itemName.Contains("허브") || item.itemName.Contains("herb") || item.itemName.ToLower().Contains("healing"))
                    {
                        Sprite herbIcon = Resources.Load<Sprite>("Healing_Herb");
                        if (herbIcon != null)
                        {
                            item.icon = herbIcon;
                        }
                        else
                        {
                            // 동적 허브 아이콘 생성
                            item.icon = CreateDynamicHerbIcon();
                        }
                    }
                    
                    slots[i].item = item;
                    slots[i].quantity = localSlot.quantity;
                    // isOccupied는 item과 quantity에 따라 자동 계산됨
                }
            }
            
            OnInventoryChanged?.Invoke();
            OnInventoryLoaded?.Invoke();
            
            if (showDebugInfo)
                Debug.Log($"[InventoryManager] 로컬에서 인벤토리 불러오기 완료: {inventoryData.slots.Count}개 슬롯");
            
            return true;
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
                Debug.LogError($"[InventoryManager] 로컬 로드 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 동적으로 허브 아이콘 생성
    /// </summary>
    private Sprite CreateDynamicHerbIcon()
    {
        int iconSize = 64;
        Texture2D texture = new Texture2D(iconSize, iconSize);
        
        // 초록색 허브 아이콘 생성
        Color[] pixels = new Color[iconSize * iconSize];
        Vector2 center = new Vector2(iconSize / 2f, iconSize / 2f);
        
        for (int x = 0; x < iconSize; x++)
        {
            for (int y = 0; y < iconSize; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                
                if (distance < iconSize / 3f)
                {
                    // 중앙 원형 - 밝은 초록색
                    pixels[y * iconSize + x] = new Color(0.2f, 0.8f, 0.2f, 1f);
                }
                else if (distance < iconSize / 2f)
                {
                    // 외곽 원형 - 어두운 초록색
                    pixels[y * iconSize + x] = new Color(0.1f, 0.5f, 0.1f, 1f);
                }
                else
                {
                    // 배경 - 투명
                    pixels[y * iconSize + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, iconSize, iconSize), new Vector2(0.5f, 0.5f));
        sprite.name = "Dynamic_Herb_Icon";
        
        return sprite;
    }
    
    #endregion
    
    #region Firebase 연동
    
    /// <summary>
    /// Firebase 초기화 대기
    /// </summary>
    private async System.Threading.Tasks.Task WaitForFirebaseInitialization()
    {
        float maxWaitTime = 15f; // 최대 15초 대기 (Firebase 인증 시간 고려)
        float elapsedTime = 0f;
        
        while (elapsedTime < maxWaitTime)
        {
            // FirebaseAuth와 firebaseInventoryManager 모두 확인
            bool authReady = FirebaseAuth.DefaultInstance != null && FirebaseAuth.DefaultInstance.CurrentUser != null;
            bool managerReady = firebaseInventoryManager != null;
            
            if (authReady && managerReady)
            {
                if (showDebugInfo)
                    Debug.Log($"[InventoryManager] Firebase 초기화 완료 - User: {FirebaseAuth.DefaultInstance.CurrentUser.UserId}");
                return;
            }
            
            if (showDebugInfo)
                Debug.Log($"[InventoryManager] Firebase 초기화 대기 중... Auth: {authReady}, Manager: {managerReady}, 경과시간: {elapsedTime:F1}초");
            
            await System.Threading.Tasks.Task.Delay(1000); // 1초마다 체크 (더 안정적)
            elapsedTime += 1f;
        }
        
        if (showDebugInfo)
            Debug.LogWarning($"[InventoryManager] Firebase 초기화 시간 초과 ({maxWaitTime}초)");
    }
    
    /// <summary>
    /// Firebase 초기화 대기 후 매니저 초기화
    /// </summary>
    private System.Collections.IEnumerator InitializeFirebaseWhenReady()
    {
        // Firebase 초기화 대기
        float maxWaitTime = 15f;
        float elapsedTime = 0f;
        
        while (elapsedTime < maxWaitTime)
        {
            if (FirebaseApp.DefaultInstance != null && 
                FirebaseDatabase.DefaultInstance != null && 
                FirebaseAuth.DefaultInstance != null)
            {
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] Firebase 초기화 완료, 매니저 초기화 시작");
                
                InitializeFirebase();
                yield break;
            }
            
            if (showDebugInfo)
                Debug.Log($"[InventoryManager] Firebase 초기화 대기 중... {elapsedTime:F1}초");
            
            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }
        
        if (showDebugInfo)
            Debug.LogError("[InventoryManager] Firebase 초기화 시간 초과!");
    }
    
    /// <summary>
    /// Firebase 매니저 초기화
    /// </summary>
    private void InitializeFirebase()
    {
        if (!enableFirebaseSync) 
        {
            if (showDebugInfo)
                Debug.Log("[InventoryManager] Firebase 연동이 비활성화되어 있습니다.");
            return;
        }
        
        try
        {
            // FirebaseInventoryManager 찾기 또는 생성
            firebaseInventoryManager = FindObjectOfType<FirebaseInventoryManager>();
            if (firebaseInventoryManager == null)
            {
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] FirebaseInventoryManager를 찾을 수 없습니다. 생성 중...");
                
                GameObject firebaseObj = new GameObject("FirebaseInventoryManager");
                firebaseInventoryManager = firebaseObj.AddComponent<FirebaseInventoryManager>();
                DontDestroyOnLoad(firebaseObj);
                
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] FirebaseInventoryManager 생성 완료");
            }
            else
            {
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] 기존 FirebaseInventoryManager 발견");
            }
            
            // 이벤트 구독 (null 체크 후)
            if (firebaseInventoryManager != null)
            {
                firebaseInventoryManager.OnInventoryLoaded += OnFirebaseInventoryLoaded;
                firebaseInventoryManager.OnInventorySaved += OnFirebaseInventorySaved;
                firebaseInventoryManager.OnError += OnFirebaseInventoryError;
                
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] Firebase 이벤트 연결 완료");
            }
            else
            {
                if (showDebugInfo)
                    Debug.LogError("[InventoryManager] FirebaseInventoryManager 초기화 실패!");
            }
        }
        catch (System.Exception e)
        {
            if (showDebugInfo)
                Debug.LogError($"[InventoryManager] Firebase 초기화 중 오류: {e?.Message ?? "알 수 없는 오류"}");
        }
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
        if (Firebase.Auth.FirebaseAuth.DefaultInstance == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("[InventoryManager] Firebase Auth가 초기화되지 않았습니다.");
            return false;
        }
        
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
            
            // firebaseInventoryManager null 체크
            if (firebaseInventoryManager == null)
            {
                if (showDebugInfo)
                    Debug.LogWarning("[InventoryManager] FirebaseInventoryManager가 null입니다. 다시 초기화를 시도합니다.");
                
                InitializeFirebase();
                
                if (firebaseInventoryManager == null)
                {
                    if (showDebugInfo)
                        Debug.LogError("[InventoryManager] FirebaseInventoryManager 초기화에 실패했습니다.");
                    return false;
                }
                else
                {
                    if (showDebugInfo)
                        Debug.Log("[InventoryManager] FirebaseInventoryManager 재초기화 성공");
                }
            }
            else
            {
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] FirebaseInventoryManager 정상 확인");
            }
            
            // Firebase 로드 시도
            List<InventorySlot> loadedSlots = null;
            try
            {
                if (firebaseInventoryManager == null)
                {
                    if (showDebugInfo)
                        Debug.LogError("[InventoryManager] firebaseInventoryManager가 null입니다!");
                    return false;
                }
                
                loadedSlots = await firebaseInventoryManager.LoadInventory();
            }
            catch (System.Exception loadException)
            {
                if (showDebugInfo)
                    Debug.LogError($"[InventoryManager] Firebase 로드 중 오류: {loadException?.Message ?? "알 수 없는 오류"}");
                throw; // 원래 예외를 다시 던짐
            }
            
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
                Debug.LogError($"[InventoryManager] Firebase 인벤토리 불러오기 실패: {e?.Message ?? "알 수 없는 오류"}");
            
            try
            {
                OnFirebaseError?.Invoke($"인벤토리 불러오기 실패: {e?.Message ?? "알 수 없는 오류"}");
            }
            catch (System.Exception invokeException)
            {
                if (showDebugInfo)
                    Debug.LogError($"[InventoryManager] OnFirebaseError 호출 중 오류: {invokeException?.Message ?? "알 수 없는 오류"}");
            }
            
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
        
        if (showDebugInfo)
            Debug.Log($"[InventoryManager] 인벤토리 변경 감지 - 자동 저장: {autoSaveOnChange}, Firebase: {enableFirebaseSync}, 로컬: {enableLocalSave}");
        
        // 자동 저장 활성화 시 저장
        if (autoSaveOnChange)
        {
            // Firebase 우선, 실패 시 로컬
            if (enableFirebaseSync && firebaseInventoryManager != null)
            {
                try
                {
                    if (showDebugInfo)
                        Debug.Log("[InventoryManager] Firebase에 인벤토리 저장 시도...");
                    await SaveInventoryToFirebase();
                    if (showDebugInfo)
                        Debug.Log("[InventoryManager] Firebase 저장 완료");
                }
                catch (System.Exception e)
                {
                    if (showDebugInfo)
                        Debug.LogWarning($"[InventoryManager] Firebase 저장 실패, 로컬로 저장: {e.Message}");
                    SaveInventoryToLocal();
                }
            }
            else if (enableLocalSave)
            {
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] 로컬에 인벤토리 저장 시도...");
                SaveInventoryToLocal();
                if (showDebugInfo)
                    Debug.Log("[InventoryManager] 로컬 저장 완료");
            }
            else
            {
                if (showDebugInfo)
                    Debug.LogWarning("[InventoryManager] 저장 옵션이 모두 비활성화되어 있습니다!");
            }
        }
        else
        {
            if (showDebugInfo)
                Debug.Log("[InventoryManager] 자동 저장이 비활성화되어 있습니다.");
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

/// <summary>
/// 로컬 저장용 인벤토리 데이터
/// </summary>
[System.Serializable]
public class LocalInventoryData
{
    public List<LocalInventorySlot> slots = new List<LocalInventorySlot>();
}

/// <summary>
/// 로컬 저장용 인벤토리 슬롯 데이터
/// </summary>
[System.Serializable]
public class LocalInventorySlot
{
    public bool isOccupied;
    public int quantity;
    public LocalItemData item;
}

/// <summary>
/// 로컬 저장용 아이템 데이터
/// </summary>
[System.Serializable]
public class LocalItemData
{
    public string itemId;
    public string itemName;
    public string description;
    public string itemType;
    public bool isStackable;
    public int maxStackSize;
    public float value;
    public bool isConsumable;
    public bool isEquippable;
    public float healthRestore;
    public float staminaRestore;
}
