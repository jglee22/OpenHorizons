using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("Target Settings")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 4, -6); // 더 가깝고, 더 높게
    
    [Header("Initial Position")]
    [SerializeField] private bool startBehindPlayer = true;
    
    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 2f;
    [SerializeField] private float minVerticalAngle = -30f;
    [SerializeField] private float maxVerticalAngle = 60f;
    
    [Header("Zoom Settings")]
    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 4f; // 최소 줌 더 감소
    [SerializeField] private float maxZoom = 12f; // 최대 줌 감소
    
    [Header("Smooth Follow")]
    [SerializeField] private float followSpeed = 5f;
    [SerializeField] private float rotationSmoothSpeed = 10f;
    
    [Header("Collision Prevention")]
    [SerializeField] private LayerMask collisionMask = 0; // 충돌 처리 완전 비활성화
    [SerializeField] private float collisionRadius = 0.2f;
    [SerializeField] private float minDistanceFromGround = 1.0f; // 지면과의 최소 거리 증가
    [SerializeField] private float minDistanceFromPlayer = 3.0f; // 플레이어와의 최소 거리 더 증가
    
    private float currentRotationX;
    private float currentRotationY;
    private float currentZoom;
    private Vector3 currentOffset;
    
    private void Awake()
    {
        // Awake에서 초기화하여 더 일찍 실행
        InitializeCamera();
    }
    
    private void Start()
    {
        // Start에서도 한 번 더 초기화 (안전장치)
        InitializeCamera();
    }
    
    private void InitializeCamera()
    {
        if (target == null)
        {
            // Find player if target is not assigned
            GameObject player = GameObject.FindGameObjectWithTag("Player");
                         if (player != null)
             {
                 if (target == null)
                 {
                     Transform cameraTarget = player.transform.Find("CameraTarget");
                     if (cameraTarget != null)
                     {
                         target = cameraTarget;
                     }
                     else
                     {
                         target = player.transform;
                     }
                 }
             }
            else
            {
                Debug.LogWarning("Player 태그를 가진 오브젝트를 찾을 수 없습니다!");
            }
        }
        
        currentOffset = offset;
        currentZoom = 6f; // 더 가깝게 시작
        
        // Set initial rotation to look at player from behind
        if (startBehindPlayer && target != null)
        {
            // Start behind the player
            currentRotationX = target.eulerAngles.y + 180f; // Behind player
            currentRotationY = 15f; // Slightly above
            
            // 각도 정규화 적용
            currentRotationX = NormalizeAngle(currentRotationX);
            currentRotationY = Mathf.Clamp(currentRotationY, minVerticalAngle, maxVerticalAngle);
            
            // Set initial position - 더 확실한 뒤쪽 위치
            Vector3 behindPosition = target.position + Vector3.back * currentZoom;
            behindPosition.y += offset.y;
            
            // 즉시 위치 설정
            transform.position = behindPosition;
            transform.LookAt(target.position);
            
            // 한 프레임 후 한 번 더 확인
            StartCoroutine(DelayedPositionCheck());
        }
        else
        {
            // Use current transform rotation
            Vector3 angles = transform.eulerAngles;
            currentRotationX = NormalizeAngle(angles.y);
            currentRotationY = Mathf.Clamp(angles.x, minVerticalAngle, maxVerticalAngle);
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraPosition();
        
        // 카메라가 플레이어와 너무 가까이 있으면 강제로 위치 조정
        float distanceToPlayer = Vector3.Distance(transform.position, target.position);
        if (distanceToPlayer < minDistanceFromPlayer)
        {
            Debug.Log($"카메라가 플레이어와 너무 가깝습니다! 거리: {distanceToPlayer}, 최소 거리: {minDistanceFromPlayer}");
            ForceCameraPosition();
        }
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
            
            // 각도 정규화 (0~360도 범위로 제한)
            currentRotationX = NormalizeAngle(currentRotationX);
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
        
        // Prevent camera from going underground or through objects (단순화)
        desiredPosition = PreventCameraCollision(targetPosition, desiredPosition);
        
        // Smooth follow
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        
        // Smooth rotation
        Quaternion desiredRotation = Quaternion.LookRotation(targetPosition - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSmoothSpeed * Time.deltaTime);
    }
    
    // 각도 정규화 메서드 (0~360도 범위로 제한)
    private float NormalizeAngle(float angle)
    {
        while (angle < 0f) angle += 360f;
        while (angle >= 360f) angle -= 360f;
        return angle;
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
    /// 카메라 위치를 강제로 올바른 위치로 설정
    /// </summary>
    private void ForceCameraPosition()
    {
        if (target == null) return;
        
        // 플레이어 뒤쪽으로 강제 이동 (더 가까운 거리 사용)
        Vector3 behindPosition = target.position + Vector3.back * 6f; // 더 가까운 거리
        behindPosition.y += offset.y;
        
        // 즉시 위치 설정 (lerp 없이)
        transform.position = behindPosition;
        transform.LookAt(target.position);
    }
    
    /// <summary>
    /// 지연된 위치 확인
    /// </summary>
    private System.Collections.IEnumerator DelayedPositionCheck()
    {
        yield return new WaitForEndOfFrame();
        
        // 카메라가 여전히 플레이어와 너무 가까이 있으면 강제 조정
        if (target != null)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < minDistanceFromPlayer)
            {
                Debug.Log($"지연 확인: 카메라가 여전히 너무 가깝습니다! 거리: {distance}");
                ForceCameraPosition();
            }
        }
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
