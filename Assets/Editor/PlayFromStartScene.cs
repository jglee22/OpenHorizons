#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class PlayFromStartScene
{
    // 메뉴: Tools/Play From Scene/Set Start Scene… 로 시작 씬 지정
    const string PrefKey = "PlayFromStartScene.Path";

    static PlayFromStartScene()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChange;
    }

    [MenuItem("Tools/Play From Scene/Set Start Scene…")]
    static void SetStartScene()
    {
        string path = EditorUtility.OpenFilePanel("Select Start Scene", "Assets", "unity");
        if (!string.IsNullOrEmpty(path) && path.StartsWith(Application.dataPath))
        {
            // 절대경로 → 프로젝트 상대경로(Assets/…)
            path = "Assets" + path.Substring(Application.dataPath.Length);
            EditorPrefs.SetString(PrefKey, path);
            Debug.Log($"[PlayFromStartScene] Start Scene: {path}");
        }
    }

    [MenuItem("Tools/Play From Scene/Clear")]
    static void Clear() => EditorPrefs.DeleteKey(PrefKey);

    static void OnPlayModeChange(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.ExitingEditMode) return;

        var startScenePath = EditorPrefs.GetString(PrefKey, "");
        if (string.IsNullOrEmpty(startScenePath)) return; // 지정 안 했으면 패스
        if (EditorSceneManager.GetActiveScene().path == startScenePath) return;

        // 변경사항 저장 확인 후 시작 씬으로 전환
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(startScenePath);
    }
}
#endif
