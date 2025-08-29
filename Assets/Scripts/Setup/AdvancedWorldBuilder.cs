using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 머티리얼 우선순위 설정과 고급 옵션을 제공하는 월드 빌더
/// </summary>
public class AdvancedWorldBuilder : MonoBehaviour
{
    [Header("Material Priority Settings")]
    [SerializeField] private MaterialPriority materialPriority = MaterialPriority.PreserveOriginal;
    [SerializeField] private bool forceURPConversion = false;
    [SerializeField] private bool preserveOriginalColors = true;
    [SerializeField] private bool preserveOriginalTextures = true;
    
    [Header("World Generation")]
    [SerializeField] private bool generateOnStart = false;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;
    
    [Header("World Size")]
    [SerializeField] private Vector2 worldSize = new Vector2(100, 100);
    [SerializeField] private float worldHeight = 10f;
    
    [Header("Object Generation")]
    [SerializeField] private GameObject[] prefabs;
    [SerializeField] private int objectCount = 20;
    [SerializeField] private float minDistance = 5f;
    [SerializeField] private bool alignToGround = true;
    [SerializeField] private float groundOffset = 0.1f;
    
    [Header("Material Override")]
    [SerializeField] private Material[] customMaterials;
    [SerializeField] private bool useCustomMaterials = false;
    [SerializeField] private int customMaterialIndex = 0;
    
    private List<GameObject> generatedObjects = new List<GameObject>();
    private System.Random random;
    
    public enum MaterialPriority
    {
        PreserveOriginal,    // 원본 머티리얼 우선 (URP 호환성만 체크)
        ForceURP,           // 강제로 URP 머티리얼 사용
        UseCustom,          // 커스텀 머티리얼 사용
        SmartConversion     // 스마트 변환 (원본 + URP 최적화)
    }
    
    private void Start()
    {
        if (generateOnStart)
        {
            GenerateWorld();
        }
    }
    
    [ContextMenu("Generate World")]
    public void GenerateWorld()
    {
        Debug.Log("고급 월드 생성을 시작합니다...");
        
        // 기존 생성된 오브젝트들 정리
        ClearWorld();
        
        // 랜덤 시드 설정
        if (useRandomSeed)
        {
            seed = Random.Range(0, 99999);
        }
        random = new System.Random(seed);
        
        Debug.Log($"월드 생성 시드: {seed}");
        Debug.Log($"머티리얼 우선순위: {materialPriority}");
        
        // 월드 생성
        GenerateObjects();
        
        Debug.Log("고급 월드 생성이 완료되었습니다!");
    }
    
