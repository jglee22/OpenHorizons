using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SceneSetup : MonoBehaviour
{
    [Header("Setup Options")]
    [SerializeField] private bool setupOnStart = true;
    [SerializeField] private bool createPlayer = true;
    [SerializeField] private bool createCamera = true;
    [SerializeField] private bool createGround = true;
    [SerializeField] private bool createGameManager = true;
    [SerializeField] private bool createUI = true;
    
    [Header("Player Settings")]
    [SerializeField] private Vector3 playerSpawnPosition = new Vector3(0, 1, 0);
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private float playerRadius = 0.5f;
    
    [Header("Camera Settings")]
    [SerializeField] private Vector3 cameraOffset = new Vector3(0, 2, -5);
    
    [Header("Ground Settings")]
    [SerializeField] private Vector3 groundSize = new Vector3(50, 1, 50);
    [SerializeField] private Material groundMaterial;
    
    private void Start()
    {
        if (setupOnStart)
        {
            SetupScene();
        }
    }
    
    [ContextMenu("Setup Scene")]
    public void SetupScene()
    {
        Debug.Log("Starting scene setup...");
        
        if (createPlayer) CreatePlayer();
        if (createCamera) CreateCamera();
        if (createGround) CreateGround();
        if (createGameManager) CreateGameManager();
        if (createUI) CreateUI();
        
        // 테스트 아이템들 생성
        CreateTestItems();
        
        Debug.Log("Scene setup complete!");
    }
    
    private void CreatePlayer()
    {
        // Check if player already exists
        GameObject existingPlayer = GameObject.FindGameObjectWithTag("Player");
        if (existingPlayer != null)
        {
            Debug.Log("Player already exists, skipping creation.");
            return;
        }
        
        // Create player GameObject
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = playerSpawnPosition;
        
        // Remove default collider and add CharacterController
        DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController characterController = player.AddComponent<CharacterController>();
        characterController.height = playerHeight;
        characterController.radius = playerRadius;
        characterController.center = Vector3.zero;
        
        // Add PlayerController script
        player.AddComponent<PlayerController>();
        
        // Add InteractionManager
        player.AddComponent<InteractionManager>();
        
        // Create a simple material for the player using URP shader
        Renderer renderer = player.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material playerMaterial = CreateURPMaterial(Color.blue);
            renderer.material = playerMaterial;
        }
        
        Debug.Log("Player created successfully!");
    }
    
    private void CreateCamera()
    {
        // Check if camera already exists
        Camera existingCamera = Camera.main;
        if (existingCamera != null && existingCamera.GetComponent<ThirdPersonCamera>() != null)
        {
            Debug.Log("Camera already has ThirdPersonCamera script, skipping creation.");
            return;
        }
        
        // Find or create main camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Main Camera");
            cameraObj.tag = "MainCamera";
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.AddComponent<AudioListener>();
        }
        
        // Position camera
        mainCamera.transform.position = playerSpawnPosition + cameraOffset;
        mainCamera.transform.LookAt(playerSpawnPosition);
        
        // Add ThirdPersonCamera script
        ThirdPersonCamera thirdPersonCamera = mainCamera.gameObject.AddComponent<ThirdPersonCamera>();
        
        // Find player and set as target
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            thirdPersonCamera.SetTarget(player.transform);
        }
        
        Debug.Log("Camera setup complete!");
    }
    
    private void CreateGround()
    {
        // Check if ground already exists
        GameObject existingGround = GameObject.Find("Ground");
        if (existingGround != null)
        {
            Debug.Log("Ground already exists, skipping creation.");
            return;
        }
        
        // Create ground
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ground.name = "Ground";
        ground.transform.position = new Vector3(0, -0.5f, 0);
        ground.transform.localScale = groundSize;
        
        // Set ground layer
        ground.layer = LayerMask.NameToLayer("Default");
        
        // Create ground material using URP shader
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        if (groundRenderer != null)
        {
            Material groundMat = CreateURPMaterial(new Color(0.3f, 0.6f, 0.3f)); // Green color
            groundRenderer.material = groundMat;
        }
        
        Debug.Log("Ground created successfully!");
    }
    
    private void CreateGameManager()
    {
        // Check if GameManager already exists
        GameManager existingManager = FindObjectOfType<GameManager>();
        if (existingManager != null)
        {
            Debug.Log("GameManager already exists, skipping creation.");
            return;
        }
        
        // Create GameManager
        GameObject gameManagerObj = new GameObject("GameManager");
        gameManagerObj.AddComponent<GameManager>();
        
        Debug.Log("GameManager created successfully!");
    }
    
    private void CreateUI()
    {
        // Check if Canvas already exists
        Canvas existingCanvas = FindObjectOfType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log("Canvas already exists, skipping creation.");
            return;
        }
        
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create EventSystem if it doesn't exist
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // Create interaction prompt UI
        CreateInteractionPrompt(canvas);
        
        Debug.Log("UI setup complete!");
    }
    
    private void CreateInteractionPrompt(Canvas canvas)
    {
        // Create interaction prompt panel
        GameObject promptPanel = new GameObject("InteractionPrompt");
        promptPanel.transform.SetParent(canvas.transform, false);
        
        Image panelImage = promptPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        
        RectTransform panelRect = promptPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = new Vector2(300, 60);
        panelRect.anchoredPosition = new Vector2(0, 100);
        
        // Create text
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(promptPanel.transform, false);
        
        Text promptText = textObj.AddComponent<Text>();
        promptText.text = "[E] 상호작용";
        promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        promptText.fontSize = 18;
        promptText.color = Color.white;
        promptText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;
        
        // Set as inactive initially
        promptPanel.SetActive(false);
        
        // Find InteractionManager and connect UI
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            InteractionManager interactionManager = player.GetComponent<InteractionManager>();
            if (interactionManager != null)
            {
                // Use reflection to set private fields (not ideal but works for setup)
                var field = typeof(InteractionManager).GetField("interactionPromptUI", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(interactionManager, promptPanel);
                }
                
                var textField = typeof(InteractionManager).GetField("promptText", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (textField != null)
                {
                    field.SetValue(interactionManager, promptText);
                }
            }
        }
    }
    
    /// <summary>
    /// URP 프로젝트에 맞는 머티리얼을 생성합니다.
    /// </summary>
    private Material CreateURPMaterial(Color color)
    {
        Debug.Log("URP 머티리얼 생성을 시작합니다...");
        
        // 사용 가능한 모든 URP 셰이더를 찾아보기
        Shader[] allShaders = Resources.FindObjectsOfTypeAll<Shader>();
        Debug.Log($"총 {allShaders.Length}개의 셰이더를 찾았습니다.");
        
        // URP 셰이더 찾기 (여러 방법 시도)
        Shader urpShader = null;
        
        // 방법 1: 정확한 이름으로 찾기
        string[] urpShaderNames = {
            "Universal Render Pipeline/Lit",
            "Universal Render Pipeline/Simple Lit",
            "Universal Render Pipeline/Unlit",
            "Universal Render Pipeline/Particles/Lit",
            "Universal Render Pipeline/Particles/Simple Lit",
            "Universal Render Pipeline/Particles/Unlit"
        };
        
        foreach (string shaderName in urpShaderNames)
        {
            urpShader = Shader.Find(shaderName);
            if (urpShader != null)
            {
                Debug.Log($"URP 셰이더를 찾았습니다: {shaderName}");
                break;
            }
        }
        
        // 방법 2: 모든 셰이더에서 URP 관련 셰이더 찾기
        if (urpShader == null)
        {
            foreach (Shader shader in allShaders)
            {
                if (shader.name.Contains("Universal Render Pipeline") || 
                    shader.name.Contains("URP") ||
                    shader.name.Contains("Universal"))
                {
                    urpShader = shader;
                    Debug.Log($"URP 관련 셰이더를 찾았습니다: {shader.name}");
                    break;
                }
            }
        }
        
        // 방법 3: Built-in 셰이더 중에서 적절한 것 찾기
        if (urpShader == null)
        {
            string[] fallbackShaders = {
                "Standard",
                "Standard (Specular setup)",
                "Unlit/Color",
                "Unlit/Texture"
            };
            
            foreach (string shaderName in fallbackShaders)
            {
                urpShader = Shader.Find(shaderName);
                if (urpShader != null)
                {
                    Debug.LogWarning($"URP 셰이더를 찾을 수 없어 대안 셰이더를 사용합니다: {shaderName}");
                    break;
                }
            }
        }
        
        if (urpShader == null)
        {
            Debug.LogError("사용 가능한 셰이더를 찾을 수 없습니다!");
            return null;
        }
        
        Debug.Log($"최종 선택된 셰이더: {urpShader.name}");
        
        // 머티리얼 생성
        Material material = new Material(urpShader);
        material.color = color;
        
        // URP Lit 셰이더인 경우 추가 설정
        if (urpShader.name.Contains("Universal Render Pipeline/Lit") || 
            urpShader.name.Contains("Universal Render Pipeline/Simple Lit"))
        {
            try
            {
                material.SetFloat("_Smoothness", 0.5f);
                material.SetFloat("_Metallic", 0.0f);
                Debug.Log("URP 셰이더 설정이 적용되었습니다.");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"URP 셰이더 설정 중 오류 발생: {e.Message}");
            }
        }
        
        Debug.Log($"머티리얼이 성공적으로 생성되었습니다. 셰이더: {material.shader.name}");
        return material;
    }
    
    /// <summary>
    /// 테스트용 수집 가능한 아이템들을 생성합니다.
    /// </summary>
    private void CreateTestItems()
    {
        Debug.Log("테스트 아이템 생성을 시작합니다...");
        
        // 플레이어 주변에 아이템들 배치
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        Vector3 playerPos = player.transform.position;
        
        // 나무 열매 (플레이어 앞쪽)
        CreateCollectibleItem("Apple", "맛있는 나무 열매입니다.", ItemType.Food, 
            playerPos + Vector3.forward * 3f + Vector3.right * 2f);
        
        // 돌 (플레이어 왼쪽)
        CreateCollectibleItem("Stone", "건축에 사용할 수 있는 돌입니다.", ItemType.Material, 
            playerPos + Vector3.left * 4f + Vector3.forward * 1f);
        
        // 나무 막대 (플레이어 오른쪽)
        CreateCollectibleItem("WoodStick", "다양한 용도로 사용할 수 있습니다.", ItemType.Tool, 
            playerPos + Vector3.right * 4f + Vector3.forward * 1f);
        
        // 보물 상자 (플레이어 뒤쪽)
        CreateCollectibleItem("Treasure", "귀중한 보물입니다!", ItemType.Treasure, 
            playerPos + Vector3.back * 3f);
        
        Debug.Log("테스트 아이템 4개가 생성되었습니다!");
    }
    
    /// <summary>
    /// 수집 가능한 아이템을 생성합니다.
    /// </summary>
    private void CreateCollectibleItem(string itemName, string description, ItemType itemType, Vector3 position)
    {
        GameObject itemObj = new GameObject(itemName);
        itemObj.transform.position = position;
        
        // CollectibleItem 컴포넌트 추가
        CollectibleItem collectibleItem = itemObj.AddComponent<CollectibleItem>();
        
        // 아이템 정보 설정 (리플렉션 사용)
        var nameField = typeof(CollectibleItem).GetField("itemName", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (nameField != null) nameField.SetValue(collectibleItem, itemName);
        
        var descField = typeof(CollectibleItem).GetField("itemDescription", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (descField != null) descField.SetValue(collectibleItem, description);
        
        var typeField = typeof(CollectibleItem).GetField("itemType", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (typeField != null) typeField.SetValue(collectibleItem, itemType);
        
        // 추가 아이템 속성 설정 (툴팁 표시용)
        SetItemProperties(collectibleItem, itemType);
        
        // 콜라이더 추가 (트리거용)
        SphereCollider collider = itemObj.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 1f;
        
        // Rigidbody 추가 (트리거 이벤트용)
        Rigidbody rb = itemObj.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
        
        // 시각적 표현 (간단한 구체)
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.transform.SetParent(itemObj.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = Vector3.one * 0.5f;
        
        // 콜라이더 제거 (부모의 트리거 콜라이더만 사용)
        DestroyImmediate(visual.GetComponent<Collider>());
        
        // 아이콘 자동 생성 및 설정
        Sprite itemIcon = CreateItemIcon(itemType, itemName);
        var iconField = typeof(CollectibleItem).GetField("itemIcon", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (iconField != null) iconField.SetValue(collectibleItem, itemIcon);
        
        // 머티리얼 설정
        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = CreateURPMaterial(GetItemColor(itemType));
            renderer.material = material;
        }
        
        Debug.Log($"아이템 '{itemName}'이(가) {position}에 생성되었습니다. (아이콘: {itemIcon != null})");
    }
    
    /// <summary>
    /// 아이템의 추가 속성을 설정합니다 (툴팁 표시용).
    /// </summary>
    private void SetItemProperties(CollectibleItem collectibleItem, ItemType itemType)
    {
        // 아이템 타입에 따른 속성 설정
        switch (itemType)
        {
            case ItemType.Food:
                SetItemProperty(collectibleItem, "isStackable", true);
                SetItemProperty(collectibleItem, "isConsumable", true);
                SetItemProperty(collectibleItem, "maxStackSize", 10);
                SetItemProperty(collectibleItem, "healthRestore", 20);
                SetItemProperty(collectibleItem, "staminaRestore", 10);
                SetItemProperty(collectibleItem, "value", 5);
                break;
                
            case ItemType.Material:
                SetItemProperty(collectibleItem, "isStackable", true);
                SetItemProperty(collectibleItem, "isConsumable", false);
                SetItemProperty(collectibleItem, "maxStackSize", 99);
                SetItemProperty(collectibleItem, "value", 2);
                break;
                
            case ItemType.Tool:
                SetItemProperty(collectibleItem, "isStackable", false);
                SetItemProperty(collectibleItem, "isConsumable", false);
                SetItemProperty(collectibleItem, "isEquippable", true);
                SetItemProperty(collectibleItem, "value", 15);
                break;
                
            case ItemType.Treasure:
                SetItemProperty(collectibleItem, "isStackable", false);
                SetItemProperty(collectibleItem, "isConsumable", false);
                SetItemProperty(collectibleItem, "value", 100);
                break;
        }
    }
    
    /// <summary>
    /// 리플렉션을 사용하여 아이템 속성을 설정합니다.
    /// </summary>
    private void SetItemProperty(CollectibleItem collectibleItem, string propertyName, object value)
    {
        var field = typeof(CollectibleItem).GetField(propertyName, 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(collectibleItem, value);
        }
    }
    
    /// <summary>
    /// 아이템 타입에 따른 아이콘을 생성합니다.
    /// </summary>
    private Sprite CreateItemIcon(ItemType itemType, string itemName)
    {
        // 간단한 텍스처 기반 아이콘 생성
        int iconSize = 64;
        Texture2D texture = new Texture2D(iconSize, iconSize);
        
        // 아이템 타입에 따른 색상 설정
        Color iconColor = GetItemColor(itemType);
        
        // 배경색으로 채우기
        Color[] pixels = new Color[iconSize * iconSize];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = iconColor;
        }
        
        // 간단한 패턴 추가 (중앙에 원형)
        Vector2 center = new Vector2(iconSize / 2f, iconSize / 2f);
        float radius = iconSize / 3f;
        
        for (int x = 0; x < iconSize; x++)
        {
            for (int y = 0; y < iconSize; y++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance < radius)
                {
                    int index = y * iconSize + x;
                    pixels[index] = Color.white;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        // Texture2D를 Sprite로 변환
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, iconSize, iconSize), new Vector2(0.5f, 0.5f));
        sprite.name = $"{itemName}_Icon";
        
        return sprite;
    }
    
    /// <summary>
    /// 아이템 타입에 따른 색상을 반환합니다.
    /// </summary>
    private Color GetItemColor(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.Food: return Color.red;
            case ItemType.Material: return Color.gray;
            case ItemType.Tool: return Color.yellow;
            case ItemType.Treasure: return Color.magenta;
            default: return Color.white;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SceneSetup))]
public class SceneSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        SceneSetup sceneSetup = (SceneSetup)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Setup Scene Now"))
        {
            sceneSetup.SetupScene();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Test URP Shader Detection"))
        {
            TestURPShaderDetection();
        }
        
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox(
            "이 스크립트는 기본 씬 구성을 자동화합니다.\n" +
            "Setup Scene Now 버튼을 클릭하거나 Play 모드에서 자동으로 실행됩니다.\n" +
            "URP 프로젝트에 맞게 URP 셰이더를 자동으로 사용합니다.\n" +
            "Test URP Shader Detection 버튼으로 URP 셰이더 감지 상태를 확인할 수 있습니다.", 
            MessageType.Info);
    }
    
    private void TestURPShaderDetection()
    {
        Debug.Log("=== URP 셰이더 감지 테스트 ===");
        
        // 사용 가능한 모든 셰이더 찾기
        Shader[] allShaders = Resources.FindObjectsOfTypeAll<Shader>();
        Debug.Log($"총 {allShaders.Length}개의 셰이더를 찾았습니다.");
        
        // URP 관련 셰이더 찾기
        int urpShaderCount = 0;
        foreach (Shader shader in allShaders)
        {
            if (shader.name.Contains("Universal Render Pipeline") || 
                shader.name.Contains("URP") ||
                shader.name.Contains("Universal"))
            {
                Debug.Log($"URP 셰이더 발견: {shader.name}");
                urpShaderCount++;
            }
        }
        
        Debug.Log($"총 {urpShaderCount}개의 URP 관련 셰이더를 찾았습니다.");
        
        if (urpShaderCount == 0)
        {
            Debug.LogWarning("URP 셰이더를 찾을 수 없습니다. URP 패키지가 올바르게 설치되었는지 확인하세요.");
        }
    }
}
#endif
