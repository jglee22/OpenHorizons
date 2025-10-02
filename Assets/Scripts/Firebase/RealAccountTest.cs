using UnityEngine;
using Firebase.Auth;
using System.Threading.Tasks;

/// <summary>
/// 실제 계정으로 Firebase 인증 테스트
/// </summary>
public class RealAccountTest : MonoBehaviour
{
    [Header("실제 계정 정보")]
    public string realEmail = "your-email@gmail.com";
    public string realPassword = "your-password";
    
    private FirebaseAuthManager authManager;
    
    private void Start()
    {
        authManager = FindObjectOfType<FirebaseAuthManager>();
        if (authManager == null)
        {
            Debug.LogError("FirebaseAuthManager를 찾을 수 없습니다!");
        }
        
        if (string.IsNullOrEmpty(realEmail) || realEmail == "your-email@gmail.com")
        {
            Debug.LogWarning("⚠️ 실제 계정 정보를 설정해주세요! Inspector에서 realEmail과 realPassword를 변경하세요.");
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10))
        {
            TestRealAccountLogin();
        }
        
        if (Input.GetKeyDown(KeyCode.F11))
        {
            TestWrongPassword();
        }
        
        if (Input.GetKeyDown(KeyCode.F12))
        {
            TestInvalidEmail();
        }
    }
    
    /// <summary>
    /// 실제 계정으로 로그인 테스트
    /// </summary>
    private async void TestRealAccountLogin()
    {
        if (string.IsNullOrEmpty(realEmail) || realEmail == "your-email@gmail.com")
        {
            Debug.LogWarning("⚠️ 실제 계정 정보를 설정해주세요! Inspector에서 realEmail과 realPassword를 변경하세요.");
            return;
        }
        
        Debug.Log("🔍 실제 계정 로그인 테스트 시작...");
        
        try
        {
            if (authManager == null)
            {
                Debug.LogError("FirebaseAuthManager를 찾을 수 없습니다!");
                return;
            }
            
            bool success = await authManager.SignInWithEmail(realEmail, realPassword);
            
            if (success)
            {
                Debug.Log("✅ 실제 계정 로그인 성공!");
                Debug.Log($"사용자 이메일: {authManager.UserEmail}");
                Debug.Log($"사용자 ID: {authManager.UserId}");
                
                // 로그아웃
                await authManager.SignOut();
                Debug.Log("✅ 로그아웃 완료");
            }
            else
            {
                Debug.LogError("❌ 로그인 결과가 null입니다!");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"❌ 실제 계정 로그인 실패: {GetErrorMessage(e)}");
        }
    }
    
    /// <summary>
    /// 잘못된 비밀번호 테스트
    /// </summary>
    private async void TestWrongPassword()
    {
        Debug.Log("🔍 잘못된 비밀번호 테스트 시작...");
        
        try
        {
            if (authManager == null)
            {
                Debug.LogError("FirebaseAuthManager를 찾을 수 없습니다!");
                return;
            }
            
            bool success = await authManager.SignInWithEmail(realEmail, "wrongpassword");
            
            if (success)
            {
                Debug.LogError("❌ 예상과 다르게 로그인 성공! (실패해야 함)");
            }
            else
            {
                Debug.Log($"✅ 예상대로 로그인 실패: {GetErrorMessage(new System.Exception("wrong-password"))}");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log($"✅ 예상대로 로그인 실패: {GetErrorMessage(e)}");
        }
    }
    
    /// <summary>
    /// 잘못된 이메일 테스트
    /// </summary>
    private async void TestInvalidEmail()
    {
        Debug.Log("🔍 잘못된 이메일 테스트 시작...");
        
        try
        {
            if (authManager == null)
            {
                Debug.LogError("FirebaseAuthManager를 찾을 수 없습니다!");
                return;
            }
            
            bool success = await authManager.SignInWithEmail("invalid-email", realPassword);
            
            if (success)
            {
                Debug.LogError("❌ 예상과 다르게 로그인 성공! (실패해야 함)");
            }
            else
            {
                Debug.Log($"✅ 예상대로 로그인 실패: {GetErrorMessage(new System.Exception("invalid-email"))}");
            }
        }
        catch (System.Exception e)
        {
            Debug.Log($"✅ 예상대로 로그인 실패: {GetErrorMessage(e)}");
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
        else
            return e.Message;
    }
    
}