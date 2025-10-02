using UnityEngine;

/// <summary>
/// 수집 가능한 아이템 클래스
/// </summary>
public class CollectibleItem : InteractableObject
{
    [Header("아이템 정보")]
    [SerializeField] private string itemName = "아이템";
    [SerializeField] private string itemDescription = "수집할 수 있는 아이템입니다.";
    [SerializeField] private Sprite itemIcon;
    [SerializeField] private ItemType itemType = ItemType.Misc;
    [SerializeField] public string itemId = ""; // 아이템 ID
    
    [Header("아이템 속성")]
    [SerializeField] private bool isStackable = false;
    [SerializeField] private int maxStackSize = 1;
    [SerializeField] private bool isConsumable = false;
    [SerializeField] private bool isEquippable = false;
    [SerializeField] private int healthRestore = 0;
    [SerializeField] private int staminaRestore = 0;
    [SerializeField] private int value = 0;
    
    [Header("수집 효과")]
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private bool destroyOnCollect = true;
    [SerializeField] private float respawnTime = 0f; // 0이면 재생성 안함
    
    [Header("수집 애니메이션")]
    [SerializeField] private bool useCollectAnimation = true;
    [SerializeField] private float collectAnimationDuration = 1f;
    [SerializeField] private Vector3 collectAnimationOffset = Vector3.up * 2f;
    
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private bool isBeingCollected = false;
    
    public string ItemName => itemName;
    public string ItemDescription => itemDescription;
    public Sprite ItemIcon => itemIcon;
    public ItemType ItemType => itemType;
    
    private void Start()
    {
        // 원본 상태 저장
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        
        // 기본 프롬프트 설정
        if (string.IsNullOrEmpty(interactionPrompt))
        {
            interactionPrompt = $"{itemName} 수집하기";
        }
    }
    
