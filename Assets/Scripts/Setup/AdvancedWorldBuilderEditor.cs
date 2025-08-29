using UnityEngine;
using UnityEditor;

/// <summary>
/// AdvancedWorldBuilder에 Inspector 버튼을 추가하는 커스텀 에디터
/// </summary>
[CustomEditor(typeof(AdvancedWorldBuilder))]
public class AdvancedWorldBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();
        
        // 구분선 추가
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("고급 월드 생성 도구", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 주요 버튼들
        if (GUILayout.Button("🚀 고급 월드 생성", GUILayout.Height(30)))
        {
            AdvancedWorldBuilder worldBuilder = (AdvancedWorldBuilder)target;
            worldBuilder.GenerateWorld();
        }
        
        if (GUILayout.Button("🗑️ 월드 정리", GUILayout.Height(25)))
        {
            AdvancedWorldBuilder worldBuilder = (AdvancedWorldBuilder)target;
            worldBuilder.ClearWorld();
        }
        
        EditorGUILayout.Space();
        
        // 설정 테스트 버튼
        if (GUILayout.Button("⚙️ 머티리얼 우선순위 테스트", GUILayout.Height(25)))
        {
            AdvancedWorldBuilder worldBuilder = (AdvancedWorldBuilder)target;
            worldBuilder.TestMaterialPriority();
        }
        
        if (GUILayout.Button("🎯 현재 설정으로 생성", GUILayout.Height(25)))
        {
            AdvancedWorldBuilder worldBuilder = (AdvancedWorldBuilder)target;
            worldBuilder.GenerateWithCurrentSettings();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("머티리얼 우선순위를 설정한 후 '🚀 고급 월드 생성' 버튼을 클릭하세요!", MessageType.Info);
    }
}
