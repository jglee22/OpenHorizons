using UnityEngine;
using Firebase;

/// <summary>
/// 간단한 Firebase 테스트 스크립트
/// </summary>
public class SimpleFirebaseTest : MonoBehaviour
{
    private void Start()
    {
        TestFirebaseConnection();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            TestFirebaseConnection();
        }
    }
    
    /// <summary>
    /// Firebase 연결 테스트
    /// </summary>
    private async void TestFirebaseConnection()
    {
        Debug.Log("간단한 Firebase 테스트 시작...");
        
        try
        {
            // Firebase 앱 초기화 확인
            var app = FirebaseApp.DefaultInstance;
            if (app != null)
            {
                Debug.Log($"Firebase 앱 이름: {app.Name}");
                Debug.Log($"Firebase 앱 옵션: {app.Options.ProjectId}");
                
                // 의존성 확인
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus == DependencyStatus.Available)
                {
                    Debug.Log("Firebase 앱이 정상적으로 초기화되었습니다!");
                }
                else
                {
                    Debug.LogError("Firebase 앱이 초기화되지 않았습니다!");
                }
            }
            else
            {
                Debug.LogError("Firebase 앱이 초기화되지 않았습니다!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase 연결 테스트 실패: {e.Message}");
        }
    }
    
}