using UnityEngine;
using UnityEditor;

/// <summary>
/// POLYARTWorldBuilder에 Inspector 버튼을 추가하는 커스텀 에디터
/// </summary>
[CustomEditor(typeof(POLYARTWorldBuilder))]
public class POLYARTWorldBuilderEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 Inspector 그리기
        DrawDefaultInspector();
        
        // 구분선 추가
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("POLYART 월드 구성 도구", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // 주요 버튼들
        if (GUILayout.Button("🏘️ POLYART 월드 구성", GUILayout.Height(30)))
        {
            POLYARTWorldBuilder worldBuilder = (POLYARTWorldBuilder)target;
            worldBuilder.BuildPOLYARTWorld();
        }
        
        if (GUILayout.Button("🗑️ 월드 정리", GUILayout.Height(25)))
        {
            POLYARTWorldBuilder worldBuilder = (POLYARTWorldBuilder)target;
            worldBuilder.ClearWorld();
        }
        
        EditorGUILayout.Space();
        
        // 특별 기능 버튼들
        if (GUILayout.Button("🏛️ 마을 중심부 생성", GUILayout.Height(25)))
        {
            POLYARTWorldBuilder worldBuilder = (POLYARTWorldBuilder)target;
            worldBuilder.GenerateVillageCenter();
        }
        
        if (GUILayout.Button("⚡ 월드 최적화", GUILayout.Height(25)))
        {
            POLYARTWorldBuilder worldBuilder = (POLYARTWorldBuilder)target;
            worldBuilder.OptimizeWorld();
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("POLYART 에셋을 배열에 할당한 후 '🏘️ POLYART 월드 구성' 버튼을 클릭하세요!", MessageType.Info);
    }
}
