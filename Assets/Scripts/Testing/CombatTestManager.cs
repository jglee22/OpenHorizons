using UnityEngine;
using AudioSystem;

public class CombatTestManager : MonoBehaviour
{
    [Header("테스트 설정")]
    public GameObject enemyPrefab;
    public GameObject weaponPrefab;
    public Transform enemySpawnPoint;
    public Transform weaponSpawnPoint;
    
    [Header("UI")]
    public KeyCode spawnEnemyKey = KeyCode.E;
    public KeyCode spawnWeaponKey = KeyCode.Q;
    public KeyCode equipWeaponKey = KeyCode.R;
    public KeyCode unequipWeaponKey = KeyCode.T;
    public KeyCode healPlayerKey = KeyCode.H;
    
    private PlayerCombatController playerCombat;
    private int enemyCount = 0;
    private InventoryManager inventoryManager; // 인벤토리 매니저 참조
    private GameObject currentEquippedWeapon; // 현재 장착된 무기 오브젝트
    
    void Start()
    {
        // 플레이어 전투 컨트롤러 찾기
        playerCombat = FindObjectOfType<PlayerCombatController>();
        
        if (playerCombat == null)
        {
            Debug.LogError("PlayerCombatController를 찾을 수 없습니다!");
        }
        
        // 인벤토리 매니저 찾기
        inventoryManager = FindObjectOfType<InventoryManager>();
        
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager를 찾을 수 없습니다!");
        }
    }
    
    void Update()
    {
        HandleTestInput();
    }
    
    void HandleTestInput()
    {
        // 적 생성
        if (Input.GetKeyDown(spawnEnemyKey))
        {
            SpawnEnemy();
        }
        
        // 무기 생성 (획득)
        if (Input.GetKeyDown(spawnWeaponKey))
        {
            SpawnWeapon();
        }
        
        // 무기 장착
        if (Input.GetKeyDown(equipWeaponKey))
        {
            EquipWeapon();
        }
        
        // 무기 해제
        if (Input.GetKeyDown(unequipWeaponKey))
        {
            UnequipWeapon();
        }
        
        // 플레이어 체력 회복
        if (Input.GetKeyDown(healPlayerKey))
        {
            HealPlayer();
        }
    }
    
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
        
        // 적 이름 설정
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
        
        // 이미 인벤토리에 무기가 있는지 확인
        if (inventoryManager.HasItem("Bronze Sword", 1))
        {
            Debug.LogWarning("이미 인벤토리에 Bronze Sword가 있습니다!");
            Debug.Log("I키로 인벤토리를 열고 R키로 장착하세요!");
            return;
        }
        
        // 무기 아이템 생성
        Item weaponItem = CreateWeaponItem();
        
        // 인벤토리에 무기 추가
        if (inventoryManager.AddItem(weaponItem, 1))
        {
            Debug.Log($"무기를 인벤토리에 추가했습니다: {weaponItem.itemName}");
            Debug.Log("I키로 인벤토리를 열고 R키로 장착하세요!");
            
            // 아이템 획득 사운드 재생
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
    
    /// <summary>
    /// 무기 아이템 생성
    /// </summary>
    Item CreateWeaponItem()
    {
        Item weaponItem = new Item();
        weaponItem.itemName = "Bronze Sword";
        weaponItem.description = "청동으로 만든 견고한 검입니다. 데미지: 25, 범위: 2";
        weaponItem.itemType = ItemType.Weapon;
        weaponItem.isStackable = false; // 무기는 스택 불가
        weaponItem.maxStackSize = 1;
        weaponItem.isEquippable = true; // 장착 가능
        weaponItem.isConsumable = false; // 소비 불가
        weaponItem.value = 100f; // 가치
        
        // 무기 아이콘 로드
        weaponItem.icon = LoadWeaponIcon();
        
        return weaponItem;
    }
    
    /// <summary>
    /// 무기 아이콘 로드
    /// </summary>
    Sprite LoadWeaponIcon()
    {
        // Resources 폴더에서 로드 시도
        Sprite weaponIcon = Resources.Load<Sprite>("Images/WeaponImages/Bronze_Sword");
        
        if (weaponIcon == null)
        {
            Debug.LogWarning("Resources 폴더에서 무기 아이콘을 찾을 수 없습니다!");
            
            // 대안: 직접 경로로 로드 시도
            weaponIcon = Resources.Load<Sprite>("Bronze_Sword");
            
            if (weaponIcon == null)
            {
                Debug.LogWarning("대안 경로에서도 무기 아이콘을 찾을 수 없습니다!");
                Debug.Log("Bronze_Sword.png를 Assets/Resources/Images/WeaponImages/ 폴더로 이동하세요.");
                Debug.Log("또는 Assets/Resources/ 폴더에 직접 Bronze_Sword.png를 넣으세요.");
            }
            else
            {
                Debug.Log("무기 아이콘을 대안 경로에서 로드했습니다.");
            }
        }
        else
        {
            Debug.Log("무기 아이콘을 성공적으로 로드했습니다: Bronze_Sword");
        }
        
        return weaponIcon;
    }
    
    void EquipWeapon()
    {
        if (playerCombat == null)
        {
            Debug.LogError("PlayerCombatController를 찾을 수 없습니다!");
            return;
        }
        
        if (inventoryManager == null)
        {
            Debug.LogError("InventoryManager를 찾을 수 없습니다!");
            return;
        }
        
        // 이미 무기가 장착되어 있는지 확인
        if (playerCombat.currentWeapon != null)
        {
            Debug.LogWarning("이미 무기가 장착되어 있습니다! T키로 먼저 해제하세요.");
            return;
        }
        
        // 인벤토리에서 무기 아이템 찾기
        Item weaponItem = FindWeaponInInventory();
        if (weaponItem == null)
        {
            Debug.LogWarning("인벤토리에 무기가 없습니다! Q키로 무기를 먼저 획득하세요.");
            return;
        }
        
        // 무기 프리팹으로 실제 무기 생성
        GameObject weaponObj = CreateWeaponFromItem(weaponItem);
        if (weaponObj == null)
        {
            Debug.LogError("무기 생성에 실패했습니다!");
            return;
        }
        
        // weaponHolder 상태 확인
        if (playerCombat.weaponHolder == null)
        {
            Debug.LogError("PlayerCombatController의 weaponHolder가 null입니다!");
            return;
        }
        
        // 무기 장착
        Weapon weaponComponent = weaponObj.GetComponent<Weapon>();
        if (weaponComponent != null)
        {
            // 현재 장착된 무기 오브젝트 저장
            currentEquippedWeapon = weaponObj;
            
            playerCombat.EquipWeapon(weaponComponent);
            Debug.Log($"무기를 장착했습니다: {weaponComponent.weaponName}");
            
            // 무기 장착 사운드 재생
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("weapon_equip");
            }
        }
        else
        {
            Debug.LogError("무기 컴포넌트를 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// 인벤토리에서 무기 아이템 찾기
    /// </summary>
    Item FindWeaponInInventory()
    {
        if (inventoryManager == null) return null;
        
        // InventoryManager의 slots에 접근하기 위해 reflection 사용
        var slotsField = inventoryManager.GetType().GetField("slots", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        if (slotsField == null)
        {
            Debug.LogError("slots 필드를 찾을 수 없습니다!");
            return null;
        }
        
        var slots = slotsField.GetValue(inventoryManager) as System.Collections.Generic.List<InventorySlot>;
        if (slots == null) return null;
        
        // 무기 아이템 찾기
        foreach (var slot in slots)
        {
            if (slot != null && slot.isOccupied && slot.item != null && slot.item.itemType == ItemType.Weapon)
            {
                return slot.item;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// 아이템으로부터 실제 무기 오브젝트 생성
    /// </summary>
    GameObject CreateWeaponFromItem(Item weaponItem)
    {
        if (weaponPrefab == null)
        {
            Debug.LogError("무기 프리팹이 설정되지 않았습니다!");
            return null;
        }
        
        // 무기 오브젝트 생성
        GameObject weaponObj = Instantiate(weaponPrefab, Vector3.zero, Quaternion.identity);
        
        // Weapon 컴포넌트 추가
        Weapon weaponComponent = weaponObj.GetComponent<Weapon>();
        if (weaponComponent == null)
        {
            weaponComponent = weaponObj.AddComponent<Weapon>();
        }
        
        // 아이템 정보로 무기 설정
        weaponComponent.weaponName = weaponItem.itemName;
        weaponComponent.damage = 25f; // 기본 데미지
        weaponComponent.range = 2f; // 기본 범위
        weaponComponent.attackSpeed = 1f;
        weaponComponent.durability = 100f;
        weaponComponent.maxDurability = 100f;
        
        return weaponObj;
    }
    
    void UnequipWeapon()
    {
        if (playerCombat == null)
        {
            Debug.LogError("PlayerCombatController를 찾을 수 없습니다!");
            return;
        }
        
        if (playerCombat.currentWeapon == null)
        {
            Debug.LogWarning("장착된 무기가 없습니다!");
            return;
        }
        
        // 무기 해제
        playerCombat.UnequipWeapon();
        
        // 현재 장착된 무기 오브젝트 제거
        if (currentEquippedWeapon != null)
        {
            Destroy(currentEquippedWeapon);
            currentEquippedWeapon = null;
        }
        
        Debug.Log("무기를 해제했습니다!");
        
        // 무기 해제 사운드 재생
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
    
    void OnGUI()
    {
        // 더 큰 폰트 스타일 설정
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 16;
        labelStyle.normal.textColor = Color.white;
        
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 18;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.yellow;
        
        // 테스트 UI 표시 - 더 큰 영역과 간격
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        
        GUILayout.Label("=== 전투 시스템 테스트 ===", headerStyle);
        GUILayout.Space(5);
        
        GUILayout.Label($"E키: 적 생성 (현재: {enemyCount}마리)", labelStyle);
        GUILayout.Label("Q키: 무기 획득", labelStyle);
        GUILayout.Label("R키: 무기 장착", labelStyle);
        GUILayout.Label("T키: 무기 해제", labelStyle);
        GUILayout.Label("H키: 플레이어 체력 회복", labelStyle);
        
        GUILayout.Space(10);
        GUILayout.Label("=== 조작법 ===", headerStyle);
        GUILayout.Space(5);
        
        GUILayout.Label("마우스 왼쪽: 공격", labelStyle);
        GUILayout.Label("마우스 오른쪽: 방어", labelStyle);
        GUILayout.Label("WASD: 이동", labelStyle);
        GUILayout.Label("Shift: 달리기", labelStyle);
        GUILayout.Label("Space: 점프", labelStyle);
        
        // 플레이어 상태 표시
        if (playerCombat != null)
        {
            GUILayout.Space(10);
            GUILayout.Label("=== 플레이어 상태 ===", headerStyle);
            GUILayout.Space(5);
            
            GUILayout.Label($"체력: {playerCombat.currentHealth:F0}/{playerCombat.maxHealth:F0}", labelStyle);
            GUILayout.Label($"무기: {playerCombat.GetWeaponInfo()}", labelStyle);
        }
        
        // 무기 상태 표시
        GUILayout.Space(10);
        GUILayout.Label("=== 무기 상태 ===", headerStyle);
        GUILayout.Space(5);
        
        if (inventoryManager != null)
        {
            int weaponCount = inventoryManager.GetItemCount("Bronze Sword");
            if (weaponCount > 0)
            {
                GUILayout.Label($"인벤토리 무기: Bronze Sword x{weaponCount}", labelStyle);
            }
            else
            {
                GUILayout.Label("인벤토리 무기: 없음", labelStyle);
            }
        }
        else
        {
            GUILayout.Label("인벤토리: 연결 안됨", labelStyle);
        }
        
        GUILayout.EndArea();
    }
}
