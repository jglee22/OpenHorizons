using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 아이템 데이터 자동 생성기 (에디터 전용)
/// </summary>
public class ItemDataGenerator : EditorWindow
{
    [System.Serializable]
    public class ItemTemplate
    {
        public string itemId;
        public string itemName;
        public string description;
        public ItemType itemType;
        public bool isStackable = true;
        public int maxStackSize = 99;
        public float value = 0f;
        public bool isConsumable = false;
        public bool isEquippable = false;
        public float healthRestore = 0f;
        public float staminaRestore = 0f;
        public string iconPath = ""; // Resources 폴더 기준 경로
    }
    
    [Header("아이템 템플릿")]
    [SerializeField] private List<ItemTemplate> itemTemplates = new List<ItemTemplate>();
    
    [Header("설정")]
    [SerializeField] private string outputPath = "Assets/Scripts/Inventory/ScriptableObjects/Items";
    [SerializeField] private bool createFoldersByType = true;
    [SerializeField] private bool updateItemDatabase = true;
    
    private ItemDatabase itemDatabase;
    
    [MenuItem("Tools/Inventory/Item Data Generator")]
    public static void ShowWindow()
    {
        GetWindow<ItemDataGenerator>("Item Data Generator");
    }
    
    private void OnEnable()
    {
        // 기본 아이템 템플릿들 추가
        InitializeDefaultTemplates();
        
        // ItemDatabase 찾기
        itemDatabase = FindObjectOfType<ItemDatabase>();
        if (itemDatabase == null)
        {
            itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
        }
    }
    
    private void OnGUI()
    {
        GUILayout.Label("아이템 데이터 자동 생성기", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        // 설정
        EditorGUILayout.LabelField("설정", EditorStyles.boldLabel);
        outputPath = EditorGUILayout.TextField("출력 경로", outputPath);
        createFoldersByType = EditorGUILayout.Toggle("타입별 폴더 생성", createFoldersByType);
        updateItemDatabase = EditorGUILayout.Toggle("ItemDatabase 자동 업데이트", updateItemDatabase);
        
        GUILayout.Space(10);
        
        // ItemDatabase 참조
        EditorGUILayout.LabelField("ItemDatabase", EditorStyles.boldLabel);
        itemDatabase = (ItemDatabase)EditorGUILayout.ObjectField("ItemDatabase", itemDatabase, typeof(ItemDatabase), false);
        
        GUILayout.Space(10);
        
        // 아이템 템플릿들
        EditorGUILayout.LabelField("아이템 템플릿", EditorStyles.boldLabel);
        
        if (GUILayout.Button("기본 템플릿 추가"))
        {
            AddDefaultTemplates();
        }
        
        if (GUILayout.Button("템플릿 초기화"))
        {
            InitializeDefaultTemplates();
        }
        
        GUILayout.Space(5);
        
        // 템플릿 리스트
        for (int i = 0; i < itemTemplates.Count; i++)
        {
            DrawItemTemplate(i);
        }
        
        if (GUILayout.Button("템플릿 추가"))
        {
            itemTemplates.Add(new ItemTemplate());
        }
        
        GUILayout.Space(20);
        
        // 생성 버튼
        GUI.enabled = itemTemplates.Count > 0;
        if (GUILayout.Button("아이템 데이터 생성", GUILayout.Height(30)))
        {
            GenerateItemData();
        }
        GUI.enabled = true;
        
        GUILayout.Space(10);
        
        // 도움말
        EditorGUILayout.HelpBox(
            "1. 아이템 템플릿을 설정하세요\n" +
            "2. 아이콘은 Resources 폴더에 넣고 경로를 입력하세요\n" +
            "3. '아이템 데이터 생성' 버튼을 클릭하세요",
            MessageType.Info);
    }
    
    private void DrawItemTemplate(int index)
    {
        var template = itemTemplates[index];
        
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"템플릿 {index + 1}", EditorStyles.boldLabel);
        if (GUILayout.Button("X", GUILayout.Width(20)))
        {
            itemTemplates.RemoveAt(index);
            return;
        }
        EditorGUILayout.EndHorizontal();
        
        template.itemId = EditorGUILayout.TextField("Item ID", template.itemId);
        template.itemName = EditorGUILayout.TextField("Item Name", template.itemName);
        template.description = EditorGUILayout.TextField("Description", template.description);
        template.itemType = (ItemType)EditorGUILayout.EnumPopup("Item Type", template.itemType);
        
        EditorGUILayout.BeginHorizontal();
        template.isStackable = EditorGUILayout.Toggle("Stackable", template.isStackable);
        if (template.isStackable)
        {
            template.maxStackSize = EditorGUILayout.IntField("Max Stack", template.maxStackSize);
        }
        EditorGUILayout.EndHorizontal();
        
        template.value = EditorGUILayout.FloatField("Value", template.value);
        
        EditorGUILayout.BeginHorizontal();
        template.isConsumable = EditorGUILayout.Toggle("Consumable", template.isConsumable);
        template.isEquippable = EditorGUILayout.Toggle("Equippable", template.isEquippable);
        EditorGUILayout.EndHorizontal();
        
        if (template.isConsumable)
        {
            template.healthRestore = EditorGUILayout.FloatField("Health Restore", template.healthRestore);
            template.staminaRestore = EditorGUILayout.FloatField("Stamina Restore", template.staminaRestore);
        }
        
        template.iconPath = EditorGUILayout.TextField("Icon Path", template.iconPath);
        
        EditorGUILayout.EndVertical();
    }
    
