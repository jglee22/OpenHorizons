using UnityEngine;
using Firebase;
using Firebase.Auth;

/// <summary>
/// Firebase Auth 문제 해결 스크립트
/// </summary>
public class FirebaseAuthFix : MonoBehaviour
{
    [Header("설정")]
    public bool useAuthEmulator = false;
    public string authEmulatorHost = "localhost";
    public int authEmulatorPort = 9099;
    
    [Header("실제 계정 테스트")]
    public string realEmail = "your-email@gmail.com";
    public string realPassword = "your-password";
    
    private void Start()
    {
        InitializeFirebaseAuth();
    }
    
    /// <summary>
    /// Firebase Auth 초기화 (에뮬레이터 문제 해결)
    /// </summary>
    private void InitializeFirebaseAuth()
    {
        try
        {
            Debug.Log("Firebase Auth 초기화 시작...");
            
            // Firebase 앱 초기화 확인
            var app = FirebaseApp.DefaultInstance;
            if (app == null)
            {
                Debug.LogError("Firebase 앱이 초기화되지 않았습니다!");
                return;
            }
            
            Debug.Log($"Firebase 앱 초기화됨: {app.Name}");
            
            // Auth 에뮬레이터 설정 (최신 SDK에서는 지원하지 않음)
            if (useAuthEmulator)
            {
                Debug.LogWarning("Auth 에뮬레이터는 최신 Firebase SDK에서 지원하지 않습니다.");
                Debug.Log("프로덕션 Auth 사용");
            }
            else
            {
                Debug.Log("프로덕션 Auth 사용");
            }
            
            // Auth 인스턴스 확인
            var auth = FirebaseAuth.DefaultInstance;
            if (auth != null)
            {
                Debug.Log("Firebase Auth 인스턴스 생성됨");
            }
            else
            {
                Debug.LogError("Firebase Auth 인스턴스 생성 실패");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Firebase Auth 초기화 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 수정된 인증 테스트
    /// </summary>
    public async void TestFixedAuth()
    {
        Debug.Log("수정된 인증 테스트 시작...");
        
        try
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth == null)
            {
                Debug.LogError("Firebase Auth가 null입니다!");
                return;
            }
            
            // 테스트용 계정 생성
            string testEmail = "test" + System.DateTime.Now.Ticks + "@test.com";
            string testPassword = "testpassword123";
            
            Debug.Log($"테스트 이메일: {testEmail}");
            
            // 계정 생성 시도
            var createResult = await auth.CreateUserWithEmailAndPasswordAsync(testEmail, testPassword);
            
            if (createResult != null && createResult.User != null)
            {
                Debug.Log("테스트 계정 생성 성공!");
                Debug.Log($"사용자 ID: {createResult.User.UserId}");
                Debug.Log($"이메일: {createResult.User.Email}");
                
                // 로그아웃
                auth.SignOut();
                Debug.Log("로그아웃 완료");
                
                // 생성된 계정으로 로그인 테스트
                Debug.Log("생성된 계정으로 로그인 테스트...");
                var signInResult = await auth.SignInWithEmailAndPasswordAsync(testEmail, testPassword);
                
                if (signInResult != null && signInResult.User != null)
                {
                    Debug.Log("생성된 계정 로그인 성공!");
                    Debug.Log($"로그인된 이메일: {signInResult.User.Email}");
                    
                    // 로그아웃
                    auth.SignOut();
                    Debug.Log("최종 로그아웃 완료");
                }
                else
                {
                    Debug.LogError("생성된 계정 로그인 실패");
                }
            }
            else
            {
                Debug.LogError("테스트 계정 생성 실패");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"수정된 인증 테스트 실패: {e.Message}");
            Debug.LogError($"오류 상세: {e}");
        }
    }
    
    /// <summary>
    /// 실제 계정으로 로그인 테스트 (수정된 버전)
    /// </summary>
    public async void TestRealAccountFixed(string email, string password)
    {
        Debug.Log("실제 계정 로그인 테스트 (수정된 버전)...");
        Debug.Log($"사용할 이메일: {email}");
        
        try
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth == null)
            {
                Debug.LogError("Firebase Auth가 null입니다!");
                return;
            }
            
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            
            if (result != null && result.User != null)
            {
                Debug.Log("실제 계정 로그인 성공!");
                Debug.Log($"사용자 이메일: {result.User.Email}");
                Debug.Log($"사용자 ID: {result.User.UserId}");
                Debug.Log($"이메일 인증됨: {result.User.IsEmailVerified}");
                
                // 로그아웃
                auth.SignOut();
                Debug.Log("로그아웃 완료");
            }
            else
            {
                Debug.LogError("로그인 결과가 null입니다!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"실제 계정 로그인 실패: {GetErrorMessage(e)}");
        }
    }
    
    /// <summary>
    /// 에러 메시지를 한국어로 변환
    /// </summary>
    private string GetErrorMessage(System.Exception e)
    {
        string message = e.Message.ToLower();
        
        if (message.Contains("invalid-email"))
            return "잘못된 이메일 형식입니다.";
        else if (message.Contains("wrong-password"))
            return "잘못된 비밀번호입니다.";
        else if (message.Contains("user-not-found"))
            return "존재하지 않는 사용자입니다.";
        else if (message.Contains("email-already-in-use"))
            return "이미 사용 중인 이메일입니다.";
        else if (message.Contains("weak-password"))
            return "비밀번호가 너무 약합니다.";
        else if (message.Contains("too-many-requests"))
            return "너무 많은 요청이 발생했습니다. 잠시 후 다시 시도해주세요.";
        else if (message.Contains("network"))
            return "네트워크 연결을 확인해주세요.";
        else if (message.Contains("internal error"))
            return "Firebase 내부 오류가 발생했습니다. 프로젝트 설정을 확인해주세요.";
        else
            return e.Message;
    }
    
    [ContextMenu("실제 계정으로 로그인 테스트")]
    public void TestRealAccountFromInspector()
    {
        if (!string.IsNullOrEmpty(realEmail) && !string.IsNullOrEmpty(realPassword))
        {
            Debug.Log($"Inspector에서 실제 계정 테스트: {realEmail}");
            TestRealAccountFixed(realEmail, realPassword);
        }
        else
        {
            Debug.LogWarning("실제 이메일과 비밀번호를 Inspector에서 설정해주세요!");
        }
    }
    
    [ContextMenu("수정된 인증 테스트")]
    public void TestFixedAuthFromInspector()
    {
        Debug.Log("Inspector에서 수정된 인증 테스트");
        TestFixedAuth();
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
        
        GUILayout.BeginArea(new Rect(10, 10, 500, 300));
        GUILayout.Label("=== Firebase Auth 수정 ===", headerStyle);
        GUILayout.Space(5);
        GUILayout.Label("F10: 수정된 인증 테스트", labelStyle);
        GUILayout.Space(10);
        
        if (GUILayout.Button("수정된 인증 테스트"))
        {
            TestFixedAuth();
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== 실제 계정 테스트 ===", headerStyle);
        GUILayout.Label("실제 이메일과 비밀번호를 Inspector에서 설정하세요", labelStyle);
        GUILayout.Space(5);
        
        if (GUILayout.Button("실제 계정으로 로그인 테스트"))
        {
            if (!string.IsNullOrEmpty(realEmail) && !string.IsNullOrEmpty(realPassword))
            {
                TestRealAccountFixed(realEmail, realPassword);
            }
            else
            {
                Debug.LogWarning("실제 이메일과 비밀번호를 Inspector에서 설정해주세요!");
            }
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== 해결 방법 ===", headerStyle);
        GUILayout.Label("1. google-services.json을", labelStyle);
        GUILayout.Label("   Assets/Resources/로 이동", labelStyle);
        GUILayout.Label("2. Unity 재시작", labelStyle);
        GUILayout.Label("3. F10으로 테스트", labelStyle);
        GUILayout.Label("4. Inspector에서 실제 계정 정보 입력", labelStyle);
        
        GUILayout.EndArea();
    }
}