using UnityEngine;
using Firebase.Database;
using System.Threading.Tasks;

/// <summary>
/// Firebase 데이터베이스 관리자
/// </summary>
public class FirebaseDataManager : MonoBehaviour
{
    private DatabaseReference databaseRef;
    
    private void Start()
    {
        if (FirebaseManager.Instance != null)
        {
            databaseRef = FirebaseManager.Instance.Database;
        }
    }
    
    /// <summary>
    /// 플레이어 데이터 저장
    /// </summary>
    public async Task<bool> SavePlayerData(PlayerData playerData)
    {
        try
        {
            if (databaseRef == null)
            {
                Debug.LogError("데이터베이스 참조가 null입니다.");
                return false;
            }
            
            string json = JsonUtility.ToJson(playerData);
            await databaseRef.Child("players").Child(playerData.userId).SetRawJsonValueAsync(json);
            
            Debug.Log($"플레이어 데이터 저장 완료: {playerData.userId}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"플레이어 데이터 저장 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 플레이어 데이터 로드
    /// </summary>
    public async Task<PlayerData> LoadPlayerData(string userId)
    {
        try
        {
            if (databaseRef == null)
            {
                Debug.LogError("데이터베이스 참조가 null입니다.");
                return null;
            }
            
            var snapshot = await databaseRef.Child("players").Child(userId).GetValueAsync();
            
            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
                Debug.Log($"플레이어 데이터 로드 완료: {userId}");
                return playerData;
            }
            else
            {
                Debug.LogWarning($"플레이어 데이터를 찾을 수 없습니다: {userId}");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"플레이어 데이터 로드 실패: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 퀘스트 데이터 저장
    /// </summary>
    public async Task<bool> SaveQuestData(QuestData questData)
    {
        try
        {
            if (databaseRef == null)
            {
                Debug.LogError("데이터베이스 참조가 null입니다.");
                return false;
            }
            
            string json = JsonUtility.ToJson(questData);
            await databaseRef.Child("quests").Child(questData.questId).SetRawJsonValueAsync(json);
            
            Debug.Log($"퀘스트 데이터 저장 완료: {questData.questId}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"퀘스트 데이터 저장 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 퀘스트 데이터 로드
    /// </summary>
    public async Task<QuestData> LoadQuestData(string questId)
    {
        try
        {
            if (databaseRef == null)
            {
                Debug.LogError("데이터베이스 참조가 null입니다.");
                return null;
            }
            
            var snapshot = await databaseRef.Child("quests").Child(questId).GetValueAsync();
            
            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                QuestData questData = JsonUtility.FromJson<QuestData>(json);
                Debug.Log($"퀘스트 데이터 로드 완료: {questId}");
                return questData;
            }
            else
            {
                Debug.LogWarning($"퀘스트 데이터를 찾을 수 없습니다: {questId}");
                return null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"퀘스트 데이터 로드 실패: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// 리더보드 업데이트
    /// </summary>
    public async Task<bool> UpdateLeaderboard(string playerName, int score)
    {
        try
        {
            if (databaseRef == null)
            {
                Debug.LogError("데이터베이스 참조가 null입니다.");
                return false;
            }
            
            var leaderboardEntry = new LeaderboardEntry
            {
                playerName = playerName,
                score = score,
                timestamp = System.DateTime.Now.ToBinary()
            };
            
            string json = JsonUtility.ToJson(leaderboardEntry);
            await databaseRef.Child("leaderboard").Child(System.Guid.NewGuid().ToString()).SetRawJsonValueAsync(json);
            
            Debug.Log($"리더보드 업데이트 완료: {playerName} - {score}");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"리더보드 업데이트 실패: {e.Message}");
            return false;
        }
    }
}

/// <summary>
/// 퀘스트 데이터 구조
/// </summary>
[System.Serializable]
public class QuestData
{
    public string questId;
    public string questName;
    public string description;
    public bool isCompleted;
    public System.DateTime completedAt;
}

/// <summary>
/// 리더보드 엔트리 구조
/// </summary>
[System.Serializable]
public class LeaderboardEntry
{
    public string playerName;
    public int score;
    public long timestamp;
}