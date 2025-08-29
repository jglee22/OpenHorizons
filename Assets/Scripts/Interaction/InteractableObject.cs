using UnityEngine;

/// <summary>
/// 상호작용 가능한 기본 오브젝트 클래스
/// </summary>
public class InteractableObject : MonoBehaviour, IInteractable
{
    [Header("상호작용 설정")]
    [SerializeField] protected string interactionPrompt = "클릭하세요";
    [SerializeField] protected float interactionRange = 3f;
    [SerializeField] protected bool canInteract = true;
    
    [Header("시각적 효과")]
    [SerializeField] private bool highlightOnHover = true;
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private float highlightIntensity = 1.5f;
    
    [Header("상호작용 효과")]
    [SerializeField] private GameObject interactionEffect;
    [SerializeField] private AudioClip interactionSound;
    
    private Renderer objectRenderer;
    private Material originalMaterial;
    private Material highlightMaterial;
    protected AudioSource audioSource;
    
    public virtual bool CanInteract => canInteract;
    
    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null && interactionSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 하이라이트 머티리얼 생성
        if (highlightOnHover && objectRenderer != null)
        {
            CreateHighlightMaterial();
        }
    }
    
    /// <summary>
    /// 하이라이트 머티리얼 생성
    /// </summary>
    private void CreateHighlightMaterial()
    {
        if (objectRenderer.material != null)
        {
            originalMaterial = objectRenderer.material;
            highlightMaterial = new Material(originalMaterial);
            highlightMaterial.color = highlightColor;
            highlightMaterial.SetFloat("_EmissionIntensity", highlightIntensity);
        }
    }
    
    /// <summary>
    /// 상호작용 실행
    /// </summary>
    public virtual void Interact()
    {
        if (!CanInteract) return;
        
        Debug.Log($"상호작용 실행: {gameObject.name}");
        
        // 시각적 효과
        if (interactionEffect != null)
        {
            Instantiate(interactionEffect, transform.position, transform.rotation);
        }
        
        // 사운드 효과
        if (audioSource != null && interactionSound != null)
        {
            audioSource.PlayOneShot(interactionSound);
        }
        
        // 하이라이트 제거
        if (highlightOnHover)
        {
            RemoveHighlight();
        }
        
        // 상호작용 후 비활성화
        canInteract = false;
    }
    
    /// <summary>
    /// 상호작용 프롬프트 반환
    /// </summary>
    public virtual string GetInteractionPrompt()
    {
        return interactionPrompt;
    }
    
    /// <summary>
    /// 상호작용 범위 내에 있는지 확인
    /// </summary>
    public virtual bool IsInRange(Vector3 playerPosition)
    {
        float distance = Vector3.Distance(transform.position, playerPosition);
        return distance <= interactionRange;
    }
    
    /// <summary>
    /// 하이라이트 적용
    /// </summary>
    public virtual void ApplyHighlight()
    {
        if (highlightOnHover && objectRenderer != null && highlightMaterial != null)
        {
            objectRenderer.material = highlightMaterial;
        }
    }
    
    /// <summary>
    /// 하이라이트 제거
    /// </summary>
    public virtual void RemoveHighlight()
    {
        if (highlightOnHover && objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }
    }
    
    /// <summary>
    /// 상호작용 가능 상태로 재설정
    /// </summary>
    public virtual void ResetInteraction()
    {
        canInteract = true;
        if (highlightOnHover)
        {
            RemoveHighlight();
        }
    }
    
    private void OnDestroy()
    {
        // 생성된 머티리얼 정리
        if (highlightMaterial != null)
        {
            DestroyImmediate(highlightMaterial);
        }
    }
    
    /// <summary>
    /// 상호작용 범위 시각화 (디버그용)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
