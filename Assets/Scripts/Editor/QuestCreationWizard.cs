using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

/// <summary>
/// 퀘스트 생성 마법사 에디터 툴
/// </summary>
public class QuestCreationWizard : EditorWindow
{
    [Header("퀘스트 기본 정보")]
    private string questId = "";
    private string questName = "";
    private string questDescription = "";
    private Sprite questIcon = null;
    private QuestCategory questCategory = QuestCategory.Combat;
    
    [Header("퀘스트 설정")]
    private bool useAutoComplete = true;
    private bool isCancelable = true;
    private bool isSavable = true;
    
    [Header("태스크 설정")]
    private TaskType taskType = TaskType.KillEnemy;
    private string taskDescription = "";
    private int needSuccessToComplete = 1;
    
    [Header("타겟 설정")]
    private string targetId = "";
    private string targetName = "";
    private Vector3 targetPosition = Vector3.zero;
    private float targetRadius = 5f;
    
    [Header("보상 설정")]
    private RewardType rewardType = RewardType.Item;
    private string rewardItemName = "";
    private int rewardAmount = 1;
    
    private Vector2 scrollPosition;

    // 최근 생성된 액션/타겟을 보관하여 에셋 저장 후 서브 에셋으로 추가
    private TaskAction lastCreatedAction;
    private TaskTarget lastCreatedTarget;
    
    [MenuItem("Tools/Quest Creation Wizard")]
    public static void ShowWindow()
    {
        GetWindow<QuestCreationWizard>("퀘스트 생성 마법사");
    }
    
    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        GUILayout.Label("퀘스트 생성 마법사", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // 퀘스트 기본 정보
        GUILayout.Label("퀘스트 기본 정보", EditorStyles.boldLabel);
        questId = EditorGUILayout.TextField("퀘스트 ID", questId);
        questName = EditorGUILayout.TextField("퀘스트 이름", questName);
        questDescription = EditorGUILayout.TextArea(questDescription, GUILayout.Height(60));
        questIcon = (Sprite)EditorGUILayout.ObjectField("퀘스트 아이콘", questIcon, typeof(Sprite), false);
        questCategory = (QuestCategory)EditorGUILayout.EnumPopup("퀘스트 카테고리", questCategory);
        
        GUILayout.Space(10);
        
        // 퀘스트 설정
        GUILayout.Label("퀘스트 설정", EditorStyles.boldLabel);
        useAutoComplete = EditorGUILayout.Toggle("자동 완료", useAutoComplete);
        isCancelable = EditorGUILayout.Toggle("취소 가능", isCancelable);
        isSavable = EditorGUILayout.Toggle("저장 가능", isSavable);
        
        GUILayout.Space(10);
        
        // 태스크 설정
        GUILayout.Label("태스크 설정", EditorStyles.boldLabel);
        taskType = (TaskType)EditorGUILayout.EnumPopup("태스크 타입", taskType);
        taskDescription = EditorGUILayout.TextField("태스크 설명", taskDescription);
        needSuccessToComplete = EditorGUILayout.IntField("완료 필요 횟수", needSuccessToComplete);
        
        GUILayout.Space(10);
        
        // 타겟 설정
        GUILayout.Label("타겟 설정", EditorStyles.boldLabel);
        
        if (taskType == TaskType.KillEnemy)
        {
            targetId = EditorGUILayout.TextField("적 ID (예: goblin_001, orc_002)", targetId);
            targetName = EditorGUILayout.TextField("적 이름 (예: 고블린, 오크)", targetName);
        }
        else if (taskType == TaskType.CollectItem)
        {
            targetId = EditorGUILayout.TextField("아이템 ID (예: herb_001, sword_002)", targetId);
            targetName = EditorGUILayout.TextField("아이템 이름 (예: 치유 허브, 철검)", targetName);
        }
        else if (taskType == TaskType.ReachLocation)
        {
            targetId = EditorGUILayout.TextField("위치 ID (예: ancient_ruins, village)", targetId);
            targetName = EditorGUILayout.TextField("위치 이름 (예: 고대 유적지, 마을)", targetName);
            targetPosition = EditorGUILayout.Vector3Field("목표 위치", targetPosition);
            targetRadius = EditorGUILayout.FloatField("도달 반경", targetRadius);
        }
        else if (taskType == TaskType.TalkToNPC)
        {
            targetId = EditorGUILayout.TextField("NPC ID (예: village_elder, merchant_001)", targetId);
            targetName = EditorGUILayout.TextField("NPC 이름 (예: 마을 장로, 상인)", targetName);
        }
        
        // 도움말 표시
        EditorGUILayout.HelpBox("ID는 게임에서 실제로 사용되는 고유 식별자입니다.\n" +
                               "• 적: GameObject의 이름이나 Tag\n" +
                               "• 아이템: 아이템 데이터의 ID\n" +
                               "• 위치: 씬의 특정 지점\n" +
                               "• NPC: GameObject의 이름이나 Tag", MessageType.Info);
        
        // ID 확인 도구
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("씬에서 적 ID 찾기", GUILayout.Height(25)))
        {
            FindEnemyIDs();
        }
        if (GUILayout.Button("아이템 ID 찾기", GUILayout.Height(25)))
        {
            FindItemIDs();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("씬에서 NPC ID 찾기", GUILayout.Height(25)))
        {
            FindNPCIDs();
        }
        if (GUILayout.Button("위치 ID 찾기", GUILayout.Height(25)))
        {
            FindLocationIDs();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        // 보상 설정
        GUILayout.Label("보상 설정", EditorStyles.boldLabel);
        rewardType = (RewardType)EditorGUILayout.EnumPopup("보상 타입", rewardType);
        
        if (rewardType == RewardType.Item)
        {
            rewardItemName = EditorGUILayout.TextField("보상 아이템 이름", rewardItemName);
            rewardAmount = EditorGUILayout.IntField("보상 수량", rewardAmount);
        }
        
        GUILayout.Space(20);
        
        // 생성 버튼
        if (GUILayout.Button("퀘스트 생성", GUILayout.Height(30)))
        {
            CreateQuest();
        }
        
        GUILayout.Space(10);
        
        // 빠른 생성 버튼들
        GUILayout.Label("빠른 생성", EditorStyles.boldLabel);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("처치 퀘스트"))
        {
            CreateKillQuest();
        }
        if (GUILayout.Button("수집 퀘스트"))
        {
            CreateCollectQuest();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("이동 퀘스트"))
        {
            CreateReachQuest();
        }
        if (GUILayout.Button("대화 퀘스트"))
        {
            CreateTalkQuest();
        }
        GUILayout.EndHorizontal();
        
