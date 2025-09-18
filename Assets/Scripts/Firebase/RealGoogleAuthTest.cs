using UnityEngine;
using Firebase.Auth;
using System.Threading.Tasks;

/// <summary>
/// 실제 Google 인증 테스트
/// </summary>
public class RealGoogleAuthTest : MonoBehaviour
{
    [Header("테스트 설정")]
    public string testEmail = "test@example.com";
    public string testPassword = "testpassword123";
    
    private FirebaseGoogleAuth googleAuth;
    
    private void Start()
    {
        googleAuth = FindObjectOfType<FirebaseGoogleAuth>();
        if (googleAuth == null)
        {
            Debug.LogError("FirebaseGoogleAuth를 찾을 수 없습니다!");
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            TestAllGoogleAuth();
        }
        
        if (Input.GetKeyDown(KeyCode.F11))
        {
            TestInvalidLogin();
        }
        
        if (Input.GetKeyDown(KeyCode.F12))
        {
            TestWrongPassword();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TestInvalidEmail();
        }
    }
    
    /// <summary>
    /// 모든 Google 인증 테스트
    /// </summary>
    private async void TestAllGoogleAuth()
    {
        Debug.Log("🔍 Google 인증 테스트 시작...");
        
        try
        {
            // 1. 잘못된 로그인 테스트
            bool invalidLogin = await TestInvalidLoginInternal();
            Debug.Log($"   잘못된 로그인: {(invalidLogin ? "예상대로 실패" : "예상과 다름")}");
            
            // 2. 잘못된 비밀번호 테스트
            bool wrongPassword = await TestWrongPasswordInternal();
            Debug.Log($"   잘못된 비밀번호: {(wrongPassword ? "예상대로 실패" : "예상과 다름")}");
            
            // 3. 잘못된 이메일 테스트
            bool invalidEmail = await TestInvalidEmailInternal();
            Debug.Log($"   잘못된 이메일: {(invalidEmail ? "예상대로 실패" : "예상과 다름")}");
            
            // 4. 유효한 계정 테스트 (실제 계정이 있는 경우)
            bool validAccount = await TestValidAccountInternal();
            Debug.Log($"   유효한 계정: {(validAccount ? "성공" : "실패 또는 계정 없음")}");
            
            Debug.Log("🎉 Google 인증 테스트 완료!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ Google 인증 테스트 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 잘못된 로그인 테스트
    /// </summary>
    private async void TestInvalidLogin()
    {
        Debug.Log("🔍 잘못된 로그인 테스트...");
        bool result = await TestInvalidLoginInternal();
        Debug.Log($"잘못된 로그인 테스트 결과: {(result ? "예상대로 실패" : "예상과 다름")}");
    }
    
    /// <summary>
    /// 잘못된 비밀번호 테스트
    /// </summary>
    private async void TestWrongPassword()
    {
        Debug.Log("🔍 잘못된 비밀번호 테스트...");
        bool result = await TestWrongPasswordInternal();
        Debug.Log($"잘못된 비밀번호 테스트 결과: {(result ? "예상대로 실패" : "예상과 다름")}");
    }
    
    /// <summary>
    /// 잘못된 이메일 테스트
    /// </summary>
    private async void TestInvalidEmail()
    {
        Debug.Log("🔍 잘못된 이메일 테스트...");
        bool result = await TestInvalidEmailInternal();
        Debug.Log($"잘못된 이메일 테스트 결과: {(result ? "예상대로 실패" : "예상과 다름")}");
    }
    
    /// <summary>
    /// 유효한 계정 테스트
    /// </summary>
    private async void TestValidAccount()
    {
        Debug.Log("🔍 유효한 계정 테스트...");
        bool result = await TestValidAccountInternal();
        Debug.Log($"유효한 계정 테스트 결과: {(result ? "성공" : "실패")}");
    }
    
    private async Task<bool> TestInvalidLoginInternal()
    {
        if (googleAuth == null) return false;
        
        try
        {
            bool success = await googleAuth.SignInWithGoogleEmail("nonexistent@example.com", "wrongpassword");
            return !success; // 실패해야 정상
        }
        catch (System.Exception e)
        {
            Debug.Log($"예상된 오류: {GetErrorMessage(e)}");
            return true; // 오류가 발생하면 정상
        }
    }
    
    private async Task<bool> TestWrongPasswordInternal()
    {
        if (googleAuth == null) return false;
        
        try
        {
            bool success = await googleAuth.SignInWithGoogleEmail(testEmail, "wrongpassword");
            return !success; // 실패해야 정상
        }
        catch (System.Exception e)
        {
            Debug.Log($"예상된 오류: {GetErrorMessage(e)}");
            return true; // 오류가 발생하면 정상
        }
    }
    
    private async Task<bool> TestInvalidEmailInternal()
    {
        if (googleAuth == null) return false;
        
        try
        {
            bool success = await googleAuth.SignInWithGoogleEmail("invalid-email", testPassword);
            return !success; // 실패해야 정상
        }
        catch (System.Exception e)
        {
            Debug.Log($"예상된 오류: {GetErrorMessage(e)}");
            return true; // 오류가 발생하면 정상
        }
    }
    
    private async Task<bool> TestValidAccountInternal()
    {
        if (googleAuth == null) return false;
        
        try
        {
            // 먼저 회원가입 시도
            bool signUpSuccess = await googleAuth.SignUpWithGoogleEmail(testEmail, testPassword);
            
            if (signUpSuccess)
            {
                Debug.Log("회원가입 성공, 로그인 테스트 진행...");
                
                // 로그아웃
                await googleAuth.SignOut();
                
                // 로그인 테스트
                bool signInSuccess = await googleAuth.SignInWithGoogleEmail(testEmail, testPassword);
                
                if (signInSuccess)
                {
                    Debug.Log("로그인 성공, 로그아웃 테스트 진행...");
                    
                    // 로그아웃
                    bool signOutSuccess = await googleAuth.SignOut();
                    
                    return signOutSuccess;
                }
            }
            
            return false;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"유효한 계정 테스트 실패: {GetErrorMessage(e)}");
            return false;
        }
    }
    
    /// <summary>
    /// 에러 메시지를 한국어로 변환
    /// </summary>
    private string GetErrorMessage(System.Exception e)
    {
        string message = e.Message.ToLower();
        
        if (message.Contains("invalid-email"))
            return "잘못된 이메일 형식";
        else if (message.Contains("wrong-password"))
            return "잘못된 비밀번호";
        else if (message.Contains("user-not-found"))
            return "존재하지 않는 사용자";
        else if (message.Contains("email-already-in-use"))
            return "이미 사용 중인 이메일";
        else if (message.Contains("weak-password"))
            return "비밀번호가 너무 약함";
        else if (message.Contains("too-many-requests"))
            return "너무 많은 요청";
        else if (message.Contains("network"))
            return "네트워크 연결 오류";
        else
            return e.Message;
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
        
        GUIStyle warningStyle = new GUIStyle(GUI.skin.label);
        warningStyle.fontSize = 12;
        warningStyle.normal.textColor = Color.red;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== 실제 Google 인증 테스트 ===", headerStyle);
        GUILayout.Space(5);
        
        if (string.IsNullOrEmpty(testEmail) || testEmail == "test@example.com")
        {
            GUILayout.Label("⚠️ Inspector에서 테스트 계정 정보를 설정해주세요!", warningStyle);
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== 테스트 키 ===", headerStyle);
        GUILayout.Label("F10: 전체 Google 인증 테스트", labelStyle);
        GUILayout.Label("F11: 잘못된 로그인 테스트", labelStyle);
        GUILayout.Label("F12: 잘못된 비밀번호 테스트", labelStyle);
        GUILayout.Label("ESC: 잘못된 이메일 테스트", labelStyle);
        
        GUILayout.Space(10);
        GUILayout.Label("=== 현재 설정 ===", headerStyle);
        GUILayout.Label($"테스트 이메일: {testEmail}", labelStyle);
        GUILayout.Label($"테스트 비밀번호: {(!string.IsNullOrEmpty(testPassword) ? "설정됨" : "설정 안됨")}", labelStyle);
        
        GUILayout.Space(10);
        GUILayout.Label("=== 사용법 ===", headerStyle);
        GUILayout.Label("1. Inspector에서 testEmail과 testPassword 설정", labelStyle);
        GUILayout.Label("2. F10으로 전체 테스트 실행", labelStyle);
        GUILayout.Label("3. F11, F12, ESC로 개별 테스트", labelStyle);
        GUILayout.Label("4. 예상대로 실패하는 것이 정상", labelStyle);
        
        GUILayout.EndArea();
    }
}