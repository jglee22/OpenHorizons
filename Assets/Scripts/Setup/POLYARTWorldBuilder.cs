using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// POLYART Ancient Village 에셋을 활용한 월드 구성 전용 빌더
/// </summary>
public class POLYARTWorldBuilder : MonoBehaviour
{
    [Header("POLYART Asset References")]
    [SerializeField] private GameObject[] buildingPrefabs;
    [SerializeField] private GameObject[] decorationPrefabs;
    [SerializeField] private GameObject[] naturePrefabs;
    [SerializeField] private GameObject[] pathPrefabs;
    
    [Header("World Layout")]
    [SerializeField] private Vector2 worldSize = new Vector2(80, 80);
    [SerializeField] private int buildingCount = 8;
    [SerializeField] private int decorationCount = 20;
    [SerializeField] private int natureCount = 30;
    [SerializeField] private int pathCount = 4;
    
    [Header("Placement Settings")]
    [SerializeField] private float minBuildingDistance = 15f;
    [SerializeField] private float minDecorationDistance = 5f;
    [SerializeField] private float minNatureDistance = 3f;
    [SerializeField] private bool alignToGround = true;
    [SerializeField] private float groundOffset = 0.1f;
    
    [Header("Generation Options")]
    [SerializeField] private bool generateOnStart = false;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;
    
    private List<GameObject> placedObjects = new List<GameObject>();
    private System.Random random;
    
    private void Start()
    {
        if (generateOnStart)
        {
            BuildPOLYARTWorld();
        }
    }
    
    [ContextMenu("Build POLYART World")]
    public void BuildPOLYARTWorld()
    {
        Debug.Log("POLYART 월드 구성을 시작합니다...");
        
        // 기존 배치된 오브젝트들 정리
        ClearWorld();
        
        // 랜덤 시드 설정
        if (useRandomSeed)
        {
            seed = Random.Range(0, 99999);
        }
        random = new System.Random(seed);
        
        Debug.Log($"월드 구성 시드: {seed}");
        
        // 월드 구성
        PlaceBuildings();
        PlaceDecorations();
        PlaceNature();
        CreatePaths();
        
        Debug.Log("POLYART 월드 구성이 완료되었습니다!");
    }
    
