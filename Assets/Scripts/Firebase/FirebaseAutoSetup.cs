using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Firebase 자동 설정 스크립트
/// </summary>
public class FirebaseAutoSetup : MonoBehaviour
{
    [Header("자동 설정")]
    public bool autoSetupOnStart = true;
    
    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupFirebaseManagers();
        }
    }
    
    [ContextMenu("Firebase 매니저 자동 설정")]
    public void SetupFirebaseManagers()
    {
        Debug.Log("Firebase 매니저 자동 설정 시작...");
        
        // FirebaseManager 설정
        SetupFirebaseManager();
        
        // FirebaseAuthManager 설정
        SetupFirebaseAuthManager();
        
        // FirebaseDataManager 설정
        SetupFirebaseDataManager();
        
        // FirebaseConnectionTester 설정
        SetupFirebaseConnectionTester();
        
        Debug.Log("Firebase 매니저 자동 설정 완료!");
    }
    
    private void SetupFirebaseManager()
    {
        // FirebaseManager가 이미 있는지 확인
        var existingManager = FindObjectOfType<FirebaseManager>();
        if (existingManager == null)
        {
            GameObject managerObj = new GameObject("FirebaseManager");
            managerObj.AddComponent<FirebaseManager>();
            Debug.Log("FirebaseManager 생성됨");
        }
        else
        {
            Debug.Log("FirebaseManager가 이미 존재함");
        }
    }
    
    private void SetupFirebaseAuthManager()
    {
        // FirebaseAuthManager가 이미 있는지 확인
        var existingAuthManager = FindObjectOfType<FirebaseAuthManager>();
        if (existingAuthManager == null)
        {
            GameObject authManagerObj = new GameObject("FirebaseAuthManager");
            authManagerObj.AddComponent<FirebaseAuthManager>();
            Debug.Log("FirebaseAuthManager 생성됨");
        }
        else
        {
            Debug.Log("FirebaseAuthManager가 이미 존재함");
        }
    }
    
    private void SetupFirebaseDataManager()
    {
        // FirebaseDataManager는 FirebaseManager와 함께 생성되므로 별도 처리 불필요
        Debug.Log("FirebaseDataManager는 FirebaseManager와 함께 생성됨");
    }
    
    private void SetupFirebaseConnectionTester()
    {
        // FirebaseConnectionTester가 이미 있는지 확인
        var existingTester = FindObjectOfType<FirebaseConnectionTester>();
        if (existingTester == null)
        {
            GameObject testerObj = new GameObject("FirebaseConnectionTester");
            testerObj.AddComponent<FirebaseConnectionTester>();
            Debug.Log("FirebaseConnectionTester 생성됨");
        }
        else
        {
            Debug.Log("FirebaseConnectionTester가 이미 존재함");
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
        GUILayout.Label("=== Firebase 자동 설정 ===", headerStyle);
        GUILayout.Space(5);
        GUILayout.Label("Inspector에서 'Firebase 매니저 자동 설정' 버튼을 클릭하세요", labelStyle);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Firebase 매니저 자동 설정"))
        {
            SetupFirebaseManagers();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== 생성되는 매니저들 ===", headerStyle);
        GUILayout.Label("• FirebaseManager", labelStyle);
        GUILayout.Label("• FirebaseAuthManager", labelStyle);
        GUILayout.Label("• FirebaseConnectionTester", labelStyle);
        
        GUILayout.EndArea();
    }
}