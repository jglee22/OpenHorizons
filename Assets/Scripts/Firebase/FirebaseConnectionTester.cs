using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Analytics;
using Firebase.Crashlytics;
using System.Threading.Tasks;

/// <summary>
/// Firebase 연결 테스터
/// </summary>
public class FirebaseConnectionTester : MonoBehaviour
{
    [Header("테스트 설정")]
    public bool autoTestOnStart = false;
    public float testDelay = 3f;
    
    private void Start()
    {
        if (autoTestOnStart)
        {
            Invoke(nameof(RunConnectionTest), testDelay);
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            RunConnectionTest();
        }
    }
    
    /// <summary>
    /// Firebase 연결 테스트 실행
    /// </summary>
    public async void RunConnectionTest()
    {
        Debug.Log("🔍 Firebase 연결 테스트 시작...");
        
        try
        {
            // 1. Firebase 앱 초기화 확인
            bool appInit = await TestFirebaseApp();
            Debug.Log($"   Firebase 앱 초기화: {(appInit ? "성공" : "실패")}");
            
            // 2. Firebase Auth 확인
            bool authInit = await TestFirebaseAuth();
            Debug.Log($"   Firebase Auth: {(authInit ? "성공" : "실패")}");
            
            // 3. Firebase Database 확인
            bool databaseInit = await TestFirebaseDatabase();
            Debug.Log($"   Firebase Database: {(databaseInit ? "성공" : "실패")}");
            
            // 4. Firebase Storage 확인
            bool storageInit = await TestFirebaseStorage();
            Debug.Log($"   Firebase Storage: {(storageInit ? "성공" : "실패")}");
            
            // 5. Firebase Analytics 확인
            bool analyticsInit = await TestFirebaseAnalytics();
            Debug.Log($"   Firebase Analytics: {(analyticsInit ? "성공" : "실패")}");
            
            // 6. Firebase Crashlytics 확인
            bool crashlyticsInit = await TestFirebaseCrashlytics();
            Debug.Log($"   Firebase Crashlytics: {(crashlyticsInit ? "성공" : "실패")}");
            
            Debug.Log("🎉 Firebase 연결 테스트 완료!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Firebase 연결 테스트 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// Firebase 앱 초기화 테스트
    /// </summary>
    private async Task<bool> TestFirebaseApp()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            if (dependencyStatus == DependencyStatus.Available)
            {
                var app = FirebaseApp.DefaultInstance;
                if (app != null)
                {
                    Debug.Log($"   Firebase 앱 이름: {app.Name}");
                    Debug.Log($"   프로젝트 ID: {app.Options.ProjectId}");
                    return true;
                }
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase 앱 초기화 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Firebase Auth 테스트
    /// </summary>
    private async Task<bool> TestFirebaseAuth()
    {
        try
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth != null)
            {
                Debug.Log("   Firebase Auth 인스턴스 생성됨");
                Debug.Log($"   현재 사용자: {(auth.CurrentUser != null ? auth.CurrentUser.Email : "없음")}");
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase Auth가 null입니다: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Firebase Database 테스트
    /// </summary>
    private async Task<bool> TestFirebaseDatabase()
    {
        try
        {
            var database = FirebaseDatabase.DefaultInstance;
            if (database != null)
            {
                Debug.Log("   Firebase Database 인스턴스 생성됨");
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase Database 초기화 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Firebase Storage 테스트
    /// </summary>
    private async Task<bool> TestFirebaseStorage()
    {
        try
        {
            var storage = FirebaseStorage.DefaultInstance;
            if (storage != null)
            {
                Debug.Log("   Firebase Storage 인스턴스 생성됨");
                Debug.Log($"   Storage URL: {storage.RootReference.ToString()}");
                return true;
            }
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase Storage 초기화 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Firebase Analytics 테스트
    /// </summary>
    private async Task<bool> TestFirebaseAnalytics()
    {
        try
        {
            FirebaseAnalytics.LogEvent("connection_test");
            Debug.Log("   Firebase Analytics 이벤트 전송됨");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase Analytics 초기화 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Firebase Crashlytics 테스트
    /// </summary>
    private async Task<bool> TestFirebaseCrashlytics()
    {
        try
        {
            Crashlytics.Log("Firebase 연결 테스트");
            Debug.Log("   Firebase Crashlytics 로그 전송됨");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase Crashlytics 초기화 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 데이터베이스 연결 테스트
    /// </summary>
    public async void TestDatabaseConnection()
    {
        Debug.Log("🔍 데이터베이스 연결 테스트 시작...");
        
        try
        {
            var database = FirebaseDatabase.DefaultInstance;
            var testRef = database.GetReference("test");
            
            // 테스트 데이터 생성
            var testData = new
            {
                message = "Firebase 연결 테스트",
                timestamp = System.DateTime.Now.ToString(),
                randomValue = Random.Range(1, 1000)
            };
            
            // 데이터 쓰기 테스트
            await testRef.SetValueAsync(testData);
            Debug.Log("   데이터 쓰기 성공");
            
            // 데이터 읽기 테스트
            var snapshot = await testRef.GetValueAsync();
            if (snapshot.Exists)
            {
                Debug.Log($"   데이터 읽기 성공: {snapshot.GetRawJsonValue()}");
            }
            
            // 테스트 데이터 삭제
            await testRef.RemoveValueAsync();
            Debug.Log("   테스트 데이터 삭제 완료");
            
            Debug.Log("✅ 데이터베이스 연결 테스트 성공!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 데이터베이스 연결 테스트 실패: {e.Message}");
        }
    }
    
    private void OnGUI()
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 14;
        labelStyle.normal.textColor = Color.white;
        
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.yellow;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 200));
        GUILayout.Label("=== Firebase 연결 테스트 ===", headerStyle);
        GUILayout.Space(5);
        GUILayout.Label("F9: 전체 연결 테스트", labelStyle);
        GUILayout.Label("F10: 데이터베이스 연결 테스트", labelStyle);
        GUILayout.Space(10);
        
        if (GUILayout.Button("전체 연결 테스트"))
        {
            RunConnectionTest();
        }
        
        if (GUILayout.Button("데이터베이스 연결 테스트"))
        {
            TestDatabaseConnection();
        }
        
        GUILayout.EndArea();
    }
}