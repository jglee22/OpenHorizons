using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 퀘스트 안내 UI 매니저
/// </summary>
public class QuestGuidanceUI : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private Canvas questCanvas;
    [SerializeField] private Transform questListParent;
    [SerializeField] private GameObject questItemPrefab;
    [SerializeField] private GameObject questMarkerPrefab;
    
    [Header("안내 설정")]
    [SerializeField] private float markerUpdateInterval = 0.5f;
    [SerializeField] private float markerDistanceThreshold = 50f;
    
    private List<QuestGuidanceItem> activeGuidanceItems = new List<QuestGuidanceItem>();
    private List<GameObject> questMarkers = new List<GameObject>();
    private Camera playerCamera;
    private Transform playerTransform;
    
    private void Start()
    {
        InitializeUI();
        FindPlayerReferences();
        
        // QuestSystem 이벤트 구독
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.onQuestRegistered += OnQuestRegistered;
            QuestSystem.Instance.onQuestCompleted += OnQuestCompleted;
            QuestSystem.Instance.onQuestCanceled += OnQuestCanceled;
        }
        
        // 주기적으로 마커 업데이트
        InvokeRepeating(nameof(UpdateQuestMarkers), 0f, markerUpdateInterval);
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (QuestSystem.Instance != null)
        {
            QuestSystem.Instance.onQuestRegistered -= OnQuestRegistered;
            QuestSystem.Instance.onQuestCompleted -= OnQuestCompleted;
            QuestSystem.Instance.onQuestCanceled -= OnQuestCanceled;
        }
    }
    
    private void InitializeUI()
    {
        // QuestCanvas가 없으면 생성
        if (questCanvas == null)
        {
            GameObject canvasObj = new GameObject("QuestGuidanceCanvas");
            questCanvas = canvasObj.AddComponent<Canvas>();
            questCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            questCanvas.sortingOrder = 50; // 다른 UI보다 위에 표시
            
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // QuestListParent가 없으면 생성
        if (questListParent == null)
        {
            GameObject listObj = new GameObject("QuestList");
            listObj.transform.SetParent(questCanvas.transform, false);
            
            RectTransform rect = listObj.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(20, -20);
            rect.sizeDelta = new Vector2(400, 300);
            
            questListParent = listObj.transform;
            
            // 스크롤뷰 추가
            ScrollRect scrollRect = listObj.AddComponent<ScrollRect>();
            listObj.AddComponent<Image>().color = new Color(0, 0, 0, 0.3f);
            
            // Content 영역
            GameObject content = new GameObject("Content");
            content.transform.SetParent(listObj.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            
            VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
            layoutGroup.spacing = 5;
            layoutGroup.padding = new RectOffset(10, 10, 10, 10);
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childForceExpandWidth = true;
            
            ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            scrollRect.content = contentRect;
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            
            questListParent = content.transform;
        }
    }
    
    private void FindPlayerReferences()
    {
        // 플레이어 카메라 찾기
        playerCamera = Camera.main;
        if (playerCamera == null)
            playerCamera = FindObjectOfType<Camera>();
        
        // 플레이어 Transform 찾기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            playerTransform = player.transform;
    }
    
    private void OnQuestRegistered(Quest quest)
    {
        CreateQuestGuidanceItem(quest);
        Debug.Log($"[QuestGuidanceUI] 퀘스트 등록됨: {quest.DisplayName}");
    }
    
    private void OnQuestCompleted(Quest quest)
    {
        RemoveQuestGuidanceItem(quest);
        Debug.Log($"[QuestGuidanceUI] 퀘스트 완료됨: {quest.DisplayName}");
    }
    
    private void OnQuestCanceled(Quest quest)
    {
        RemoveQuestGuidanceItem(quest);
        Debug.Log($"[QuestGuidanceUI] 퀘스트 취소됨: {quest.DisplayName}");
    }
    
    private void CreateQuestGuidanceItem(Quest quest)
    {
        GameObject itemObj = new GameObject($"QuestGuidance_{quest.CodeName}");
        itemObj.transform.SetParent(questListParent, false);
        
        RectTransform rect = itemObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(380, 80);
        
        Image bg = itemObj.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // 퀘스트 제목
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(itemObj.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 25);
        titleRect.sizeDelta = new Vector2(360, 25);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 16;
        titleText.color = Color.yellow;
        titleText.alignment = TextAnchor.MiddleLeft;
        titleText.text = quest.DisplayName;
        
        // 퀘스트 설명
        GameObject descObj = new GameObject("Description");
        descObj.transform.SetParent(itemObj.transform, false);
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchoredPosition = new Vector2(0, 0);
        descRect.sizeDelta = new Vector2(360, 20);
        Text descText = descObj.AddComponent<Text>();
        descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        descText.fontSize = 12;
        descText.color = Color.white;
        descText.alignment = TextAnchor.MiddleLeft;
        descText.text = quest.Description;
        
        // 진행 상황
        GameObject progressObj = new GameObject("Progress");
        progressObj.transform.SetParent(itemObj.transform, false);
        RectTransform progressRect = progressObj.AddComponent<RectTransform>();
        progressRect.anchoredPosition = new Vector2(0, -25);
        progressRect.sizeDelta = new Vector2(360, 20);
        Text progressText = progressObj.AddComponent<Text>();
        progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        progressText.fontSize = 12;
        progressText.color = Color.green;
        progressText.alignment = TextAnchor.MiddleLeft;
        
        QuestGuidanceItem guidanceItem = itemObj.AddComponent<QuestGuidanceItem>();
        guidanceItem.Initialize(quest, titleText, descText, progressText);
        
        activeGuidanceItems.Add(guidanceItem);
    }
    
    private void RemoveQuestGuidanceItem(Quest quest)
    {
        var item = activeGuidanceItems.FirstOrDefault(x => x.Quest.CodeName == quest.CodeName);
        if (item != null)
        {
            activeGuidanceItems.Remove(item);
            Destroy(item.gameObject);
        }
    }
    
    private void UpdateQuestMarkers()
    {
        if (playerCamera == null || playerTransform == null) return;
        
        // 기존 마커들 제거
        foreach (var marker in questMarkers)
        {
            if (marker != null) Destroy(marker);
        }
        questMarkers.Clear();
        
        // 각 활성 퀘스트의 목표에 마커 생성
        foreach (var guidanceItem in activeGuidanceItems)
        {
            CreateMarkersForQuest(guidanceItem.Quest);
        }
    }
    
    private void CreateMarkersForQuest(Quest quest)
    {
        if (quest.CurrentTaskGroup == null) return;
        
        foreach (var task in quest.CurrentTaskGroup.Tasks)
        {
            if (task.IsComplete) continue;
            
            // 위치 기반 목표 (ReachLocationAction)
            if (task.Action is ReachLocationAction locationAction)
            {
                CreateLocationMarker(locationAction.targetPosition, quest.DisplayName, task);
            }
            
            // 아이템 수집 목표 (CollectItemAction)
            else if (task.Action is CollectItemAction collectAction)
            {
                CreateItemCollectionMarker(collectAction, quest.DisplayName, task);
            }
            
            // NPC 대화 목표 (TalkToNPCAction)
            else if (task.Action is TalkToNPCAction npcAction)
            {
                CreateNPCMarker(npcAction, quest.DisplayName, task);
            }
        }
    }
    
    private void CreateLocationMarker(Vector3 targetPosition, string questName, Task task)
    {
        // 거리 체크
        float distance = Vector3.Distance(playerTransform.position, targetPosition);
        if (distance > markerDistanceThreshold) return;
        
        GameObject marker = new GameObject($"LocationMarker_{questName}");
        marker.transform.SetParent(questCanvas.transform, false);
        
        RectTransform rect = marker.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(30, 30);
        
        Image image = marker.AddComponent<Image>();
        image.color = Color.red;
        image.sprite = CreateCircleSprite();
        
        // 거리 텍스트
        GameObject textObj = new GameObject("DistanceText");
        textObj.transform.SetParent(marker.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchoredPosition = new Vector2(0, -25);
        textRect.sizeDelta = new Vector2(60, 20);
        Text text = textObj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 10;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = $"{distance:F0}m";
        
        QuestMarker questMarker = marker.AddComponent<QuestMarker>();
        questMarker.Initialize(targetPosition, questName, task);
        
        questMarkers.Add(marker);
    }
    
    private void CreateItemCollectionMarker(CollectItemAction action, string questName, Task task)
    {
        // 씬에서 해당 아이템 찾기
        GameObject[] items = GameObject.FindGameObjectsWithTag("Collectible");
        foreach (var item in items)
        {
            var collectible = item.GetComponent<CollectibleItem>();
            if (collectible != null && collectible.itemId == action.itemId)
            {
                float distance = Vector3.Distance(playerTransform.position, item.transform.position);
                if (distance > markerDistanceThreshold) continue;
                
                GameObject marker = new GameObject($"ItemMarker_{questName}");
                marker.transform.SetParent(questCanvas.transform, false);
                
                RectTransform rect = marker.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(25, 25);
                
                Image image = marker.AddComponent<Image>();
                image.color = Color.green;
                image.sprite = CreateCircleSprite();
                
                QuestMarker questMarker = marker.AddComponent<QuestMarker>();
                questMarker.Initialize(item.transform.position, questName, task);
                
                questMarkers.Add(marker);
            }
        }
    }
    
    private void CreateNPCMarker(TalkToNPCAction action, string questName, Task task)
    {
        // 씬에서 해당 NPC 찾기
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (var npc in npcs)
        {
            if (npc.name.Contains(action.npcId))
            {
                float distance = Vector3.Distance(playerTransform.position, npc.transform.position);
                if (distance > markerDistanceThreshold) continue;
                
                GameObject marker = new GameObject($"NPCMarker_{questName}");
                marker.transform.SetParent(questCanvas.transform, false);
                
                RectTransform rect = marker.AddComponent<RectTransform>();
                rect.sizeDelta = new Vector2(25, 25);
                
                Image image = marker.AddComponent<Image>();
                image.color = Color.blue;
                image.sprite = CreateCircleSprite();
                
                QuestMarker questMarker = marker.AddComponent<QuestMarker>();
                questMarker.Initialize(npc.transform.position, questName, task);
                
                questMarkers.Add(marker);
            }
        }
    }
    
    private Sprite CreateCircleSprite()
    {
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        Vector2 center = new Vector2(16, 16);
        float radius = 15f;
        
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * 32 + x] = distance <= radius ? Color.white : Color.clear;
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    private void Update()
    {
        // 마커 위치 업데이트
        foreach (var marker in questMarkers)
        {
            if (marker != null)
            {
                var questMarker = marker.GetComponent<QuestMarker>();
                if (questMarker != null)
                {
                    questMarker.UpdatePosition(playerCamera);
                }
            }
        }
    }
}
