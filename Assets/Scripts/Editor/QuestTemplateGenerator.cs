using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 퀘스트 템플릿 자동 생성 툴
/// </summary>
public class QuestTemplateGenerator : EditorWindow
{
    [Header("템플릿 설정")]
    private int questCount = 5;
    private string questPrefix = "Quest";
    private QuestCategory defaultCategory = QuestCategory.Combat;
    
    [Header("폴더 설정")]
    private string outputFolder = "Assets/Scripts/QuestSystems/ScriptableObject/Quest";
    
    [MenuItem("Tools/Quest Template Generator")]
    public static void ShowWindow()
    {
        GetWindow<QuestTemplateGenerator>("퀘스트 템플릿 생성기");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("퀘스트 템플릿 생성기", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // 설정
        questCount = EditorGUILayout.IntField("생성할 퀘스트 수", questCount);
        questPrefix = EditorGUILayout.TextField("퀘스트 접두사", questPrefix);
        defaultCategory = (QuestCategory)EditorGUILayout.EnumPopup("기본 카테고리", defaultCategory);
        
        GUILayout.Space(10);
        
        // 출력 폴더
        GUILayout.Label("출력 폴더", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        outputFolder = EditorGUILayout.TextField("폴더 경로", outputFolder);
        if (GUILayout.Button("선택", GUILayout.Width(50)))
        {
            string selectedPath = EditorUtility.OpenFolderPanel("퀘스트 저장 폴더 선택", "Assets", "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                outputFolder = "Assets" + selectedPath.Substring(Application.dataPath.Length);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(20);
        
        // 생성 버튼들
        GUILayout.Label("템플릿 생성", EditorStyles.boldLabel);
        
        if (GUILayout.Button("기본 퀘스트 템플릿 생성", GUILayout.Height(30)))
        {
            GenerateBasicQuestTemplates();
        }
        
        if (GUILayout.Button("처치 퀘스트 템플릿 생성", GUILayout.Height(30)))
        {
            GenerateKillQuestTemplates();
        }
        
        if (GUILayout.Button("수집 퀘스트 템플릿 생성", GUILayout.Height(30)))
        {
            GenerateCollectQuestTemplates();
        }
        
        if (GUILayout.Button("탐험 퀘스트 템플릿 생성", GUILayout.Height(30)))
        {
            GenerateExplorationQuestTemplates();
        }
        
        if (GUILayout.Button("대화 퀘스트 템플릿 생성", GUILayout.Height(30)))
        {
            GenerateSocialQuestTemplates();
        }
        
        GUILayout.Space(10);
        
        // 폴더 생성
        if (GUILayout.Button("폴더 구조 생성", GUILayout.Height(25)))
        {
            CreateFolderStructure();
        }
    }
    
    private void GenerateBasicQuestTemplates()
    {
        CreateFolderIfNotExists(outputFolder);
        
        for (int i = 1; i <= questCount; i++)
        {
            string questId = $"{questPrefix}_{i:D2}";
            CreateQuestTemplate(questId, $"퀘스트 {i}", $"기본 퀘스트 {i}입니다.", defaultCategory);
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{questCount}개의 기본 퀘스트 템플릿이 생성되었습니다.", "확인");
    }
    
    private void GenerateKillQuestTemplates()
    {
        CreateFolderIfNotExists(outputFolder);
        
        string[] enemyTypes = { "고블린", "오크", "스켈레톤", "좀비", "드래곤" };
        
        for (int i = 0; i < questCount && i < enemyTypes.Length; i++)
        {
            string questId = $"kill_{enemyTypes[i].ToLower()}_quest";
            string questName = $"{enemyTypes[i]} 처치";
            string questDescription = $"{enemyTypes[i]}을(를) 처치하세요.";
            
            CreateQuestTemplate(questId, questName, questDescription, QuestCategory.Combat);
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{questCount}개의 처치 퀘스트 템플릿이 생성되었습니다.", "확인");
    }
    
    private void GenerateCollectQuestTemplates()
    {
        CreateFolderIfNotExists(outputFolder);
        
        string[] itemTypes = { "나무", "돌", "철", "보석", "약초" };
        
        for (int i = 0; i < questCount && i < itemTypes.Length; i++)
        {
            string questId = $"collect_{itemTypes[i].ToLower()}_quest";
            string questName = $"{itemTypes[i]} 수집";
            string questDescription = $"{itemTypes[i]}을(를) 수집하세요.";
            
            CreateQuestTemplate(questId, questName, questDescription, QuestCategory.Collection);
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{questCount}개의 수집 퀘스트 템플릿이 생성되었습니다.", "확인");
    }
    
    private void GenerateExplorationQuestTemplates()
    {
        CreateFolderIfNotExists(outputFolder);
        
        string[] locations = { "동굴", "숲", "산", "바다", "사막" };
        
        for (int i = 0; i < questCount && i < locations.Length; i++)
        {
            string questId = $"explore_{locations[i].ToLower()}_quest";
            string questName = $"{locations[i]} 탐험";
            string questDescription = $"{locations[i]}을(를) 탐험하세요.";
            
            CreateQuestTemplate(questId, questName, questDescription, QuestCategory.Exploration);
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{questCount}개의 탐험 퀘스트 템플릿이 생성되었습니다.", "확인");
    }
    
    private void GenerateSocialQuestTemplates()
    {
        CreateFolderIfNotExists(outputFolder);
        
        string[] npcTypes = { "상인", "마을장", "마법사", "기사", "농부" };
        
        for (int i = 0; i < questCount && i < npcTypes.Length; i++)
        {
            string questId = $"talk_{npcTypes[i].ToLower()}_quest";
            string questName = $"{npcTypes[i]}와 대화";
            string questDescription = $"{npcTypes[i]}와 대화하세요.";
            
            CreateQuestTemplate(questId, questName, questDescription, QuestCategory.Social);
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", $"{questCount}개의 대화 퀘스트 템플릿이 생성되었습니다.", "확인");
    }
    
    private void CreateQuestTemplate(string questId, string questName, string questDescription, QuestCategory category)
    {
        Quest quest = ScriptableObject.CreateInstance<Quest>();
        quest.name = questId;
        
        // 기본 정보 설정 (실제 Quest 클래스의 필드에 맞게 수정 필요)
        // quest.codeName = questId;
        // quest.displayName = questName;
        // quest.description = questDescription;
        // quest.category = GetCategoryFromEnum(category);
        // quest.useAutoComplete = true;
        // quest.isCancelable = true;
        // quest.isSavable = true;
        
        // 파일 저장
        string fileName = $"{questId}.asset";
        string filePath = Path.Combine(outputFolder, fileName);
        AssetDatabase.CreateAsset(quest, filePath);
        
        Debug.Log($"퀘스트 템플릿 생성: {filePath}");
    }
    
    private void CreateFolderStructure()
    {
        string[] folders = {
            "Assets/Scripts/QuestSystems/ScriptableObject",
            "Assets/Scripts/QuestSystems/ScriptableObject/Quest",
            "Assets/Scripts/QuestSystems/ScriptableObject/Task",
            "Assets/Scripts/QuestSystems/ScriptableObject/Task/Action",
            "Assets/Scripts/QuestSystems/ScriptableObject/Task/Target",
            "Assets/Scripts/QuestSystems/ScriptableObject/Reward",
            "Assets/Scripts/QuestSystems/ScriptableObject/Category",
            "Assets/Resources"
        };
        
        foreach (string folder in folders)
        {
            CreateFolderIfNotExists(folder);
        }
        
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("완료", "폴더 구조가 생성되었습니다.", "확인");
    }
    
    private void CreateFolderIfNotExists(string folderPath)
    {
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            string parentFolder = Path.GetDirectoryName(folderPath);
            string folderName = Path.GetFileName(folderPath);
            
            if (!string.IsNullOrEmpty(parentFolder) && AssetDatabase.IsValidFolder(parentFolder))
            {
                AssetDatabase.CreateFolder(parentFolder, folderName);
            }
        }
    }
}
