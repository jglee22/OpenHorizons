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
                   QuestSystem.Instance.ReceiveReport("Item", itemName, 1);
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
                   Item item = new Item(itemName, itemDescription, itemType);
                   item.icon = itemIcon;
                   
                   // 아이템 속성 설정
                   item.isStackable = isStackable;
                   item.maxStackSize = maxStackSize;
                   item.isConsumable = isConsumable;
                   item.isEquippable = isEquippable;
                   item.healthRestore = healthRestore;
                   item.staminaRestore = staminaRestore;
                   item.value = value;
                   
                   // 인벤토리에 추가
                   if (inventoryManager.AddItem(item, 1))
                   {
                       Debug.Log($"{itemName}이(가) 인벤토리에 추가되었습니다.");
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