    [ContextMenu("Clear World")]
    public void ClearWorld()
    {
        Debug.Log("생성된 월드를 정리합니다...");
        
        foreach (GameObject obj in generatedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        
        generatedObjects.Clear();
        Debug.Log("월드 정리가 완료되었습니다.");
    }
    
    private void GenerateObjects()
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("프리팹이 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("오브젝트를 생성합니다...");
        
        List<Vector3> objectPositions = new List<Vector3>();
        
        for (int i = 0; i < objectCount; i++)
        {
            Vector3 position = GetRandomWorldPosition();
            
            // 다른 오브젝트와의 거리 확인
            bool tooClose = false;
            foreach (Vector3 existingPos in objectPositions)
            {
                if (Vector3.Distance(position, existingPos) < minDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (tooClose) continue;
            
            // 오브젝트 생성
            GameObject prefab = prefabs[random.Next(prefabs.Length)];
            GameObject obj = Instantiate(prefab, position, Quaternion.Euler(0, random.Next(0, 360), 0));
            obj.transform.SetParent(transform);
            obj.name = $"GeneratedObject_{i + 1}";
            
            // 지형에 맞춰 위치 조정
            if (alignToGround)
            {
                AlignToGround(obj);
            }
            
            // 머티리얼 처리 (우선순위에 따라)
            ProcessMaterials(obj);
            
            objectPositions.Add(position);
            generatedObjects.Add(obj);
        }
        
        Debug.Log($"{objectCount}개의 오브젝트 생성 완료");
    }
    
    private void ProcessMaterials(GameObject obj)
    {
        switch (materialPriority)
        {
            case MaterialPriority.PreserveOriginal:
                // 원본 머티리얼 유지, URP 호환성만 체크
                CheckAndFixMaterials(obj, false);
                break;
                
            case MaterialPriority.ForceURP:
                // 강제로 URP 머티리얼 사용
                ForceURPMaterials(obj);
                break;
                
            case MaterialPriority.UseCustom:
                // 커스텀 머티리얼 사용
                ApplyCustomMaterials(obj);
                break;
                
            case MaterialPriority.SmartConversion:
                // 스마트 변환
                SmartMaterialConversion(obj);
                break;
        }
    }
    
    private void CheckAndFixMaterials(GameObject obj, bool forceConversion = false)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer.materials != null)
            {
                Material[] materials = renderer.materials;
                bool materialsChanged = false;
                
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        // 기존 머티리얼이 URP 셰이더를 사용하는지 확인
                        if (IsURPShader(materials[i].shader))
                        {
                            // 이미 URP 셰이더 사용 중 - 그대로 유지
                            Debug.Log($"오브젝트 {obj.name}의 머티리얼 {materials[i].name}은 이미 URP 셰이더를 사용합니다.");
                        }
                        else if (forceConversion)
                        {
                            // Built-in 셰이더 사용 중 - URP 머티리얼로 변환
                            Debug.Log($"오브젝트 {obj.name}의 머티리얼 {materials[i].name}을 URP 머티리얼로 변환합니다.");
                            materials[i] = ConvertToURPMaterial(materials[i]);
                            materialsChanged = true;
                        }
                        else
                        {
                            // Built-in 셰이더 사용 중이지만 변환하지 않음
                            Debug.Log($"오브젝트 {obj.name}의 머티리얼 {materials[i].name}은 Built-in 셰이더를 사용합니다. (변환하지 않음)");
                        }
                    }
                }
                
                // 머티리얼이 변경되었다면 적용
                if (materialsChanged)
                {
                    renderer.materials = materials;
                }
            }
        }
    }
    
    private void ForceURPMaterials(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer.materials != null)
            {
                Material[] materials = renderer.materials;
                bool materialsChanged = false;
                
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        // 모든 머티리얼을 URP로 변환
                        Debug.Log($"오브젝트 {obj.name}의 머티리얼 {materials[i].name}을 강제로 URP 머티리얼로 변환합니다.");
                        materials[i] = ConvertToURPMaterial(materials[i]);
                        materialsChanged = true;
                    }
                }
                
                if (materialsChanged)
                {
                    renderer.materials = materials;
                }
            }
        }
    }
    
    private void ApplyCustomMaterials(GameObject obj)
    {
        if (customMaterials == null || customMaterials.Length == 0)
        {
            Debug.LogWarning("커스텀 머티리얼이 설정되지 않았습니다.");
            return;
        }
        
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer.materials != null)
            {
                Material[] materials = renderer.materials;
                
                for (int i = 0; i < materials.Length; i++)
                {
                    // 커스텀 머티리얼 적용
                    int materialIndex = (customMaterialIndex + i) % customMaterials.Length;
                    materials[i] = customMaterials[materialIndex];
                }
                
                renderer.materials = materials;
                Debug.Log($"오브젝트 {obj.name}에 커스텀 머티리얼을 적용했습니다.");
            }
        }
    }
    
    private void SmartMaterialConversion(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
        
        foreach (Renderer renderer in renderers)
        {
            if (renderer.materials != null)
            {
                Material[] materials = renderer.materials;
                bool materialsChanged = false;
                
                for (int i = 0; i < materials.Length; i++)
                {
                    if (materials[i] != null)
                    {
                        if (IsURPShader(materials[i].shader))
                        {
                            // 이미 URP - 최적화만 수행
                            OptimizeURPMaterial(materials[i]);
                        }
                        else
                        {
                            // Built-in - 스마트 변환
                            materials[i] = SmartConvertToURP(materials[i]);
                            materialsChanged = true;
                        }
                    }
                }
                
                if (materialsChanged)
                {
                    renderer.materials = materials;
                }
            }
        }
    }
    
    private Material SmartConvertToURP(Material originalMaterial)
    {
        // 원본 머티리얼의 특성을 분석하여 최적의 URP 셰이더 선택
        Shader urpShader = SelectOptimalURPShader(originalMaterial);
        Material urpMaterial = new Material(urpShader);
        
        // 원본 속성들을 최대한 보존
        if (preserveOriginalColors && originalMaterial.HasProperty("_Color"))
        {
            urpMaterial.color = originalMaterial.color;
        }
        
        if (preserveOriginalTextures && originalMaterial.HasProperty("_MainTex") && originalMaterial.mainTexture != null)
        {
            urpMaterial.mainTexture = originalMaterial.mainTexture;
        }
        
        // URP 최적화 설정
        OptimizeURPMaterial(urpMaterial);
        
        return urpMaterial;
    }
    
    private Shader SelectOptimalURPShader(Material originalMaterial)
    {
        // 원본 머티리얼의 특성에 따라 최적의 URP 셰이더 선택
        if (originalMaterial.HasProperty("_EmissionColor") && originalMaterial.GetColor("_EmissionColor") != Color.black)
        {
            // 발광 머티리얼
            Shader emissionShader = Shader.Find("Universal Render Pipeline/Lit");
            if (emissionShader != null) return emissionShader;
        }
        
        if (originalMaterial.HasProperty("_BumpMap") && originalMaterial.GetTexture("_BumpMap") != null)
        {
            // 노멀맵이 있는 머티리얼
            Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
            if (litShader != null) return litShader;
        }
        
        // 기본 URP 셰이더
        Shader defaultShader = Shader.Find("Universal Render Pipeline/Lit");
        if (defaultShader != null) return defaultShader;
        
        defaultShader = Shader.Find("Universal Render Pipeline/Simple Lit");
        if (defaultShader != null) return defaultShader;
        
        return Shader.Find("Standard");
    }
    
    private void OptimizeURPMaterial(Material material)
    {
        if (material.shader.name.Contains("Universal Render Pipeline/Lit"))
        {
            // URP Lit 셰이더 최적화
            if (!material.HasProperty("_Smoothness") || material.GetFloat("_Smoothness") < 0)
            {
                material.SetFloat("_Smoothness", 0.5f);
            }
            
            if (!material.HasProperty("_Metallic") || material.GetFloat("_Metallic") < 0)
            {
                material.SetFloat("_Metallic", 0.0f);
            }
            
            if (!material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 0.0f); // Opaque
            }
            
            if (!material.HasProperty("_Cull"))
            {
                material.SetFloat("_Cull", 2.0f); // Back
            }
        }
    }
    
    private bool IsURPShader(Shader shader)
    {
        if (shader == null) return false;
        
        string shaderName = shader.name.ToLower();
        return shaderName.Contains("universal render pipeline") || 
               shaderName.Contains("urp") ||
               shaderName.Contains("lit") ||
               shaderName.Contains("simple lit");
    }
    
    private Material ConvertToURPMaterial(Material originalMaterial)
    {
        // URP 셰이더 찾기
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader == null)
        {
            urpShader = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        
        if (urpShader == null)
        {
            urpShader = Shader.Find("Standard");
        }
        
        // 새 URP 머티리얼 생성
        Material urpMaterial = new Material(urpShader);
        
        // 기존 머티리얼의 속성들을 복사
        if (preserveOriginalColors && originalMaterial.HasProperty("_Color"))
        {
            urpMaterial.color = originalMaterial.color;
        }
        
        if (preserveOriginalTextures && originalMaterial.HasProperty("_MainTex") && originalMaterial.mainTexture != null)
        {
            urpMaterial.mainTexture = originalMaterial.mainTexture;
        }
        
        // URP 전용 속성 설정
        if (urpShader.name.Contains("Universal Render Pipeline/Lit"))
        {
            urpMaterial.SetFloat("_Smoothness", 0.5f);
            urpMaterial.SetFloat("_Metallic", 0.0f);
            urpMaterial.SetFloat("_Surface", 0.0f); // Opaque
            urpMaterial.SetFloat("_Cull", 2.0f); // Back
            urpMaterial.SetFloat("_AlphaClip", 0.0f);
        }
        
        return urpMaterial;
    }
    
    private void AlignToGround(GameObject obj)
    {
        RaycastHit hit;
        if (Physics.Raycast(obj.transform.position + Vector3.up * 100, Vector3.down, out hit))
        {
            obj.transform.position = hit.point + Vector3.up * groundOffset;
        }
    }
    
    private Vector3 GetRandomWorldPosition()
    {
        float x = (float)(random.NextDouble() - 0.5) * worldSize.x;
        float z = (float)(random.NextDouble() - 0.5) * worldSize.y;
        float y = worldHeight * 2f; // 높은 위치에서 시작
        
        return new Vector3(x, y, z);
    }
    
    [ContextMenu("Test Material Priority")]
    public void TestMaterialPriority()
    {
        Debug.Log($"현재 머티리얼 우선순위: {materialPriority}");
        Debug.Log($"URP 강제 변환: {forceURPConversion}");
        Debug.Log($"원본 색상 보존: {preserveOriginalColors}");
        Debug.Log($"원본 텍스처 보존: {preserveOriginalTextures}");
        Debug.Log($"커스텀 머티리얼 사용: {useCustomMaterials}");
        
        if (customMaterials != null)
        {
            Debug.Log($"커스텀 머티리얼 개수: {customMaterials.Length}");
        }
    }
    
    [ContextMenu("Generate with Current Settings")]
    public void GenerateWithCurrentSettings()
    {
        Debug.Log("현재 설정으로 월드를 생성합니다...");
        GenerateWorld();
    }
    
    private void OnDrawGizmos()
    {
        // 월드 범위 시각화
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(worldSize.x, worldHeight, worldSize.y));
        
        // 생성된 오브젝트들 시각화
        if (generatedObjects != null)
        {
            Gizmos.color = Color.yellow;
            foreach (GameObject obj in generatedObjects)
            {
                if (obj != null)
                {
                    Gizmos.DrawWireSphere(obj.transform.position, 1f);
                }
            }
        }
    }
}
