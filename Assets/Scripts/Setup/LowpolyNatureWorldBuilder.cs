using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// LowpolyNatureBundle 에셋들을 위한 스마트 배치 시스템
/// 계절별 테마, 생태계별 배치, 충돌 방지 기능 포함
/// </summary>
public class LowpolyNatureWorldBuilder : MonoBehaviour
{
    [Header("🌍 월드 설정")]
    [SerializeField] private Vector2 worldSize = new Vector2(100f, 100f);
    [SerializeField] private float gridSize = 3f;              // 5 → 3으로 감소 (더 세밀한 배치)
    [SerializeField] private LayerMask groundLayer = 1;
    
    [Header("🌱 계절별 나무")]
    [SerializeField] private GameObject[] springTrees;      // 봄 나무들
    [SerializeField] private GameObject[] autumnTrees;      // 가을 나무들  
    [SerializeField] private GameObject[] winterTrees;      // 겨울 나무들
    
    [Header("🏔️ 지형 요소")]
    [SerializeField] private GameObject[] rocks;            // 바위들
    [SerializeField] private GameObject[] trunks;           // 통나무들
    [SerializeField] private GameObject[] fallenTrees;      // 쓰러진 나무들
    
    [Header("🌿 식물 및 장식")]
    [SerializeField] private GameObject[] bushes;           // 관목들
    [SerializeField] private GameObject[] bamboos;          // 대나무들
    [SerializeField] private GameObject[] mushrooms;        // 버섯들
    [SerializeField] private GameObject[] pottedPlants;     // 화분 식물들
    
    [Header("🏗️ 구조물 및 소품")]
    [SerializeField] private GameObject[] woodenFences;     // 나무 울타리들
    [SerializeField] private GameObject[] brickWalls;       // 벽돌 벽들
    [SerializeField] private GameObject[] furniture;        // 가구들 (테이블, 의자 등)
    [SerializeField] private GameObject[] props;            // 소품들 (모닥불, 물뿌리개 등)
    
    [Header("🎨 테마 설정")]
    [SerializeField] private Season currentSeason = Season.Spring;
    [SerializeField] private bool useMixedSeason = false;
    [SerializeField] private float treeDensity = 0.6f;        // 0.3 → 0.6으로 증가
    [SerializeField] private float rockDensity = 0.4f;        // 0.2 → 0.4로 증가
    [SerializeField] private float decorationDensity = 0.7f;  // 0.4 → 0.7로 증가
    
    [Header("📐 배치 규칙")]
    [SerializeField] private float minTreeDistance = 2.5f;    // 4 → 2.5로 감소
    [SerializeField] private float minRockDistance = 2f;      // 3 → 2로 감소
    [SerializeField] private float minStructureDistance = 5f; // 8 → 5로 감소
    [SerializeField] private float minDecorationDistance = 1.5f; // 2 → 1.5로 감소
    
    [Header("🔧 자동 프리팹 등록")]
    [SerializeField] private string prefabPath = "Assets/LowpolyNatureBundle/Prefab";
    [SerializeField] private bool autoLoadPrefabs = false;
    
    [Header("📁 부모 오브젝트 관리")]
    [SerializeField] private GameObject worldParent;        // 월드 오브젝트들의 부모
    [SerializeField] private GameObject treesParent;        // 나무들의 부모
    [SerializeField] private GameObject rocksParent;        // 바위들의 부모
    [SerializeField] private GameObject vegetationParent;   // 식물들의 부모
    [SerializeField] private GameObject structuresParent;   // 구조물들의 부모
    [SerializeField] private GameObject propsParent;        // 소품들의 부모
    
    // 내부 변수들
    private GridCell[,] worldGrid;
    private int gridWidth, gridHeight;
    private List<GameObject> generatedObjects = new List<GameObject>();
    
    // 계절 열거형
    public enum Season
    {
        Spring,     // 봄 - 생동감 넘치는 초록색
        Autumn,     // 가을 - 따뜻한 주황/노란색
        Winter,     // 겨울 - 차가운 하늘색/흰색
        Mixed       // 혼합 - 모든 계절의 요소
    }
    
    // 그리드 셀 클래스
    [System.Serializable]
    public class GridCell
    {
        public bool isOccupied;
        public float height;
        public TerrainType terrainType;
        public List<GameObject> objects = new List<GameObject>();
        public Vector3 worldPosition;
    }
    
    // 지형 타입
    public enum TerrainType
    {
        Water,      // 물
        Plain,      // 평지
        Forest,     // 숲
        Mountain,   // 산
        Valley      // 계곡
    }
    
    #region Unity Lifecycle
    
