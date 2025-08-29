using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Unity에서 Open Horizons 프로젝트를 빠르게 설정하는 가이드
/// 이 스크립트는 설정 과정을 안내하는 용도입니다.
/// </summary>
public class QuickSetupGuide : MonoBehaviour
{
    [Header("Setup Status")]
    [SerializeField] private bool playerExists = false;
    [SerializeField] private bool cameraExists = false;
    [SerializeField] private bool groundExists = false;
    [SerializeField] private bool gameManagerExists = false;
    [SerializeField] private bool uiExists = false;
    
    [Header("Auto Check")]
    [SerializeField] private bool autoCheckOnStart = true;
    
    private void Start()
    {
        if (autoCheckOnStart)
        {
            CheckSetupStatus();
        }
    }
    
    [ContextMenu("Check Setup Status")]
    public void CheckSetupStatus()
    {
        playerExists = GameObject.FindGameObjectWithTag("Player") != null;
        cameraExists = Camera.main != null && Camera.main.GetComponent<ThirdPersonCamera>() != null;
        groundExists = GameObject.Find("Ground") != null;
        gameManagerExists = FindObjectOfType<GameManager>() != null;
        uiExists = FindObjectOfType<Canvas>() != null;
        
        LogSetupStatus();
    }
    
    private void LogSetupStatus()
    {
        Debug.Log("=== Open Horizons Setup Status ===");
        Debug.Log($"Player: {(playerExists ? "✅" : "❌")}");
        Debug.Log($"Camera: {(cameraExists ? "✅" : "❌")}");
        Debug.Log($"Ground: {(groundExists ? "✅" : "❌")}");
        Debug.Log($"GameManager: {(gameManagerExists ? "✅" : "❌")}");
        Debug.Log($"UI: {(uiExists ? "✅" : "❌")}");
        
        if (playerExists && cameraExists && groundExists && gameManagerExists && uiExists)
        {
            Debug.Log("🎉 모든 설정이 완료되었습니다! 게임을 실행할 수 있습니다.");
        }
        else
        {
            Debug.Log("⚠️ 일부 설정이 누락되었습니다. SceneSetup 스크립트를 사용하여 자동 설정을 진행하세요.");
        }
    }
    
    [ContextMenu("Quick Fix - Add Missing Components")]
    public void QuickFix()
    {
        Debug.Log("Quick Fix 시작...");
        
        // Player가 없으면 생성
        if (!playerExists)
        {
            CreateBasicPlayer();
        }
        
        // Camera가 없으면 설정
        if (!cameraExists)
        {
            SetupBasicCamera();
        }
        
        // Ground가 없으면 생성
        if (!groundExists)
        {
            CreateBasicGround();
        }
        
        // GameManager가 없으면 생성
        if (!gameManagerExists)
        {
            CreateGameManager();
        }
        
        // UI가 없으면 생성
        if (!uiExists)
        {
            CreateBasicUI();
        }
        
        CheckSetupStatus();
    }
    
    private void CreateBasicPlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0, 1, 0);
        
        // Remove default collider and add CharacterController
        DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.height = 2f;
        characterController.radius = 0.5f;
        
        // Add scripts
        // Add scripts
        player.AddComponent<PlayerController>();
        player.AddComponent<InteractionManager>();
        player.AddComponent<InventoryManager>();
        player.AddComponent<InventoryUI>();
        
        // Add Rigidbody for trigger events (Is Kinematic)
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        // Set material using URP shader
        Renderer renderer = player.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material playerMaterial = CreateURPMaterial(Color.blue);
            renderer.material = playerMaterial;
        }
        
        Debug.Log("기본 플레이어가 생성되었습니다.");
    }
    
    private void SetupBasicCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
        }
        
        // Position camera
        mainCamera.transform.position = new Vector3(0, 3, -5);
        mainCamera.transform.LookAt(Vector3.zero);
        
        // Add ThirdPersonCamera script
        ThirdPersonCamera thirdPersonCamera = mainCamera.gameObject.AddComponent<ThirdPersonCamera>();
        
        // Find player and set as target
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            thirdPersonCamera.SetTarget(player.transform);
        }
        
        Debug.Log("기본 카메라가 설정되었습니다.");
    }
    
    private void CreateBasicGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, -0.5f, 0);
        ground.transform.localScale = new Vector3(50, 1, 50);
        
        // Set material using URP shader
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            Material groundMat = CreateURPMaterial(new Color(0.3f, 0.6f, 0.3f));
            groundRenderer.material = groundMat;
        }
        
        Debug.Log("기본 지면이 생성되었습니다.");
    }
    
    private void CreateGameManager()
    {
        GameObject gameManagerObj = new GameObject("GameManager");
        gameManagerObj.AddComponent<GameManager>();
        Debug.Log("GameManager가 생성되었습니다.");
    }
    
    private void CreateBasicUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        Debug.Log("기본 UI가 생성되었습니다.");
    }
    
    /// <summary>
    /// URP 프로젝트에 맞는 머티리얼을 생성합니다.
    /// </summary>
    private Material CreateURPMaterial(Color color)
    {
        // URP 셰이더를 찾아서 머티리얼 생성
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader == null)
        {
            // URP 셰이더를 찾을 수 없는 경우 대안 사용
            urpShader = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        
        if (urpShader == null)
        {
            // URP 셰이더를 전혀 찾을 수 없는 경우 기본 셰이더 사용
            urpShader = Shader.Find("Standard");
            Debug.LogWarning("URP 셰이더를 찾을 수 없습니다. 기본 셰이더를 사용합니다.");
        }
        
        Material material = new Material(urpShader);
        material.color = color;
        
        // URP Lit 셰이더인 경우 추가 설정
        if (urpShader.name.Contains("Universal Render Pipeline/Lit"))
        {
            // URP Lit 셰이더의 기본 설정
            material.SetFloat("_Smoothness", 0.5f);
            material.SetFloat("_Metallic", 0.0f);
        }
        
        return material;
    }
    
    private void OnDrawGizmos()
    {
        // Draw setup status indicators
        if (!playerExists)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(Vector3.zero, 1f);
        }
        
        if (!cameraExists)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(new Vector3(0, 3, -5), 1f);
        }
        
        if (!groundExists)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(new Vector3(0, -0.5f, 0), 50f);
        }
    }
}
