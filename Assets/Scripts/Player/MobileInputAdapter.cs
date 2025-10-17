using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// 조이스틱/버튼 입력을 PlayerController의 외부 입력 필드로 전달하는 어댑터
public class MobileInputAdapter : MonoBehaviour
{
    [Header("Targets")]
    public PlayerController player;

    [Header("Joystick (Joystick Pack)")]
    // Joystick Pack의 공통 Joystick 컴포넌트 (Fixed/Dynamic/Floating/Variable 모두 동일 인터페이스)
    public Joystick joystick;

    [Header("Visibility")]
    public bool showInEditor = false; // false: 에디터에선 숨김, 모바일 기기에서만 표시
    public GameObject uiRoot;         // 비활성화할 루트(미지정 시 현재 오브젝트)

    [Header("Buttons")]
    public Button jumpButton;      // 점프 버튼
    public Button attackButton;   // 공격 버튼
    public Button runButton;       // 달리기 버튼(토글 방식)
    public Button cameraRotateButton; // 카메라 회전 버튼

    [Header("Camera Rotate")]
    public float touchRotateSensitivity = 0.15f; // 터치 드래그 -> 회전 감도
    private bool cameraRotateHeld;
    private int cameraRotateFingerId = -1;
    private ThirdPersonCamera cachedThirdPersonCamera;

    private void Awake()
    {
        ApplyVisibility();
        // 카메라 참조 캐시
        if (Camera.main != null)
        {
            cachedThirdPersonCamera = Camera.main.GetComponent<ThirdPersonCamera>();
        }

        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpClicked);
        }
        
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackClicked);
        }

        if (runButton != null)
        {
            runButton.onClick.AddListener(OnRunButtonClicked);
        }
        
        if (cameraRotateButton != null)
        {
            // 클릭 시 회전이 아니라, 누르고 드래그하는 동안 회전하도록 EventTrigger로 처리
            var trigger = cameraRotateButton.gameObject.GetComponent<EventTrigger>();
            if (trigger == null) trigger = cameraRotateButton.gameObject.AddComponent<EventTrigger>();

            // PointerDown
            var entryDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            entryDown.callback.AddListener(OnCameraRotateDown);
            trigger.triggers.Add(entryDown);

            // PointerUp
            var entryUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            entryUp.callback.AddListener(OnCameraRotateUp);
            trigger.triggers.Add(entryUp);
        }
    }

    private void OnEnable()
    {
        ApplyVisibility();
    }

    private void Update()
    {
        if (player == null || !player.useExternalInput) return;

        // 조이스틱 방향 입력 주입
        Vector2 move = Vector2.zero;
        if (joystick != null)
        {
            move.x = joystick.Horizontal;
            move.y = joystick.Vertical;
        }
        player.externalMoveInput = move;
        // 달리기 입력은 버튼으로 토글하는 방식만 사용

        // 카메라 회전 홀드 중이면 드래그 델타를 카메라에 전달
        if (cameraRotateHeld && cachedThirdPersonCamera != null)
        {
            Vector2 delta = Vector2.zero;
#if UNITY_ANDROID || UNITY_IOS
            // 눌렀던 동일 fingerId의 델타만 사용
            for (int i = 0; i < Input.touchCount; i++)
            {
                var t = Input.GetTouch(i);
                if (t.fingerId == cameraRotateFingerId)
                {
                    delta = t.deltaPosition;
                    break;
                }
            }
#else
            // 에디터/PC: 마우스 이동량
            float dx = Input.GetAxis("Mouse X");
            float dy = Input.GetAxis("Mouse Y");
            delta = new Vector2(dx, dy) * 10f; // 마우스 축은 이미 작으니 스케일 업
#endif
            if (delta.sqrMagnitude > 0.0001f)
            {
                cachedThirdPersonCamera.HandleExternalRotate(delta * touchRotateSensitivity);
            }
        }
    }

    private void ApplyVisibility()
    {
        GameObject root = uiRoot != null ? uiRoot : gameObject;
        bool isMobilePlatform = false;
#if UNITY_ANDROID || UNITY_IOS
        isMobilePlatform = true;
#endif
        bool shouldShow = showInEditor || (isMobilePlatform && !Application.isEditor);
        if (root != null)
        {
            root.SetActive(shouldShow);
        }
    }

    private void OnJumpClicked()
    {
        if (player == null || !player.useExternalInput) return;
        // 한 프레임 소비되도록 PlayerController에서 처리
        player.externalJumpPressed = true;
    }
    
    private void OnAttackClicked()
    {
        // 공격은 PlayerCombatController에서 처리
        // 마우스 클릭을 시뮬레이션
        if (player != null && player.useExternalInput)
        {
            // 공격 입력을 직접 처리하거나 이벤트로 전달
            var combatController = player.GetComponent<PlayerCombatController>();
            if (combatController != null && combatController.CanAttack())
            {
                combatController.PerformComboAttack();
            }
        }
    }

    private void OnRunButtonClicked()
    {
        if (player == null || !player.useExternalInput) return;
        // 버튼을 누를 때마다 토글
        player.externalRunPressed = !player.externalRunPressed;
    }
    
    private void OnCameraRotateDown(BaseEventData data)
    {
        cameraRotateHeld = true;
        cameraRotateFingerId = -1;
        var ped = data as PointerEventData;
        if (ped != null)
        {
            cameraRotateFingerId = ped.pointerId; // 터치면 fingerId, 마우스면 -1/0
        }
    }

    private void OnCameraRotateUp(BaseEventData data)
    {
        cameraRotateHeld = false;
        cameraRotateFingerId = -1;
    }
}


