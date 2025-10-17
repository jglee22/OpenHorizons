using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class LODGeneratorWindow : EditorWindow
{
    [Header("LOD Settings")]
    public float lod1Ratio = 0.4f;
    public float lod2Ratio = 0.15f;
    public float lod3Ratio = 0.05f;
    
    [Header("Transition Heights")]
    public float lod1Transition = 0.6f;
    public float lod2Transition = 0.3f;
    public float lod3Transition = 0.05f;
    
    [Header("Mesh Simplifier Settings")]
    public bool preserveBorders = true;
    public bool recalculateNormals = true;
    public bool preserveUVs = true;
    public bool preserveUV2s = false;
    public bool preserveUV3s = false;
    public bool preserveUV4s = false;
    public bool preserveColors = false;
    public bool preserveBlendShapes = false;
    public bool enableSmartLink = true;
    public int maxIterations = 100;
    
    [Header("Target Objects")]
    public List<GameObject> targetObjects = new List<GameObject>();
    
    [Header("Output Settings")]
    public string outputFolder = "Assets/LOD_Meshes";
    public bool overwriteExisting = false;
    
    private Vector2 scrollPosition;
    private bool showAdvancedSettings = false;
    
    [MenuItem("Tools/Mobile Optimization/LOD Generator")]
    public static void ShowWindow()
    {
        LODGeneratorWindow window = GetWindow<LODGeneratorWindow>("LOD Generator");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }
    
    private void OnGUI()
    {
        // 예외 발생 시에도 EndScrollView가 보장되도록 try/finally 사용
        EditorGUILayout.BeginHorizontal(); // 균형 보호용 최소 한쌍
        EditorGUILayout.EndHorizontal();
        
        try
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            EditorGUILayout.LabelField("LOD Generator", EditorStyles.boldLabel);
            EditorGUILayout.Space();
        
        // LOD Settings
        EditorGUILayout.LabelField("LOD Settings", EditorStyles.boldLabel);
        lod1Ratio = EditorGUILayout.Slider("LOD1 Ratio", lod1Ratio, 0.1f, 0.8f);
        lod2Ratio = EditorGUILayout.Slider("LOD2 Ratio", lod2Ratio, 0.05f, 0.3f);
        lod3Ratio = EditorGUILayout.Slider("LOD3 Ratio", lod3Ratio, 0.01f, 0.1f);
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Transition Heights", EditorStyles.boldLabel);
        lod1Transition = EditorGUILayout.Slider("LOD1 Transition", lod1Transition, 0.3f, 0.8f);
        lod2Transition = EditorGUILayout.Slider("LOD2 Transition", lod2Transition, 0.1f, 0.5f);
        lod3Transition = EditorGUILayout.Slider("LOD3 Transition", lod3Transition, 0.01f, 0.2f);
        
        EditorGUILayout.Space();
        
        // Target Objects
        EditorGUILayout.LabelField("Target Objects", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("씬에서 선택한 오브젝트들이 자동으로 추가됩니다.", MessageType.Info);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Selected Objects"))
        {
            AddSelectedObjects();
        }
        if (GUILayout.Button("Clear All"))
        {
            targetObjects.Clear();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Display target objects
        if (targetObjects.Count > 0)
        {
            EditorGUILayout.LabelField($"Target Objects ({targetObjects.Count})", EditorStyles.boldLabel);
            for (int i = 0; i < targetObjects.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                targetObjects[i] = (GameObject)EditorGUILayout.ObjectField(targetObjects[i], typeof(GameObject), true);
                if (GUILayout.Button("Remove", GUILayout.Width(60)))
                {
                    targetObjects.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        
        EditorGUILayout.Space();
        
        // Advanced Settings
        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "Advanced Settings");
        if (showAdvancedSettings)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Mesh Simplifier Settings", EditorStyles.boldLabel);
            preserveBorders = EditorGUILayout.Toggle("Preserve Borders", preserveBorders);
            recalculateNormals = EditorGUILayout.Toggle("Recalculate Normals", recalculateNormals);
            preserveUVs = EditorGUILayout.Toggle("Preserve UVs", preserveUVs);
            preserveUV2s = EditorGUILayout.Toggle("Preserve UV2s", preserveUV2s);
            preserveUV3s = EditorGUILayout.Toggle("Preserve UV3s", preserveUV3s);
            preserveUV4s = EditorGUILayout.Toggle("Preserve UV4s", preserveUV4s);
            preserveColors = EditorGUILayout.Toggle("Preserve Colors", preserveColors);
            preserveBlendShapes = EditorGUILayout.Toggle("Preserve Blend Shapes", preserveBlendShapes);
            enableSmartLink = EditorGUILayout.Toggle("Enable Smart Link", enableSmartLink);
            maxIterations = EditorGUILayout.IntField("Max Iterations", maxIterations);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Output Settings", EditorStyles.boldLabel);
            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);
            overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.Space();
        
        // Generate Button
        EditorGUI.BeginDisabledGroup(targetObjects.Count == 0);
        if (GUILayout.Button("Generate LODs", GUILayout.Height(30)))
        {
            GenerateLODs();
        }
        EditorGUI.EndDisabledGroup();
        
        if (targetObjects.Count == 0)
        {
            EditorGUILayout.HelpBox("최소 하나의 타겟 오브젝트를 선택해주세요.", MessageType.Warning);
        }
        
        EditorGUILayout.Space();
        
        // Help
        EditorGUILayout.LabelField("사용법", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("1. 씬에서 LOD를 적용할 오브젝트들을 선택\n2. 'Add Selected Objects' 버튼 클릭\n3. 필요시 설정값 조정\n4. 'Generate LODs' 버튼 클릭", MessageType.Info);
        
        }
        finally
        {
            // BeginScrollView가 성공했을 때만 EndScrollView 호출 시도
            try { EditorGUILayout.EndScrollView(); } catch { }
        }
    }
    
    private void AddSelectedObjects()
    {
        GameObject[] selected = Selection.gameObjects;
        foreach (GameObject obj in selected)
        {
            if (!targetObjects.Contains(obj))
            {
                // MeshRenderer가 있는 오브젝트만 추가
                if (obj.GetComponent<MeshRenderer>() != null)
                {
                    targetObjects.Add(obj);
                }
            }
        }
        
        if (selected.Length == 0)
        {
            EditorUtility.DisplayDialog("경고", "선택된 오브젝트가 없습니다.", "확인");
        }
    }
    
    private void GenerateLODs()
    {
        if (targetObjects.Count == 0)
        {
            EditorUtility.DisplayDialog("오류", "타겟 오브젝트가 없습니다.", "확인");
            return;
        }
        
        // 출력 폴더 생성
        if (!AssetDatabase.IsValidFolder(outputFolder))
        {
            string[] folders = outputFolder.Split('/');
            string currentPath = folders[0];
            
            for (int i = 1; i < folders.Length; i++)
            {
                string newPath = currentPath + "/" + folders[i];
                if (!AssetDatabase.IsValidFolder(newPath))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath = newPath;
            }
        }
        
        int successCount = 0;
        int totalCount = targetObjects.Count;
        
        for (int i = 0; i < targetObjects.Count; i++)
        {
            GameObject obj = targetObjects[i];
            if (obj == null) continue;
            
            EditorUtility.DisplayProgressBar("LOD 생성 중", $"처리 중: {obj.name} ({i + 1}/{totalCount})", (float)i / totalCount);
            
            try
            {
                if (GenerateLODForObject(obj))
                {
                    successCount++;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"LOD 생성 실패: {obj.name} - {e.Message}");
            }
        }
        
        EditorUtility.ClearProgressBar();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("완료", 
            $"LOD 생성 완료!\n성공: {successCount}/{totalCount}개", "확인");
    }
    
    private bool GenerateLODForObject(GameObject obj)
    {
        MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer>();
        MeshFilter meshFilter = obj.GetComponent<MeshFilter>();
        
        if (meshRenderer == null || meshFilter == null)
        {
            Debug.LogWarning($"MeshRenderer 또는 MeshFilter가 없습니다: {obj.name}");
            return false;
        }
        
        Mesh originalMesh = meshFilter.sharedMesh;
        if (originalMesh == null)
        {
            Debug.LogWarning($"메시가 없습니다: {obj.name}");
            return false;
        }
        
        // LODGroup이 이미 있으면 제거
        LODGroup existingLODGroup = obj.GetComponent<LODGroup>();
        if (existingLODGroup != null)
        {
            DestroyImmediate(existingLODGroup);
        }
        
        // LOD 메시들 생성
        List<Mesh> lodMeshes = new List<Mesh>();
        List<float> transitionHeights = new List<float>();
        
        // 원본 메시 (LOD0)
        lodMeshes.Add(originalMesh);
        transitionHeights.Add(1.0f);
        
        // LOD1 생성
        Mesh lod1Mesh = CreateSimplifiedMesh(originalMesh, lod1Ratio, $"{obj.name}_LOD1");
        if (lod1Mesh != null)
        {
            lodMeshes.Add(lod1Mesh);
            transitionHeights.Add(lod1Transition);
        }
        
        // LOD2 생성
        Mesh lod2Mesh = CreateSimplifiedMesh(originalMesh, lod2Ratio, $"{obj.name}_LOD2");
        if (lod2Mesh != null)
        {
            lodMeshes.Add(lod2Mesh);
            transitionHeights.Add(lod2Transition);
        }
        
        // LOD3 생성 (선택적)
        if (lod3Ratio > 0.01f)
        {
            Mesh lod3Mesh = CreateSimplifiedMesh(originalMesh, lod3Ratio, $"{obj.name}_LOD3");
            if (lod3Mesh != null)
            {
                lodMeshes.Add(lod3Mesh);
                transitionHeights.Add(lod3Transition);
            }
        }
        
        // 기존 생성된 자식(Generated) 정리
        for (int ci = obj.transform.childCount - 1; ci >= 0; ci--)
        {
            var child = obj.transform.GetChild(ci);
            if (child.name.EndsWith("(Generated)"))
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // LODGroup 생성
        LODGroup lodGroup = obj.AddComponent<LODGroup>();

        // LOD 레벨들에 들어갈 렌더러 생성(각 LOD마다 별도 GO)
        LOD[] lods = new LOD[lodMeshes.Count];
        Renderer[] createdRenderers = new Renderer[lodMeshes.Count];
        for (int i = 0; i < lodMeshes.Count; i++)
        {
            string childName = i == 0 ? "LOD 0 (Generated)" : (i == 1 ? "LOD 1 (Generated)" : (i == 2 ? "LOD 2 (Generated)" : "LOD 3 (Generated)"));
            var childGo = new GameObject(childName);
            childGo.transform.SetParent(obj.transform, false);

            var mf = childGo.AddComponent<MeshFilter>();
            var mr = childGo.AddComponent<MeshRenderer>();
            mf.sharedMesh = lodMeshes[i];
            mr.sharedMaterials = meshRenderer.sharedMaterials;

            // LOD 단계에 따라 그림자 비용 낮춤
            if (i >= 2) // LOD2 이상은 그림자 끄기 권장
            {
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                mr.receiveShadows = false;
            }

            createdRenderers[i] = mr;
            lods[i] = new LOD(transitionHeights[i], new Renderer[] { mr });
        }

        // 원본 렌더러는 비활성화(LOD 전용 렌더러만 사용)
        meshRenderer.enabled = false;

        // LODGroup 설정
        lodGroup.SetLODs(lods);
        lodGroup.RecalculateBounds();
        
        Debug.Log($"LOD 생성 완료: {obj.name} (LOD 레벨: {lodMeshes.Count})");
        return true;
    }
    
    private Mesh CreateSimplifiedMesh(Mesh originalMesh, float ratio, string meshName)
    {
        try
        {
            // UnityMeshSimplifier 사용
            MeshSimplifier meshSimplifier = new MeshSimplifier();
            meshSimplifier.Initialize(originalMesh);
            
            // 메시 단순화
            meshSimplifier.SimplifyMesh(ratio);
            
            // 결과 메시 생성
            Mesh simplifiedMesh = meshSimplifier.ToMesh();
            simplifiedMesh.name = meshName;
            
            // 필요 시 노멀/바운즈 재계산 (Unity 표준 API)
            if (recalculateNormals)
            {
                simplifiedMesh.RecalculateNormals();
            }
            simplifiedMesh.RecalculateBounds();
            
            // 메시를 에셋으로 저장
            string meshPath = $"{outputFolder}/{meshName}.asset";
            
            // 기존 파일이 있고 덮어쓰기가 비활성화된 경우 스킵
            if (File.Exists(meshPath) && !overwriteExisting)
            {
                Debug.LogWarning($"메시가 이미 존재합니다 (스킵): {meshPath}");
                return AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            }
            
            AssetDatabase.CreateAsset(simplifiedMesh, meshPath);
            AssetDatabase.SaveAssets();
            
            return simplifiedMesh;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"메시 단순화 실패: {meshName} - {e.Message}");
            return null;
        }
    }
    
    private void OnSelectionChange()
    {
        Repaint();
    }
}
