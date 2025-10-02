using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// 퀘스트 데이터베이스 관리 에디터 툴
/// </summary>
public class QuestDatabaseManager : EditorWindow
{
    private QuestDatabase questDatabase;
    private QuestDatabase achievementDatabase;
    private Vector2 scrollPosition;
    
    [MenuItem("Tools/Quest Database Manager")]
    public static void ShowWindow()
    {
        GetWindow<QuestDatabaseManager>("퀘스트 데이터베이스 관리자");
    }
    
    private void OnEnable()
    {
        LoadDatabases();
    }
    
    private void OnGUI()
    {
        GUILayout.Label("퀘스트 데이터베이스 관리자", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // 데이터베이스 로드
        GUILayout.Label("데이터베이스 설정", EditorStyles.boldLabel);
        questDatabase = (QuestDatabase)EditorGUILayout.ObjectField("퀘스트 데이터베이스", questDatabase, typeof(QuestDatabase), false);
        achievementDatabase = (QuestDatabase)EditorGUILayout.ObjectField("업적 데이터베이스", achievementDatabase, typeof(QuestDatabase), false);
        
        if (GUILayout.Button("데이터베이스 새로고침"))
        {
            LoadDatabases();
        }
        
        GUILayout.Space(10);
        
        // 퀘스트 관리
        if (questDatabase != null)
        {
            DrawQuestManagement();
        }
        else
        {
            GUILayout.Label("퀘스트 데이터베이스를 먼저 설정해주세요.", EditorStyles.helpBox);
        }
        
        GUILayout.Space(10);
        
        // 업적 관리
        if (achievementDatabase != null)
        {
            DrawAchievementManagement();
        }
        else
        {
            GUILayout.Label("업적 데이터베이스를 먼저 설정해주세요.", EditorStyles.helpBox);
        }
    }
    
    private void DrawQuestManagement()
    {
        GUILayout.Label("퀘스트 관리", EditorStyles.boldLabel);
        
        // 퀘스트 목록
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));
        
