using UnityEngine;
using AudioSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    
    [Header("Jump Settings")]
    public float jumpForce = 5f; // 점프 힘
    public float gravity = -9.81f; // 중력
    
    [Header("CharacterController Settings")]
    public float slopeLimit = 45f; // 경사 제한
    public float stepOffset = 0.3f; // 계단 높이
    public float skinWidth = 0.08f; // 피부 두께
    
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    
    
    [Header("Animation")]
    public Animator animator;
    
    [Header("Audio")]
    public bool enableFootstepSounds = true;
    public float footstepInterval = 0.5f;
    
    [Header("Mobile/Input Bridge")]
    [Tooltip("외부 입력(가상 조이스틱/버튼)을 사용할지 여부")] public bool useExternalInput = false;
    [Tooltip("외부에서 주입되는 이동 입력 (x: 좌우, y: 전후)")] public Vector2 externalMoveInput;
    [Tooltip("외부에서 주입되는 달리기 버튼 상태")] public bool externalRunPressed;
    [Tooltip("외부에서 주입되는 점프 트리거(한 프레임 소비)")] public bool externalJumpPressed;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private Transform cameraTransform;
    
    // Audio variables
    private float lastFootstepTime;
    private bool wasMoving;
    private bool wasRunning;
    private FootstepSoundManager footstepManager;
    private AddressableFootstepManager addressableFootstepManager;
    
    
    // Animation parameter names
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int isRunningHash = Animator.StringToHash("IsRunning");
    private readonly int isJumpingHash = Animator.StringToHash("IsJumping");
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int jumpTriggerHash = Animator.StringToHash("Jump");
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded");
    
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // CharacterController 설정 최적화
        controller.slopeLimit = slopeLimit;
        controller.stepOffset = stepOffset;
        controller.skinWidth = skinWidth;
        
        // 자동으로 카메라 찾기
        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        // 자동으로 Animator 찾기 (자식 오브젝트에서)
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogError("자식 오브젝트에서 Animator를 찾을 수 없습니다! Rio 오브젝트에 Animator가 있는지 확인해주세요.");
        }
        else
        {
            Debug.Log($"PlayerController에서 Animator를 찾았습니다: {animator.name}");
        }
        
        // Ground check 오브젝트 생성
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheck = groundCheckObj.transform;
            
            // CharacterController의 높이와 중심을 고려해서 위치 설정
            float controllerHeight = controller.height;
            Vector3 controllerCenter = controller.center;
            groundCheck.localPosition = new Vector3(controllerCenter.x, -controllerHeight * 0.5f + 0.1f, controllerCenter.z);
        }
        
        // Ground mask 설정
        if (groundMask == 0)
        {
            groundMask = LayerMask.GetMask("Ground");
        }
        
        // FootstepSoundManager 찾기
        footstepManager = FindObjectOfType<FootstepSoundManager>();
        if (footstepManager == null)
        {
            Debug.LogWarning("FootstepSoundManager를 찾을 수 없습니다!");
        }
        
        // AddressableFootstepManager 찾기
        addressableFootstepManager = FindObjectOfType<AddressableFootstepManager>();
        if (addressableFootstepManager == null)
        {
            Debug.LogWarning("AddressableFootstepManager를 찾을 수 없습니다!");
        }
    }
    
    void Update()
    {
        HandleGroundCheck();
        HandleJump();
        HandleMovement();
        UpdateAnimations();
    }
    
    void HandleGroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

    // ★ 공중에서는 계단 보정 끄기
    controller.stepOffset = isGrounded ? stepOffset : 0f;

         if (isGrounded && velocity.y < 0f)
         {
             velocity.y = -2f;
         }
    }
    
    void HandleMovement()
    {
        float horizontal;
        float vertical;
        
        if (useExternalInput)
        {
            horizontal = externalMoveInput.x;
            vertical = externalMoveInput.y;
        }
        else
        {
            horizontal = Input.GetAxis("Horizontal");
            vertical = Input.GetAxis("Vertical");
        }
        
        Vector3 dir;
    if (cameraTransform != null)
        dir = cameraTransform.forward * vertical + cameraTransform.right * horizontal;
    else
        dir = transform.forward * vertical + transform.right * horizontal;
    dir.y = 0f;

    // --- ② 회전 및 속도 결정(기존 로직 유지)
    Vector3 moveXZ = Vector3.zero;
    if (dir.sqrMagnitude >= 0.01f)
    {
        float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);

        bool running = useExternalInput ? externalRunPressed : Input.GetKey(KeyCode.LeftShift);
        float curSpeed = running ? runSpeed : moveSpeed;
        moveXZ = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward * curSpeed;
    }

         // --- ③ 중력 적용
         velocity.y += gravity * Time.deltaTime;

    // --- ④ 수평 + 수직을 한 번에 Move
    Vector3 move = new Vector3(moveXZ.x, velocity.y, moveXZ.z);
    controller.Move(move * Time.deltaTime);
    }
    
    void HandleJump()
    {
        bool jumpPressed = useExternalInput ? ConsumeExternalJumpPressed() : Input.GetButtonDown("Jump");
        if (jumpPressed && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
            
            // 점프 애니메이션 트리거
            if (animator != null)
            {
                animator.SetTrigger(jumpTriggerHash);
            }
            
            // 점프 사운드 재생 (우선순위: Addressable > Footstep > AudioManager)
            if (addressableFootstepManager != null)
            {
                addressableFootstepManager.PlayJumpSound();
            }
            else if (footstepManager != null)
            {
                footstepManager.PlayJumpSound();
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("jump");
            }
        }
    }

    // 외부 점프 트리거는 한 프레임만 소비되도록 처리
    private bool ConsumeExternalJumpPressed()
    {
        if (!externalJumpPressed) return false;
        externalJumpPressed = false;
        return true;
    }
    
    
    void UpdateAnimations()
    {
        if (animator == null) 
        {
            Debug.LogWarning("Animator가 null입니다!");
            return;
        }
        
        // 입력 기반으로 이동 상태 확인 (더 엄격한 임계값)
        float horizontal = useExternalInput ? externalMoveInput.x : Input.GetAxis("Horizontal");
        float vertical = useExternalInput ? externalMoveInput.y : Input.GetAxis("Vertical");
        bool hasInput = Mathf.Abs(horizontal) > 0.2f || Mathf.Abs(vertical) > 0.2f;
        
        // 이동 속도 계산 (입력 기반으로 추정)
        bool runHeld = useExternalInput ? externalRunPressed : Input.GetKey(KeyCode.LeftShift);
        float speed = hasInput ? (runHeld ? runSpeed : moveSpeed) : 0f;
        
        // 이동 상태 확인 (입력만으로 판단, 더 즉각적인 반응)
        bool isMoving = hasInput;
        bool isRunning = runHeld && isMoving;
        bool isWalking = isMoving && !isRunning;
        
        // 입력이 없으면 즉시 정지 상태로 설정
        if (!hasInput)
        {
            isMoving = false;
            isRunning = false;
            isWalking = false;
        }
        
        // 점프 상태 확인 (점프 중에는 다른 애니메이션 무시)
        bool isJumping = !isGrounded && velocity.y > 0;
        
        // 점프 후 착지 대기 상태 확인 (점프 애니메이션이 끝나고 착지할 때까지)
        bool isWaitingForLanding = !isGrounded && velocity.y <= 0 && animator.GetCurrentAnimatorStateInfo(0).IsName("HumanF@Jump01");
        
        // 애니메이션 파라미터 업데이트
        animator.SetFloat(speedHash, speed);
        animator.SetBool(isWalkingHash, isWalking && !isJumping && !isWaitingForLanding);
        animator.SetBool(isRunningHash, isRunning && !isJumping && !isWaitingForLanding);
        animator.SetBool(isJumpingHash, isJumping);
        animator.SetBool(isGroundedHash, isGrounded);
        
        // 발걸음 소리 처리
        HandleFootstepSounds(isMoving, isRunning);
        
        // 디버그 로그 (1초마다 출력)
        if (Time.frameCount % 60 == 0)
        {
            Debug.Log($"애니메이션 파라미터 - Speed: {speed:F2}, HasInput: {hasInput}, IsMoving: {isMoving}, IsRunning: {isRunning}, IsWalking: {isWalking}, IsJumping: {isJumping}, IsGrounded: {isGrounded}, RunHeld: {runHeld}");
            
            // 현재 애니메이션 상태 확인
            if (animator != null)
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            }
        }
    }
    
    void HandleFootstepSounds(bool isMoving, bool isRunning)
    {
        if (!enableFootstepSounds) return;
        
        // 땅에 닿아있고 이동 중일 때만 발걸음 소리 재생
        if (isGrounded && isMoving)
        {
            float currentTime = Time.time;
            float interval = isRunning ? footstepInterval * 0.7f : footstepInterval; // 달리기일 때 더 빠르게
            
            if (currentTime - lastFootstepTime >= interval)
            {
                // 우선순위: Addressable > Footstep > AudioManager
                if (addressableFootstepManager != null)
                {
                    addressableFootstepManager.PlayFootstepSound(isRunning);
                }
                else if (footstepManager != null)
                {
                    footstepManager.PlayFootstepSound(isRunning);
                }
                else if (AudioManager.Instance != null)
                {
                    // 백업: 기본 AudioManager 사용
                    if (isRunning)
                    {
                        AudioManager.Instance.PlaySFX("run");
                    }
                    else
                    {
                        AudioManager.Instance.PlaySFX("footstep");
                    }
                }
                
                lastFootstepTime = currentTime;
            }
        }
        
        // 착지 소리 (점프 후 착지할 때)
        if (isGrounded && !wasMoving && isMoving)
        {
            if (addressableFootstepManager != null)
            {
                addressableFootstepManager.PlayLandSound();
            }
            else if (footstepManager != null)
            {
                footstepManager.PlayLandSound();
            }
            else if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("land");
            }
        }
        
        // 상태 업데이트
        wasMoving = isMoving;
        wasRunning = isRunning;
    }
    
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
