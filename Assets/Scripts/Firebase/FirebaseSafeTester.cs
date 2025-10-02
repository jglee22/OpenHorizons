using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Analytics;
using System.Threading.Tasks;

/// <summary>
/// 안전한 Firebase 연결 테스트 스크립트 (무한루프 방지)
/// </summary>
public class FirebaseSafeTester : MonoBehaviour
{
    [Header("테스트 설정")]
    public bool enableAutoTest = false;
    public float testDelay = 3f;
    
    private bool isTestRunning = false;
    
    private void Start()
    {
        if (enableAutoTest)
        {
            Invoke(nameof(SafeConnectionTest), testDelay);
        }
    }
    
    private void Update()
    {
        // 수동 테스트 키
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SafeConnectionTest();
        }
    }
    
    /// <summary>
    /// 안전한 연결 테스트 (무한루프 방지)
    /// </summary>
    public async void SafeConnectionTest()
    {
        if (isTestRunning)
        {
            Debug.Log("테스트가 이미 실행 중입니다. 잠시 후 다시 시도해주세요.");
            return;
        }
        
        isTestRunning = true;
        Debug.Log("Firebase 안전 연결 테스트 시작...");
        
        try
        {
            // 1. Firebase 초기화 확인
            bool initTest = await TestFirebaseInit();
            
            // 2. 각 서비스 개별 테스트
            bool authTest = await TestAuthService();
            bool dbTest = await TestDatabaseService();
            bool storageTest = TestStorageService();
            bool analyticsTest = TestAnalyticsService();
            
            // 결과 출력
            Debug.Log("Firebase 안전 테스트 결과:");
            Debug.Log($"  Firebase 초기화: {(initTest ? "성공" : "실패")}");
            Debug.Log($"  인증 서비스: {(authTest ? "성공" : "실패")}");
            Debug.Log($"  데이터베이스: {(dbTest ? "성공" : "실패")}");
            Debug.Log($"  스토리지: {(storageTest ? "성공" : "실패")}");
            Debug.Log($"  Analytics: {(analyticsTest ? "성공" : "실패")}");
            
            int successCount = (initTest ? 1 : 0) + (authTest ? 1 : 0) + (dbTest ? 1 : 0) + (storageTest ? 1 : 0) + (analyticsTest ? 1 : 0);
            Debug.Log($"전체 성공률: {successCount}/5 ({(successCount * 20)}%)");
            
            if (successCount == 5)
            {
                Debug.Log("Firebase 모든 서비스가 정상적으로 연결되었습니다!");
            }
            else if (successCount >= 3)
            {
                Debug.Log("Firebase 주요 서비스가 정상적으로 연결되었습니다!");
            }
            else
            {
                Debug.LogWarning("Firebase 연결에 문제가 있습니다. 설정을 확인해주세요.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"테스트 중 오류 발생: {e.Message}");
        }
        finally
        {
            isTestRunning = false;
        }
    }
    
    private async System.Threading.Tasks.Task<bool> TestFirebaseInit()
    {
        try
        {
            var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
            bool isReady = dependencyStatus == DependencyStatus.Available;
            
            Debug.Log($"Firebase 초기화 상태: {(isReady ? "준비됨" : "실패")}");
            return isReady;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase 초기화 테스트 실패: {e.Message}");
            return false;
        }
    }
    
    private async System.Threading.Tasks.Task<bool> TestAuthService()
    {
        try
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth == null)
            {
                Debug.LogError("Firebase Auth 인스턴스를 가져올 수 없습니다.");
                return false;
            }
            
            Debug.Log("Firebase 인증 서비스: 정상");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase 인증 테스트 실패: {e.Message}");
            return false;
        }
    }
    
    private async System.Threading.Tasks.Task<bool> TestDatabaseService()
    {
        try
        {
            var database = FirebaseDatabase.DefaultInstance;
            if (database == null)
            {
                Debug.LogError("Firebase Database 인스턴스를 가져올 수 없습니다.");
                return false;
            }
            
            // 간단한 연결 테스트 (타임아웃 설정)
            var testTask = database.GetReference("test").GetValueAsync();
            var timeoutTask = System.Threading.Tasks.Task.Delay(5000); // 5초 타임아웃
            
            var completedTask = await System.Threading.Tasks.Task.WhenAny(testTask, timeoutTask);
            
            if (completedTask == timeoutTask)
            {
                Debug.LogWarning("Firebase Database 연결 타임아웃");
                return false;
            }
            
            Debug.Log("Firebase 데이터베이스: 정상");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase 데이터베이스 테스트 실패: {e.Message}");
            return false;
        }
    }
    
    private bool TestStorageService()
    {
        try
        {
            var storage = FirebaseStorage.DefaultInstance;
            if (storage == null)
            {
                Debug.LogError("Firebase Storage 인스턴스를 가져올 수 없습니다.");
                return false;
            }
            
            Debug.Log("Firebase 스토리지: 정상");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase 스토리지 테스트 실패: {e.Message}");
            return false;
        }
    }
    
    private bool TestAnalyticsService()
    {
        try
        {
            // Analytics 이벤트 전송 테스트
            FirebaseAnalytics.LogEvent("firebase_safe_test");
            
            Debug.Log("Firebase Analytics: 정상");
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase Analytics 테스트 실패: {e.Message}");
            return false;
        }
    }
    
}