    private void Start()
    {
        InitializeWorldGrid();
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// 현재 설정으로 월드 생성
    /// </summary>
    [ContextMenu("🌍 스마트 월드 생성")]
    public void GenerateSmartWorld()
    {
        ClearWorld();
        CreateParentObjects();  // 부모 오브젝트들 자동 생성
        InitializeWorldGrid();
        AnalyzeTerrain();
        GenerateWorldContent();
        Debug.Log($"🌍 {currentSeason} 테마로 스마트 월드가 생성되었습니다!");
    }
    
    /// <summary>
    /// 봄 테마로 월드 생성
    /// </summary>
    [ContextMenu("🌸 봄 테마 월드 생성")]
    public void GenerateSpringWorld()
    {
        currentSeason = Season.Spring;
        GenerateSmartWorld();
    }
    
    /// <summary>
    /// 가을 테마로 월드 생성
    /// </summary>
    [ContextMenu("🍂 가을 테마 월드 생성")]
    public void GenerateAutumnWorld()
    {
        currentSeason = Season.Autumn;
        GenerateSmartWorld();
    }
    
    /// <summary>
    /// 겨울 테마로 월드 생성
    /// </summary>
    [ContextMenu("❄️ 겨울 테마 월드 생성")]
    public void GenerateWinterWorld()
    {
        currentSeason = Season.Winter;
        GenerateSmartWorld();
    }
    
    /// <summary>
    /// 혼합 테마로 월드 생성
    /// </summary>
    [ContextMenu("🌈 혼합 테마 월드 생성")]
    public void GenerateMixedWorld()
    {
        currentSeason = Season.Mixed;
        GenerateSmartWorld();
    }
    
    /// <summary>
    /// 숲 생태계 생성
    /// </summary>
    [ContextMenu("🌲 숲 생태계 생성")]
    public void GenerateForestEcosystem()
    {
        GenerateForestArea();
    }
    
    /// <summary>
    /// 산악 생태계 생성
    /// </summary>
    [ContextMenu("🏔️ 산악 생태계 생성")]
    public void GenerateMountainEcosystem()
    {
        GenerateMountainArea();
    }
    
    /// <summary>
    /// 초원 생태계 생성
    /// </summary>
    [ContextMenu("🌾 초원 생태계 생성")]
    public void GeneratePlainEcosystem()
    {
        GeneratePlainArea();
    }
    
    /// <summary>
    /// 월드 정리
    /// </summary>
    [ContextMenu("🗑️ 월드 정리")]
    public void ClearWorld()
    {
        foreach (var obj in generatedObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        generatedObjects.Clear();
        
        if (worldGrid != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int z = 0; z < gridHeight; z++)
                {
                    if (worldGrid[x, z] != null)
                    {
                        worldGrid[x, z].objects.Clear();
                        worldGrid[x, z].isOccupied = false;
                    }
                }
            }
        }
        
        Debug.Log("🗑️ 월드가 정리되었습니다.");
    }
    
    /// <summary>
    /// 월드 최적화
    /// </summary>
    [ContextMenu("⚡ 월드 최적화")]
    public void OptimizeWorld()
    {
        // LOD 설정
        foreach (var obj in generatedObjects)
        {
            if (obj != null)
            {
                var renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                }
            }
        }
        
