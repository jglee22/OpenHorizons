using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

/// <summary>
/// Firebase Realtime Database를 사용한 인벤토리 데이터 관리
/// </summary>
public class FirebaseInventoryManager : MonoBehaviour
{
    [Header("Firebase 설정")]
    public bool enableDebugLogs = true;
    
    [Header("데이터베이스 경로")]
    public string inventoryPath = "inventory";
    
    [Header("아이템 데이터베이스")]
    public ItemDatabase itemDatabase;
    
    // Firebase 참조
    private DatabaseReference databaseReference;
    private FirebaseAuth auth;
    
    // 이벤트
    public System.Action<List<InventorySlot>> OnInventoryLoaded;
    public System.Action<bool> OnInventorySaved;
    public System.Action<string> OnError;
    
    private void Start()
    {
        StartCoroutine(InitializeFirebaseWhenReady());
    }
    
    private System.Collections.IEnumerator InitializeFirebaseWhenReady()
    {
        // Firebase 초기화 대기
        float maxWaitTime = 15f;
        float elapsedTime = 0f;
        
        while (elapsedTime < maxWaitTime)
        {
            // Firebase 초기화 상태 확인
            bool firebaseAppReady = FirebaseApp.DefaultInstance != null;
            bool firebaseDatabaseReady = FirebaseDatabase.DefaultInstance != null;
            bool firebaseAuthReady = FirebaseAuth.DefaultInstance != null;
            
            if (enableDebugLogs)
                Debug.Log($"[FirebaseInventory] Firebase 상태 체크 - App: {firebaseAppReady}, Database: {firebaseDatabaseReady}, Auth: {firebaseAuthReady}");
            
            if (firebaseAppReady && firebaseDatabaseReady && firebaseAuthReady)
            {
                if (enableDebugLogs)
                    Debug.Log("[FirebaseInventory] Firebase 초기화 완료, 초기화 시작");
                
                InitializeFirebase();
                
                // 초기화 후 DatabaseReference 확인
                if (databaseReference != null)
                {
                    if (enableDebugLogs)
                        Debug.Log("[FirebaseInventory] DatabaseReference 초기화 성공!");
                    yield break;
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.LogError("[FirebaseInventory] DatabaseReference 초기화 실패!");
                }
            }
            
            yield return new WaitForSeconds(1f);
            elapsedTime += 1f;
        }
        
        if (enableDebugLogs)
            Debug.LogError("[FirebaseInventory] Firebase 초기화 시간 초과!");
    }
    
