using UnityEngine;
using UnityEditor;

/// <summary>
/// LowpolyNatureWorldBuilder에 Inspector 버튼을 추가하는 커스텀 에디터
/// </summary>
[CustomEditor(typeof(LowpolyNatureWorldBuilder))]
public class LowpolyNatureWorldBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();
        
        // 구분선 추가
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🌍 스마트 월드 생성 도구", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 계절별 월드 생성 버튼들
        EditorGUILayout.LabelField("🌸 계절별 테마", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🌸 봄 테마 월드 생성", GUILayout.Height(30)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.GenerateSpringWorld();
        }
        
        if (GUILayout.Button("🍂 가을 테마 월드 생성", GUILayout.Height(30)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.GenerateAutumnWorld();
        }
        
        if (GUILayout.Button("❄️ 겨울 테마 월드 생성", GUILayout.Height(30)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.GenerateWinterWorld();
        }
        
        if (GUILayout.Button("🌈 혼합 테마 월드 생성", GUILayout.Height(30)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.GenerateMixedWorld();
        }
        
        EditorGUILayout.Space();
        
        // 생태계별 생성 버튼들
        EditorGUILayout.LabelField("🌲 생태계별 생성", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🌲 숲 생태계 생성", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.GenerateForestEcosystem();
        }
        
        if (GUILayout.Button("🏔️ 산악 생태계 생성", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.GenerateMountainEcosystem();
        }
        
        if (GUILayout.Button("🌾 초원 생태계 생성", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.GeneratePlainEcosystem();
        }
        
        EditorGUILayout.Space();
        
        // 유틸리티 버튼들
        EditorGUILayout.LabelField("🔧 유틸리티", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🌍 스마트 월드 생성", GUILayout.Height(30)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.GenerateSmartWorld();
        }
        
        if (GUILayout.Button("🗑️ 월드 정리", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.ClearWorld();
        }
        
        if (GUILayout.Button("⚡ 월드 최적화", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.OptimizeWorld();
        }
        
        EditorGUILayout.Space();
        
        // 밀도 설정 버튼들
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🌳 밀도 설정", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🌳 고밀도 설정 (풍성한 월드)", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.SetHighDensity();
        }
        
        if (GUILayout.Button("🌲 중밀도 설정 (균형잡힌 월드)", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.SetMediumDensity();
        }
        
        if (GUILayout.Button("🌿 저밀도 설정 (여유로운 월드)", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.SetLowDensity();
        }
        
        EditorGUILayout.Space();
        
        // 부모 오브젝트 관리 버튼들
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("📁 부모 오브젝트 관리", EditorStyles.boldLabel);
        
        if (GUILayout.Button("📁 부모 오브젝트 자동 생성", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.CreateParentObjects();
        }
        
        if (GUILayout.Button("🗑️ 부모 오브젝트 정리", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.ClearParentObjects();
        }
        
        EditorGUILayout.Space();
        
        // 도움말
        EditorGUILayout.HelpBox(
            "🔧 자동 프리팹 등록 버튼을 클릭하면 프리팹들이 자동으로 할당됩니다!\n" +
            "🌳 밀도 설정으로 월드의 풍성함을 조절할 수 있습니다!\n" +
            "📁 부모 오브젝트 관리로 하이어라키를 깔끔하게 유지할 수 있습니다!\n" +
            "🌸 계절별 테마: 봄/가을/겨울/혼합\n" +
            "🌲 생태계별: 숲/산악/초원\n" +
            "🔧 유틸리티: 전체 생성/정리/최적화", 
            MessageType.Info
        );
        
        // 자동 프리팹 등록 버튼들
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🔧 자동 프리팹 등록", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🔧 모든 프리팹 자동 등록", GUILayout.Height(30)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.AutoLoadAllPrefabs();
        }
        
        EditorGUILayout.Space();
        
        // 계절별 자동 등록
        EditorGUILayout.LabelField("🌸 계절별 자동 등록", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🌸 봄 나무들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadSpringTrees();
        }
        
        if (GUILayout.Button("🍂 가을 나무들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadAutumnTrees();
        }
        
        if (GUILayout.Button("❄️ 겨울 나무들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadWinterTrees();
        }
        
        EditorGUILayout.Space();
        
        // 지형 요소 자동 등록
        EditorGUILayout.LabelField("🏔️ 지형 요소 자동 등록", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🏔️ 바위들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadRocks();
        }
        
        if (GUILayout.Button("🪵 통나무들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadTrunks();
        }
        
        if (GUILayout.Button("🌳 쓰러진 나무들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadFallenTrees();
        }
        
        EditorGUILayout.Space();
        
        // 식물 및 장식 자동 등록
        EditorGUILayout.LabelField("🌿 식물 및 장식 자동 등록", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🌿 관목들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadBushes();
        }
        
        if (GUILayout.Button("🎋 대나무들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadBamboos();
        }
        
        if (GUILayout.Button("🍄 버섯들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadMushrooms();
        }
        
        if (GUILayout.Button("🪴 화분 식물들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadPottedPlants();
        }
        
        EditorGUILayout.Space();
        
        // 구조물 및 소품 자동 등록
        EditorGUILayout.LabelField("🏗️ 구조물 및 소품 자동 등록", EditorStyles.boldLabel);
        
        if (GUILayout.Button("🪵 나무 울타리들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadWoodenFences();
        }
        
        if (GUILayout.Button("🧱 벽돌 벽들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadBrickWalls();
        }
        
        if (GUILayout.Button("🪑 가구들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadFurniture();
        }
        
        if (GUILayout.Button("🎭 소품들 자동 등록", GUILayout.Height(25)))
        {
            LowpolyNatureWorldBuilder worldBuilder = (LowpolyNatureWorldBuilder)target;
            worldBuilder.LoadProps();
        }
        
        EditorGUILayout.Space();
        
        // 프리팹 할당 가이드
        EditorGUILayout.LabelField("📋 프리팹 할당 가이드", EditorStyles.boldLabel);
        
        if (GUILayout.Button("📋 프리팹 할당 가이드 보기", GUILayout.Height(25)))
        {
            ShowPrefabAssignmentGuide();
        }
    }
    
    /// <summary>
    /// 프리팹 할당 가이드를 보여주는 창
    /// </summary>
    private void ShowPrefabAssignmentGuide()
    {
        string guideText = 
            "🌱 계절별 나무:\n" +
            "• Spring Trees: SpringTree_01 ~ SpringTree_07\n" +
            "• Autumn Trees: AutumnTree_01 ~ AutumnTree_06\n" +
            "• Winter Trees: WinterTree_01 ~ WinterTree_06\n\n" +
            
            "🏔️ 지형 요소:\n" +
            "• Rocks: Rock_01 ~ Rock_06\n" +
            "• Trunks: Trunk_01 ~ Trunk_03\n" +
            "• Fallen Trees: FallenTree_01 ~ FallenTree_04\n\n" +
            
            "🌿 식물 및 장식:\n" +
            "• Bushes: Bush_01 ~ Bush_04\n" +
            "• Bamboos: Bamboo_01 ~ Bamboo_03\n" +
            "• Mushrooms: Mushroom_01 ~ Mushroom_16\n" +
            "• Potted Plants: PottedPlants_01\n\n" +
            
            "🏗️ 구조물 및 소품:\n" +
            "• Wooden Fences: WoodenFence_01 ~ WoodenFence_03\n" +
            "• Brick Walls: BrickWall_01 ~ BrickWall_02\n" +
            "• Furniture: WoodenTable_01, WoodenStool_01\n" +
            "• Props: BonFire_01, WateringKettle_01\n\n" +
            
            "📍 경로: Assets/LowpolyNatureBundle/Prefab";
        
        EditorUtility.DisplayDialog("📋 프리팹 할당 가이드", guideText, "확인");
    }
}
