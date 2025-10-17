using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_RENDER_PIPELINE_URP
using UnityEngine.Rendering.Universal;
#endif

public class MobileOptimizationWindow : EditorWindow
{
    private float urpRenderScale = 0.9f;
    private int urpMsaa = 0; // 0,2,4,8
    private float shadowDistance = 35f;
    private int shadowCascades = 2;

    private int textureAndroidMaxSize = 1024;
    private bool textureUseCrunch = true;

    private float[] layerCullDistances = new float[32];
    private float defaultCullDistance = 150f;

    [MenuItem("Tools/Mobile Optimization/One-Click")] 
    public static void ShowWindow()
    {
        var w = GetWindow<MobileOptimizationWindow>(true, "Mobile Optimization");
        w.minSize = new Vector2(420, 520);
        w.Show();
    }

    private void OnEnable()
    {
        for (int i = 0; i < layerCullDistances.Length; i++)
        {
            layerCullDistances[i] = defaultCullDistance;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("URP 모바일 프리셋", EditorStyles.boldLabel);
        urpRenderScale = EditorGUILayout.Slider("Render Scale", urpRenderScale, 0.5f, 1.0f);
        urpMsaa = EditorGUILayout.IntPopup("MSAA", urpMsaa, new[] { "Disabled", "2x", "4x", "8x" }, new[] { 0, 2, 4, 8 });
        shadowDistance = EditorGUILayout.Slider("Shadow Distance", shadowDistance, 10f, 80f);
        shadowCascades = EditorGUILayout.IntPopup("Shadow Cascades", shadowCascades, new[] { "No Cascades", "2", "4" }, new[] { 1, 2, 4 });

        if (GUILayout.Button("Apply URP Mobile Preset"))
        {
            ApplyUrpMobilePreset();
        }

        EditorGUILayout.Space();
        GUILayout.Label("머티리얼/셰이더", EditorStyles.boldLabel);
        if (GUILayout.Button("Convert Materials to Simple Lit/Unlit (Project-wide)"))
        {
            ConvertMaterialsProjectWide();
        }

        EditorGUILayout.Space();
        GUILayout.Label("텍스처 임포트(Android)", EditorStyles.boldLabel);
        textureAndroidMaxSize = EditorGUILayout.IntPopup("Max Size", textureAndroidMaxSize, new[] { "512", "1024", "2048" }, new[] { 512, 1024, 2048 });
        textureUseCrunch = EditorGUILayout.Toggle("Use Crunch", textureUseCrunch);
        if (GUILayout.Button("Apply Android Compression (ETC2/ASTC if available)"))
        {
            ApplyAndroidTextureCompression();
        }

        EditorGUILayout.Space();
        GUILayout.Label("카메라 컬링 거리(현재 씬)", EditorStyles.boldLabel);
        defaultCullDistance = EditorGUILayout.FloatField("Default Distance", defaultCullDistance);
        if (GUILayout.Button("Set On Main Camera"))
        {
            SetCameraCullDistances();
        }

        EditorGUILayout.Space();
        GUILayout.Label("검증/리포트", EditorStyles.boldLabel);
        if (GUILayout.Button("Validate & Report"))
        {
            ValidateAndReport();
        }
    }

    private void ApplyUrpMobilePreset()
    {
        try
        {
            GraphicsSettings.useScriptableRenderPipelineBatching = true;

#if UNITY_RENDER_PIPELINE_URP
            var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (rp != null)
            {
                Undo.RecordObject(rp, "URP Mobile Preset");
                // 기본 렌더러 설정 접근은 RendererData 필요. 여기서는 핵심 전역만 처리
                rp.supportsHDR = false;
                rp.msaaSampleCount = Mathf.Clamp(urpMsaa, 0, 8);
                rp.renderScale = Mathf.Clamp(urpRenderScale, 0.5f, 1.0f);
                rp.supportsCameraDepthTexture = false;
                rp.supportsCameraOpaqueTexture = false;
                rp.shadowDistance = shadowDistance;
                rp.shadowCascadeCount = shadowCascades;
                rp.supportsMainLightShadows = true;
                rp.supportsAdditionalLightShadows = false;
                rp.supportsSoftShadows = false;

                EditorUtility.SetDirty(rp);
            }
            else
            {
                Debug.LogWarning("현재 활성화된 URP Asset을 찾을 수 없습니다. GraphicsSettings에서 URP 자산을 확인하세요.");
            }
#else
            Debug.LogWarning("URP가 활성화된 프로젝트에서만 동작합니다. (UNITY_RENDER_PIPELINE_URP)");
#endif

            QualitySettings.shadowDistance = shadowDistance;
            Debug.Log("URP 모바일 프리셋 적용 완료");
        }
        catch (System.Exception ex)
        {
            Debug.LogError("URP 모바일 프리셋 적용 중 오류: " + ex.Message);
        }
    }

    private void ConvertMaterialsProjectWide()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int changed = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null || mat.shader == null) continue;

#if UNITY_RENDER_PIPELINE_URP
                // 투명/파티클 등은 유지, 기본 Lit는 Simple Lit로 다운그레이드
                if (mat.shader.name == "Universal Render Pipeline/Lit")
                {
                    var simple = Shader.Find("Universal Render Pipeline/Simple Lit");
                    if (simple != null)
                    {
                        Undo.RecordObject(mat, "Convert to Simple Lit");
                        mat.shader = simple;
                        EditorUtility.SetDirty(mat);
                        changed++;
                    }
                }
#endif
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }
        Debug.Log($"머티리얼 변환 완료: {changed}개 변경");
    }

    private void ApplyAndroidTextureCompression()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D");
        int changed = 0;
        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) continue;

                var platform = importer.GetPlatformTextureSettings("Android");
                platform.overridden = true;
                platform.maxTextureSize = textureAndroidMaxSize;
                // 포맷은 자동으로(ETC2/ASTC). 강제 지정 시 기기 호환 이슈가 생길 수 있어 Auto로 둠
                platform.format = TextureImporterFormat.Automatic;
                platform.androidETC2FallbackOverride = AndroidETC2FallbackOverride.UseBuildSettings;

                importer.SetPlatformTextureSettings(platform);
                importer.crunchedCompression = textureUseCrunch;
                importer.textureCompression = TextureImporterCompression.CompressedHQ;

                EditorUtility.SetDirty(importer);
                changed++;
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
        }
        Debug.Log($"Android 텍스처 압축 설정 완료: {changed}개 적용");
    }

    private void SetCameraCullDistances()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("Main Camera를 찾을 수 없습니다.");
            return;
        }
        for (int i = 0; i < 32; i++)
        {
            layerCullDistances[i] = defaultCullDistance;
        }
        cam.layerCullDistances = layerCullDistances.ToArray();
        cam.layerCullSpherical = true;
        Debug.Log("메인 카메라 컬링 거리 적용 완료");
    }

    private void ValidateAndReport()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("==== Mobile Optimization Report ====");