        EditorGUILayout.EndScrollView();
    }
    
    private void CreateQuest()
    {
        try
        {
            if (string.IsNullOrEmpty(questId) || string.IsNullOrEmpty(questName))
            {
                EditorUtility.DisplayDialog("오류", "퀘스트 ID와 이름을 입력해주세요.", "확인");
                return;
            }
        
        // Quest ScriptableObject 생성
        Quest newQuest = ScriptableObject.CreateInstance<Quest>();
        
        // 기본 정보 설정
        newQuest.name = questId;
        
        // SerializedObject를 사용해서 private 필드에 접근
        SerializedObject serializedQuest = new SerializedObject(newQuest);
        serializedQuest.FindProperty("codeName").stringValue = questId;
        serializedQuest.FindProperty("displayName").stringValue = questName;
        serializedQuest.FindProperty("description").stringValue = questDescription;
        serializedQuest.FindProperty("icon").objectReferenceValue = questIcon;
        serializedQuest.FindProperty("useAutoComplete").boolValue = useAutoComplete;
        serializedQuest.FindProperty("isCancelable").boolValue = isCancelable;
        serializedQuest.FindProperty("isSavable").boolValue = isSavable;
        
        // 카테고리 설정 (Category ScriptableObject 필요)
        Category category = GetOrCreateCategory(questCategory);
        if (category != null)
        {
            serializedQuest.FindProperty("category").objectReferenceValue = category;
        }
        
        serializedQuest.ApplyModifiedProperties();
        
        // 태스크 생성
        Task newTask = CreateTask();
        if (newTask != null)
        {
            // Task의 카테고리를 퀘스트 카테고리와 동일하게 설정
            SerializedObject serializedTaskForCategory = new SerializedObject(newTask);
            serializedTaskForCategory.FindProperty("category").objectReferenceValue = category;
            serializedTaskForCategory.ApplyModifiedProperties();

            // TaskGroup은 일반 클래스이므로 직접 생성하고 Quest에 할당
            TaskGroup taskGroup = new TaskGroup();
            
            // TaskGroup의 tasks 배열에 Task 추가
            taskGroup.tasks = new Task[] { newTask };
            
            // Quest의 taskGroups 배열에 TaskGroup 추가
            serializedQuest.FindProperty("taskGroups").arraySize = 1;
            SerializedProperty taskGroupsProperty = serializedQuest.FindProperty("taskGroups");
            SerializedProperty firstTaskGroupProperty = taskGroupsProperty.GetArrayElementAtIndex(0);
            
            // TaskGroup의 tasks 배열을 SerializedProperty로 설정
            SerializedProperty tasksProperty = firstTaskGroupProperty.FindPropertyRelative("tasks");
            tasksProperty.arraySize = 1;
            tasksProperty.GetArrayElementAtIndex(0).objectReferenceValue = newTask;
        }
        
        // 보상 생성
        Reward newReward = CreateReward();
        if (newReward != null)
        {
            serializedQuest.FindProperty("rewards").arraySize = 1;
            serializedQuest.FindProperty("rewards").GetArrayElementAtIndex(0).objectReferenceValue = newReward;
        }
        
        serializedQuest.ApplyModifiedProperties();
        
        // 파일 저장
        string path = $"Assets/Scripts/QuestSystems/ScriptableObject/Quest/{questId}.asset";
        
        // 디렉토리 존재 확인 및 생성
        string questDir = "Assets/Scripts/QuestSystems/ScriptableObject/Quest";
        if (!AssetDatabase.IsValidFolder(questDir))
        {
            AssetDatabase.CreateFolder("Assets/Scripts/QuestSystems/ScriptableObject", "Quest");
        }
        
        // Quest 에셋을 먼저 저장
        AssetDatabase.CreateAsset(newQuest, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // Quest 에셋이 저장된 후 서브 에셋들을 추가
        if (newTask != null)
        {
            AssetDatabase.AddObjectToAsset(newTask, newQuest);
        }
        
        if (newReward != null)
        {
            AssetDatabase.AddObjectToAsset(newReward, newQuest);
        }

        // Task에 Action/Target 서브 에셋 추가 (신규 생성된 객체만)
        if (newTask != null)
        {
            if (lastCreatedAction != null)
                AssetDatabase.AddObjectToAsset(lastCreatedAction, newTask);
            if (lastCreatedTarget != null)
                AssetDatabase.AddObjectToAsset(lastCreatedTarget, newTask);
        }
        
        // TaskGroup의 tasks 배열을 다시 설정 (에셋 추가 후)
        if (newTask != null)
        {
            serializedQuest.Update();
            SerializedProperty taskGroupsProperty = serializedQuest.FindProperty("taskGroups");
            if (taskGroupsProperty.arraySize > 0)
            {
                SerializedProperty firstTaskGroupProperty = taskGroupsProperty.GetArrayElementAtIndex(0);
                SerializedProperty tasksProperty = firstTaskGroupProperty.FindPropertyRelative("tasks");
                tasksProperty.arraySize = 1;
                tasksProperty.GetArrayElementAtIndex(0).objectReferenceValue = newTask;
            }
            serializedQuest.ApplyModifiedProperties();
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        EditorUtility.DisplayDialog("성공", $"퀘스트가 생성되었습니다!\n경로: {path}", "확인");
        
        // 생성된 퀘스트 선택
        Selection.activeObject = newQuest;
        EditorGUIUtility.PingObject(newQuest);
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("오류", $"퀘스트 생성 중 오류가 발생했습니다:\n{e.Message}", "확인");
            Debug.LogError($"Quest Creation Error: {e}");
        }
    }
    
    private Task CreateTask()
    {
        Task newTask = ScriptableObject.CreateInstance<Task>();
        newTask.name = $"{questId}_Task";
        
        // SerializedObject로 Task 설정
        SerializedObject serializedTask = new SerializedObject(newTask);
        serializedTask.FindProperty("codeName").stringValue = questId;
        serializedTask.FindProperty("description").stringValue = taskDescription;
        serializedTask.FindProperty("needSuccessToComplete").intValue = needSuccessToComplete;
        
        // 태스크 타입에 따른 액션과 타겟 설정
        TaskAction action = CreateTaskAction();
        TaskTarget target = CreateTaskTarget();

        // 저장 시점에 서브 에셋으로 추가하기 위해 보관 (이미 에셋이면 추가하지 않음)
        lastCreatedAction = (action != null && !EditorUtility.IsPersistent(action)) ? action : null;
        lastCreatedTarget = (target != null && !EditorUtility.IsPersistent(target)) ? target : null;
        
        if (action != null)
        {
            serializedTask.FindProperty("action").objectReferenceValue = action;
        }
        
        if (target != null)
        {
            serializedTask.FindProperty("targets").arraySize = 1;
            serializedTask.FindProperty("targets").GetArrayElementAtIndex(0).objectReferenceValue = target;
        }
        
        serializedTask.ApplyModifiedProperties();
        return newTask;
    }
    
    private Reward CreateReward()
    {
        if (rewardType == RewardType.Item)
        {
            ItemReward itemReward = ScriptableObject.CreateInstance<ItemReward>();
            itemReward.name = $"{questId}_Reward";
            
            // SerializedObject로 ItemReward 설정
            SerializedObject serializedReward = new SerializedObject(itemReward);
            serializedReward.FindProperty("itemName").stringValue = rewardItemName;
            serializedReward.FindProperty("itemDescription").stringValue = $"{rewardItemName} 보상";
            serializedReward.FindProperty("itemType").enumValueIndex = (int)ItemType.Misc;
            serializedReward.FindProperty("itemCount").intValue = rewardAmount; // amount -> itemCount로 수정
            serializedReward.ApplyModifiedProperties();
            
            return itemReward;
        }
        
        return null;
    }
    
    private Category GetOrCreateCategory(QuestCategory questCategory)
    {
        // 기존 Category 찾기
        string[] guids = AssetDatabase.FindAssets("t:Category");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Category category = AssetDatabase.LoadAssetAtPath<Category>(path);
            if (category != null && category.DisplayName == questCategory.ToString())
            {
                return category;
            }
        }
        
        // Category가 없으면 생성
        Category newCategory = ScriptableObject.CreateInstance<Category>();
        newCategory.name = questCategory.ToString();
        
        SerializedObject serializedCategory = new SerializedObject(newCategory);
        serializedCategory.FindProperty("codeName").stringValue = questCategory.ToString().ToLower();
        serializedCategory.FindProperty("displayName").stringValue = questCategory.ToString();
        serializedCategory.ApplyModifiedProperties();
        
        // 디렉토리 존재 확인 및 생성
        string categoryDir = "Assets/Scripts/QuestSystems/ScriptableObject/Category";
        if (!AssetDatabase.IsValidFolder(categoryDir))
        {
            AssetDatabase.CreateFolder("Assets/Scripts/QuestSystems/ScriptableObject", "Category");
        }
        
        string categoryPath = $"{categoryDir}/{questCategory}.asset";
        AssetDatabase.CreateAsset(newCategory, categoryPath);
        AssetDatabase.SaveAssets();
        
        return newCategory;
    }
    
    private TaskAction CreateTaskAction()
    {
        TaskAction action = null;
        
        switch (taskType)
        {
            case TaskType.KillEnemy:
                action = GetOrCreateAction<SimpleCount>();
                break;
            case TaskType.CollectItem:
                action = GetOrCreateAction<CollectItemAction>();
                break;
            case TaskType.ReachLocation:
                action = GetOrCreateAction<ReachLocationAction>();
                break;
            case TaskType.TalkToNPC:
                action = GetOrCreateAction<TalkToNPCAction>();
                break;
            default:
                return null;
        }
        
        return action;
    }
    
    private TaskTarget CreateTaskTarget()
    {
        TaskTarget target = null;
        
        switch (taskType)
        {
            case TaskType.KillEnemy:
                target = GetOrCreateEnemyTarget();
                break;
            case TaskType.CollectItem:
                target = GetOrCreateItemTarget();
                break;
            case TaskType.ReachLocation:
                target = GetOrCreateLocationTarget();
                break;
            case TaskType.TalkToNPC:
                target = GetOrCreateNPCTarget();
                break;
            default:
                return null;
        }
        
        return target;
    }
    
    private T GetOrCreateAction<T>() where T : TaskAction
    {
        // 기존 Action 찾기
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T action = AssetDatabase.LoadAssetAtPath<T>(path);
            if (action != null)
            {
                return action;
            }
        }
        
        // Action이 없으면 생성 (메모리에서만)
        T newAction = ScriptableObject.CreateInstance<T>();
        newAction.name = typeof(T).Name;
        
        return newAction;
    }
    
    private TaskTarget GetOrCreateEnemyTarget()
    {
        StringTarget target = ScriptableObject.CreateInstance<StringTarget>();
        target.name = $"EnemyTarget_{questId}";
        
        SerializedObject serializedTarget = new SerializedObject(target);
        string enemyId = string.IsNullOrEmpty(targetId) ? "enemy_001" : targetId;
        serializedTarget.FindProperty("value").stringValue = enemyId;
        serializedTarget.ApplyModifiedProperties();
        
        return target;
    }
    
    private TaskTarget GetOrCreateItemTarget()
    {
        ItemTarget target = ScriptableObject.CreateInstance<ItemTarget>();
        target.name = $"ItemTarget_{questId}";
        
        SerializedObject serializedTarget = new SerializedObject(target);
        string itemId = string.IsNullOrEmpty(targetId) ? "item_001" : targetId;
        string itemName = string.IsNullOrEmpty(targetName) ? "테스트 아이템" : targetName;
        serializedTarget.FindProperty("itemId").stringValue = itemId;
        serializedTarget.FindProperty("itemName").stringValue = itemName;
        serializedTarget.FindProperty("requiredAmount").intValue = needSuccessToComplete;
        serializedTarget.ApplyModifiedProperties();
        
        return target;
    }
    
    private TaskTarget GetOrCreateLocationTarget()
    {
        LocationTarget target = ScriptableObject.CreateInstance<LocationTarget>();
        target.name = $"LocationTarget_{questId}";
        
        SerializedObject serializedTarget = new SerializedObject(target);
        Vector3 position = targetPosition == Vector3.zero ? Vector3.zero : targetPosition;
        float radius = targetRadius <= 0 ? 5f : targetRadius;
        string locationName = string.IsNullOrEmpty(targetName) ? "목표 지점" : targetName;
        
        serializedTarget.FindProperty("targetPosition").vector3Value = position;
        serializedTarget.FindProperty("reachDistance").floatValue = radius;
        serializedTarget.FindProperty("locationName").stringValue = locationName;
        serializedTarget.ApplyModifiedProperties();
        
        return target;
    }
    
    private TaskTarget GetOrCreateNPCTarget()
    {
        NPCTarget target = ScriptableObject.CreateInstance<NPCTarget>();
        target.name = $"NPCTarget_{questId}";
        
        SerializedObject serializedTarget = new SerializedObject(target);
        string npcId = string.IsNullOrEmpty(targetId) ? "npc_001" : targetId;
        string npcName = string.IsNullOrEmpty(targetName) ? "테스트 NPC" : targetName;
        
        serializedTarget.FindProperty("npcId").stringValue = npcId;
        serializedTarget.FindProperty("npcName").stringValue = npcName;
        serializedTarget.FindProperty("npcTag").stringValue = "NPC";
        serializedTarget.ApplyModifiedProperties();
        
        return target;
    }
    
    private void CreateKillQuest()
    {
        try
        {
            questId = "kill_enemy_quest";
            questName = "적 처치";
            questDescription = "적을 처치하세요";
            questCategory = QuestCategory.Combat;
            taskType = TaskType.KillEnemy;
            taskDescription = "적 처치";
            needSuccessToComplete = 3;
            targetId = "goblin_001";
            targetName = "고블린";
            rewardType = RewardType.Item;
            rewardItemName = "경험치";
            rewardAmount = 100;
            
            CreateQuest();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("오류", $"처치 퀘스트 생성 중 오류가 발생했습니다:\n{e.Message}", "확인");
            Debug.LogError($"Kill Quest Creation Error: {e}");
        }
    }
    
    private void CreateCollectQuest()
    {
        try
        {
            questId = "collect_item_quest";
            questName = "아이템 수집";
            questDescription = "아이템을 수집하세요";
            questCategory = QuestCategory.Collection;
            taskType = TaskType.CollectItem;
            taskDescription = "아이템 수집";
            needSuccessToComplete = 5;
            targetId = "herb_001";
            targetName = "치유 허브";
            rewardType = RewardType.Item;
            rewardItemName = "골드";
            rewardAmount = 50;
            
            CreateQuest();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("오류", $"수집 퀘스트 생성 중 오류가 발생했습니다:\n{e.Message}", "확인");
            Debug.LogError($"Collect Quest Creation Error: {e}");
        }
    }
    
    private void CreateReachQuest()
    {
        try
        {
            questId = "reach_location_quest";
            questName = "목표 지점 도달";
            questDescription = "지정된 위치에 도달하세요";
            questCategory = QuestCategory.Exploration;
            taskType = TaskType.ReachLocation;
            taskDescription = "목표 지점 도달";
            needSuccessToComplete = 1;
            targetId = "ancient_ruins";
            targetName = "고대 유적지";
            targetPosition = new Vector3(100, 0, 50);
            targetRadius = 10f;
            rewardType = RewardType.Item;
            rewardItemName = "탐험가의 증표";
            rewardAmount = 1;
            
            CreateQuest();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("오류", $"이동 퀘스트 생성 중 오류가 발생했습니다:\n{e.Message}", "확인");
            Debug.LogError($"Reach Quest Creation Error: {e}");
        }
    }
    
    private void CreateTalkQuest()
    {
        try
        {
            questId = "talk_npc_quest";
            questName = "NPC와 대화";
            questDescription = "NPC와 대화하세요";
            questCategory = QuestCategory.Social;
            taskType = TaskType.TalkToNPC;
            taskDescription = "NPC 대화";
            needSuccessToComplete = 1;
            targetId = "village_elder";
            targetName = "마을 장로";
            rewardType = RewardType.Item;
            rewardItemName = "정보";
            rewardAmount = 1;
            
            CreateQuest();
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("오류", $"대화 퀘스트 생성 중 오류가 발생했습니다:\n{e.Message}", "확인");
            Debug.LogError($"Talk Quest Creation Error: {e}");
        }
    }
    
    private void FindEnemyIDs()
    {
        string message = "씬에서 찾은 적 ID들:\n\n";
        
        // Enemy 태그를 가진 오브젝트들 찾기
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        if (enemies.Length > 0)
        {
            message += "Enemy 태그 오브젝트들:\n";
            foreach (var enemy in enemies)
            {
                message += $"• {enemy.name}\n";
            }
        }
        
        // "Enemy"가 이름에 포함된 오브젝트들 찾기
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        var enemyObjects = allObjects.Where(obj => obj.name.ToLower().Contains("enemy") || 
                                                  obj.name.ToLower().Contains("goblin") ||
                                                  obj.name.ToLower().Contains("orc") ||
                                                  obj.name.ToLower().Contains("skeleton")).ToArray();
        
        if (enemyObjects.Length > 0)
        {
            message += "\n적으로 보이는 오브젝트들:\n";
            foreach (var obj in enemyObjects)
            {
                message += $"• {obj.name} (Tag: {obj.tag})\n";
            }
        }
        
        if (enemies.Length == 0 && enemyObjects.Length == 0)
        {
            message += "씬에서 적을 찾을 수 없습니다.\n";
            message += "적 오브젝트에 'Enemy' 태그를 설정하거나\n";
            message += "이름에 'enemy', 'goblin', 'orc' 등을 포함시키세요.";
        }
        
        EditorUtility.DisplayDialog("적 ID 목록", message, "확인");
    }
    
    private void FindItemIDs()
    {
        string message = "프로젝트에서 찾은 아이템 ID들:\n\n";
        
        // ItemData ScriptableObject들 찾기
        string[] guids = AssetDatabase.FindAssets("t:ItemData");
        if (guids.Length > 0)
        {
            message += "ItemData 에셋들:\n";
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (itemData != null)
                {
                    message += $"• {itemData.itemId} ({itemData.itemName})\n";
                }
            }
        }
        
        // CollectibleItem 컴포넌트를 가진 오브젝트들 찾기
        CollectibleItem[] collectibles = FindObjectsOfType<CollectibleItem>();
        if (collectibles.Length > 0)
        {
            message += "\n씬의 수집 가능한 아이템들:\n";
            foreach (var collectible in collectibles)
            {
                message += $"• {collectible.gameObject.name}\n";
            }
        }
        
        if (guids.Length == 0 && collectibles.Length == 0)
        {
            message += "아이템을 찾을 수 없습니다.\n";
            message += "ItemData ScriptableObject를 생성하거나\n";
            message += "CollectibleItem 컴포넌트를 추가하세요.";
        }
        
        EditorUtility.DisplayDialog("아이템 ID 목록", message, "확인");
    }
    
    private void FindNPCIDs()
    {
        string message = "씬에서 찾은 NPC ID들:\n\n";
        
        // NPC 태그를 가진 오브젝트들 찾기
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        if (npcs.Length > 0)
        {
            message += "NPC 태그 오브젝트들:\n";
            foreach (var npc in npcs)
            {
                message += $"• {npc.name}\n";
            }
        }
        
        // "NPC"가 이름에 포함된 오브젝트들 찾기
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        var npcObjects = allObjects.Where(obj => obj.name.ToLower().Contains("npc") || 
                                                obj.name.ToLower().Contains("merchant") ||
                                                obj.name.ToLower().Contains("elder") ||
                                                obj.name.ToLower().Contains("villager")).ToArray();
        
        if (npcObjects.Length > 0)
        {
            message += "\nNPC로 보이는 오브젝트들:\n";
            foreach (var obj in npcObjects)
            {
                message += $"• {obj.name} (Tag: {obj.tag})\n";
            }
        }
        
        if (npcs.Length == 0 && npcObjects.Length == 0)
        {
            message += "씬에서 NPC를 찾을 수 없습니다.\n";
            message += "NPC 오브젝트에 'NPC' 태그를 설정하거나\n";
            message += "이름에 'npc', 'merchant', 'elder' 등을 포함시키세요.";
        }
        
        EditorUtility.DisplayDialog("NPC ID 목록", message, "확인");
    }
    
    private void FindLocationIDs()
    {
        string message = "씬에서 찾은 위치 ID들:\n\n";
        
        // 특정 이름을 가진 오브젝트들 찾기
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        var locationObjects = allObjects.Where(obj => obj.name.ToLower().Contains("ruins") || 
                                                     obj.name.ToLower().Contains("village") ||
                                                     obj.name.ToLower().Contains("castle") ||
                                                     obj.name.ToLower().Contains("cave") ||
                                                     obj.name.ToLower().Contains("temple")).ToArray();
        
        if (locationObjects.Length > 0)
        {
            message += "위치로 보이는 오브젝트들:\n";
            foreach (var obj in locationObjects)
            {
                Vector3 pos = obj.transform.position;
                message += $"• {obj.name} (위치: {pos.x:F1}, {pos.y:F1}, {pos.z:F1})\n";
            }
        }
        
        // 빈 GameObject들 (마커용)
        var emptyObjects = allObjects.Where(obj => obj.transform.childCount == 0 && 
                                                  obj.GetComponents<Component>().Length == 1).ToArray();
        
        if (emptyObjects.Length > 0)
        {
            message += "\n마커용 빈 오브젝트들:\n";
            foreach (var obj in emptyObjects.Take(10)) // 최대 10개만 표시
            {
                Vector3 pos = obj.transform.position;
                message += $"• {obj.name} (위치: {pos.x:F1}, {pos.y:F1}, {pos.z:F1})\n";
            }
        }
        
        if (locationObjects.Length == 0 && emptyObjects.Length == 0)
        {
            message += "위치를 찾을 수 없습니다.\n";
            message += "위치 마커 오브젝트를 생성하거나\n";
            message += "이름에 'ruins', 'village', 'castle' 등을 포함시키세요.";
        }
        
        EditorUtility.DisplayDialog("위치 ID 목록", message, "확인");
    }
}

// 열거형들
public enum QuestCategory
{
    Combat,
    Collection,
    Exploration,
    Social,
    Survival
}

public enum TaskType
{
    KillEnemy,
    CollectItem,
    ReachLocation,
    TalkToNPC,
    SurviveTime
}

public enum RewardType
{
    Item,
    Experience,
    Gold
}
