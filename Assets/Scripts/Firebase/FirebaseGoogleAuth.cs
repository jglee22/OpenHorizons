using UnityEngine;
using Firebase.Auth;
using System.Threading.Tasks;

/// <summary>
/// Firebase Google 인증 관리자
/// </summary>
public class FirebaseGoogleAuth : MonoBehaviour
{
    [Header("인증 설정")]
    public bool enableDebugLogs = true;
    
    [Header("사용자 정보")]
    public string UserEmail => FirebaseAuth.DefaultInstance?.CurrentUser?.Email ?? "없음";
    public string UserId => FirebaseAuth.DefaultInstance?.CurrentUser?.UserId ?? "없음";
    public bool IsSignedIn => FirebaseAuth.DefaultInstance?.CurrentUser != null;
    
    private void Start()
    {
        // Firebase Auth 상태 변경 이벤트 구독
        FirebaseAuth.DefaultInstance.StateChanged += OnAuthStateChanged;
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (FirebaseAuth.DefaultInstance != null)
        {
            FirebaseAuth.DefaultInstance.StateChanged -= OnAuthStateChanged;
        }
    }
    
    /// <summary>
    /// Google 로그인 (시뮬레이션)
    /// </summary>
    public async Task<bool> SignInWithGoogle()
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log("Google 로그인 시뮬레이션 시작...");
            
            // 실제 Google 로그인 구현은 Google Play Games Services나
            // Google Sign-In SDK를 사용해야 합니다.
            // 여기서는 시뮬레이션만 제공합니다.
            
            await System.Threading.Tasks.Task.Delay(1000); // 1초 대기 (시뮬레이션)
            
            if (enableDebugLogs)
                Debug.Log("Google 로그인 시뮬레이션 완료");
            
            return true;
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"Google 로그인 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Google 로그인 시뮬레이션
    /// </summary>
    private async Task<bool> SimulateGoogleSignIn()
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log("Google 로그인 시뮬레이션 중...");
            
            await System.Threading.Tasks.Task.Delay(2000); // 2초 대기
            
            if (enableDebugLogs)
                Debug.Log("Google 로그인 시뮬레이션 완료");
            
            return true;
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"Google 로그인 시뮬레이션 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Google 이메일로 회원가입
    /// </summary>
    public async Task<bool> SignUpWithGoogleEmail(string email, string password)
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log($"Google 이메일 회원가입 시작: {email}");
            
            var createTask = FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password);
            var createCompletedTask = await System.Threading.Tasks.Task.WhenAny(createTask, System.Threading.Tasks.Task.Delay(10000));
            
            if (createCompletedTask == createTask)
            {
                if (createTask.IsCompletedSuccessfully)
                {
                    if (enableDebugLogs)
                        Debug.Log("Google 사용자 생성 성공");
                    return true;
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.LogError($"Google 사용자 생성 실패: {createTask.Exception?.GetBaseException().Message}");
                    return false;
                }
            }
            else
            {
                throw new System.Exception("사용자 생성 시간 초과");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"Google 이메일 회원가입 실패: {GetErrorMessage(e)}");
            return false;
        }
    }
    
    /// <summary>
    /// Google 이메일로 로그인
    /// </summary>
    public async Task<bool> SignInWithGoogleEmail(string email, string password)
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log($"Google 이메일 로그인 시작: {email}");
            
            var signInTask = FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password);
            var signInCompletedTask = await System.Threading.Tasks.Task.WhenAny(signInTask, System.Threading.Tasks.Task.Delay(10000));
            
            if (signInCompletedTask == signInTask)
            {
                if (signInTask.IsCompletedSuccessfully)
                {
                    if (enableDebugLogs)
                        Debug.Log("Google 이메일 로그인 성공");
                    return true;
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.LogError($"Google 이메일 로그인 실패: {signInTask.Exception?.GetBaseException().Message}");
                    return false;
                }
            }
            else
            {
                throw new System.Exception("로그인 시간 초과");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"Google 이메일 로그인 실패: {GetErrorMessage(e)}");
            return false;
        }
    }
    
    /// <summary>
    /// 로그아웃
    /// </summary>
    public async Task<bool> SignOut()
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log("Google 로그아웃 시작...");
            
            FirebaseAuth.DefaultInstance.SignOut();
            
            if (enableDebugLogs)
                Debug.Log("Google 로그아웃 완료");
            
            return true;
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"Google 로그아웃 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 인증 상태 변경 이벤트 핸들러
    /// </summary>
    private void OnAuthStateChanged(object sender, System.EventArgs e)
    {
        if (IsSignedIn)
        {
            if (enableDebugLogs)
                Debug.Log($"Google 사용자 인증됨: {UserEmail}");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("Google 사용자 로그아웃됨");
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