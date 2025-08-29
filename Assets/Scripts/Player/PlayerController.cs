using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float gravity = -9.81f;
    
    [Header("Camera Reference")]
    [SerializeField] private Transform cameraTransform;
    
    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    
    [Header("Animation")]
    [SerializeField] private Animator animator;
    
    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private float currentSpeed;
    
    // Animation parameter names
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int isRunningHash = Animator.StringToHash("IsRunning");
    private readonly int isJumpingHash = Animator.StringToHash("IsJumping");
    
    private void Start()
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        
        // Create ground check if not assigned
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -0.9f, 0);
            groundCheck = groundCheckObj.transform;
        }
        
        // 자동으로 카메라 찾기
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                cameraTransform = mainCamera.transform;
                Debug.Log("카메라가 자동으로 연결되었습니다: " + mainCamera.name);
            }
            else
            {
                Camera[] cameras = FindObjectsOfType<Camera>();
                if (cameras.Length > 0)
                {
                    cameraTransform = cameras[0].transform;
                    Debug.Log("카메라가 자동으로 연결되었습니다: " + cameras[0].name);
                }
            }
        }
    }
    
    private void Update()
    {
        HandleMovement();
        HandleJump();
        HandleAnimation();
    }
    
    private void HandleMovement()
    {
        // Get input
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        // Calculate movement direction based on camera orientation
        Vector3 move = Vector3.zero;
        
        if (cameraTransform != null)
        {
            // Get camera's forward and right vectors (ignore Y component)
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            
            // Flatten to ground plane
            cameraForward.y = 0;
            cameraRight.y = 0;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // Calculate movement direction
            move = cameraForward * vertical + cameraRight * horizontal;
        }
        else
        {
            // Fallback to local transform if no camera
            move = transform.right * horizontal + transform.forward * vertical;
        }
        
        // Apply movement
        controller.Move(move * currentSpeed * Time.deltaTime);
        
        // Handle running
        if (Input.GetKey(KeyCode.LeftShift) && move.magnitude > 0.1f)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }
    }
    
    private void HandleJump()
    {
        // Ground check
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        
        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
        
        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    
    private void HandleAnimation()
    {
        if (animator == null) return;
        
        // Get movement magnitude
        float moveMagnitude = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).magnitude;
        
        // Update animation parameters
        animator.SetBool(isWalkingHash, moveMagnitude > 0.1f && currentSpeed == walkSpeed);
        animator.SetBool(isRunningHash, moveMagnitude > 0.1f && currentSpeed == runSpeed);
        animator.SetBool(isJumpingHash, !isGrounded);
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
