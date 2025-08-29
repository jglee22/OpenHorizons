using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 월드 배치를 자동화하고 편리하게 할 수 있는 월드 빌더
/// </summary>
public class WorldBuilder : MonoBehaviour
{
    [Header("World Generation")]
    [SerializeField] private bool generateOnStart = false;
    [SerializeField] private bool useRandomSeed = true;
    [SerializeField] private int seed = 12345;
    
    [Header("World Size")]
    [SerializeField] private Vector2 worldSize = new Vector2(100, 100);
    [SerializeField] private float worldHeight = 10f;
    
    [Header("Terrain Generation")]
    [SerializeField] private bool generateTerrain = true;
    [SerializeField] private int terrainResolution = 256;
    [SerializeField] private float noiseScale = 50f;
    [SerializeField] private int octaves = 4;
    [SerializeField] private float persistence = 0.5f;
    [SerializeField] private float lacunarity = 2f;
    
    [Header("Structure Generation")]
    [SerializeField] private bool generateStructures = true;
    [SerializeField] private GameObject[] structurePrefabs;
    [SerializeField] private int minStructures = 5;
    [SerializeField] private int maxStructures = 15;
    [SerializeField] private float minStructureDistance = 10f;
    
    [Header("Environment Objects")]
    [SerializeField] private bool generateEnvironment = true;
    [SerializeField] private GameObject[] treePrefabs;
    [SerializeField] private GameObject[] rockPrefabs;
    [SerializeField] private GameObject[] grassPrefabs;
    [SerializeField] private int environmentDensity = 100;
    
    [Header("Path Generation")]
    [SerializeField] private bool generatePaths = true;
    [SerializeField] private GameObject pathPrefab;
    [SerializeField] private int pathCount = 3;
    [SerializeField] private float pathWidth = 3f;
    
    [Header("Spawn Points")]
    [SerializeField] private bool generateSpawnPoints = true;
    [SerializeField] private GameObject spawnPointPrefab;
    [SerializeField] private int spawnPointCount = 3;
    
    private List<GameObject> generatedObjects = new List<GameObject>();
    private System.Random random;
    
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
        Debug.Log("월드 생성을 시작합니다...");
        
        // 기존 생성된 오브젝트들 정리
        ClearGeneratedWorld();
        
        // 랜덤 시드 설정
        if (useRandomSeed)
        {
            seed = Random.Range(0, 99999);
        }
        random = new System.Random(seed);
        
        Debug.Log($"월드 생성 시드: {seed}");
        
        // 월드 생성
        if (generateTerrain) GenerateTerrain();
        if (generateStructures) GenerateStructures();
        if (generateEnvironment) GenerateEnvironment();
        if (generatePaths) GeneratePaths();
        if (generateSpawnPoints) GenerateSpawnPoints();
        
