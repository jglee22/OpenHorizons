using UnityEngine;

/// <summary>
/// URP 셰이더를 강제로 적용하여 Built-in 셰이더 문제를 해결하는 스크립트
/// </summary>
public class URPShaderForcer : MonoBehaviour
{
    [Header("Force URP Shaders")]
    [SerializeField] private bool forceOnStart = true;
    [SerializeField] private bool forceOnUpdate = false;
    
    [Header("Shader Names")]
    [SerializeField] private string[] urpShaderNames = {
        "Universal Render Pipeline/Lit",
        "Universal Render Pipeline/Simple Lit",
        "Universal Render Pipeline/Unlit"
    };
    
    [Header("Target Objects")]
    [SerializeField] private bool forcePlayerMaterials = true;
    [SerializeField] private bool forceGroundMaterials = true;
    [SerializeField] private bool forceAllRenderers = false;
    
    private Shader urpShader;
    
    private void Start()
    {
        if (forceOnStart)
        {
            ForceURPShaders();
        }
    }
    
    private void Update()
    {
        if (forceOnUpdate)
        {
            ForceURPShaders();
        }
    }
    
    [ContextMenu("Force URP Shaders Now")]
    public void ForceURPShaders()
    {
        Debug.Log("URP 셰이더 강제 적용을 시작합니다...");
        
        // URP 셰이더 찾기
        urpShader = FindURPShader();
        
        if (urpShader == null)
        {
            Debug.LogError("URP 셰이더를 찾을 수 없습니다!");
            return;
        }
        
        Debug.Log($"사용할 URP 셰이더: {urpShader.name}");
        
        // 플레이어 머티리얼 강제 적용
        if (forcePlayerMaterials)
        {
            ForcePlayerMaterials();
        }
        
        // 지면 머티리얼 강제 적용
        if (forceGroundMaterials)
        {
            ForceGroundMaterials();
        }
        
        // 모든 렌더러에 강제 적용
        if (forceAllRenderers)
        {
            ForceAllRenderers();
        }
        
        Debug.Log("URP 셰이더 강제 적용이 완료되었습니다!");
    }
    