    [ContextMenu("Clear World")]
    public void ClearWorld()
    {
        Debug.Log("배치된 월드를 정리합니다...");
        
        foreach (GameObject obj in placedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        
        placedObjects.Clear();
        Debug.Log("월드 정리가 완료되었습니다.");
    }
    
    private void PlaceBuildings()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0)
        {
            Debug.LogWarning("건물 프리팹이 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("건물을 배치합니다...");
        
        List<Vector3> buildingPositions = new List<Vector3>();
        
        for (int i = 0; i < buildingCount; i++)
        {
            Vector3 position = GetRandomWorldPosition();
            
            // 다른 건물과의 거리 확인
            bool tooClose = false;
            foreach (Vector3 existingPos in buildingPositions)
            {
                if (Vector3.Distance(position, existingPos) < minBuildingDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (tooClose) continue;
            
            // 건물 생성
            GameObject prefab = buildingPrefabs[random.Next(buildingPrefabs.Length)];
            GameObject building = Instantiate(prefab, position, Quaternion.Euler(0, random.Next(0, 360), 0));
            building.transform.SetParent(transform);
            building.name = $"Building_{i + 1}";
            
            // 지형에 맞춰 위치 조정
            if (alignToGround)
            {
                AlignToGround(building);
            }
            
            // 프리팹에 머티리얼이 있는지 확인하고 URP 호환성 검사
            CheckAndFixMaterials(building);
            
            buildingPositions.Add(position);
            placedObjects.Add(building);
        }
        
        Debug.Log($"{buildingCount}개의 건물 배치 완료");
    }
    
    private void PlaceDecorations()
    {
        if (decorationPrefabs == null || decorationPrefabs.Length == 0)
        {
            Debug.LogWarning("장식 프리팹이 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("장식을 배치합니다...");
        
        List<Vector3> decorationPositions = new List<Vector3>();
        
        for (int i = 0; i < decorationCount; i++)
        {
            Vector3 position = GetRandomWorldPosition();
            
            // 다른 장식과의 거리 확인
            bool tooClose = false;
            foreach (Vector3 existingPos in decorationPositions)
            {
                if (Vector3.Distance(position, existingPos) < minDecorationDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (tooClose) continue;
            
            // 장식 생성
            GameObject prefab = decorationPrefabs[random.Next(decorationPrefabs.Length)];
            GameObject decoration = Instantiate(prefab, position, Quaternion.Euler(0, random.Next(0, 360), 0));
            decoration.transform.SetParent(transform);
            decoration.name = $"Decoration_{i + 1}";
            
            // 지형에 맞춰 위치 조정
            if (alignToGround)
            {
                AlignToGround(decoration);
            }
            
            // 랜덤 스케일
            float scale = random.Next(80, 120) / 100f;
            decoration.transform.localScale = Vector3.one * scale;
            
            // 프리팹에 머티리얼이 있는지 확인하고 URP 호환성 검사
            CheckAndFixMaterials(decoration);
            
            decorationPositions.Add(position);
            placedObjects.Add(decoration);
        }
        
        Debug.Log($"{decorationCount}개의 장식 배치 완료");
    }
    
    private void PlaceNature()
    {
        if (naturePrefabs == null || naturePrefabs.Length == 0)
        {
            Debug.LogWarning("자연 오브젝트 프리팹이 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("자연 오브젝트를 배치합니다...");
        
        List<Vector3> naturePositions = new List<Vector3>();
        
        for (int i = 0; i < natureCount; i++)
        {
            Vector3 position = GetRandomWorldPosition();
            
            // 다른 자연 오브젝트와의 거리 확인
            bool tooClose = false;
            foreach (Vector3 existingPos in naturePositions)
            {
                if (Vector3.Distance(position, existingPos) < minNatureDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (tooClose) continue;
            
            // 자연 오브젝트 생성
            GameObject prefab = naturePrefabs[random.Next(naturePrefabs.Length)];
            GameObject nature = Instantiate(prefab, position, Quaternion.Euler(0, random.Next(0, 360), 0));
            nature.transform.SetParent(transform);
            nature.name = $"Nature_{i + 1}";
            
            // 지형에 맞춰 위치 조정
            if (alignToGround)
            {
                AlignToGround(nature);
            }
            
            // 랜덤 스케일
            float scale = random.Next(70, 130) / 100f;
            nature.transform.localScale = Vector3.one * scale;
            
            // 프리팹에 머티리얼이 있는지 확인하고 URP 호환성 검사
            CheckAndFixMaterials(nature);
            
            naturePositions.Add(position);
            placedObjects.Add(nature);
        }
        
        Debug.Log($"{natureCount}개의 자연 오브젝트 배치 완료");
    }
    
    private void CreatePaths()
    {
        if (pathPrefabs == null || pathPrefabs.Length == 0)
        {
            Debug.LogWarning("경로 프리팹이 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("경로를 생성합니다...");
        
        for (int i = 0; i < pathCount; i++)
        {
            Vector3 startPos = GetRandomWorldPosition();
            Vector3 endPos = GetRandomWorldPosition();
            
            // 경로 생성
            CreatePathBetweenPoints(startPos, endPos);
        }
        
        Debug.Log($"{pathCount}개의 경로 생성 완료");
    }
    
    private void CreatePathBetweenPoints(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        int segments = Mathf.CeilToInt(distance / 8f);
        
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            Vector3 position = Vector3.Lerp(start, end, t);
            
            // 지형에 맞춰 위치 조정
            if (alignToGround)
            {
                RaycastHit hit;
                if (Physics.Raycast(position + Vector3.up * 100, Vector3.down, out hit))
                {
                    position = hit.point + Vector3.up * groundOffset;
                }
            }
            
            GameObject pathPrefab = pathPrefabs[random.Next(pathPrefabs.Length)];
            GameObject pathSegment = Instantiate(pathPrefab, position, Quaternion.LookRotation(direction));
            pathSegment.transform.SetParent(transform);
            pathSegment.name = $"PathSegment_{placedObjects.Count}";
            
            placedObjects.Add(pathSegment);
        }
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
        float y = 100f; // 높은 위치에서 시작
        
        return new Vector3(x, y, z);
    }
    
    /// <summary>
    /// 오브젝트의 머티리얼을 확인하고 URP 호환성을 검사하여 필요시 수정
    /// </summary>
    private void CheckAndFixMaterials(GameObject obj)
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
                        else
                        {
                            // Built-in 셰이더 사용 중 - URP 머티리얼로 변환
                            Debug.Log($"오브젝트 {obj.name}의 머티리얼 {materials[i].name}을 URP 머티리얼로 변환합니다.");
                            materials[i] = ConvertToURPMaterial(materials[i]);
                            materialsChanged = true;
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
    
    /// <summary>
    /// 셰이더가 URP 셰이더인지 확인
    /// </summary>
    private bool IsURPShader(Shader shader)
    {
        if (shader == null) return false;
        
        string shaderName = shader.name.ToLower();
        return shaderName.Contains("universal render pipeline") || 
               shaderName.Contains("urp") ||
               shaderName.Contains("lit") ||
               shaderName.Contains("simple lit");
    }
    
    /// <summary>
    /// Built-in 머티리얼을 URP 머티리얼로 변환
    /// </summary>
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
        if (originalMaterial.HasProperty("_Color"))
        {
            urpMaterial.color = originalMaterial.color;
        }
        
        if (originalMaterial.HasProperty("_MainTex") && originalMaterial.mainTexture != null)
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
    
    [ContextMenu("Generate Village Center")]
    public void GenerateVillageCenter()
    {
        Debug.Log("마을 중심부를 생성합니다...");
        
        ClearWorld();
        
        // 중심부 건물들 배치
        PlaceCentralBuildings();
        
        // 중심부 장식 배치
        PlaceCentralDecorations();
        
        // 중심부 경로 생성
        CreateCentralPaths();
        
        Debug.Log("마을 중심부 생성 완료");
    }
    
    private void PlaceCentralBuildings()
    {
        if (buildingPrefabs == null || buildingPrefabs.Length == 0) return;
        
        // 중심부에 주요 건물들 배치
        Vector3 center = Vector3.zero;
        float radius = 20f;
        
        for (int i = 0; i < Mathf.Min(buildingCount, 5); i++)
        {
            float angle = (float)i / 5f * 2f * Mathf.PI;
            Vector3 position = center + new Vector3(Mathf.Cos(angle) * radius, 100, Mathf.Sin(angle) * radius);
            
            GameObject prefab = buildingPrefabs[random.Next(buildingPrefabs.Length)];
            GameObject building = Instantiate(prefab, position, Quaternion.Euler(0, angle * Mathf.Rad2Deg, 0));
            building.transform.SetParent(transform);
            building.name = $"CentralBuilding_{i + 1}";
            
            if (alignToGround)
            {
                AlignToGround(building);
            }
            
            placedObjects.Add(building);
        }
    }
    
    private void PlaceCentralDecorations()
    {
        if (decorationPrefabs == null || decorationPrefabs.Length == 0) return;
        
        // 중심부에 장식 배치
        Vector3 center = Vector3.zero;
        float radius = 15f;
        
        for (int i = 0; i < Mathf.Min(decorationCount, 8); i++)
        {
            float angle = (float)i / 8f * 2f * Mathf.PI;
            Vector3 position = center + new Vector3(Mathf.Cos(angle) * radius, 100, Mathf.Sin(angle) * radius);
            
            GameObject prefab = decorationPrefabs[random.Next(decorationPrefabs.Length)];
            GameObject decoration = Instantiate(prefab, position, Quaternion.Euler(0, random.Next(0, 360), 0));
            decoration.transform.SetParent(transform);
            decoration.name = $"CentralDecoration_{i + 1}";
            
            if (alignToGround)
            {
                AlignToGround(decoration);
            }
            
            placedObjects.Add(decoration);
        }
    }
    
    private void CreateCentralPaths()
    {
        if (pathPrefabs == null || pathPrefabs.Length == 0) return;
        
        // 중심부에서 방사형 경로 생성
        Vector3 center = Vector3.zero;
        float radius = 25f;
        
        for (int i = 0; i < 4; i++)
        {
            float angle = (float)i / 4f * 2f * Mathf.PI;
            Vector3 endPos = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            
            CreatePathBetweenPoints(center, endPos);
        }
    }
    
    [ContextMenu("Optimize World")]
    public void OptimizeWorld()
    {
        Debug.Log("월드 최적화를 시작합니다...");
        
        foreach (GameObject obj in placedObjects)
        {
            if (obj != null)
            {
                // LOD 그룹 추가 (있는 경우)
                if (obj.GetComponent<LODGroup>() == null)
                {
                    // 간단한 최적화
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                        renderer.receiveShadows = true;
                    }
                }
            }
        }
        
        Debug.Log("월드 최적화 완료");
    }
    
    private void OnDrawGizmos()
    {
        // 월드 범위 시각화
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(worldSize.x, 1, worldSize.y));
        
        // 배치된 오브젝트들 시각화
        if (placedObjects != null)
        {
            Gizmos.color = Color.green;
            foreach (GameObject obj in placedObjects)
            {
                if (obj != null)
                {
                    Gizmos.DrawWireSphere(obj.transform.position, 1f);
                }
            }
        }
        
        // 중심부 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, 2f);
    }
}