    private void InitializeFirebase()
    {
        try
        {
            // Firebase 초기화 상태 확인
            if (FirebaseApp.DefaultInstance == null)
            {
                if (enableDebugLogs)
                    Debug.LogError("[FirebaseInventory] Firebase가 초기화되지 않았습니다!");
                return;
            }
            
            if (FirebaseDatabase.DefaultInstance == null)
            {
                if (enableDebugLogs)
                    Debug.LogError("[FirebaseInventory] FirebaseDatabase가 초기화되지 않았습니다!");
                return;
            }
            
            databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
            auth = FirebaseAuth.DefaultInstance;
            
            if (databaseReference == null)
            {
                if (enableDebugLogs)
                    Debug.LogError("[FirebaseInventory] DatabaseReference가 null입니다!");
                return;
            }
            
            if (auth == null)
            {
                if (enableDebugLogs)
                    Debug.LogError("[FirebaseInventory] FirebaseAuth가 null입니다!");
                return;
            }
            
            // ItemDatabase 자동 찾기
            if (itemDatabase == null)
            {
                itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
                if (itemDatabase == null)
                {
                    // Resources에서 찾을 수 없으면 모든 ItemDatabase 찾기
                    ItemDatabase[] databases = Resources.FindObjectsOfTypeAll<ItemDatabase>();
                    if (databases.Length > 0)
                    {
                        itemDatabase = databases[0];
                        if (enableDebugLogs)
                            Debug.Log($"[FirebaseInventory] ItemDatabase 자동 할당: {itemDatabase.name}");
                    }
                    else
                    {
                        Debug.LogWarning("[FirebaseInventory] ItemDatabase를 찾을 수 없습니다!");
                    }
                }
            }
            
            if (enableDebugLogs)
                Debug.Log("[FirebaseInventory] Firebase 초기화 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseInventory] Firebase 초기화 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 현재 로그인한 사용자의 인벤토리 저장
    /// </summary>
    public async Task<bool> SaveInventory(List<InventorySlot> inventorySlots)
    {
        if (auth.CurrentUser == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[FirebaseInventory] 로그인된 사용자가 없습니다.");
            OnError?.Invoke("로그인된 사용자가 없습니다.");
            return false;
        }
        
        try
        {
            string userId = auth.CurrentUser.UserId;
            string path = $"{inventoryPath}/{userId}";
            
            // 인벤토리 데이터를 Firebase에 저장할 수 있는 형태로 변환
            var inventoryData = ConvertInventoryToFirebaseData(inventorySlots);
            
            // Firebase에 저장
            await databaseReference.Child(path).SetRawJsonValueAsync(JsonUtility.ToJson(inventoryData));
            
            if (enableDebugLogs)
                Debug.Log($"[FirebaseInventory] 인벤토리 저장 완료: {userId}");
            
            OnInventorySaved?.Invoke(true);
            return true;
        }
        catch (Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"[FirebaseInventory] 인벤토리 저장 실패: {e.Message}");
            
            OnError?.Invoke($"인벤토리 저장 실패: {e.Message}");
            OnInventorySaved?.Invoke(false);
            return false;
        }
    }
    
    /// <summary>
    /// 현재 로그인한 사용자의 인벤토리 불러오기
    /// </summary>
    public async Task<List<InventorySlot>> LoadInventory()
    {
        // Firebase 초기화 상태 확인
        if (databaseReference == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[FirebaseInventory] DatabaseReference가 null입니다. 재초기화 시도...");
            
            // 재초기화 시도
            InitializeFirebase();
            
            if (databaseReference == null)
            {
                if (enableDebugLogs)
                    Debug.LogError("[FirebaseInventory] DatabaseReference 재초기화 실패!");
                OnError?.Invoke("DatabaseReference가 초기화되지 않았습니다.");
                return new List<InventorySlot>();
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log("[FirebaseInventory] DatabaseReference 재초기화 성공!");
            }
        }
        
        if (auth == null)
        {
            if (enableDebugLogs)
                Debug.LogError("[FirebaseInventory] FirebaseAuth가 null입니다!");
            OnError?.Invoke("FirebaseAuth가 초기화되지 않았습니다.");
            return new List<InventorySlot>();
        }
        
        if (auth.CurrentUser == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[FirebaseInventory] 로그인된 사용자가 없습니다.");
            OnError?.Invoke("로그인된 사용자가 없습니다.");
            return new List<InventorySlot>();
        }
        
        try
        {
            string userId = auth.CurrentUser.UserId;
            string path = $"{inventoryPath}/{userId}";
            
            if (enableDebugLogs)
                Debug.Log($"[FirebaseInventory] 인벤토리 로드 시작: {path}");
            
            // Firebase에서 데이터 불러오기
            var snapshot = await databaseReference.Child(path).GetValueAsync();
            
            if (!snapshot.Exists)
            {
                if (enableDebugLogs)
                    Debug.Log($"[FirebaseInventory] 저장된 인벤토리가 없습니다: {userId}");
                return new List<InventorySlot>();
            }
            
            // JSON 데이터를 인벤토리 데이터로 변환
            string jsonData = snapshot.GetRawJsonValue();
            var firebaseInventoryData = JsonUtility.FromJson<FirebaseInventoryData>(jsonData);
            
            // InventorySlot 리스트로 변환
            var inventorySlots = ConvertFirebaseDataToInventory(firebaseInventoryData);
            
            if (enableDebugLogs)
                Debug.Log($"[FirebaseInventory] 인벤토리 불러오기 완료: {userId}, 슬롯 수: {inventorySlots.Count}");
            
            OnInventoryLoaded?.Invoke(inventorySlots);
            return inventorySlots;
        }
        catch (Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"[FirebaseInventory] 인벤토리 불러오기 실패: {e.Message}");
            
            OnError?.Invoke($"인벤토리 불러오기 실패: {e.Message}");
            return new List<InventorySlot>();
        }
    }
    
    /// <summary>
    /// 인벤토리 데이터를 Firebase에 저장할 수 있는 형태로 변환
    /// </summary>
    private FirebaseInventoryData ConvertInventoryToFirebaseData(List<InventorySlot> inventorySlots)
    {
        var firebaseData = new FirebaseInventoryData();
        firebaseData.slots = new List<FirebaseInventorySlot>();
        
        foreach (var slot in inventorySlots)
        {
            var firebaseSlot = new FirebaseInventorySlot();
            firebaseSlot.isOccupied = slot.isOccupied;
            firebaseSlot.quantity = slot.quantity;
            
            if (slot.isOccupied && slot.item != null)
            {
                firebaseSlot.item = new FirebaseItemData
                {
                    itemId = GetItemIdFromItem(slot.item), // 아이템 ID 추가
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
            
            firebaseData.slots.Add(firebaseSlot);
        }
        
        return firebaseData;
    }
    
    /// <summary>
    /// Firebase 데이터를 InventorySlot 리스트로 변환
    /// </summary>
    private List<InventorySlot> ConvertFirebaseDataToInventory(FirebaseInventoryData firebaseData)
    {
        var inventorySlots = new List<InventorySlot>();
        
        foreach (var firebaseSlot in firebaseData.slots)
        {
            var slot = new InventorySlot();
            slot.quantity = firebaseSlot.quantity;
            
            if (firebaseSlot.isOccupied && firebaseSlot.item != null)
            {
                // ScriptableObject에서 아이템 데이터 찾기 (ID 우선, 이름 폴백)
                ItemData itemData = null;
                if (itemDatabase != null)
                {
                    // 먼저 아이템 ID로 찾기
                    if (!string.IsNullOrEmpty(firebaseSlot.item.itemId))
                    {
                        itemData = itemDatabase.GetItemById(firebaseSlot.item.itemId);
                    }
                    
                    // ID로 찾을 수 없으면 이름으로 찾기 (하위 호환성)
                    if (itemData == null)
                    {
                        itemData = itemDatabase.GetItem(firebaseSlot.item.itemName);
                    }
                }
                
                if (itemData != null)
                {
                    // ScriptableObject에서 Item 생성 (아이콘 포함)
                    slot.item = itemData.ToItem();
                    
                    if (enableDebugLogs)
                        Debug.Log($"[FirebaseInventory] ScriptableObject에서 아이템 로드: {itemData.itemName} (ID: {itemData.itemId})");
                }
                else
                {
                    // ScriptableObject에서 찾을 수 없으면 기존 방식으로 생성
                    slot.item = new Item
                    {
                        itemName = firebaseSlot.item.itemName,
                        description = firebaseSlot.item.description,
                        itemType = (ItemType)Enum.Parse(typeof(ItemType), firebaseSlot.item.itemType),
                        isStackable = firebaseSlot.item.isStackable,
                        maxStackSize = firebaseSlot.item.maxStackSize,
                        value = firebaseSlot.item.value,
                        isConsumable = firebaseSlot.item.isConsumable,
                        isEquippable = firebaseSlot.item.isEquippable,
                        healthRestore = firebaseSlot.item.healthRestore,
                        staminaRestore = firebaseSlot.item.staminaRestore
                    };
                    
                    // 아이템 아이콘 재생성 - 허브 아이템 특별 처리
                    if (slot.item.itemName.Contains("허브") || slot.item.itemName.Contains("herb") || slot.item.itemName.ToLower().Contains("healing"))
                    {
                        // 허브 아이템 아이콘 로드 (강화된 방법)
                        string[] herbPaths = {
                            "Healing_Herb",
                            "Healing_Herb.png",
                            "healing_herb", 
                            "Healing Herb",
                            "healing herb",
                            "치유 허브",
                            "허브"
                        };
                        
                        Sprite herbIcon = null;
                        
                        // 먼저 일반적인 경로들로 시도
                        foreach (string path in herbPaths)
                        {
                            if (enableDebugLogs)
                                Debug.Log($"[FirebaseInventory] 허브 아이콘 로드 시도: {path}");
                            herbIcon = Resources.Load<Sprite>(path);
                            if (herbIcon != null)
                            {
                                if (enableDebugLogs)
                                    Debug.Log($"[FirebaseInventory] 허브 아이콘 로드 성공: {path}");
                                break;
                            }
                        }
                        
                        // 모든 경로 실패 시 Resources 폴더에서 모든 스프라이트 검색
                        if (herbIcon == null)
                        {
                            if (enableDebugLogs)
                                Debug.Log("[FirebaseInventory] Resources 폴더에서 모든 스프라이트 검색 중...");
                            Sprite[] allSprites = Resources.LoadAll<Sprite>("");
                            if (enableDebugLogs)
                                Debug.Log($"[FirebaseInventory] Resources 폴더에서 발견된 스프라이트 수: {allSprites.Length}");
                            
                            // 모든 스프라이트 이름 출력
                            if (enableDebugLogs)
                            {
                                foreach (Sprite sprite in allSprites)
                                {
                                    Debug.Log($"[FirebaseInventory] 발견된 스프라이트: {sprite.name}");
                                }
                            }
                            
                            foreach (Sprite sprite in allSprites)
                            {
                                if (sprite.name.ToLower().Contains("herb") || 
                                    sprite.name.ToLower().Contains("healing") ||
                                    sprite.name.Contains("허브"))
                                {
                                    herbIcon = sprite;
                                    if (enableDebugLogs)
                                        Debug.Log($"[FirebaseInventory] 허브 아이콘 발견: {sprite.name}");
                                    break;
                                }
                            }
                        }
                        
                        if (herbIcon != null)
                        {
                            slot.item.icon = herbIcon;
                            if (enableDebugLogs)
                                Debug.Log($"[FirebaseInventory] 허브 아이콘 설정 완료: {herbIcon.name}");
                        }
                        else
                        {
                            if (enableDebugLogs)
                                Debug.LogWarning("[FirebaseInventory] 모든 방법으로 허브 아이콘을 찾을 수 없습니다. 동적 생성합니다.");
                            slot.item.icon = CreateDynamicHerbIcon();
                        }
                    }
                    else
                    {
                        // 다른 아이템들은 기존 방식 사용
                        slot.item.icon = CreateItemIcon(slot.item.itemType, slot.item.itemName);
                    }
                    
                    if (enableDebugLogs)
                        Debug.LogWarning($"[FirebaseInventory] ScriptableObject에서 아이템을 찾을 수 없어 기존 방식 사용: {firebaseSlot.item.itemName}");
                }
            }
            
            inventorySlots.Add(slot);
        }
        
        return inventorySlots;
    }
    
    /// <summary>
    /// Item에서 아이템 ID 가져오기
    /// </summary>
    private string GetItemIdFromItem(Item item)
    {
        if (itemDatabase != null)
        {
            // ScriptableObject에서 같은 이름의 아이템 찾기
            var itemData = itemDatabase.GetItem(item.itemName);
            if (itemData != null)
            {
                return itemData.itemId;
            }
        }
        
        // ScriptableObject에서 찾을 수 없으면 이름을 ID로 사용
        return item.itemName.ToLower().Replace(" ", "_");
    }
    
    /// <summary>
    /// 아이템 아이콘 생성 (Firebase에서 불러온 아이템용)
    /// </summary>
    private Sprite CreateItemIcon(ItemType itemType, string itemName)
    {
        if (enableDebugLogs)
            Debug.Log($"[FirebaseInventory] 아이콘 생성 시도: {itemName} ({itemType})");
        
        // 먼저 Resources 폴더에서 아이콘 로드 시도
        Sprite loadedIcon = LoadItemIconFromResources(itemName, itemType);
        if (loadedIcon != null)
        {
            if (enableDebugLogs)
                Debug.Log($"[FirebaseInventory] Resources에서 아이콘 로드 성공: {itemName}");
            return loadedIcon;
        }
        
        // Resources에서 찾을 수 없으면 동적으로 생성
        if (enableDebugLogs)
            Debug.Log($"[FirebaseInventory] Resources에서 찾을 수 없어 동적 생성: {itemName}");
        return CreateDynamicItemIcon(itemType, itemName);
    }
    
    /// <summary>
    /// Resources 폴더에서 아이템 아이콘 로드 시도
    /// </summary>
    private Sprite LoadItemIconFromResources(string itemName, ItemType itemType)
    {
        // 여러 경로에서 시도 (대소문자 구분 없이)
        string[] possiblePaths = {
            itemName, // 직접 이름
            $"{itemName}", // 확장자 없이
            $"Images/Items/{itemName}",
            $"Images/Items/{itemType}/{itemName}",
            $"Icons/{itemName}",
            $"Icons/{itemType}/{itemName}",
            // 대소문자 변형들
            itemName.ToLower(),
            itemName.ToUpper(),
            itemName.Replace(" ", "_"),
            itemName.Replace(" ", ""),
            // Bronze_Sword 특별 처리
            "Bronze_Sword",
            "bronze_sword",
            "Bronze Sword",
            "bronze sword",
            // 허브 아이템 특별 처리
            "Healing_Herb",
            "healing_herb",
            "치유 허브",
            "허브"
        };
        
        foreach (string path in possiblePaths)
        {
            if (enableDebugLogs)
                Debug.Log($"[FirebaseInventory] 아이콘 로드 시도: {path}");
            
            Sprite icon = Resources.Load<Sprite>(path);
            if (icon != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"[FirebaseInventory] 아이콘 로드 성공: {path} (원본: {itemName})");
                return icon;
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log($"[FirebaseInventory] 아이콘 로드 실패: {path}");
            }
        }
        
        if (enableDebugLogs)
            Debug.LogWarning($"[FirebaseInventory] Resources에서 아이콘을 찾을 수 없습니다: {itemName}");
        
        return null;
    }
    
    /// <summary>
    /// 동적으로 아이템 아이콘 생성
    /// </summary>
    private Sprite CreateDynamicItemIcon(ItemType itemType, string itemName)
    {
        int iconSize = 64;
        Texture2D texture = new Texture2D(iconSize, iconSize);
        
        // 아이템 타입에 따른 색상 설정
        Color iconColor = GetItemTypeColor(itemType);
        
        // 배경색으로 채우기
        Color[] pixels = new Color[iconSize * iconSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = iconColor;
        }
        
        // 간단한 패턴 추가 (중앙에 원형)
        Vector2 center = new Vector2(iconSize / 2f, iconSize / 2f);
        float radius = iconSize / 3f;
        
        for (int x = 0; x < iconSize; x++)
        {
            for (int y = 0; y < iconSize; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance < radius)
                {
                    int index = y * iconSize + x;
                    pixels[index] = Color.white;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Texture2D를 Sprite로 변환
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, iconSize, iconSize), new Vector2(0.5f, 0.5f));
        sprite.name = $"{itemName}_Icon";
        
        if (enableDebugLogs)
            Debug.Log($"[FirebaseInventory] 동적 아이콘 생성: {itemName} ({itemType})");
        
        return sprite;
    }
    
    /// <summary>
    /// 동적 허브 아이콘 생성
    /// </summary>
    private Sprite CreateDynamicHerbIcon()
    {
        int iconSize = 64;
        Texture2D texture = new Texture2D(iconSize, iconSize);
        
        // 허브 색상 (녹색 계열)
        Color herbColor = new Color(0.2f, 0.8f, 0.3f, 1f); // 밝은 녹색
        Color stemColor = new Color(0.4f, 0.6f, 0.2f, 1f); // 어두운 녹색
        
        // 배경색으로 채우기
        Color[] pixels = new Color[iconSize * iconSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = herbColor;
        }
        
        // 허브 모양 그리기 (중앙에 원형)
        Vector2 center = new Vector2(iconSize / 2f, iconSize / 2f);
        float radius = iconSize / 3f;
        
        for (int x = 0; x < iconSize; x++)
        {
            for (int y = 0; y < iconSize; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance < radius)
                {
                    int index = y * iconSize + x;
                    // 허브 잎 부분
                    if (y > center.y)
                    {
                        pixels[index] = herbColor;
                    }
                    // 줄기 부분
                    else
                    {
                        pixels[index] = stemColor;
                    }
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Texture2D를 Sprite로 변환
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, iconSize, iconSize), new Vector2(0.5f, 0.5f));
        sprite.name = "DynamicHerbIcon";
        
        if (enableDebugLogs)
            Debug.Log("[FirebaseInventory] 동적 허브 아이콘 생성 완료");
        
        return sprite;
    }
    
    /// <summary>
    /// 아이템 타입에 따른 색상 반환
    /// </summary>
    private Color GetItemTypeColor(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Food:
                return new Color(0.2f, 0.8f, 0.2f); // 초록색 (허브 아이템용)
            case ItemType.Material:
                return new Color(0.6f, 0.6f, 0.6f); // 회색
            case ItemType.Tool:
                return new Color(0.4f, 0.4f, 0.8f); // 파란색
            case ItemType.Weapon:
                return new Color(0.8f, 0.2f, 0.2f); // 빨간색
            case ItemType.Armor:
                return new Color(0.2f, 0.8f, 0.2f); // 초록색
            case ItemType.Treasure:
                return new Color(1f, 0.8f, 0f); // 금색
            case ItemType.Misc:
            default:
                return new Color(0.5f, 0.5f, 0.5f); // 중간 회색
        }
    }
    
    /// <summary>
    /// 인벤토리 데이터 삭제
    /// </summary>
    public async Task<bool> DeleteInventory()
    {
        if (auth.CurrentUser == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[FirebaseInventory] 로그인된 사용자가 없습니다.");
            return false;
        }
        
        try
        {
            string userId = auth.CurrentUser.UserId;
            string path = $"{inventoryPath}/{userId}";
            
            await databaseReference.Child(path).RemoveValueAsync();
            
            if (enableDebugLogs)
                Debug.Log($"[FirebaseInventory] 인벤토리 삭제 완료: {userId}");
            
            return true;
        }
        catch (Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"[FirebaseInventory] 인벤토리 삭제 실패: {e.Message}");
            return false;
        }
    }
}

/// <summary>
/// Firebase에 저장할 인벤토리 데이터 구조
/// </summary>
[System.Serializable]
public class FirebaseInventoryData
{
    public List<FirebaseInventorySlot> slots = new List<FirebaseInventorySlot>();
}

/// <summary>
/// Firebase에 저장할 인벤토리 슬롯 데이터
/// </summary>
[System.Serializable]
public class FirebaseInventorySlot
{
    public bool isOccupied;
    public int quantity;
    public FirebaseItemData item;
}

/// <summary>
/// Firebase에 저장할 아이템 데이터
/// </summary>
[System.Serializable]
public class FirebaseItemData
{
    public string itemId; // 아이템 ID 추가
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
