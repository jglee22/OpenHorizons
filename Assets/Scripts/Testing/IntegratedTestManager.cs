using UnityEngine;
using AudioSystem;
using TMPro;

/// <summary>
/// 통합 테스트 매니저 - 전투, 퀘스트, 인벤토리 시스템 테스트
/// </summary>
public class IntegratedTestManager : MonoBehaviour
{
    [Header("전투 테스트 설정")]
    public GameObject enemyPrefab;
    public GameObject weaponPrefab;
    public Transform enemySpawnPoint;
    public Transform weaponSpawnPoint;
    
    [Header("퀘스트 테스트 설정")]
    public Quest testQuest;
    public GameObject testItemPrefab;
    
    [Header("UI 설정")]
    public TextMeshProUGUI questStatusText;
    
    // 전투 관련
    private PlayerCombatController playerCombat;
    private InventoryManager inventoryManager;
    private GameObject currentEquippedWeapon;
    private int enemyCount = 0;
    
    // 퀘스트 관련
    private Quest activeTestQuest;
    
    void Start()
    {
        // 컴포넌트 찾기
        playerCombat = FindObjectOfType<PlayerCombatController>();
        inventoryManager = FindObjectOfType<InventoryManager>();
        
        if (playerCombat == null)
            Debug.LogError("PlayerCombatController를 찾을 수 없습니다!");
        if (inventoryManager == null)
            Debug.LogError("InventoryManager를 찾을 수 없습니다!");
            
        UpdateQuestStatus();
    }
    
    void Update()
    {
        HandleTestInput();
    }
    
    void HandleTestInput()
    {
        // === 전투 시스템 테스트 (F키 그룹) ===
        if (Input.GetKeyDown(KeyCode.F7))
        {
            SpawnEnemy();
        }
        
        if (Input.GetKeyDown(KeyCode.F8))
        {
            SpawnWeapon();
        }
        
        if (Input.GetKeyDown(KeyCode.F9))
        {
            EquipWeapon();
        }
        
        if (Input.GetKeyDown(KeyCode.F10))
        {
            UnequipWeapon();
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            HealPlayer();
        }
        
        // === 퀘스트 시스템 테스트 ===
        if (Input.GetKeyDown(KeyCode.F1))
        {
            RegisterTestQuest();
        }
        
        if (Input.GetKeyDown(KeyCode.F2))
        {
            CompleteTestQuest();
        }
        
        if (Input.GetKeyDown(KeyCode.F3))
        {
            TestKillEnemy();
        }
        
        if (Input.GetKeyDown(KeyCode.F4))
        {
            TestCollectItem();
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            UpdateQuestStatus();
        }
    }
    