    private void InitializeDefaultTemplates()
    {
        itemTemplates.Clear();
        
        // 기본 아이템들
        itemTemplates.Add(new ItemTemplate
        {
            itemId = "bronze_sword",
            itemName = "Bronze Sword",
            description = "청동으로 만든 견고한 검입니다.",
            itemType = ItemType.Weapon,
            isStackable = false,
            maxStackSize = 1,
            value = 100f,
            isEquippable = true,
            iconPath = "Bronze_Sword"
        });
        
        itemTemplates.Add(new ItemTemplate
        {
            itemId = "health_potion",
            itemName = "Health Potion",
            description = "체력을 회복시키는 물약입니다.",
            itemType = ItemType.Food,
            isStackable = true,
            maxStackSize = 99,
            value = 25f,
            isConsumable = true,
            healthRestore = 50f,
            iconPath = "Health_Potion"
        });
        
        itemTemplates.Add(new ItemTemplate
        {
            itemId = "iron_ore",
            itemName = "Iron Ore",
            description = "제련에 사용되는 철광석입니다.",
            itemType = ItemType.Material,
            isStackable = true,
            maxStackSize = 99,
            value = 10f,
            iconPath = "Iron_Ore"
        });
        
        itemTemplates.Add(new ItemTemplate
        {
            itemId = "leather_armor",
            itemName = "Leather Armor",
            description = "가죽으로 만든 기본적인 갑옷입니다.",
            itemType = ItemType.Armor,
            isStackable = false,
            maxStackSize = 1,
            value = 75f,
            isEquippable = true,
            iconPath = "Leather_Armor"
        });
    }
    
    private void AddDefaultTemplates()
    {
        InitializeDefaultTemplates();
    }
    
    private void GenerateItemData()
    {
        if (itemTemplates.Count == 0)
        {
            EditorUtility.DisplayDialog("오류", "생성할 아이템 템플릿이 없습니다.", "확인");
            return;
        }
        
        // 출력 폴더 생성
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }
        
        List<ItemData> createdItems = new List<ItemData>();
        
        foreach (var template in itemTemplates)
        {
            if (string.IsNullOrEmpty(template.itemId) || string.IsNullOrEmpty(template.itemName))
            {
                Debug.LogWarning($"템플릿이 비어있습니다: {template.itemName}");
                continue;
            }
            
            // ItemData 생성
            ItemData itemData = ScriptableObject.CreateInstance<ItemData>();
            itemData.itemId = template.itemId;
            itemData.itemName = template.itemName;
            itemData.description = template.description;
            itemData.itemType = template.itemType;
            itemData.isStackable = template.isStackable;
            itemData.maxStackSize = template.maxStackSize;
            itemData.value = template.value;
            itemData.isConsumable = template.isConsumable;
            itemData.isEquippable = template.isEquippable;
            itemData.healthRestore = template.healthRestore;
            itemData.staminaRestore = template.staminaRestore;
            
            // 아이콘 로드
            if (!string.IsNullOrEmpty(template.iconPath))
            {
                itemData.icon = Resources.Load<Sprite>(template.iconPath);
                if (itemData.icon == null)
                {
                    Debug.LogWarning($"아이콘을 찾을 수 없습니다: {template.iconPath}");
                }
            }
            
            // 파일 저장
            string fileName = $"{template.itemId}.asset";
            string filePath = Path.Combine(outputPath, fileName);
            
            if (createFoldersByType)
            {
                string typeFolder = Path.Combine(outputPath, template.itemType.ToString());
                if (!Directory.Exists(typeFolder))
                {
                    Directory.CreateDirectory(typeFolder);
                }
                filePath = Path.Combine(typeFolder, fileName);
            }
            
            AssetDatabase.CreateAsset(itemData, filePath);
            createdItems.Add(itemData);
            
            Debug.Log($"아이템 데이터 생성됨: {filePath}");
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        // ItemDatabase 업데이트
        if (updateItemDatabase && itemDatabase != null)
        {
            UpdateItemDatabase(createdItems);
        }
        
        EditorUtility.DisplayDialog("완료", 
            $"{createdItems.Count}개의 아이템 데이터가 생성되었습니다!\n경로: {outputPath}", "확인");
    }
    
    private void UpdateItemDatabase(List<ItemData> newItems)
    {
        if (itemDatabase == null)
        {
            Debug.LogWarning("ItemDatabase가 설정되지 않았습니다.");
            return;
        }
        
        // ItemDatabase에 새 아이템들 추가
        foreach (var item in newItems)
        {
            itemDatabase.AddItem(item);
        }
        
        EditorUtility.SetDirty(itemDatabase);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"ItemDatabase에 {newItems.Count}개의 아이템이 추가되었습니다.");
    }
}