#if UNITY_RENDER_PIPELINE_URP
        var rp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (rp != null)
        {
            sb.AppendLine("[URP Asset]");
            sb.AppendLine($"- SRP Batcher: {GraphicsSettings.useScriptableRenderPipelineBatching}");
            sb.AppendLine($"- HDR: {rp.supportsHDR}");
            sb.AppendLine($"- MSAA: {rp.msaaSampleCount}x");
            sb.AppendLine($"- Render Scale: {rp.renderScale:F2}");
            sb.AppendLine($"- OpaqueTex: {rp.supportsCameraOpaqueTexture}, DepthTex: {rp.supportsCameraDepthTexture}");
            sb.AppendLine($"- Shadow Distance: {rp.shadowDistance}, Cascades: {rp.shadowCascadeCount}");
        }
        else
        {
            sb.AppendLine("[URP Asset] 활성 자산을 찾을 수 없습니다.");
        }
#else
        sb.AppendLine("[URP] UNITY_RENDER_PIPELINE_URP가 활성화되지 않았습니다.");
#endif

        // Materials summary
        int totalMats = 0, urpSimple = 0, urpLit = 0, urpUnlit = 0, nonUrp = 0;
        var matGuids = AssetDatabase.FindAssets("t:Material");
        foreach (var g in matGuids)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(g));
            if (mat == null || mat.shader == null) continue;
            totalMats++;
            string s = mat.shader.name;
            if (s.Contains("Universal Render Pipeline/Simple Lit")) urpSimple++;
            else if (s.Contains("Universal Render Pipeline/Lit")) urpLit++;
            else if (s.Contains("Universal Render Pipeline/Unlit")) urpUnlit++;
            else nonUrp++;
        }
        sb.AppendLine("[Materials]");
        sb.AppendLine($"- Total: {totalMats}, SimpleLit: {urpSimple}, Lit: {urpLit}, Unlit: {urpUnlit}, Non-URP: {nonUrp}");

        // Textures Android platform settings summary
        int totalTex = 0, overridden = 0, crunchOn = 0;
        int max512 = 0, max1024 = 0, max2048 = 0, others = 0;
        var texGuids = AssetDatabase.FindAssets("t:Texture2D");
        foreach (var g in texGuids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) continue;
            totalTex++;
            var pt = ti.GetPlatformTextureSettings("Android");
            if (pt.overridden) overridden++;
            if (ti.crunchedCompression) crunchOn++;
            switch (pt.maxTextureSize)
            {
                case 512: max512++; break;
                case 1024: max1024++; break;
                case 2048: max2048++; break;
                default: others++; break;
            }
        }
        sb.AppendLine("[Textures - Android]");
        sb.AppendLine($"- Total: {totalTex}, Overridden: {overridden}, CrunchOn: {crunchOn}");
        sb.AppendLine($"- MaxSize 512:{max512}, 1024:{max1024}, 2048:{max2048}, Other:{others}");

        // Camera cull distances
        var cam = Camera.main;
        if (cam != null)
        {
            var d = cam.layerCullDistances;
            float min = d != null && d.Length > 0 ? d.Min() : 0f;
            float max = d != null && d.Length > 0 ? d.Max() : 0f;
            sb.AppendLine("[Camera]");
            sb.AppendLine($"- Cull Distances min:{min:F1}, max:{max:F1}, Spherical:{cam.layerCullSpherical}");
        }
        else
        {
            sb.AppendLine("[Camera] Main Camera 없음");
        }

        Debug.Log(sb.ToString());
    }
}


