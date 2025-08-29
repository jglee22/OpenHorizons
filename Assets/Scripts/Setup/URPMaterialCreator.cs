using UnityEngine;

/// <summary>
/// URP 프로젝트에서 사용할 수 있는 기본 머티리얼들을 생성하는 유틸리티
/// </summary>
public class URPMaterialCreator : MonoBehaviour
{
    [Header("Material Creation")]
    [SerializeField] private bool createMaterialsOnStart = false;
    [SerializeField] private string materialsFolderPath = "Assets/Materials";
    
    [Header("Default Materials")]
    [SerializeField] private bool createPlayerMaterial = true;
    [SerializeField] private bool createGroundMaterial = true;
    [SerializeField] private bool createTestObjectMaterial = true;
    
    [Header("Material Settings")]
    [SerializeField] private Color playerColor = Color.blue;
    [SerializeField] private Color groundColor = new Color(0.3f, 0.6f, 0.3f);
    [SerializeField] private Color testObjectColor = Color.yellow;
    
    private void Start()
    {
        if (createMaterialsOnStart)
        {
            CreateAllMaterials();
        }
    }
    
    [ContextMenu("Create All Materials")]
    public void CreateAllMaterials()
    {
        Debug.Log("URP 머티리얼 생성을 시작합니다...");
        
        if (createPlayerMaterial)
        {
            CreateMaterial("PlayerMaterial", playerColor, "Universal Render Pipeline/Lit");
        }
        
        if (createGroundMaterial)
        {
            CreateMaterial("GroundMaterial", groundColor, "Universal Render Pipeline/Lit");
        }
        
        if (createTestObjectMaterial)
        {
            CreateMaterial("TestObjectMaterial", testObjectColor, "Universal Render Pipeline/Lit");
        }
        
        Debug.Log("URP 머티리얼 생성이 완료되었습니다!");
    }
    
    [ContextMenu("Create Player Material")]
    public void CreatePlayerMaterial()
    {
        CreateMaterial("PlayerMaterial", playerColor, "Universal Render Pipeline/Lit");
    }
    
    [ContextMenu("Create Ground Material")]
    public void CreateGroundMaterial()
    {
        CreateMaterial("GroundMaterial", groundColor, "Universal Render Pipeline/Lit");
    }
    
    [ContextMenu("Create Test Object Material")]
    public void CreateTestObjectMaterial()
    {
        CreateMaterial("TestObjectMaterial", testObjectColor, "Universal Render Pipeline/Lit");
    }
    
    private void CreateMaterial(string materialName, Color color, string shaderName)
    {
        // URP 셰이더 찾기
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            // 대안 셰이더 시도
            shader = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        
        if (shader == null)
        {
            Debug.LogError($"URP 셰이더를 찾을 수 없습니다: {shaderName}");
            return;
        }
        
        // 머티리얼 생성
        Material material = new Material(shader);
        material.name = materialName;
        material.color = color;
        
        // URP Lit 셰이더인 경우 추가 설정
        if (shader.name.Contains("Universal Render Pipeline/Lit"))
        {
            material.SetFloat("_Smoothness", 0.5f);
            material.SetFloat("_Metallic", 0.0f);
        }
        
        // 머티리얼을 Assets 폴더에 저장 (Editor에서만 작동)
        #if UNITY_EDITOR
        SaveMaterialToAssets(material, materialName);
        #endif
        
        Debug.Log($"머티리얼이 생성되었습니다: {materialName}");
    }
    
    #if UNITY_EDITOR
    private void SaveMaterialToAssets(Material material, string materialName)
    {
        // Materials 폴더가 없으면 생성
        if (!System.IO.Directory.Exists(materialsFolderPath))
        {
            System.IO.Directory.CreateDirectory(materialsFolderPath);
        }
        
        // 머티리얼 파일 경로
        string materialPath = $"{materialsFolderPath}/{materialName}.mat";
        
        // 이미 존재하는 경우 덮어쓰기
        if (System.IO.File.Exists(materialPath))
        {
            Material existingMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (existingMaterial != null)
            {
                existingMaterial.color = material.color;
                existingMaterial.shader = material.shader;
                
                // URP Lit 셰이더인 경우 추가 설정
                if (material.shader.name.Contains("Universal Render Pipeline/Lit"))
                {
                    existingMaterial.SetFloat("_Smoothness", material.GetFloat("_Smoothness"));
                    existingMaterial.SetFloat("_Metallic", material.GetFloat("_Metallic"));
                }
                
                UnityEditor.EditorUtility.SetDirty(existingMaterial);
                UnityEditor.AssetDatabase.SaveAssets();
                Debug.Log($"기존 머티리얼이 업데이트되었습니다: {materialPath}");
                return;
            }
        }
        
        // 새 머티리얼 생성
        UnityEditor.AssetDatabase.CreateAsset(material, materialPath);
        UnityEditor.AssetDatabase.SaveAssets();
        Debug.Log($"새 머티리얼이 생성되었습니다: {materialPath}");
    }
    #endif
    
    /// <summary>
    /// 런타임에서 URP 머티리얼을 생성하는 정적 메서드
    /// </summary>
    public static Material CreateURPMaterial(Color color, string shaderName = "Universal Render Pipeline/Lit")
    {
        // URP 셰이더 찾기
        Shader shader = Shader.Find(shaderName);
        if (shader == null)
        {
            // 대안 셰이더 시도
            shader = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        
        if (shader == null)
        {
            Debug.LogWarning("URP 셰이더를 찾을 수 없습니다. 기본 셰이더를 사용합니다.");
            shader = Shader.Find("Standard");
        }
        
        // 머티리얼 생성
        Material material = new Material(shader);
        material.color = color;
        
        // URP Lit 셰이더인 경우 추가 설정
        if (shader.name.Contains("Universal Render Pipeline/Lit"))
        {
            material.SetFloat("_Smoothness", 0.5f);
            material.SetFloat("_Metallic", 0.0f);
        }
        
        return material;
    }
    
    /// <summary>
    /// 런타임에서 기본 URP 머티리얼들을 생성하는 정적 메서드
    /// </summary>
    public static void CreateDefaultURPMaterials()
    {
        Debug.Log("기본 URP 머티리얼들을 생성합니다...");
        
        // 플레이어 머티리얼
        Material playerMat = CreateURPMaterial(Color.blue);
        playerMat.name = "PlayerMaterial";
        
        // 지면 머티리얼
        Material groundMat = CreateURPMaterial(new Color(0.3f, 0.6f, 0.3f));
        groundMat.name = "GroundMaterial";
        
        // 테스트 오브젝트 머티리얼
        Material testMat = CreateURPMaterial(Color.yellow);
        testMat.name = "TestObjectMaterial";
        
        Debug.Log("기본 URP 머티리얼들이 생성되었습니다!");
    }
}
