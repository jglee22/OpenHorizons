using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);
    
    [Header("Initial Position")]
    [SerializeField] private bool startBehindPlayer = true;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 2f;
    [SerializeField] private float maxZoom = 10f;
    
    [Header("Smooth Follow")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSmoothSpeed = 10f;
    
    [Header("Collision Prevention")]
    [SerializeField] private LayerMask collisionMask = 1;
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float minDistanceFromGround = 0.5f;
    [SerializeField] private float minDistanceFromPlayer = 1.5f;
    
    private float currentRotationX;
    private float currentRotationY;
    private float currentZoom;
    private Vector3 currentOffset;
    
    private void Start()
    {
        if (target == null)
        {
            // Find player if target is not assigned
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
        
        currentOffset = offset;
        currentZoom = offset.magnitude;
        
        // Set initial rotation to look at player from behind
        if (startBehindPlayer && target != null)
        {
            // Start behind the player
            currentRotationX = target.eulerAngles.y + 180f; // Behind player
            currentRotationY = 15f; // Slightly above
            
            // Set initial position
            Vector3 behindPosition = target.position + Quaternion.Euler(0, currentRotationX, 0) * Vector3.forward * currentZoom;
            behindPosition.y += offset.y;
            transform.position = behindPosition;
            
            // Set initial rotation
            transform.LookAt(target.position);
        }
        else
        {
            // Use current transform rotation
            Vector3 angles = transform.eulerAngles;
            currentRotationX = angles.y;
            currentRotationY = angles.x;
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraPosition();
    }
    
    private void HandleInput()
    {
        // UI가 열려있으면 카메라 입력 무시
        if (IsUIOpen()) return;
        
        // Mouse rotation
        if (Input.GetMouseButton(1)) // Right mouse button
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
            float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;
            
            currentRotationX += mouseX;
            currentRotationY -= mouseY;
            currentRotationY = Mathf.Clamp(currentRotationY, minVerticalAngle, maxVerticalAngle);
        }
        
        // Zoom with mouse wheel
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentZoom -= scroll * zoomSpeed;
            currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        }
    }
    
    private void UpdateCameraPosition()
    {
        // Calculate target position
        Vector3 targetPosition = target.position;
        
        // Calculate rotation
        Quaternion rotation = Quaternion.Euler(currentRotationY, currentRotationX, 0);
        
        // Calculate offset with zoom
        Vector3 zoomedOffset = rotation * Vector3.forward * currentZoom;
        zoomedOffset.y += offset.y;
        
        // Calculate desired position
        Vector3 desiredPosition = targetPosition + zoomedOffset;
        
        // Prevent camera from going underground or through objects
        desiredPosition = PreventCameraCollision(targetPosition, desiredPosition);
        
        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        
        // Smooth rotation
        Quaternion desiredRotation = Quaternion.LookRotation(targetPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// 카메라가 지형이나 오브젝트를 통과하지 않도록 방지
    /// </summary>
    private Vector3 PreventCameraCollision(Vector3 targetPos, Vector3 desiredPos)
    {
        // Calculate direction from target to desired position
        Vector3 direction = (desiredPos - targetPos).normalized;
        float distance = Vector3.Distance(targetPos, desiredPos);
        
        // Ensure minimum distance from player
        if (distance < minDistanceFromPlayer)
        {
            desiredPos = targetPos + direction * minDistanceFromPlayer;
        }
        
        // Check for collision from target to desired position
        if (Physics.SphereCast(targetPos, collisionRadius, direction, out RaycastHit hit, distance, collisionMask))
        {
            // If collision detected, adjust position
            float safeDistance = hit.distance - collisionRadius - minDistanceFromGround;
            safeDistance = Mathf.Max(safeDistance, minDistanceFromGround);
            
            // Ensure minimum distance from player even after collision adjustment
            safeDistance = Mathf.Max(safeDistance, minDistanceFromPlayer);
            
            // Calculate safe position
            Vector3 safePosition = targetPos + direction * safeDistance;
            
            // Ensure minimum height from ground
            if (Physics.Raycast(safePosition + Vector3.up * 10f, Vector3.down, out RaycastHit groundHit, 20f, collisionMask))
            {
                float groundHeight = groundHit.point.y + minDistanceFromGround;
                safePosition.y = Mathf.Max(safePosition.y, groundHeight);
            }
            
            return safePosition;
        }
        
        // Ensure minimum height from ground even without collision
        if (Physics.Raycast(desiredPos + Vector3.up * 10f, Vector3.down, out RaycastHit groundCheck, 20f, collisionMask))
        {
            float groundHeight = groundCheck.point.y + minDistanceFromGround;
            desiredPos.y = Mathf.Max(desiredPos.y, groundHeight);
        }
        
        return desiredPos;
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void SetOffset(Vector3 newOffset)
    {
        offset = newOffset;
        currentOffset = newOffset;
    }
    
    /// <summary>
    /// UI가 열려있는지 확인
    /// </summary>
    private bool IsUIOpen()
    {
        // 인벤토리 UI가 열려있는지 확인
        InventoryUI inventoryUI = FindObjectOfType<InventoryUI>();
        if (inventoryUI != null && inventoryUI.IsInventoryOpen())
        {
            return true;
        }
        
        // 다른 UI 체크도 추가 가능
        return false;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(target.position, 0.5f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}