    // === 전투 시스템 메서드 ===
    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("적 프리팹이 설정되지 않았습니다!");
            return;
        }
        
        Vector3 spawnPosition = enemySpawnPoint != null ? 
            enemySpawnPoint.position : 
            transform.position + Random.insideUnitSphere * 5f;
        
        GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
        enemyCount++;
        enemy.name = $"Enemy_{enemyCount}";
        
        Debug.Log($"적을 생성했습니다! 총 적 수: {enemyCount}");
    }
    
    void SpawnWeapon()
    {
        if (weaponPrefab == null)
        {
            Debug.LogWarning("무기 프리팹이 설정되지 않았습니다!");
            return;
        }
        
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager를 찾을 수 없습니다!");
            return;
        }
        
        if (inventoryManager.HasItem("Bronze Sword", 1))
        {
            Debug.LogWarning("이미 인벤토리에 Bronze Sword가 있습니다!");
            return;
        }
        
        Item weaponItem = CreateWeaponItem();
        
        if (inventoryManager.AddItem(weaponItem, 1))
        {
            Debug.Log($"무기를 인벤토리에 추가했습니다: {weaponItem.itemName}");
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("item_pickup");
            }
        }
        else
        {
            Debug.LogWarning("인벤토리 공간이 부족합니다!");
        }
    }
    
    Item CreateWeaponItem()
    {
        Item weaponItem = new Item();
        weaponItem.itemId = "bronze_sword";
        weaponItem.itemName = "Bronze Sword";
        weaponItem.description = "청동으로 만든 견고한 검입니다. 데미지: 25, 범위: 2";
        weaponItem.itemType = ItemType.Weapon;
        weaponItem.isStackable = false;
        weaponItem.maxStackSize = 1;
        weaponItem.isEquippable = true;
        weaponItem.isConsumable = false;
        weaponItem.value = 100f;
        weaponItem.icon = LoadWeaponIcon();
        
        return weaponItem;
    }
    
    Sprite LoadWeaponIcon()
    {
        Sprite weaponIcon = Resources.Load<Sprite>("Images/WeaponImages/Bronze_Sword");
        
        if (weaponIcon == null)
        {
            weaponIcon = Resources.Load<Sprite>("Bronze_Sword");
        }
        
        return weaponIcon;
    }
    
    void EquipWeapon()
    {
        if (playerCombat == null || inventoryManager == null)
        {
            Debug.LogError("필요한 컴포넌트를 찾을 수 없습니다!");
            return;
        }
        
        if (playerCombat.currentWeapon != null)
        {
            Debug.LogWarning("이미 무기가 장착되어 있습니다!");
            return;
        }
        
        Item weaponItem = FindWeaponInInventory();
        if (weaponItem == null)
        {
            Debug.LogWarning("인벤토리에 무기가 없습니다!");
            return;
        }
        
        GameObject weaponObj = CreateWeaponFromItem(weaponItem);
        if (weaponObj == null)
        {
            Debug.LogError("무기 생성에 실패했습니다!");
            return;
        }
        
        Weapon weaponComponent = weaponObj.GetComponent<Weapon>();
        if (weaponComponent != null)
        {
            currentEquippedWeapon = weaponObj;
            playerCombat.EquipWeapon(weaponComponent);
            Debug.Log($"무기를 장착했습니다: {weaponComponent.weaponName}");
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("weapon_equip");
            }
        }
    }
    
    Item FindWeaponInInventory()
    {
        if (inventoryManager == null) return null;
        
        var slotsField = inventoryManager.GetType().GetField("slots", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (slotsField == null) return null;
        
        var slots = slotsField.GetValue(inventoryManager) as System.Collections.Generic.List<InventorySlot>;
        if (slots == null) return null;
        
        foreach (var slot in slots)
        {
            if (slot != null && slot.isOccupied && slot.item != null && slot.item.itemType == ItemType.Weapon)
            {
                return slot.item;
            }
        }
        
        return null;
    }
    
    GameObject CreateWeaponFromItem(Item weaponItem)
    {
        Debug.Log($"CreateWeaponFromItem 시작 - weaponPrefab: {weaponPrefab}");
        
        if (weaponPrefab == null)
        {
            Debug.LogError("weaponPrefab이 null입니다! Inspector에서 Weapon Prefab을 할당하세요.");
            return null;
        }
        
        Debug.Log("weaponPrefab으로 GameObject 생성 시도");
        GameObject weaponObj = Instantiate(weaponPrefab, Vector3.zero, Quaternion.identity);
        
        if (weaponObj == null)
        {
            Debug.LogError("weaponPrefab Instantiate 실패!");
            return null;
        }
        
        Debug.Log("Weapon 컴포넌트 설정 시도");
        Weapon weaponComponent = weaponObj.GetComponent<Weapon>();
        
        if (weaponComponent == null)
        {
            Debug.Log("Weapon 컴포넌트가 없어서 추가합니다.");
            weaponComponent = weaponObj.AddComponent<Weapon>();
        }
        
        weaponComponent.weaponName = weaponItem.itemName;
        weaponComponent.damage = 25f;
        weaponComponent.range = 2f;
        weaponComponent.attackSpeed = 1f;
        weaponComponent.durability = 100f;
        weaponComponent.maxDurability = 100f;
        
        Debug.Log($"무기 생성 완료: {weaponObj.name}");
        return weaponObj;
    }
    
    void UnequipWeapon()
    {
        if (playerCombat == null) return;
        
        if (playerCombat.currentWeapon == null)
        {
            Debug.LogWarning("장착된 무기가 없습니다!");
            return;
        }
        
        playerCombat.UnequipWeapon();
        
        if (currentEquippedWeapon != null)
        {
            Destroy(currentEquippedWeapon);
            currentEquippedWeapon = null;
        }
        
        Debug.Log("무기를 해제했습니다!");
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX("weapon_unequip");
        }
    }
    
    void HealPlayer()
    {
        if (playerCombat != null)
        {
            playerCombat.Heal(50f);
            Debug.Log("플레이어를 치료했습니다!");
        }
    }
    
    // === 퀘스트 시스템 메서드 ===
    void RegisterTestQuest()
    {
        Debug.Log("=== 퀘스트 등록 시도 ===");
        
        if (testQuest == null)
        {
            Debug.LogError("테스트 퀘스트가 설정되지 않았습니다! Inspector에서 Test Quest 필드를 할당하세요.");
            return;
        }
        
        Debug.Log($"테스트 퀘스트: {testQuest.DisplayName}");
        
        if (QuestSystem.Instance == null)
        {
            Debug.LogWarning("QuestSystem을 찾을 수 없습니다! 자동으로 생성합니다.");
            
            // QuestSystem 자동 생성
            GameObject questSystemObj = new GameObject("Quest System");
            questSystemObj.AddComponent<QuestSystem>();
            
            Debug.Log("QuestSystem이 자동으로 생성되었습니다.");
        }
        
        Debug.Log("QuestSystem 발견됨");
        
        try
        {
            activeTestQuest = QuestSystem.Instance.Register(testQuest);
            Debug.Log($"퀘스트 등록 성공: {activeTestQuest.DisplayName}");
            UpdateQuestStatus();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"퀘스트 등록 실패: {e.Message}");
        }
    }
    
    void CompleteTestQuest()
    {
        if (activeTestQuest != null && QuestSystem.Instance != null)
        {
            QuestSystem.Instance.CompleteWaitingQuests();
            Debug.Log("대기 중인 퀘스트 완료 시도");
            UpdateQuestStatus();
        }
    }
    
    void TestKillEnemy()
    {
        if (enemyPrefab != null)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 5f;
            spawnPos.y = transform.position.y;
            
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            enemy.name = enemyPrefab.name;
            Debug.Log("테스트 적 생성: " + enemy.name);
        }
        else
        {
            Debug.LogWarning("테스트 적 프리팹이 설정되지 않았습니다!");
        }
    }
    
    void TestCollectItem()
    {
        if (testItemPrefab != null)
        {
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 3f;
            spawnPos.y = transform.position.y;
            
            GameObject item = Instantiate(testItemPrefab, spawnPos, Quaternion.identity);
            Debug.Log("테스트 아이템 생성: " + item.name);
        }
        else
        {
            Debug.LogWarning("테스트 아이템 프리팹이 설정되지 않았습니다!");
        }
    }
    
    void UpdateQuestStatus()
    {
        if (questStatusText == null || QuestSystem.Instance == null) return;
        
        string status = "=== 퀘스트 상태 ===\n";
        status += $"활성 퀘스트: {QuestSystem.Instance.ActiveQuests.Count}\n";
        status += $"완료 퀘스트: {QuestSystem.Instance.CompletedQuests.Count}\n";
        status += $"활성 업적: {QuestSystem.Instance.ActiveAchievements.Count}\n";
        status += $"완료 업적: {QuestSystem.Instance.CompletedAchievements.Count}\n";
        
        if (activeTestQuest != null)
        {
            status += $"\n테스트 퀘스트: {activeTestQuest.DisplayName}\n";
            status += $"상태: {activeTestQuest.State}\n";
            status += $"완료 가능: {activeTestQuest.IsComplatable}\n";
        }
        
        questStatusText.text = status;
    }
    
}