        Debug.Log("월드 생성이 완료되었습니다!");
    }
    
    [ContextMenu("Clear World")]
    public void ClearGeneratedWorld()
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
    
    private void GenerateTerrain()
    {
        Debug.Log("지형을 생성합니다...");
        
        // 지형 오브젝트 생성
        GameObject terrain = new GameObject("GeneratedTerrain");
        terrain.transform.SetParent(transform);
        
        // 지형 메시 생성
        MeshFilter meshFilter = terrain.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrain.AddComponent<MeshRenderer>();
        
        // 지형 메시 생성
        Mesh terrainMesh = GenerateTerrainMesh();
        meshFilter.mesh = terrainMesh;
        
        // URP 머티리얼 적용
        Material terrainMaterial = CreateTerrainMaterial();
        meshRenderer.material = terrainMaterial;
        
        // 콜라이더 추가
        MeshCollider meshCollider = terrain.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = terrainMesh;
        
        generatedObjects.Add(terrain);
        Debug.Log("지형 생성 완료");
    }
    
    private Mesh GenerateTerrainMesh()
    {
        int resolution = terrainResolution;
        Vector2 size = worldSize;
        
        Mesh mesh = new Mesh();
        Vector3[] vertices = new Vector3[(resolution + 1) * (resolution + 1)];
        int[] triangles = new int[resolution * resolution * 6];
        Vector2[] uvs = new Vector2[vertices.Length];
        
        // 정점 생성
        for (int i = 0, z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++)
            {
                float xCoord = (float)x / resolution * size.x - size.x * 0.5f;
                float zCoord = (float)z / resolution * size.y - size.y * 0.5f;
                float yCoord = GenerateHeight(xCoord, zCoord);
                
                vertices[i] = new Vector3(xCoord, yCoord, zCoord);
                uvs[i] = new Vector2((float)x / resolution, (float)z / resolution);
                i++;
            }
        }
        
        // 삼각형 생성
        int tris = 0;
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int vertIndex = z * (resolution + 1) + x;
                
                triangles[tris + 0] = vertIndex;
                triangles[tris + 1] = vertIndex + resolution + 1;
                triangles[tris + 2] = vertIndex + 1;
                
                triangles[tris + 3] = vertIndex + 1;
                triangles[tris + 4] = vertIndex + resolution + 1;
                triangles[tris + 5] = vertIndex + resolution + 2;
                
                tris += 6;
            }
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    private float GenerateHeight(float x, float z)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseHeight = 0f;
        
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / noiseScale * frequency;
            float sampleZ = z / noiseScale * frequency;
            
            float perlinValue = Mathf.PerlinNoise(sampleX, sampleZ) * 2f - 1f;
            noiseHeight += perlinValue * amplitude;
            
            amplitude *= persistence;
            frequency *= lacunarity;
        }
        
        return noiseHeight * worldHeight;
    }
    
    private Material CreateTerrainMaterial()
    {
        // URP 머티리얼 생성
        Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
        if (urpShader == null)
        {
            urpShader = Shader.Find("Universal Render Pipeline/Simple Lit");
        }
        
        if (urpShader == null)
        {
            urpShader = Shader.Find("Standard");
        }
        
        Material material = new Material(urpShader);
        material.color = new Color(0.4f, 0.7f, 0.3f); // 초록색 지형
        
        if (urpShader.name.Contains("Universal Render Pipeline/Lit"))
        {
            material.SetFloat("_Smoothness", 0.1f);
            material.SetFloat("_Metallic", 0.0f);
        }
        
        return material;
    }
    
    private void GenerateStructures()
    {
        if (structurePrefabs == null || structurePrefabs.Length == 0)
        {
            Debug.LogWarning("구조물 프리팹이 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("구조물을 생성합니다...");
        
        int structureCount = random.Next(minStructures, maxStructures + 1);
        List<Vector3> structurePositions = new List<Vector3>();
        
        for (int i = 0; i < structureCount; i++)
        {
            Vector3 position = GetRandomWorldPosition();
            
            // 다른 구조물과의 거리 확인
            bool tooClose = false;
            foreach (Vector3 existingPos in structurePositions)
            {
                if (Vector3.Distance(position, existingPos) < minStructureDistance)
                {
                    tooClose = true;
                    break;
                }
            }
            
            if (tooClose) continue;
            
            // 구조물 생성
            GameObject prefab = structurePrefabs[random.Next(structurePrefabs.Length)];
            GameObject structure = Instantiate(prefab, position, Quaternion.Euler(0, random.Next(0, 360), 0));
            structure.transform.SetParent(transform);
            
            // 지형에 맞춰 위치 조정
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 100, Vector3.down, out hit))
            {
                structure.transform.position = hit.point;
            }
            
            // 프리팹에 머티리얼이 있는지 확인하고 URP 호환성 검사
            CheckAndFixMaterials(structure);
            
            structurePositions.Add(position);
            generatedObjects.Add(structure);
        }
        
        Debug.Log($"{structureCount}개의 구조물 생성 완료");
    }
    
    private void GenerateEnvironment()
    {
        Debug.Log("환경 오브젝트를 생성합니다...");
        
        int objectCount = environmentDensity;
        
        for (int i = 0; i < objectCount; i++)
        {
            Vector3 position = GetRandomWorldPosition();
            
            // 지형에 맞춰 위치 조정
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 100, Vector3.down, out hit))
            {
                position = hit.point;
            }
            
            // 랜덤하게 오브젝트 타입 선택
            GameObject[] prefabArray = null;
            if (random.NextDouble() < 0.4f && treePrefabs.Length > 0)
            {
                prefabArray = treePrefabs;
            }
            else if (random.NextDouble() < 0.3f && rockPrefabs.Length > 0)
            {
                prefabArray = rockPrefabs;
            }
            else if (grassPrefabs.Length > 0)
            {
                prefabArray = grassPrefabs;
            }
            
            if (prefabArray != null && prefabArray.Length > 0)
            {
                GameObject prefab = prefabArray[random.Next(prefabArray.Length)];
                GameObject envObject = Instantiate(prefab, position, Quaternion.Euler(0, random.Next(0, 360), 0));
                envObject.transform.SetParent(transform);
                
                // 랜덤 스케일
                float scale = random.Next(80, 120) / 100f;
                envObject.transform.localScale = Vector3.one * scale;
                
                // 프리팹에 머티리얼이 있는지 확인하고 URP 호환성 검사
                CheckAndFixMaterials(envObject);
                
                generatedObjects.Add(envObject);
            }
        }
        
        Debug.Log($"{objectCount}개의 환경 오브젝트 생성 완료");
    }
    
    private void GeneratePaths()
    {
        if (pathPrefab == null)
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
            CreatePath(startPos, endPos);
        }
        
        Debug.Log($"{pathCount}개의 경로 생성 완료");
    }
    
    private void CreatePath(Vector3 start, Vector3 end)
    {
        Vector3 direction = (end - start).normalized;
        float distance = Vector3.Distance(start, end);
        int segments = Mathf.CeilToInt(distance / 5f);
        
        for (int i = 0; i < segments; i++)
        {
            float t = (float)i / segments;
            Vector3 position = Vector3.Lerp(start, end, t);
            
            // 지형에 맞춰 위치 조정
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 100, Vector3.down, out hit))
            {
                position = hit.point + Vector3.up * 0.1f;
            }
            
            GameObject pathSegment = Instantiate(pathPrefab, position, Quaternion.LookRotation(direction));
            pathSegment.transform.SetParent(transform);
            pathSegment.transform.localScale = new Vector3(pathWidth, 0.1f, 5f);
            
            generatedObjects.Add(pathSegment);
        }
    }
    
    private void GenerateSpawnPoints()
    {
        if (spawnPointPrefab == null)
        {
            Debug.LogWarning("스폰 포인트 프리팹이 설정되지 않았습니다.");
            return;
        }
        
        Debug.Log("스폰 포인트를 생성합니다...");
        
        for (int i = 0; i < spawnPointCount; i++)
        {
            Vector3 position = GetRandomWorldPosition();
            
            // 지형에 맞춰 위치 조정
            RaycastHit hit;
            if (Physics.Raycast(position + Vector3.up * 100, Vector3.down, out hit))
            {
                position = hit.point + Vector3.up * 1f;
            }
            
            GameObject spawnPoint = Instantiate(spawnPointPrefab, position, Quaternion.identity);
            spawnPoint.name = $"SpawnPoint_{i + 1}";
            spawnPoint.transform.SetParent(transform);
            
            generatedObjects.Add(spawnPoint);
        }
        
        Debug.Log($"{spawnPointCount}개의 스폰 포인트 생성 완료");
    }
    
    private Vector3 GetRandomWorldPosition()
    {
        float x = (float)(random.NextDouble() - 0.5) * worldSize.x;
        float z = (float)(random.NextDouble() - 0.5) * worldSize.y;
        float y = worldHeight * 2f; // 높은 위치에서 시작
        
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
    
    [ContextMenu("Generate Simple World")]
    public void GenerateSimpleWorld()
    {
        Debug.Log("간단한 월드를 생성합니다...");
        
        ClearGeneratedWorld();
        
        // 기본 지형만 생성
        generateTerrain = true;
        generateStructures = false;
        generateEnvironment = false;
        generatePaths = false;
        generateSpawnPoints = false;
        
        GenerateWorld();
        
        Debug.Log("간단한 월드 생성 완료");
    }
    
    [ContextMenu("Generate Full World")]
    public void GenerateFullWorld()
    {
        Debug.Log("완전한 월드를 생성합니다...");
        
        ClearGeneratedWorld();
        
        // 모든 요소 생성
        generateTerrain = true;
        generateStructures = true;
        generateEnvironment = true;
        generatePaths = true;
        generateSpawnPoints = true;
        
        GenerateWorld();
        
        Debug.Log("완전한 월드 생성 완료");
    }
    
    private void OnDrawGizmos()
    {
        // 월드 범위 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(worldSize.x, worldHeight, worldSize.y));
        
        // 생성된 오브젝트들 시각화
        if (generatedObjects != null)
        {
            Gizmos.color = Color.green;
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