        if (questDatabase.EditableQuests != null)
        {
            for (int i = 0; i < questDatabase.EditableQuests.Count; i++)
            {
                Quest quest = questDatabase.EditableQuests[i];
                if (quest != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // 퀘스트 정보 표시
                    EditorGUILayout.LabelField($"{i + 1}. {quest.DisplayName}", GUILayout.Width(200));
                    EditorGUILayout.LabelField(quest.Category?.DisplayName ?? "Unknown", GUILayout.Width(100));
                    
                    // 버튼들
                    if (GUILayout.Button("선택", GUILayout.Width(50)))
                    {
                        Selection.activeObject = quest;
                        EditorGUIUtility.PingObject(quest);
                    }
                    
                    if (GUILayout.Button("삭제", GUILayout.Width(50)))
                    {
                        if (EditorUtility.DisplayDialog("확인", $"'{quest.DisplayName}' 퀘스트를 삭제하시겠습니까?", "삭제", "취소"))
                        {
                            RemoveQuestFromDatabase(questDatabase, quest);
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        
        EditorGUILayout.EndScrollView();
        
        // 퀘스트 추가 버튼
        if (GUILayout.Button("새 퀘스트 추가"))
        {
            AddNewQuestToDatabase();
        }
        
        // 자동 스캔 버튼
        if (GUILayout.Button("폴더에서 퀘스트 자동 스캔"))
        {
            ScanQuestsFromFolder();
        }
    }
    
    private void DrawAchievementManagement()
    {
        GUILayout.Label("업적 관리", EditorStyles.boldLabel);
        
        // 업적 목록
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(150));
        
        if (achievementDatabase.EditableQuests != null)
        {
            for (int i = 0; i < achievementDatabase.EditableQuests.Count; i++)
            {
                Quest achievement = achievementDatabase.EditableQuests[i];
                if (achievement != null)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    // 업적 정보 표시
                    EditorGUILayout.LabelField($"{i + 1}. {achievement.DisplayName}", GUILayout.Width(200));
                    EditorGUILayout.LabelField(achievement.Category?.DisplayName ?? "Unknown", GUILayout.Width(100));
                    
                    // 버튼들
                    if (GUILayout.Button("선택", GUILayout.Width(50)))
                    {
                        Selection.activeObject = achievement;
                        EditorGUIUtility.PingObject(achievement);
                    }
                    
                    if (GUILayout.Button("삭제", GUILayout.Width(50)))
                    {
                        if (EditorUtility.DisplayDialog("확인", $"'{achievement.DisplayName}' 업적을 삭제하시겠습니까?", "삭제", "취소"))
                        {
                            RemoveQuestFromDatabase(achievementDatabase, achievement);
                        }
                    }
                    
                    EditorGUILayout.EndHorizontal();
                }
            }
        }
        
        EditorGUILayout.EndScrollView();
        
        // 업적 추가 버튼
        if (GUILayout.Button("새 업적 추가"))
        {
            AddNewAchievementToDatabase();
        }
    }
    
    private void LoadDatabases()
    {
        // Resources 폴더에서 데이터베이스 로드
        questDatabase = Resources.Load<QuestDatabase>("QuestDatabase");
        achievementDatabase = Resources.Load<QuestDatabase>("AchievementDatabase");
        
        if (questDatabase == null)
        {
            Debug.LogWarning("QuestDatabase를 찾을 수 없습니다. Resources 폴더에 생성해주세요.");
        }
        
        if (achievementDatabase == null)
        {
            Debug.LogWarning("AchievementDatabase를 찾을 수 없습니다. Resources 폴더에 생성해주세요.");
        }
    }
    
    private void AddNewQuestToDatabase()
    {
        if (questDatabase == null)
        {
            EditorUtility.DisplayDialog("오류", "퀘스트 데이터베이스가 설정되지 않았습니다.", "확인");
            return;
        }
        
        // 퀘스트 생성 마법사 열기
        QuestCreationWizard.ShowWindow();
    }
    
    private void AddNewAchievementToDatabase()
    {
        if (achievementDatabase == null)
        {
            EditorUtility.DisplayDialog("오류", "업적 데이터베이스가 설정되지 않았습니다.", "확인");
            return;
        }
        
        // 업적 생성 마법사 열기 (퀘스트와 동일한 방식)
        QuestCreationWizard.ShowWindow();
    }
    
    private void RemoveQuestFromDatabase(QuestDatabase database, Quest quest)
    {
        if (database.EditableQuests.Contains(quest))
        {
            database.EditableQuests.Remove(quest);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            Debug.Log($"'{quest.DisplayName}'이 데이터베이스에서 제거되었습니다.");
        }
    }
    
    private void ScanQuestsFromFolder()
    {
        if (questDatabase == null)
        {
            EditorUtility.DisplayDialog("오류", "퀘스트 데이터베이스가 설정되지 않았습니다.", "확인");
            return;
        }
        
        string questFolderPath = "Assets/Scripts/QuestSystems/ScriptableObject/Quest";
        
        if (!Directory.Exists(questFolderPath))
        {
            EditorUtility.DisplayDialog("오류", "퀘스트 폴더를 찾을 수 없습니다.", "확인");
            return;
        }
        
        // 폴더에서 모든 Quest 에셋 찾기
        string[] questGuids = AssetDatabase.FindAssets("t:Quest", new[] { questFolderPath });
        
        int addedCount = 0;
        foreach (string guid in questGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Quest quest = AssetDatabase.LoadAssetAtPath<Quest>(path);
            
            if (quest != null && !questDatabase.EditableQuests.Contains(quest))
            {
                questDatabase.EditableQuests.Add(quest);
                addedCount++;
            }
        }
        
        if (addedCount > 0)
        {
            EditorUtility.SetDirty(questDatabase);
            AssetDatabase.SaveAssets();
            EditorUtility.DisplayDialog("완료", $"{addedCount}개의 퀘스트가 데이터베이스에 추가되었습니다.", "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("알림", "추가할 퀘스트가 없습니다.", "확인");
        }
    }
}
