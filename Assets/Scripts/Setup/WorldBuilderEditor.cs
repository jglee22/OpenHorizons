using UnityEngine;
using UnityEditor;

/// <summary>
/// WorldBuilder에 Inspector 버튼을 추가하는 커스텀 에디터
/// </summary>
[CustomEditor(typeof(WorldBuilder))]
public class WorldBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();
        
        // 구분선 추가
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("월드 생성 도구", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 버튼들 추가
        if (GUILayout.Button("🌍 월드 생성", GUILayout.Height(30)))
        {
            WorldBuilder worldBuilder = (WorldBuilder)target;
            worldBuilder.GenerateWorld();
        }
        
        if (GUILayout.Button("🗑️ 월드 정리", GUILayout.Height(25)))
        {
            WorldBuilder worldBuilder = (WorldBuilder)target;
            worldBuilder.ClearGeneratedWorld();
        }
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("🏔️ 간단한 월드 생성", GUILayout.Height(25)))
        {
            WorldBuilder worldBuilder = (WorldBuilder)target;
            worldBuilder.GenerateSimpleWorld();
        }
        
        if (GUILayout.Button("🏙️ 완전한 월드 생성", GUILayout.Height(25)))
        {
            WorldBuilder worldBuilder = (WorldBuilder)target;
            worldBuilder.GenerateFullWorld();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("월드 생성을 시작하려면 '🌍 월드 생성' 버튼을 클릭하세요!", MessageType.Info);
    }
}
