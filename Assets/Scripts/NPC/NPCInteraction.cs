using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// PC/모바일 공용 상호작용 시스템
/// PC: E키 + 레이캐스트, 모바일: 터치 + 레이캐스트
/// </summary>
public class NPCInteraction : MonoBehaviour
{
    [Header("Settings")]
    public float interactionRange = 3f;
    public LayerMask interactableLayer = -1;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("UI")]
    public GameObject interactionPrompt; // "E: 대화" 또는 "Tap: 대화" UI
    public TextMeshProUGUI interactionText;
    
    private Camera playerCamera;
    private IInteractable currentInteractable;
    private bool isMobile;
    
    void Start()
    {
        Debug.Log("[NPCInteraction] Start() 호출됨");
        
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
            
        Debug.Log($"[NPCInteraction] 카메라 설정: {playerCamera?.name}");
            
        isMobile = Application.isMobilePlatform;
        
        Debug.Log($"[NPCInteraction] 플랫폼: {(isMobile ? "모바일" : "PC")}");
        
        // UI 초기화
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
            
        Debug.Log($"[NPCInteraction] 초기화 완료 - Range: {interactionRange}, Layer: {interactableLayer.value}");
    }
    
    void Update()
    {
        // 매 프레임마다 디버그 (일시적으로)
        if (Time.frameCount % 60 == 0) // 1초마다
        {
            Debug.Log($"[NPCInteraction] Update 실행 중 - 카메라: {playerCamera?.name}, 현재 상호작용: {currentInteractable?.GetType().Name}");
        }
        
        CheckForInteractables();
        HandleInteractionInput();
    }
    
    void CheckForInteractables()
    {
        Ray ray;
        
        if (isMobile)
        {
            // 모바일: 터치 입력이 있을 때만 레이캐스트
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    ray = playerCamera.ScreenPointToRay(touch.position);
                    PerformRaycast(ray);
                }
            }
        }
        else
        {
            // PC: 마우스 중앙에서 레이캐스트 (매 프레임)
            ray = playerCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
            
            // 레이캐스트 시각화 (디버그용)
            Debug.DrawRay(ray.origin, ray.direction * interactionRange, Color.red, 0.1f);
            
            PerformRaycast(ray);
        }
    }
    
    void PerformRaycast(Ray ray)
    {
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, interactionRange, interactableLayer))
        {
            Debug.Log($"[NPCInteraction] 레이캐스트 히트: {hit.collider.name}, Layer: {hit.collider.gameObject.layer}");
            
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            
            if (interactable != null && interactable.CanInteract)
            {
                Debug.Log($"[NPCInteraction] 상호작용 가능한 오브젝트 발견: {hit.collider.name}");
                
                // 범위 내에 있는지 확인
                if (interactable.IsInRange(transform.position))
                {
                    Debug.Log($"[NPCInteraction] 범위 내 상호작용 오브젝트: {hit.collider.name}");
                    
                    if (currentInteractable != interactable)
                    {
                        // 이전 상호작용 오브젝트 숨김
                        if (currentInteractable != null)
                            HideCurrentPrompt();
                        
                        // 새로운 상호작용 오브젝트 표시
                        currentInteractable = interactable;
                        ShowCurrentPrompt();
                    }
                }
                else
                {
                    Debug.Log($"[NPCInteraction] 범위 밖 상호작용 오브젝트: {hit.collider.name}");
                    ClearCurrentInteractable();
                }
            }
            else
            {
                Debug.Log($"[NPCInteraction] 상호작용 불가능한 오브젝트: {hit.collider.name}");
                ClearCurrentInteractable();
            }
        }
        else
        {
            ClearCurrentInteractable();
        }
    }
    
    void HandleInteractionInput()
    {
        bool inputPressed = false;
        
        if (isMobile)
        {
            // 모바일: 터치 입력
            inputPressed = Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
        }
        else
        {
            // PC: E키 입력
            inputPressed = Input.GetKeyDown(interactionKey);
        }
        
        if (inputPressed && currentInteractable != null && currentInteractable.CanInteract)
        {
            // UI 터치인지 확인 (모바일)
            if (isMobile && EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
                
            currentInteractable.Interact();
        }
    }
    
    void ShowCurrentPrompt()
    {
        if (currentInteractable != null)
        {
            // UI 텍스트 업데이트
            if (interactionText != null)
            {
                string text = currentInteractable.GetInteractionPrompt();
                interactionText.text = isMobile ? $"Tap: {text}" : $"E: {text}";
            }
            
            // UI 표시
            if (interactionPrompt != null)
                interactionPrompt.SetActive(true);
        }
    }
    
    void HideCurrentPrompt()
    {
        if (interactionPrompt != null)
            interactionPrompt.SetActive(false);
    }
    
    void ClearCurrentInteractable()
    {
        if (currentInteractable != null)
        {
            HideCurrentPrompt();
            currentInteractable = null;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        // 상호작용 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
