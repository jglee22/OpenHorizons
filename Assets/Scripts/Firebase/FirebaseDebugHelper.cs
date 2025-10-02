using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Analytics;
using Firebase.Crashlytics;
using System.Threading.Tasks;

/// <summary>
/// Firebase 디버그 헬퍼
/// </summary>
public class FirebaseDebugHelper : MonoBehaviour
{
    [Header("디버그 설정")]
    public bool enableDebugLogs = true;
    public bool enableDetailedLogs = false;
    
    private void Start()
    {
        if (enableDebugLogs)
        {
            Debug.Log("🔍 Firebase 디버그 헬퍼 시작");
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            RunFullDebugTest();
        }
        
        if (Input.GetKeyDown(KeyCode.F10))
        {
            TestFirebaseServices();
        }
    }
    
    /// <summary>
    /// 전체 디버그 테스트 실행
    /// </summary>
    public async void RunFullDebugTest()
    {
        Debug.Log("🔍 Firebase 전체 디버그 테스트 시작...");
        
        try
        {
            // 1. Firebase 앱 상태 확인
            DebugFirebaseApp();
            
            // 2. Firebase Auth 상태 확인
            DebugFirebaseAuth();
            
            // 3. Firebase Database 상태 확인
            DebugFirebaseDatabase();
            
            // 4. Firebase Storage 상태 확인
            DebugFirebaseStorage();
            
            // 5. Firebase Analytics 상태 확인
            DebugFirebaseAnalytics();
            
            // 6. Firebase Crashlytics 상태 확인
            DebugFirebaseCrashlytics();
            
            Debug.Log("✅ Firebase 전체 디버그 테스트 완료!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Firebase 디버그 테스트 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// Firebase 서비스 테스트
    /// </summary>
    public async void TestFirebaseServices()
    {
        Debug.Log("🔍 Firebase 서비스 테스트 시작...");
        
        try
        {
            // Analytics 이벤트 전송
            FirebaseAnalytics.LogEvent("debug_test");
            Debug.Log("   Analytics 이벤트 전송됨");
            
            // Crashlytics 로그 전송
            Crashlytics.Log("Firebase 서비스 테스트");
            Debug.Log("   Crashlytics 로그 전송됨");
            
            Debug.Log("✅ Firebase 서비스 테스트 완료!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Firebase 서비스 테스트 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// Firebase 앱 디버그
    /// </summary>
    private async void DebugFirebaseApp()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            Debug.Log($"   의존성 상태: {dependencyStatus}");
            
            var app = FirebaseApp.DefaultInstance;
            if (app != null)
            {
                Debug.Log($"   앱 이름: {app.Name}");
                Debug.Log($"   프로젝트 ID: {app.Options.ProjectId}");
                Debug.Log($"   앱 ID: {app.Options.AppId}");
                
                if (enableDetailedLogs)
                {
                    Debug.Log($"   API 키: {app.Options.ApiKey}");
                    Debug.Log($"   스토리지 버킷: {app.Options.StorageBucket}");
                }
            }
            else
            {
                Debug.LogError("   Firebase 앱이 null입니다!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase 앱 디버그 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// Firebase Auth 디버그
    /// </summary>
    private async void DebugFirebaseAuth()
    {
        try
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth != null)
            {
                Debug.Log("   Firebase Auth 인스턴스 생성됨");
                Debug.Log($"   현재 사용자: {(auth.CurrentUser != null ? auth.CurrentUser.Email : "없음")}");
                
                if (auth.CurrentUser != null)
                {
                    Debug.Log($"   사용자 ID: {auth.CurrentUser.UserId}");
                    Debug.Log($"   이메일 확인됨: {auth.CurrentUser.IsEmailVerified}");
                    Debug.Log($"   익명 사용자: {auth.CurrentUser.IsAnonymous}");
                    
                    if (enableDetailedLogs)
                    {
                        Debug.Log($"   사용자 정보 상세 로그 활성화됨");
                    }
                }
            }
            else
            {
                Debug.LogError("   Firebase Auth가 null입니다!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase Auth 디버그 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// Firebase Database 디버그
    /// </summary>
    private async void DebugFirebaseDatabase()
    {
        try
        {
            var database = FirebaseDatabase.DefaultInstance;
            if (database != null)
            {
                Debug.Log("   Firebase Database 인스턴스 생성됨");
                
                if (enableDetailedLogs)
                {
                    Debug.Log($"   Root Reference: {database.RootReference.Key}");
                }
            }
            else
            {
                Debug.LogError("   Firebase Database가 null입니다!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase Database 디버그 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// Firebase Storage 디버그
    /// </summary>
    private async void DebugFirebaseStorage()
    {
        try
        {
            var storage = FirebaseStorage.DefaultInstance;
            if (storage != null)
            {
                Debug.Log("   Firebase Storage 인스턴스 생성됨");
                Debug.Log($"   Storage URL: {storage.RootReference.ToString()}");
                
                if (enableDetailedLogs)
                {
                    Debug.Log($"   Root Reference: {storage.RootReference.Name}");
                    Debug.Log($"   Storage URL: {storage.RootReference.ToString()}");
                }
            }
            else
            {
                Debug.LogError("   Firebase Storage가 null입니다!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase Storage 디버그 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// Firebase Analytics 디버그
    /// </summary>
    private async void DebugFirebaseAnalytics()
    {
        try
        {
            Debug.Log("   Firebase Analytics 사용 가능");
            
            if (enableDetailedLogs)
            {
                // Analytics 이벤트 전송
                FirebaseAnalytics.LogEvent("debug_analytics_test");
                Debug.Log("   Analytics 테스트 이벤트 전송됨");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase Analytics 디버그 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// Firebase Crashlytics 디버그
    /// </summary>
    private async void DebugFirebaseCrashlytics()
    {
        try
        {
            Debug.Log("   Firebase Crashlytics 사용 가능");
            
            if (enableDetailedLogs)
            {
                // Crashlytics 로그 전송
                Crashlytics.Log("Firebase 디버그 테스트");
                Debug.Log("   Crashlytics 테스트 로그 전송됨");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"   Firebase Crashlytics 디버그 실패: {e.Message}");
        }
    }
    
}