        Debug.Log("⚡ 월드가 최적화되었습니다.");
    }
    
    /// <summary>
    /// 부모 오브젝트들 자동 생성
    /// </summary>
    [ContextMenu("📁 부모 오브젝트 자동 생성")]
    public void CreateParentObjects()
    {
        // 메인 월드 부모 생성
        if (worldParent == null)
        {
            worldParent = new GameObject("🌍 World Objects");
            worldParent.transform.position = Vector3.zero;
        }
        
        // 카테고리별 부모 오브젝트들 생성
        if (treesParent == null)
        {
            treesParent = new GameObject("🌳 Trees");
            treesParent.transform.SetParent(worldParent.transform);
        }
        
        if (rocksParent == null)
        {
            rocksParent = new GameObject("🏔️ Rocks");
            rocksParent.transform.SetParent(worldParent.transform);
        }
        
        if (vegetationParent == null)
        {
            vegetationParent = new GameObject("🌿 Vegetation");
            vegetationParent.transform.SetParent(worldParent.transform);
        }
        
        if (structuresParent == null)
        {
            structuresParent = new GameObject("🏗️ Structures");
            structuresParent.transform.SetParent(worldParent.transform);
        }
        
        if (propsParent == null)
        {
            propsParent = new GameObject("🎭 Props");
            propsParent.transform.SetParent(worldParent.transform);
        }
        
        Debug.Log("📁 부모 오브젝트들이 생성되었습니다!");
    }
    
    /// <summary>
    /// 모든 부모 오브젝트 정리
    /// </summary>
    [ContextMenu("🗑️ 부모 오브젝트 정리")]
    public void ClearParentObjects()
    {
        if (worldParent != null)
        {
            DestroyImmediate(worldParent);
            worldParent = null;
            treesParent = null;
            rocksParent = null;
            vegetationParent = null;
            structuresParent = null;
            propsParent = null;
        }
        
        Debug.Log("🗑️ 부모 오브젝트들이 정리되었습니다!");
    }
    
    /// <summary>
    /// 모든 프리팹 자동 등록
    /// </summary>
    [ContextMenu("🔧 모든 프리팹 자동 등록")]
    public void AutoLoadAllPrefabs()
    {
        LoadSpringTrees();
        LoadAutumnTrees();
        LoadWinterTrees();
        LoadRocks();
        LoadTrunks();
        LoadFallenTrees();
        LoadBushes();
        LoadBamboos();
        LoadMushrooms();
        LoadPottedPlants();
        LoadWoodenFences();
        LoadBrickWalls();
        LoadFurniture();
        LoadProps();
        
        Debug.Log("🔧 모든 프리팹이 자동으로 등록되었습니다!");
    }
    
    /// <summary>
    /// 봄 나무들 자동 등록
    /// </summary>
    [ContextMenu("🌸 봄 나무들 자동 등록")]
    public void LoadSpringTrees()
    {
        springTrees = LoadPrefabsByPattern("SpringTree_");
        Debug.Log($"🌸 봄 나무 {springTrees.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 가을 나무들 자동 등록
    /// </summary>
    [ContextMenu("🍂 가을 나무들 자동 등록")]
    public void LoadAutumnTrees()
    {
        autumnTrees = LoadPrefabsByPattern("AutumnTree_");
        Debug.Log($"🍂 가을 나무 {autumnTrees.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 겨울 나무들 자동 등록
    /// </summary>
    [ContextMenu("❄️ 겨울 나무들 자동 등록")]
    public void LoadWinterTrees()
    {
        winterTrees = LoadPrefabsByPattern("WinterTree_");
        Debug.Log($"❄️ 겨울 나무 {winterTrees.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 바위들 자동 등록
    /// </summary>
    [ContextMenu("🏔️ 바위들 자동 등록")]
    public void LoadRocks()
    {
        rocks = LoadPrefabsByPattern("Rock_");
        Debug.Log($"🏔️ 바위 {rocks.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 통나무들 자동 등록
    /// </summary>
    [ContextMenu("🪵 통나무들 자동 등록")]
    public void LoadTrunks()
    {
        trunks = LoadPrefabsByPattern("Trunk_");
        Debug.Log($"🪵 통나무 {trunks.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 쓰러진 나무들 자동 등록
    /// </summary>
    [ContextMenu("🌳 쓰러진 나무들 자동 등록")]
    public void LoadFallenTrees()
    {
        fallenTrees = LoadPrefabsByPattern("FallenTree_");
        Debug.Log($"🌳 쓰러진 나무 {fallenTrees.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 관목들 자동 등록
    /// </summary>
    [ContextMenu("🌿 관목들 자동 등록")]
    public void LoadBushes()
    {
        bushes = LoadPrefabsByPattern("Bush_");
        Debug.Log($"🌿 관목 {bushes.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 대나무들 자동 등록
    /// </summary>
    [ContextMenu("🎋 대나무들 자동 등록")]
    public void LoadBamboos()
    {
        bamboos = LoadPrefabsByPattern("Bamboo_");
        Debug.Log($"🎋 대나무 {bamboos.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 버섯들 자동 등록
    /// </summary>
    [ContextMenu("🍄 버섯들 자동 등록")]
    public void LoadMushrooms()
    {
        mushrooms = LoadPrefabsByPattern("Mushroom_");
        Debug.Log($"🍄 버섯 {mushrooms.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 화분 식물들 자동 등록
    /// </summary>
    [ContextMenu("🪴 화분 식물들 자동 등록")]
    public void LoadPottedPlants()
    {
        pottedPlants = LoadPrefabsByPattern("PottedPlants_");
        Debug.Log($"🪴 화분 식물 {pottedPlants.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 나무 울타리들 자동 등록
    /// </summary>
    [ContextMenu("🪵 나무 울타리들 자동 등록")]
    public void LoadWoodenFences()
    {
        woodenFences = LoadPrefabsByPattern("WoodenFence_");
        Debug.Log($"🪵 나무 울타리 {woodenFences.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 벽돌 벽들 자동 등록
    /// </summary>
    [ContextMenu("🧱 벽돌 벽들 자동 등록")]
    public void LoadBrickWalls()
    {
        brickWalls = LoadPrefabsByPattern("BrickWall_");
        Debug.Log($"🧱 벽돌 벽 {brickWalls.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 가구들 자동 등록
    /// </summary>
    [ContextMenu("🪑 가구들 자동 등록")]
    public void LoadFurniture()
    {
        furniture = LoadPrefabsByPattern("WoodenTable_", "WoodenStool_");
        Debug.Log($"🪑 가구 {furniture.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 소품들 자동 등록
    /// </summary>
    [ContextMenu("🎭 소품들 자동 등록")]
    public void LoadProps()
    {
        props = LoadPrefabsByPattern("BonFire_", "WateringKettle_", "FoodTrough_", "Bottle_", "Pot_");
        Debug.Log($"🎭 소품 {props.Length}개가 등록되었습니다.");
    }
    
    /// <summary>
    /// 고밀도 설정으로 변경 (풍성한 월드)
    /// </summary>
    [ContextMenu("🌳 고밀도 설정 (풍성한 월드)")]
    public void SetHighDensity()
    {
        treeDensity = 0.8f;
        rockDensity = 0.6f;
        decorationDensity = 0.9f;
        minTreeDistance = 2f;
        minRockDistance = 1.5f;
        minStructureDistance = 4f;
        minDecorationDistance = 1f;
        gridSize = 2.5f;
        
        Debug.Log($"🌳 고밀도 설정이 적용되었습니다! 나무: {treeDensity}, 바위: {rockDensity}, 장식: {decorationDensity}");
    }
    
    /// <summary>
    /// 중밀도 설정으로 변경 (균형잡힌 월드)
    /// </summary>
    [ContextMenu("🌲 중밀도 설정 (균형잡힌 월드)")]
    public void SetMediumDensity()
    {
        treeDensity = 0.6f;
        rockDensity = 0.4f;
        decorationDensity = 0.7f;
        minTreeDistance = 2.5f;
        minRockDistance = 2f;
        minStructureDistance = 5f;
        minDecorationDistance = 1.5f;
        gridSize = 3f;
        
        Debug.Log($"🌲 중밀도 설정이 적용되었습니다! 나무: {treeDensity}, 바위: {rockDensity}, 장식: {decorationDensity}");
    }
    
    /// <summary>
    /// 저밀도 설정으로 변경 (여유로운 월드)
    /// </summary>
    [ContextMenu("🌿 저밀도 설정 (여유로운 월드)")]
    public void SetLowDensity()
    {
        treeDensity = 0.3f;
        rockDensity = 0.2f;
        decorationDensity = 0.4f;
        minTreeDistance = 4f;
        minRockDistance = 3f;
        minStructureDistance = 8f;
        minDecorationDistance = 2f;
        gridSize = 5f;
        
        Debug.Log($"🌿 저밀도 설정이 적용되었습니다! 나무: {treeDensity}, 바위: {rockDensity}, 장식: {decorationDensity}");
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// 월드 그리드 초기화
    /// </summary>
    private void InitializeWorldGrid()
    {
        gridWidth = Mathf.CeilToInt(worldSize.x / gridSize);
        gridHeight = Mathf.CeilToInt(worldSize.y / gridSize);
        
        // 그리드 크기 제한 (너무 크면 성능 문제)
        int maxGridSize = 100;
        if (gridWidth > maxGridSize) gridWidth = maxGridSize;
        if (gridHeight > maxGridSize) gridHeight = maxGridSize;
        
        Debug.Log($"🗺️ 그리드 초기화: {gridWidth}x{gridHeight} (월드 크기: {worldSize}, 그리드 크기: {gridSize})");
        
        worldGrid = new GridCell[gridWidth, gridHeight];
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                worldGrid[x, z] = new GridCell();
                worldGrid[x, z].worldPosition = new Vector3(
                    x * gridSize - worldSize.x * 0.5f,
                    0,
                    z * gridSize - worldSize.y * 0.5f
                );
            }
        }
    }
    
    /// <summary>
    /// 지형 분석
    /// </summary>
    private void AnalyzeTerrain()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                Vector3 worldPos = worldGrid[x, z].worldPosition;
                worldPos.y = 100f; // 높은 곳에서 시작
                
                // 지형 높이 확인
                if (Physics.Raycast(worldPos, Vector3.down, out RaycastHit hit, 200f, groundLayer))
                {
                    worldGrid[x, z].height = hit.point.y;
                    worldGrid[x, z].terrainType = ClassifyTerrain(worldGrid[x, z].height);
                }
            }
        }
    }
    
    /// <summary>
    /// 지형 타입 분류
    /// </summary>
    private TerrainType ClassifyTerrain(float height)
    {
        if (height < 0f) return TerrainType.Water;
        if (height < 2f) return TerrainType.Plain;
        if (height < 5f) return TerrainType.Forest;
        if (height < 10f) return TerrainType.Mountain;
        return TerrainType.Valley;
    }
    
    /// <summary>
    /// 월드 콘텐츠 생성
    /// </summary>
    private void GenerateWorldContent()
    {
        // 계절별 나무 생성
        GenerateSeasonalTrees();
        
        // 지형 요소 생성
        GenerateTerrainElements();
        
        // 식물 및 장식 생성
        GenerateVegetation();
        
        // 구조물 생성
        GenerateStructures();
        
        // 소품 생성
        GenerateProps();
    }
    
    /// <summary>
    /// 계절별 나무 생성
    /// </summary>
    private void GenerateSeasonalTrees()
    {
        GameObject[] selectedTrees = GetTreesForSeason(currentSeason);
        Debug.Log($"🌳 나무 생성 시작 - 선택된 나무: {selectedTrees?.Length ?? 0}개, 계절: {currentSeason}");
        
        if (selectedTrees == null || selectedTrees.Length == 0) 
        {
            Debug.LogWarning("⚠️ 나무 프리팹이 없습니다!");
            return;
        }
        
        int treeCount = Mathf.RoundToInt(gridWidth * gridHeight * treeDensity * 0.1f);
        Debug.Log($"🌳 나무 생성 목표: {treeCount}개, 밀도: {treeDensity}, 그리드: {gridWidth}x{gridHeight}");
        
        int actualTreeCount = 0;
        int attempts = 0;
        int maxAttempts = treeCount * 3; // 최대 시도 횟수
        
        while (actualTreeCount < treeCount && attempts < maxAttempts)
        {
            // 완전히 랜덤한 위치 선택
            int x = Random.Range(0, gridWidth);
            int z = Random.Range(0, gridHeight);
            
            // 기본 조건만 확인
            if (!worldGrid[x, z].isOccupied && worldGrid[x, z].terrainType != TerrainType.Water)
            {
                GameObject treePrefab = selectedTrees[Random.Range(0, selectedTrees.Length)];
                PlaceTree(new Vector2Int(x, z), treePrefab);
                actualTreeCount++;
            }
            
            attempts++;
        }
        
        Debug.Log($"🌳 나무 생성 완료 - 실제 생성: {actualTreeCount}개 / 목표: {treeCount}개 (시도: {attempts}회)");
    }
    
    /// <summary>
    /// 계절에 맞는 나무 선택
    /// </summary>
    private GameObject[] GetTreesForSeason(Season season)
    {
        switch (season)
        {
            case Season.Spring:
                return springTrees;
            case Season.Autumn:
                return autumnTrees;
            case Season.Winter:
                return winterTrees;
            case Season.Mixed:
                // 모든 계절의 나무를 섞어서 사용
                var allTrees = new List<GameObject>();
                if (springTrees != null) allTrees.AddRange(springTrees);
                if (autumnTrees != null) allTrees.AddRange(autumnTrees);
                if (winterTrees != null) allTrees.AddRange(winterTrees);
                return allTrees.ToArray();
            default:
                return springTrees;
        }
    }
    
    /// <summary>
    /// 나무 배치에 적합한 위치 찾기
    /// </summary>
    private Vector2Int FindSuitableTreePosition()
    {
        List<Vector2Int> suitablePositions = new List<Vector2Int>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (CanPlaceTree(x, z))
                {
                    suitablePositions.Add(new Vector2Int(x, z));
                }
            }
        }
        
        if (suitablePositions.Count > 0)
        {
            return suitablePositions[Random.Range(0, suitablePositions.Count)];
        }
        
        return Vector2Int.zero;
    }
    
    /// <summary>
    /// 나무 배치 가능 여부 확인
    /// </summary>
    private bool CanPlaceTree(int x, int z)
    {
        if (worldGrid[x, z].isOccupied) return false;
        if (worldGrid[x, z].terrainType == TerrainType.Water) return false;
        
        // 다른 나무와의 거리 확인 (더 세밀하게)
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;
                
                if (nx >= 0 && nx < gridWidth && nz >= 0 && nz < gridHeight)
                {
                    if (worldGrid[nx, z].isOccupied || worldGrid[x, nz].isOccupied)
                    {
                        float distance = Mathf.Sqrt(dx * dx + dz * dz) * gridSize;
                        if (distance < minTreeDistance) return false;
                    }
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 나무 배치
    /// </summary>
    private void PlaceTree(Vector2Int gridPos, GameObject treePrefab)
    {
        try
        {
            Vector3 worldPos = worldGrid[gridPos.x, gridPos.y].worldPosition;
            
            // 지형에 맞춰 높이 조정
            if (Physics.Raycast(worldPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                worldPos.y = hit.point.y;
            }
            
            GameObject tree = Instantiate(treePrefab, worldPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
            tree.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
            tree.name = $"Tree_{generatedObjects.Count} (Generated)";
            
            // 부모 오브젝트의 자식으로 설정
            if (treesParent != null)
            {
                tree.transform.SetParent(treesParent.transform);
            }
            
            // 그리드 셀에 등록
            worldGrid[gridPos.x, gridPos.y].isOccupied = true;
            worldGrid[gridPos.x, gridPos.y].objects.Add(tree);
            
            generatedObjects.Add(tree);
            
            Debug.Log($"✅ 나무 배치 성공: {tree.name} at {worldPos}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 나무 배치 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 지형 요소 생성
    /// </summary>
    private void GenerateTerrainElements()
    {
        // 바위 생성
        if (rocks != null && rocks.Length > 0)
        {
            GenerateRocks();
        }
        
        // 통나무 생성
        if (trunks != null && trunks.Length > 0)
        {
            GenerateTrunks();
        }
    }
    
    /// <summary>
    /// 바위 생성
    /// </summary>
    private void GenerateRocks()
    {
        int rockCount = Mathf.RoundToInt(gridWidth * gridHeight * rockDensity * 0.05f);
        Debug.Log($"🪨 바위 생성 시작 - 목표: {rockCount}개, 밀도: {rockDensity}, 그리드: {gridWidth}x{gridHeight}");
        
        int actualRockCount = 0;
        int attempts = 0;
        int maxAttempts = rockCount * 3; // 최대 시도 횟수
        
        while (actualRockCount < rockCount && attempts < maxAttempts)
        {
            // 완전히 랜덤한 위치 선택
            int x = Random.Range(0, gridWidth);
            int z = Random.Range(0, gridHeight);
            
            // 기본 조건만 확인
            if (!worldGrid[x, z].isOccupied && worldGrid[x, z].terrainType != TerrainType.Water)
            {
                GameObject rockPrefab = rocks[Random.Range(0, rocks.Length)];
                PlaceRock(new Vector2Int(x, z), rockPrefab);
                actualRockCount++;
            }
            
            attempts++;
        }
        
        Debug.Log($"🪨 바위 생성 완료 - 실제 생성: {actualRockCount}개 / 목표: {rockCount}개 (시도: {attempts}회)");
    }
    
    /// <summary>
    /// 바위 배치에 적합한 위치 찾기
    /// </summary>
    private Vector2Int FindSuitableRockPosition()
    {
        List<Vector2Int> suitablePositions = new List<Vector2Int>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (CanPlaceRock(x, z))
                {
                    suitablePositions.Add(new Vector2Int(x, z));
                }
            }
        }
        
        if (suitablePositions.Count > 0)
        {
            return suitablePositions[Random.Range(0, suitablePositions.Count)];
        }
        
        Debug.LogWarning($"⚠️ 바위 배치 가능한 위치를 찾을 수 없습니다! 그리드: {gridWidth}x{gridHeight}, 적합한 위치: {suitablePositions.Count}개");
        return Vector2Int.zero;
    }
    
    /// <summary>
    /// 바위 배치 가능 여부 확인
    /// </summary>
    private bool CanPlaceRock(int x, int z)
    {
        // 기본 조건만 확인 (거리 조건 완화)
        if (worldGrid[x, z].isOccupied) return false;
        if (worldGrid[x, z].terrainType == TerrainType.Water) return false;
        
        // 간단한 랜덤 배치 (70% 확률로 배치)
        return Random.value < 0.7f;
    }
    
    /// <summary>
    /// 바위 배치
    /// </summary>
    private void PlaceRock(Vector2Int gridPos, GameObject rockPrefab)
    {
        try
        {
            Vector3 worldPos = worldGrid[gridPos.x, gridPos.y].worldPosition;
            
            // 지형에 맞춰 높이 조정
            if (Physics.Raycast(worldPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                worldPos.y = hit.point.y;
            }
            
            GameObject rock = Instantiate(rockPrefab, worldPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
            rock.transform.localScale = Vector3.one * Random.Range(0.7f, 1.3f);
            rock.name = $"Rock_{generatedObjects.Count} (Generated)";
            
            // 부모 오브젝트의 자식으로 설정
            if (rocksParent != null)
            {
                rock.transform.SetParent(rocksParent.transform);
            }
            
            // 그리드 셀에 등록
            worldGrid[gridPos.x, gridPos.y].isOccupied = true;
            worldGrid[gridPos.x, gridPos.y].objects.Add(rock);
            
            generatedObjects.Add(rock);
            
            Debug.Log($"✅ 바위 배치 성공: {rock.name} at {worldPos}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 바위 배치 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 통나무 생성
    /// </summary>
    private void GenerateTrunks()
    {
        int trunkCount = Mathf.RoundToInt(gridWidth * gridHeight * 0.02f);
        
        for (int i = 0; i < trunkCount; i++)
        {
            Vector2Int gridPos = FindSuitableTrunkPosition();
            if (gridPos != Vector2Int.zero)
            {
                GameObject trunkPrefab = trunks[Random.Range(0, trunks.Length)];
                PlaceTrunk(gridPos, trunkPrefab);
            }
        }
    }
    
    /// <summary>
    /// 통나무 배치에 적합한 위치 찾기
    /// </summary>
    private Vector2Int FindSuitableTrunkPosition()
    {
        List<Vector2Int> suitablePositions = new List<Vector2Int>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (CanPlaceTrunk(x, z))
                {
                    suitablePositions.Add(new Vector2Int(x, z));
                }
            }
        }
        
        if (suitablePositions.Count > 0)
        {
            return suitablePositions[Random.Range(0, suitablePositions.Count)];
        }
        
        return Vector2Int.zero;
    }
    
    /// <summary>
    /// 통나무 배치 가능 여부 확인
    /// </summary>
    private bool CanPlaceTrunk(int x, int z)
    {
        if (worldGrid[x, z].isOccupied) return false;
        if (worldGrid[x, z].terrainType == TerrainType.Water) return false;
        
        return true;
    }
    
    /// <summary>
    /// 통나무 배치
    /// </summary>
    private void PlaceTrunk(Vector2Int gridPos, GameObject trunkPrefab)
    {
        Vector3 worldPos = worldGrid[gridPos.x, gridPos.y].worldPosition;
        
        // 지형에 맞춰 높이 조정
        if (Physics.Raycast(worldPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            worldPos.y = hit.point.y;
        }
        
        GameObject trunk = Instantiate(trunkPrefab, worldPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        
        // 부모 오브젝트의 자식으로 설정
        if (rocksParent != null)
        {
            trunk.transform.SetParent(rocksParent.transform);
        }
        
        // 그리드 셀에 등록
        worldGrid[gridPos.x, gridPos.y].isOccupied = true;
        worldGrid[gridPos.x, gridPos.y].objects.Add(trunk);
        
        generatedObjects.Add(trunk);
    }
    
    /// <summary>
    /// 식물 및 장식 생성
    /// </summary>
    private void GenerateVegetation()
    {
        // 관목 생성
        if (bushes != null && bushes.Length > 0)
        {
            GenerateBushes();
        }
        
        // 대나무 생성
        if (bamboos != null && bamboos.Length > 0)
        {
            GenerateBamboos();
        }
        
        // 버섯 생성
        if (mushrooms != null && mushrooms.Length > 0)
        {
            GenerateMushrooms();
        }
    }
    
    /// <summary>
    /// 관목 생성
    /// </summary>
    private void GenerateBushes()
    {
        int bushCount = Mathf.RoundToInt(gridWidth * gridHeight * decorationDensity * 0.1f);
        Debug.Log($"🌿 관목 생성 시작 - 목표: {bushCount}개, 밀도: {decorationDensity}, 그리드: {gridWidth}x{gridHeight}");
        
        int actualBushCount = 0;
        int attempts = 0;
        int maxAttempts = bushCount * 3; // 최대 시도 횟수
        
        while (actualBushCount < bushCount && attempts < maxAttempts)
        {
            // 완전히 랜덤한 위치 선택
            int x = Random.Range(0, gridWidth);
            int z = Random.Range(0, gridHeight);
            
            // 기본 조건만 확인
            if (!worldGrid[x, z].isOccupied && worldGrid[x, z].terrainType != TerrainType.Water)
            {
                GameObject bushPrefab = bushes[Random.Range(0, bushes.Length)];
                PlaceDecoration(new Vector2Int(x, z), bushPrefab);
                actualBushCount++;
            }
            
            attempts++;
        }
        
        Debug.Log($"🌿 관목 생성 완료 - 실제 생성: {actualBushCount}개 / 목표: {bushCount}개 (시도: {attempts}회)");
    }
    
    /// <summary>
    /// 대나무 생성
    /// </summary>
    private void GenerateBamboos()
    {
        int bambooCount = Mathf.RoundToInt(gridWidth * gridHeight * 0.03f);
        
        for (int i = 0; i < bambooCount; i++)
        {
            Vector2Int gridPos = FindSuitableDecorationPosition();
            if (gridPos != Vector2Int.zero)
            {
                GameObject bambooPrefab = bamboos[Random.Range(0, bamboos.Length)];
                PlaceDecoration(gridPos, bambooPrefab);
            }
        }
    }
    
    /// <summary>
    /// 버섯 생성
    /// </summary>
    private void GenerateMushrooms()
    {
        int mushroomCount = Mathf.RoundToInt(gridWidth * gridHeight * 0.05f);
        
        for (int i = 0; i < mushroomCount; i++)
        {
            Vector2Int gridPos = FindSuitableDecorationPosition();
            if (gridPos != Vector2Int.zero)
            {
                GameObject mushroomPrefab = mushrooms[Random.Range(0, mushrooms.Length)];
                PlaceDecoration(gridPos, mushroomPrefab);
            }
        }
    }
    
    /// <summary>
    /// 장식 배치에 적합한 위치 찾기
    /// </summary>
    private Vector2Int FindSuitableDecorationPosition()
    {
        List<Vector2Int> suitablePositions = new List<Vector2Int>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (CanPlaceDecoration(x, z))
                {
                    suitablePositions.Add(new Vector2Int(x, z));
                }
            }
        }
        
        if (suitablePositions.Count > 0)
        {
            return suitablePositions[Random.Range(0, suitablePositions.Count)];
        }
        
        Debug.LogWarning($"⚠️ 장식 배치 가능한 위치를 찾을 수 없습니다! 그리드: {gridWidth}x{gridHeight}, 적합한 위치: {suitablePositions.Count}개");
        return Vector2Int.zero;
    }
    
    /// <summary>
    /// 장식 배치 가능 여부 확인
    /// </summary>
    private bool CanPlaceDecoration(int x, int z)
    {
        // 기본 조건만 확인
        if (worldGrid[x, z].isOccupied) return false;
        if (worldGrid[x, z].terrainType == TerrainType.Water) return false;
        
        // 간단한 랜덤 배치 (80% 확률로 배치)
        return Random.value < 0.8f;
    }
    
    /// <summary>
    /// 장식 배치
    /// </summary>
    private void PlaceDecoration(Vector2Int gridPos, GameObject decorationPrefab)
    {
        try
        {
            Vector3 worldPos = worldGrid[gridPos.x, gridPos.y].worldPosition;
            
            // 지형에 맞춰 높이 조정
            if (Physics.Raycast(worldPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                worldPos.y = hit.point.y;
            }
            
            GameObject decoration = Instantiate(decorationPrefab, worldPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
            decoration.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
            decoration.name = $"Decoration_{generatedObjects.Count} (Generated)";
            
            // 부모 오브젝트의 자식으로 설정
            if (vegetationParent != null)
            {
                decoration.transform.SetParent(vegetationParent.transform);
            }
            
            // 그리드 셀에 등록
            worldGrid[gridPos.x, gridPos.y].isOccupied = true;
            worldGrid[gridPos.x, gridPos.y].objects.Add(decoration);
            
            generatedObjects.Add(decoration);
            
            Debug.Log($"✅ 장식 배치 성공: {decoration.name} at {worldPos}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 장식 배치 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 구조물 생성
    /// </summary>
    private void GenerateStructures()
    {
        // 울타리 생성
        if (woodenFences != null && woodenFences.Length > 0)
        {
            GenerateFences();
        }
        
        // 가구 생성
        if (furniture != null && furniture.Length > 0)
        {
            GenerateFurniture();
        }
    }
    
    /// <summary>
    /// 울타리 생성
    /// </summary>
    private void GenerateFences()
    {
        int fenceCount = Mathf.RoundToInt(gridWidth * gridHeight * 0.02f);
        
        for (int i = 0; i < fenceCount; i++)
        {
            Vector2Int gridPos = FindSuitableStructurePosition();
            if (gridPos != Vector2Int.zero)
            {
                GameObject fencePrefab = woodenFences[Random.Range(0, woodenFences.Length)];
                PlaceStructure(gridPos, fencePrefab);
            }
        }
    }
    
    /// <summary>
    /// 가구 생성
    /// </summary>
    private void GenerateFurniture()
    {
        int furnitureCount = Mathf.RoundToInt(gridWidth * gridHeight * 0.01f);
        
        for (int i = 0; i < furnitureCount; i++)
        {
            Vector2Int gridPos = FindSuitableStructurePosition();
            if (gridPos != Vector2Int.zero)
            {
                GameObject furniturePrefab = furniture[Random.Range(0, furniture.Length)];
                PlaceStructure(gridPos, furniturePrefab);
            }
        }
    }
    
    /// <summary>
    /// 구조물 배치에 적합한 위치 찾기
    /// </summary>
    private Vector2Int FindSuitableStructurePosition()
    {
        List<Vector2Int> suitablePositions = new List<Vector2Int>();
        
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (CanPlaceStructure(x, z))
                {
                    suitablePositions.Add(new Vector2Int(x, z));
                }
            }
        }
        
        if (suitablePositions.Count > 0)
        {
            return suitablePositions[Random.Range(0, suitablePositions.Count)];
        }
        
        return Vector2Int.zero;
    }
    
    /// <summary>
    /// 구조물 배치 가능 여부 확인
    /// </summary>
    private bool CanPlaceStructure(int x, int z)
    {
        if (worldGrid[x, z].isOccupied) return false;
        if (worldGrid[x, z].terrainType == TerrainType.Water) return false;
        if (worldGrid[x, z].terrainType == TerrainType.Mountain) return false;
        
        // 다른 구조물과의 거리 확인
        for (int dx = -3; dx <= 3; dx++)
        {
            for (int dz = -3; dz <= 3; dz++)
            {
                int nx = x + dx;
                int nz = z + dz;
                
                if (nx >= 0 && nx < gridWidth && nz >= 0 && nz < gridHeight)
                {
                    if (worldGrid[nx, nz].isOccupied)
                    {
                        float distance = Mathf.Sqrt(dx * dx + dz * dz) * gridSize;
                        if (distance < minStructureDistance) return false;
                    }
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 구조물 배치
    /// </summary>
    private void PlaceStructure(Vector2Int gridPos, GameObject structurePrefab)
    {
        Vector3 worldPos = worldGrid[gridPos.x, gridPos.y].worldPosition;
        
        // 지형에 맞춰 높이 조정
        if (Physics.Raycast(worldPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            worldPos.y = hit.point.y;
        }
        
        GameObject structure = Instantiate(structurePrefab, worldPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
        
        // 부모 오브젝트의 자식으로 설정
        if (structuresParent != null)
        {
            structure.transform.SetParent(structuresParent.transform);
        }
        
        // 그리드 셀에 등록
        worldGrid[gridPos.x, gridPos.y].isOccupied = true;
        worldGrid[gridPos.x, gridPos.y].objects.Add(structure);
        
        generatedObjects.Add(structure);
    }
    
    /// <summary>
    /// 소품 생성
    /// </summary>
    private void GenerateProps()
    {
        Debug.Log($"🎭 Props 생성 시작 - 사용 가능한 Props: {props?.Length ?? 0}개");
        
        if (props == null || props.Length == 0) 
        {
            Debug.LogWarning("⚠️ Props 프리팹이 없습니다!");
            return;
        }
        
        int propCount = Mathf.RoundToInt(gridWidth * gridHeight * 0.05f); // 0.01f → 0.05f로 증가
        Debug.Log($"🎭 Props 생성 목표: {propCount}개, 그리드: {gridWidth}x{gridHeight}");
        
        int actualPropCount = 0;
        int attempts = 0;
        int maxAttempts = propCount * 3; // 최대 시도 횟수
        
        while (actualPropCount < propCount && attempts < maxAttempts)
        {
            // 완전히 랜덤한 위치 선택
            int x = Random.Range(0, gridWidth);
            int z = Random.Range(0, gridHeight);
            
            // 기본 조건만 확인
            if (!worldGrid[x, z].isOccupied && worldGrid[x, z].terrainType != TerrainType.Water)
            {
                GameObject propPrefab = props[Random.Range(0, props.Length)];
                PlaceProp(new Vector2Int(x, z), propPrefab);
                actualPropCount++;
            }
            
            attempts++;
        }
        
        Debug.Log($"🎭 Props 생성 완료 - 실제 생성: {actualPropCount}개 / 목표: {propCount}개 (시도: {attempts}회)");
    }
    
    /// <summary>
    /// Props 배치
    /// </summary>
    private void PlaceProp(Vector2Int gridPos, GameObject propPrefab)
    {
        try
        {
            Vector3 worldPos = worldGrid[gridPos.x, gridPos.y].worldPosition;
            
            // 지형에 맞춰 높이 조정
            if (Physics.Raycast(worldPos + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f, groundLayer))
            {
                worldPos.y = hit.point.y;
            }
            
            GameObject prop = Instantiate(propPrefab, worldPos, Quaternion.Euler(0, Random.Range(0, 360), 0));
            prop.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
            prop.name = $"Prop_{generatedObjects.Count} (Generated)";
            
            // 부모 오브젝트의 자식으로 설정
            if (propsParent != null)
            {
                prop.transform.SetParent(propsParent.transform);
            }
            
            // 그리드 셀에 등록
            worldGrid[gridPos.x, gridPos.y].isOccupied = true;
            worldGrid[gridPos.x, gridPos.y].objects.Add(prop);
            
            generatedObjects.Add(prop);
            
            Debug.Log($"✅ Props 배치 성공: {prop.name} at {worldPos}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Props 배치 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 숲 생태계 생성
    /// </summary>
    private void GenerateForestArea()
    {
        // 숲 지역에 집중적으로 나무와 관목 배치
        int centerX = gridWidth / 2;
        int centerZ = gridHeight / 2;
        int forestRadius = Mathf.Min(gridWidth, gridHeight) / 3; // 1/4 → 1/3으로 증가
        
        for (int x = centerX - forestRadius; x <= centerX + forestRadius; x++)
        {
            for (int z = centerZ - forestRadius; z <= centerZ + forestRadius; z++)
            {
                if (x >= 0 && x < gridWidth && z >= 0 && z < gridHeight)
                {
                    float distance = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));
                    if (distance <= forestRadius)
                    {
                        // 숲 밀도 대폭 증가 (0.3 → 0.6)
                        if (Random.value < 0.6f && !worldGrid[x, z].isOccupied)
                        {
                            GameObject[] trees = GetTreesForSeason(currentSeason);
                            if (trees != null && trees.Length > 0)
                            {
                                GameObject treePrefab = trees[Random.Range(0, trees.Length)];
                                PlaceTree(new Vector2Int(x, z), treePrefab);
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log("🌲 숲 생태계가 생성되었습니다!");
    }
    
    /// <summary>
    /// 산악 생태계 생성
    /// </summary>
    private void GenerateMountainArea()
    {
        // 높은 지형에 바위와 통나무 집중 배치
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (worldGrid[x, z].terrainType == TerrainType.Mountain && !worldGrid[x, z].isOccupied)
                {
                    if (Random.value < 0.4f)
                    {
                        if (rocks != null && rocks.Length > 0)
                        {
                            GameObject rockPrefab = rocks[Random.Range(0, rocks.Length)];
                            PlaceRock(new Vector2Int(x, z), rockPrefab);
                        }
                    }
                    else if (Random.value < 0.3f)
                    {
                        if (trunks != null && trunks.Length > 0)
                        {
                            GameObject trunkPrefab = trunks[Random.Range(0, trunks.Length)];
                            PlaceTrunk(new Vector2Int(x, z), trunkPrefab);
                        }
                    }
                }
            }
        }
        
        Debug.Log("🏔️ 산악 생태계가 생성되었습니다!");
    }
    
    /// <summary>
    /// 초원 생태계 생성
    /// </summary>
    private void GeneratePlainArea()
    {
        // 평평한 지형에 관목과 소품 집중 배치
        for (int x = 0; x < gridWidth; x++)
        {
            for (int z = 0; z < gridHeight; z++)
            {
                if (worldGrid[x, z].terrainType == TerrainType.Plain && !worldGrid[x, z].isOccupied)
                {
                    if (Random.value < 0.3f)
                    {
                        if (bushes != null && bushes.Length > 0)
                        {
                            GameObject bushPrefab = bushes[Random.Range(0, bushes.Length)];
                            PlaceDecoration(new Vector2Int(x, z), bushPrefab);
                        }
                    }
                    else if (Random.value < 0.2f)
                    {
                        if (mushrooms != null && mushrooms.Length > 0)
                        {
                            GameObject mushroomPrefab = mushrooms[Random.Range(0, mushrooms.Length)];
                            PlaceDecoration(new Vector2Int(x, z), mushroomPrefab);
                        }
                    }
                }
            }
        }
        
        Debug.Log("🌾 초원 생태계가 생성되었습니다!");
    }
    
    #endregion
    
    #region 프리팹 로드 메서드
    
    /// <summary>
    /// 패턴에 맞는 프리팹들을 자동으로 로드
    /// </summary>
    private GameObject[] LoadPrefabsByPattern(params string[] patterns)
    {
        List<GameObject> loadedPrefabs = new List<GameObject>();
        
        #if UNITY_EDITOR
        // 지정된 경로에서 모든 프리팹 파일 찾기
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { prefabPath });
        
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            
            if (prefab != null)
            {
                // 패턴과 일치하는지 확인
                foreach (string pattern in patterns)
                {
                    if (prefab.name.StartsWith(pattern))
                    {
                        loadedPrefabs.Add(prefab);
                        break;
                    }
                }
            }
        }
        
        // 이름 순으로 정렬
        loadedPrefabs.Sort((a, b) => string.Compare(a.name, b.name));
        #endif
        
        return loadedPrefabs.ToArray();
    }
    
    /// <summary>
    /// 프리팹 경로 변경 시 자동으로 프리팹들 다시 로드
    /// </summary>
    private void OnValidate()
    {
        if (autoLoadPrefabs)
        {
            autoLoadPrefabs = false; // 무한 루프 방지
            AutoLoadAllPrefabs();
        }
    }
    
    #endregion
}