    private Shader FindURPShader()
    {
        // 여러 방법으로 URP 셰이더 찾기
        foreach (string shaderName in urpShaderNames)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader != null)
            {
                Debug.Log($"URP 셰이더를 찾았습니다: {shaderName}");
                return shader;
            }
        }
        
        // 대안 방법: 모든 셰이더에서 URP 관련 셰이더 찾기
        Shader[] allShaders = Resources.FindObjectsOfTypeAll<Shader>();
        foreach (Shader shader in allShaders)
        {
            if (shader.name.Contains("Universal Render Pipeline") || 
                shader.name.Contains("URP") ||
                shader.name.Contains("Universal"))
            {
                Debug.Log($"URP 관련 셰이더를 찾았습니다: {shader.name}");
                return shader;
            }
        }
        
        return null;
    }
    
    private void ForcePlayerMaterials()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Renderer renderer = player.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                // 현재 머티리얼의 색상과 텍스처 보존
                Color originalColor = renderer.material.color;
                Texture originalTexture = renderer.material.mainTexture;
                
                // 새 URP 머티리얼 생성
                Material newMaterial = new Material(urpShader);
                newMaterial.color = originalColor;
                if (originalTexture != null)
                {
                    newMaterial.mainTexture = originalTexture;
                }
                
                // URP 셰이더 설정 적용
                ApplyURPMaterialSettings(newMaterial);
                
                // 머티리얼 적용
                renderer.material = newMaterial;
                
                Debug.Log($"플레이어 머티리얼이 URP 셰이더로 변경되었습니다: {urpShader.name}");
            }
        }
    }
    
    private void ForceGroundMaterials()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground != null)
        {
            Renderer renderer = ground.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                // 현재 머티리얼의 색상과 텍스처 보존
                Color originalColor = renderer.material.color;
                Texture originalTexture = renderer.material.mainTexture;
                
                // 새 URP 머티리얼 생성
                Material newMaterial = new Material(urpShader);
                newMaterial.color = originalColor;
                if (originalTexture != null)
                {
                    newMaterial.mainTexture = originalTexture;
                }
                
                // URP 셰이더 설정 적용
                ApplyURPMaterialSettings(newMaterial);
                
                // 머티리얼 적용
                renderer.material = newMaterial;
                
                Debug.Log($"지면 머티리얼이 URP 셰이더로 변경되었습니다: {urpShader.name}");
            }
        }
    }
    
    private void ForceAllRenderers()
    {
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        int changedCount = 0;
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer.material != null && renderer.material.shader != null)
            {
                // Built-in 셰이더인 경우에만 변경
                if (renderer.material.shader.name.Contains("Standard") || 
                    renderer.material.shader.name.Contains("Legacy Shaders") ||
                    renderer.material.shader.name.Contains("Unlit"))
                {
                    // 현재 머티리얼의 속성들 보존
                    Color originalColor = renderer.material.color;
                    Texture originalTexture = renderer.material.mainTexture;
                    
                    // 새 URP 머티리얼 생성
                    Material newMaterial = new Material(urpShader);
                    newMaterial.color = originalColor;
                    if (originalTexture != null)
                    {
                        newMaterial.mainTexture = originalTexture;
                    }
                    
                    // URP 셰이더 설정 적용
                    ApplyURPMaterialSettings(newMaterial);
                    
                    // 머티리얼 적용
                    renderer.material = newMaterial;
                    changedCount++;
                }
            }
        }
        
        Debug.Log($"총 {changedCount}개의 머티리얼이 URP 셰이더로 변경되었습니다.");
    }
    
    private void ApplyURPMaterialSettings(Material material)
    {
        try
        {
            // URP Lit 셰이더인 경우 기본 설정 적용
            if (material.shader.name.Contains("Universal Render Pipeline/Lit") || 
                material.shader.name.Contains("Universal Render Pipeline/Simple Lit"))
            {
                material.SetFloat("_Smoothness", 0.5f);
                material.SetFloat("_Metallic", 0.0f);
                
                // 추가 URP 속성들 설정
                if (material.shader.name.Contains("Universal Render Pipeline/Lit"))
                {
                    material.SetFloat("_Surface", 0); // Opaque
                    material.SetFloat("_Cull", 2); // Back
                    material.SetFloat("_AlphaClip", 0);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"URP 머티리얼 설정 중 오류 발생: {e.Message}");
        }
    }
    
    [ContextMenu("Check Current Shaders")]
    public void CheckCurrentShaders()
    {
        Debug.Log("=== 현재 씬의 셰이더 상태 확인 ===");
        
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        int builtinCount = 0;
        int urpCount = 0;
        int otherCount = 0;
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer.material != null && renderer.material.shader != null)
            {
                string shaderName = renderer.material.shader.name;
                
                if (shaderName.Contains("Standard") || 
                    shaderName.Contains("Legacy Shaders") ||
                    shaderName.Contains("Unlit"))
                {
                    builtinCount++;
                    Debug.Log($"Built-in 셰이더: {renderer.name} - {shaderName}");
                }
                else if (shaderName.Contains("Universal Render Pipeline") || 
                         shaderName.Contains("URP"))
                {
                    urpCount++;
                    Debug.Log($"URP 셰이더: {renderer.name} - {shaderName}");
                }
                else
                {
                    otherCount++;
                    Debug.Log($"기타 셰이더: {renderer.name} - {shaderName}");
                }
            }
        }
        
        Debug.Log($"=== 셰이더 통계 ===");
        Debug.Log($"Built-in 셰이더: {builtinCount}개");
        Debug.Log($"URP 셰이더: {urpCount}개");
        Debug.Log($"기타 셰이더: {otherCount}개");
        
        if (builtinCount > 0)
        {
            Debug.LogWarning("Built-in 셰이더를 사용하는 오브젝트가 있습니다. Force URP Shaders Now를 실행하여 해결하세요.");
        }
    }
    
    private void OnDrawGizmos()
    {
        // URP 셰이더가 적용되지 않은 오브젝트들을 시각적으로 표시
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        
        foreach (Renderer renderer in allRenderers)
        {
            if (renderer.material != null && renderer.material.shader != null)
            {
                string shaderName = renderer.material.shader.name;
                
                if (shaderName.Contains("Standard") || 
                    shaderName.Contains("Legacy Shaders") ||
                    shaderName.Contains("Unlit"))
                {
                    // Built-in 셰이더 사용 중인 오브젝트를 빨간색으로 표시
                    Gizmos.color = Color.red;
                    Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
                }
            }
        }
    }
}
