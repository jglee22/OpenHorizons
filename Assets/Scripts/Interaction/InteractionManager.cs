using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// 플레이어의 상호작용을 관리하는 매니저 (콜라이더 기반)
/// </summary>
public class InteractionManager : MonoBehaviour
{
    [Header("상호작용 설정")]
    [SerializeField] private KeyCode interactionKey = KeyCode.E;
    
    [Header("UI 참조")]
    [SerializeField] private GameObject interactionPromptUI;
    [SerializeField] private TMPro.TextMeshProUGUI promptText;
    
    [Header("디버그")]
    [SerializeField] private bool showDebugInfo = true;
    
    private IInteractable currentInteractable;
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();
    
    private void Start()
    {
        Debug.Log("[InteractionManager] Start() 호출됨");
        
        // UI 초기화
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
        else
        {
            // UI가 없으면 자동 생성
            CreateSimplePromptUI();
        }
        
        // Collider 확인
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            Debug.LogError("[InteractionManager] 플레이어에 Collider가 없습니다!");
        }
        else
        {
            Debug.Log($"[InteractionManager] Collider 발견: {col.name}, IsTrigger: {col.isTrigger}");
        }
    }
    
    private void Update()
    {
        HandleInteractionInput();
    }
    
    /// <summary>
    /// 상호작용 입력 처리
    /// </summary>
    private void HandleInteractionInput()
    {
        if (Input.GetKeyDown(interactionKey) && currentInteractable != null)
        {
            // QuestGiver인 경우 다이얼로그 진행 처리
            if (currentInteractable is QuestGiver questGiver)
            {
                if (questGiver.IsShowingDialogue())
                {
                    questGiver.NextDialogue();
                }
                else
                {
                    currentInteractable.Interact();
                }
            }
            else
            {
                currentInteractable.Interact();
            }
        }
    }
    
    /// <summary>
    /// 상호작용 가능한 오브젝트가 범위에 들어옴
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[InteractionManager] OnTriggerEnter: {other.name}, Layer: {other.gameObject.layer}");
        
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            Debug.Log($"[InteractionManager] IInteractable 발견: {other.name}, CanInteract: {interactable.CanInteract}");
            
            if (interactable.CanInteract)
            {
                if (!nearbyInteractables.Contains(interactable))
                {
                    nearbyInteractables.Add(interactable);
                    if (showDebugInfo)
                    {
                        Debug.Log($"상호작용 가능한 오브젝트 범위 진입: {other.name}");
                    }
                }
                
                // 가장 가까운 오브젝트를 현재 상호작용 대상으로 설정
                UpdateCurrentInteractable();
            }
            else
            {
                Debug.Log($"[InteractionManager] 상호작용 불가능: {other.name} - CanInteract = false");
            }
        }
        else
        {
            Debug.Log($"[InteractionManager] IInteractable 없음: {other.name}");
        }
    }
    
    /// <summary>
    /// 상호작용 가능한 오브젝트가 범위에서 나감
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null)
        {
            nearbyInteractables.Remove(interactable);
            if (showDebugInfo)
            {
                Debug.Log($"상호작용 가능한 오브젝트 범위 이탈: {other.name}");
            }
            
            // 현재 상호작용 대상이 나간 경우 UI 숨기기
            if (currentInteractable == interactable)
            {
                currentInteractable = null;
                HideInteractionPrompt();
            }
            
            // 남은 오브젝트 중에서 가장 가까운 것을 선택
            UpdateCurrentInteractable();
        }
    }
    
    /// <summary>
    /// 현재 상호작용 대상 업데이트
    /// </summary>
    private void UpdateCurrentInteractable()
    {
        if (nearbyInteractables.Count == 0)
        {
            currentInteractable = null;
            HideInteractionPrompt();
            return;
        }
        
        // 가장 가까운 오브젝트 찾기
        IInteractable closest = null;
        float closestDistance = float.MaxValue;
        
        foreach (IInteractable interactable in nearbyInteractables)
        {
            if (interactable != null && interactable.CanInteract)
            {
                // IInteractable을 MonoBehaviour로 캐스팅하여 transform 접근
                MonoBehaviour interactableMono = interactable as MonoBehaviour;
                if (interactableMono != null)
                {
                    float distance = Vector3.Distance(transform.position, interactableMono.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = interactable;
                    }
                }
            }
        }
        
        // 새로운 상호작용 대상 설정
        if (closest != currentInteractable)
        {
            currentInteractable = closest;
            if (currentInteractable != null)
            {
                ShowInteractionPrompt(currentInteractable.GetInteractionPrompt());
                if (showDebugInfo)
                {
                    // IInteractable을 MonoBehaviour로 캐스팅하여 name 접근
                    MonoBehaviour interactableMono = currentInteractable as MonoBehaviour;
                    string objectName = interactableMono != null ? interactableMono.name : "Unknown";
                    Debug.Log($"현재 상호작용 대상: {objectName}");
                }
            }
        }
    }
    
    /// <summary>
    /// 상호작용 프롬프트 표시
    /// </summary>
    private void ShowInteractionPrompt(string prompt)
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(true);
            if (promptText != null)
            {
                promptText.text = prompt;
            }
        }
    }
    
    /// <summary>
    /// 상호작용 프롬프트 숨기기
    /// </summary>
    private void HideInteractionPrompt()
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }
    
    /// <summary>
    /// 간단한 프롬프트 UI 자동 생성
    /// </summary>
    private void CreateSimplePromptUI()
    {
        // Canvas 찾기 또는 생성
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // EventSystem 생성
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
        }
        
        // 프롬프트 패널 생성
        GameObject promptPanel = new GameObject("InteractionPrompt");
        promptPanel.transform.SetParent(canvas.transform, false);
        
        UnityEngine.UI.Image panelImage = promptPanel.AddComponent<UnityEngine.UI.Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform panelRect = promptPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = new Vector2(300, 60);
        panelRect.anchoredPosition = new Vector2(0, 100);
        
        // TMP 텍스트 생성
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(promptPanel.transform, false);
        
        TMPro.TextMeshProUGUI promptText = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        promptText.text = "[E] 상호작용";
        promptText.fontSize = 18;
        promptText.color = Color.white;
        promptText.alignment = TMPro.TextAlignmentOptions.Center;
        
        // TMP 폰트 자동 할당
        TMPro.TMP_FontAsset defaultFont = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
        if (defaultFont != null)
        {
            promptText.font = defaultFont;
        }
        else
        {
            // 대안 폰트 찾기
            TMPro.TMP_FontAsset[] allFonts = Resources.FindObjectsOfTypeAll<TMPro.TMP_FontAsset>();
            if (allFonts.Length > 0)
            {
                promptText.font = allFonts[0];
            }
        }
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        // UI 연결
        interactionPromptUI = promptPanel;
        this.promptText = promptText;
        
        // 초기 상태 설정
        promptPanel.SetActive(false);
        
        Debug.Log("간단한 상호작용 프롬프트 UI가 자동 생성되었습니다.");
    }
    
    /// <summary>
    /// 상호작용 범위 시각화 (디버그용)
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 플레이어 콜라이더 범위 표시
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(playerCollider.bounds.center, playerCollider.bounds.size);
        }
    }
}
