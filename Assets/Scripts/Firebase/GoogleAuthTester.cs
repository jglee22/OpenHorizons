using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Google 인증 테스터
/// </summary>
public class GoogleAuthTester : MonoBehaviour
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
            TestSignUp();
        }
        
        if (Input.GetKeyDown(KeyCode.F12))
        {
            TestSignIn();
        }
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TestSignOut();
        }
    }
    
    /// <summary>
    /// 모든 Google 인증 테스트
    /// </summary>
    private async void TestAllGoogleAuth()
    {
        Debug.Log("구글 인증 테스트 시작...");
        
        try
        {
            // 1. 회원가입 테스트
            bool signUp = await TestSignUpInternal();
            Debug.Log($"   회원가입: {(signUp ? "성공" : "실패")}");
            
            // 2. 로그인 테스트
            bool signIn = await TestSignInInternal();
            Debug.Log($"   로그인: {(signIn ? "성공" : "실패")}");
            
            // 3. 로그인 상태 확인
            bool isSignedIn = googleAuth != null && googleAuth.IsSignedIn;
            Debug.Log($"   로그인 상태: {(isSignedIn ? "로그인됨" : "로그아웃됨")}");
            
            // 4. 로그아웃 테스트
            bool signOut = await TestSignOutInternal();
            Debug.Log($"   결과: {(signOut ? "성공" : "실패")}");
            
            Debug.Log("🎉 구글 인증 테스트 완료!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 구글 인증 테스트 중 오류: {e.Message}");
        }
    }
    
    /// <summary>
    /// 이메일 회원가입 테스트
    /// </summary>
    private async void TestSignUp()
    {
        Debug.Log("🔍 이메일 회원가입 테스트...");
        bool success = await TestSignUpInternal();
        Debug.Log($"이메일 회원가입 결과: {(success ? "성공" : "실패")}");
    }
    
    /// <summary>
    /// 이메일 로그인 테스트
    /// </summary>
    private async void TestSignIn()
    {
        Debug.Log("🔍 이메일 로그인 테스트...");
        bool success = await TestSignInInternal();
        Debug.Log($"이메일 로그인 결과: {(success ? "성공" : "실패")}");
    }
    
    /// <summary>
    /// 로그아웃 테스트
    /// </summary>
    private async void TestSignOut()
    {
        Debug.Log("🔍 로그아웃 테스트...");
        bool success = await TestSignOutInternal();
        Debug.Log($"로그아웃 결과: {(success ? "성공" : "실패")}");
    }
    
    private async Task<bool> TestSignUpInternal()
    {
        if (googleAuth == null) return false;
        
        try
        {
            return await googleAuth.SignUpWithGoogleEmail(testEmail, testPassword);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"회원가입 테스트 실패: {e.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestSignInInternal()
    {
        if (googleAuth == null) return false;
        
        try
        {
            return await googleAuth.SignInWithGoogleEmail(testEmail, testPassword);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로그인 테스트 실패: {e.Message}");
            return false;
        }
    }
    
    private async Task<bool> TestSignOutInternal()
    {
        if (googleAuth == null) return false;
        
        try
        {
            return await googleAuth.SignOut();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로그아웃 테스트 실패: {e.Message}");
            return false;
        }
    }
}