    /// <summary>
    /// 아이템 수집
    /// </summary>
    public override void Interact()
    {
        if (!CanInteract || isBeingCollected) return;
        
        isBeingCollected = true;
        
        // 수집 효과
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, transform.rotation);
        }
        
        // 수집 사운드
        if (audioSource != null && collectSound != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
        // 수집 애니메이션
        if (useCollectAnimation)
        {
            StartCoroutine(CollectAnimation());
        }
        else
        {
            OnCollectComplete();
        }
    }
    
    /// <summary>
    /// 수집 애니메이션 코루틴
    /// </summary>
    private System.Collections.IEnumerator CollectAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + collectAnimationOffset;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero;
        
        float elapsed = 0f;
        
        while (elapsed < collectAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / collectAnimationDuration;
            
            // 부드러운 애니메이션
            transform.position = Vector3.Lerp(startPos, endPos, progress);
            transform.localScale = Vector3.Lerp(startScale, endScale, progress);
            
            yield return null;
        }
        
        OnCollectComplete();
    }
    
               /// <summary>
           /// 수집 완료 처리
           /// </summary>
           private void OnCollectComplete()
           {
               // 인벤토리에 추가
               AddToInventory();
               
               // 퀘스트 리포팅 - 아이템 수집
               if (QuestSystem.Instance != null)
               {
                   // itemId가 있으면 itemId 사용, 없으면 itemName 사용
                   string reportId = !string.IsNullOrEmpty(itemId) ? itemId : itemName;
                   QuestSystem.Instance.ReportItemCollected(reportId, 1);
                   Debug.Log($"[CollectibleItem] 퀘스트 리포팅: Collection, {reportId}, 1");
               }
               
               Debug.Log($"{itemName}을(를) 수집했습니다!");
        
        if (destroyOnCollect)
        {
            gameObject.SetActive(false);
            
            // 재생성 시간이 설정되어 있으면 재생성
            if (respawnTime > 0)
            {
                Invoke(nameof(RespawnItem), respawnTime);
            }
        }
        else
        {
            // 상호작용 비활성화
            canInteract = false;
            
            // 재생성 시간이 설정되어 있으면 재생성
            if (respawnTime > 0)
            {
                Invoke(nameof(RespawnItem), respawnTime);
            }
        }
        
                       isBeingCollected = false;
           }
           
           /// <summary>
           /// 인벤토리에 아이템 추가
           /// </summary>
           private void AddToInventory()
           {
               // 플레이어의 InventoryManager 찾기
               InventoryManager inventoryManager = FindObjectOfType<InventoryManager>();
               if (inventoryManager != null)
               {
                   // Item 객체 생성
                   Item item = new Item(itemName.ToLower().Replace(" ", "_"), itemName, itemDescription, itemType);
                   
                   // 아이콘 설정 - 허브 아이템 특별 처리
                   if (itemIcon != null)
                   {
                       item.icon = itemIcon;
                       Debug.Log($"[CollectibleItem] Inspector 아이콘 사용: {itemName}");
                   }
                   else
                   {
                       // 허브 아이템 특별 처리
                       if (itemName.Contains("허브") || itemName.Contains("herb") || itemName.ToLower().Contains("healing"))
                       {
                           // 여러 경로에서 허브 아이콘 로드 시도
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
                               Debug.Log($"[CollectibleItem] 허브 아이콘 로드 시도: {path}");
                               herbIcon = Resources.Load<Sprite>(path);
                               if (herbIcon != null)
                               {
                                   Debug.Log($"[CollectibleItem] 허브 아이콘 로드 성공: {path}");
                                   break;
                               }
                               else
                               {
                                   Debug.Log($"[CollectibleItem] 허브 아이콘 로드 실패: {path}");
                               }
                           }
                           
                           // 모든 경로 실패 시 Resources 폴더에서 모든 스프라이트 검색
                           if (herbIcon == null)
                           {
                               Debug.Log("[CollectibleItem] Resources 폴더에서 모든 스프라이트 검색 중...");
                               Sprite[] allSprites = Resources.LoadAll<Sprite>("");
                               Debug.Log($"[CollectibleItem] Resources 폴더에서 발견된 스프라이트 수: {allSprites.Length}");
                               
                               // 모든 스프라이트 이름 출력
                               foreach (Sprite sprite in allSprites)
                               {
                                   Debug.Log($"[CollectibleItem] 발견된 스프라이트: {sprite.name}");
                               }
                               
                               foreach (Sprite sprite in allSprites)
                               {
                                   if (sprite.name.ToLower().Contains("herb") || 
                                       sprite.name.ToLower().Contains("healing") ||
                                       sprite.name.Contains("허브"))
                                   {
                                       herbIcon = sprite;
                                       Debug.Log($"[CollectibleItem] 허브 아이콘 발견: {sprite.name}");
                                       break;
                                   }
                               }
                           }
                           
                           if (herbIcon != null)
                           {
                               item.icon = herbIcon;
                               Debug.Log($"[CollectibleItem] 허브 아이콘 설정 완료: {herbIcon.name}");
                           }
                           else
                           {
                               Debug.LogWarning($"[CollectibleItem] 모든 경로에서 허브 아이콘을 찾을 수 없습니다. 동적 생성합니다.");
                               // 동적 아이콘 생성
                               item.icon = CreateDynamicHerbIcon();
                               Debug.Log($"[CollectibleItem] 동적 허브 아이콘 생성 완료: {item.icon.name}");
                           }
                       }
                       else
                       {
                           // 다른 아이템들
                           string iconPath = GetIconPath(itemName, itemType);
                           Sprite loadedIcon = Resources.Load<Sprite>(iconPath);
                           if (loadedIcon != null)
                           {
                               item.icon = loadedIcon;
                               Debug.Log($"[CollectibleItem] 아이콘 로드 성공: {iconPath}");
                           }
                           else
                           {
                               Debug.LogWarning($"[CollectibleItem] 아이콘을 찾을 수 없습니다: {iconPath}");
                           }
                       }
                   }
                   
                   // 아이템 속성 설정
                   item.isStackable = isStackable;
                   item.maxStackSize = maxStackSize;
                   item.isConsumable = isConsumable;
                   item.isEquippable = isEquippable;
                   item.healthRestore = healthRestore;
                   item.staminaRestore = staminaRestore;
                   item.value = value;
                   
                   // 인벤토리에 추가 (자동 저장 포함)
                   if (inventoryManager.AddItemWithAutoSave(item, 1))
                   {
                       Debug.Log($"[CollectibleItem] {itemName}이(가) 인벤토리에 추가되었습니다. (자동 저장 활성화)");
                       
                       // 저장 상태 확인
                       if (inventoryManager.AutoSaveOnChange)
                       {
                           Debug.Log($"[CollectibleItem] 자동 저장이 활성화되어 있습니다. Firebase: {inventoryManager.EnableFirebaseSync}, 로컬: {inventoryManager.EnableLocalSave}");
                       }
                       else
                       {
                           Debug.LogWarning("[CollectibleItem] 자동 저장이 비활성화되어 있습니다!");
                       }
                   }
                   else
                   {
                       Debug.Log("인벤토리 공간이 부족합니다!");
                   }
               }
               else
               {
                   Debug.LogWarning("InventoryManager를 찾을 수 없습니다!");
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
        
        Debug.Log("[CollectibleItem] 동적 허브 아이콘 생성 완료");
        return sprite;
    }
    
    /// <summary>
    /// 아이템 타입과 이름에 따른 아이콘 경로 반환
    /// </summary>
    private string GetIconPath(string itemName, ItemType itemType)
    {
        // 허브 아이템 특별 처리
        if (itemName.Contains("허브") || itemName.Contains("herb") || itemName.ToLower().Contains("healing"))
        {
            return "Healing_Herb";
        }
        
        // 아이템 타입별 기본 경로
        switch (itemType)
        {
            case ItemType.Food:
                return $"Food/{itemName}";
            case ItemType.Material:
                return $"Material/{itemName}";
            case ItemType.Tool:
                return $"Tool/{itemName}";
            case ItemType.Weapon:
                return $"Weapon/{itemName}";
            case ItemType.Armor:
                return $"Armor/{itemName}";
            case ItemType.Treasure:
                return $"Treasure/{itemName}";
            default:
                return itemName;
        }
    }
    
    /// <summary>
    /// 아이템 재생성
    /// </summary>
    private void RespawnItem()
    {
        if (destroyOnCollect)
        {
            gameObject.SetActive(true);
        }
        
        // 원본 상태로 복원
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
        
        // 상호작용 활성화
        canInteract = true;
        isBeingCollected = false;
    }
    
    /// <summary>
    /// 수집 가능한지 확인 (오버라이드)
    /// </summary>
    public override bool CanInteract => base.CanInteract && !isBeingCollected;
}
