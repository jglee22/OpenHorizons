using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// Firebase 인증 관리자
/// </summary>
public class FirebaseAuthManager : MonoBehaviour
{
    private const string ExpectedProjectId = "openhorizons-7144e"; 
private bool _isProjectMismatched = false;

    [Header("인증 설정")]
    public bool enableDebugLogs = true;
    
    // 인증 상태
    public bool IsSignedIn => FirebaseAuth.DefaultInstance.CurrentUser != null;
    public string UserId => FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
    public string UserEmail => FirebaseAuth.DefaultInstance.CurrentUser?.Email;
    
    // 이벤트
    public System.Action OnSignInSuccess;
    public System.Action<string> OnSignInFailed;
    public System.Action OnSignOut;
    public System.Action OnEmailVerificationSent;
    public System.Action<string> OnEmailVerificationFailed;
    public System.Action OnEmailVerified;
    
  private async void Start()
{
    var dep = await FirebaseApp.CheckAndFixDependenciesAsync();
    if (dep != DependencyStatus.Available) { Debug.LogError(dep); return; }

    var app = FirebaseApp.DefaultInstance;
    var pid = app.Options.ProjectId;
    Debug.Log($"[Firebase] ProjectId={pid}, AppId={app.Options.AppId}");

    // ⛔ 다른 프로젝트에 붙어있으면 바로 알림 + 이후 가입 차단용 플래그 세팅
    _isProjectMismatched = !string.Equals(pid, ExpectedProjectId, System.StringComparison.Ordinal);
    if (_isProjectMismatched)
        Debug.LogError($"[Firebase] ProjectId 불일치! Expected={ExpectedProjectId}, Actual={pid}");

    FirebaseAuth.DefaultInstance.StateChanged += OnAuthStateChanged;
}

    private void OnDestroy()
    {
        if (FirebaseAuth.DefaultInstance != null)
        {
            FirebaseAuth.DefaultInstance.StateChanged -= OnAuthStateChanged;
        }
    }
    
    
    /// <summary>
    /// 이메일로 로그인
    /// </summary>
    public async Task<bool> SignInWithEmail(string email, string password)
    {
        try
        {
            var result = await FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(email, password);
            
            if (enableDebugLogs)
                Debug.Log($"로그인 성공: {result.User.Email}");
            
            OnSignInSuccess?.Invoke();
            return true;
        }
        catch (System.Exception e)
        {
            string errorMessage = GetErrorMessage(e);
            Debug.LogError($"로그인 실패: {errorMessage}");
            OnSignInFailed?.Invoke(errorMessage);
            return false;
        }
    }
    
    /// <summary>
    /// 게스트 로그인
    /// </summary>
    public async Task<bool> SignInAnonymously()
    {
        try
        {
            var result = await FirebaseAuth.DefaultInstance.SignInAnonymouslyAsync();
            
            if (enableDebugLogs)
                Debug.Log($"게스트 로그인 성공: {result.User.UserId}");
            
            OnSignInSuccess?.Invoke();
            return true;
        }
        catch (System.Exception e)
        {
            string errorMessage = GetErrorMessage(e);
            Debug.LogError($"게스트 로그인 실패: {errorMessage}");
            OnSignInFailed?.Invoke(errorMessage);
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
            FirebaseAuth.DefaultInstance.SignOut();
            
            if (enableDebugLogs)
                Debug.Log("로그아웃 성공");
            
            OnSignOut?.Invoke();
            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로그아웃 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 비밀번호 재설정 이메일 전송
    /// </summary>
    public async Task<bool> SendPasswordResetEmail(string email)
    {
        try
        {
            await FirebaseAuth.DefaultInstance.SendPasswordResetEmailAsync(email);
            
            if (enableDebugLogs)
                Debug.Log($"비밀번호 재설정 이메일 전송: {email}");
            
            return true;
        }
        catch (System.Exception e)
        {
            string errorMessage = GetErrorMessage(e);
            Debug.LogError($"비밀번호 재설정 이메일 전송 실패: {errorMessage}");
            return false;
        }
    }
    
    /// <summary>
    /// 인증 상태 변경 감지
    /// </summary>
    private void OnAuthStateChanged(object sender, System.EventArgs e)
    {
        if (IsSignedIn)
        {
            if (enableDebugLogs)
                Debug.Log($"사용자 인증됨: {UserEmail}");
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("사용자 로그아웃됨");
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
            return $"인증 오류가 발생했습니다: {e.Message}";
    }
    
    /// <summary>
    /// 이메일 인증 메일 전송
    /// </summary>
    public async Task<bool> SendEmailVerification()
    {
        try
        {
            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null)
            {
                if (enableDebugLogs)
                    Debug.LogError("로그인된 사용자가 없습니다.");
                OnEmailVerificationFailed?.Invoke("로그인된 사용자가 없습니다.");
                return false;
            }
            
            if (user.IsEmailVerified)
            {
                if (enableDebugLogs)
                    Debug.Log("이미 인증된 이메일입니다.");
                OnEmailVerified?.Invoke();
                return true;
            }
            
            await user.SendEmailVerificationAsync();
            
            if (enableDebugLogs)
                Debug.Log($"인증 메일이 전송되었습니다: {user.Email}");
            
            OnEmailVerificationSent?.Invoke();
            return true;
        }
        catch (System.Exception e)
        {
            string errorMessage = GetErrorMessage(e);
            if (enableDebugLogs)
                Debug.LogError($"인증 메일 전송 실패: {errorMessage}");
            
            OnEmailVerificationFailed?.Invoke(errorMessage);
            return false;
        }
    }
    
    /// <summary>
    /// 이메일 인증 상태 확인
    /// </summary>
    public bool IsEmailVerified()
    {
        var user = FirebaseAuth.DefaultInstance.CurrentUser;
        return user?.IsEmailVerified ?? false;
    }
    
    /// <summary>
    /// 사용자 새로고침 (인증 상태 업데이트)
    /// </summary>
    public async Task<bool> RefreshUser()
    {
        try
        {
            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user == null) return false;
            
            await user.ReloadAsync();
            
            if (enableDebugLogs)
                Debug.Log($"사용자 정보 새로고침 완료. 이메일 인증 상태: {user.IsEmailVerified}");
            
            if (user.IsEmailVerified)
            {
                OnEmailVerified?.Invoke();
            }
            
            return true;
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"사용자 새로고침 실패: {e.Message}");
            return false;
        }
    }
    
    #region 회원가입 관련 메서드
    
    /// <summary>
    /// 이메일 중복 체크 (더미 비밀번호 로그인 방식)
    /// </summary>
    public async Task<bool> IsEmailRegistered(string email)
    {
        if (string.IsNullOrEmpty(email))
            return false;
            
        try
        {
            if (enableDebugLogs)
                Debug.Log($"[Auth] 이메일 중복 체크 시작: {email}");
            
            // 더미 비밀번호로 로그인 시도
            string dummyPassword = "dummy_password_12345_very_long_string_impossible_to_guess_98765";
            
            // 현재 사용자 백업
            var currentUser = FirebaseAuth.DefaultInstance.CurrentUser;
            
            try
            {
                // 더미 비밀번호로 로그인 시도
                var credential = EmailAuthProvider.GetCredential(email, dummyPassword);
                await FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(credential);
                
                // 여기까지 오면 이메일이 존재한다는 뜻
                if (enableDebugLogs)
                    Debug.Log($"[Auth] 이메일 존재함: {email}");
                
                // 원래 사용자로 복원
                if (currentUser != null)
                {
                    await FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(
                        EmailAuthProvider.GetCredential(currentUser.Email, "temp"));
                }
                else
                {
                    FirebaseAuth.DefaultInstance.SignOut();
                }
                
                return true;
            }
            catch (FirebaseException e)
            {
                // 원래 사용자로 복원
                if (currentUser != null)
                {
                    try
                    {
                        await FirebaseAuth.DefaultInstance.SignInWithCredentialAsync(
                            EmailAuthProvider.GetCredential(currentUser.Email, "temp"));
                    }
                    catch
                    {
                        FirebaseAuth.DefaultInstance.SignOut();
                    }
                }
                else
                {
                    FirebaseAuth.DefaultInstance.SignOut();
                }
                
                // 오류 메시지로 판단
                if (e.Message.Contains("wrong-password") || e.Message.Contains("WrongPassword"))
                {
                    if (enableDebugLogs)
                        Debug.Log($"[Auth] 이메일 존재함 (WrongPassword): {email}");
                    return true;
                }
                else if (e.Message.Contains("user-not-found") || e.Message.Contains("UserNotFound"))
                {
                    if (enableDebugLogs)
                        Debug.Log($"[Auth] 이메일 없음 (UserNotFound): {email}");
                    return false;
                }
                else
                {
                    if (enableDebugLogs)
                        Debug.LogError($"[Auth] 이메일 체크 중 오류: {e.Message}");
                    return false;
                }
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"[Auth] 이메일 중복 체크 오류: {e}");
            return false;
        }
    }
    
    /// <summary>
    /// 이메일/비밀번호로 회원가입
    /// </summary>
    public async Task<bool> SignUpWithEmail(string email, string password)
    {
        if (_isProjectMismatched)
        {
            if (enableDebugLogs)
                Debug.LogError("[Auth] 프로젝트 불일치로 회원가입 차단");
            return false;
        }
        
        try
        {
            if (enableDebugLogs)
                Debug.Log($"[Auth] 회원가입 시도: {email}");
            
            await FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password);
            
            if (enableDebugLogs)
                Debug.Log($"[Auth] 회원가입 성공: {email}");
            
            OnSignInSuccess?.Invoke();
            return true;
        }
        catch (FirebaseException e)
        {
            string errorMessage = GetAuthErrorMessage(e);
            if (enableDebugLogs)
                Debug.LogError($"[Auth] 회원가입 실패: {errorMessage}");
            
            // OnSignInFailed 대신 직접 오류 메시지 반환
            return false;
        }
        catch (System.Exception e)
        {
            string errorMessage = GetErrorMessage(e);
            if (enableDebugLogs)
                Debug.LogError($"[Auth] 회원가입 오류: {errorMessage}");
            
            OnSignInFailed?.Invoke(errorMessage);
            return false;
        }
    }
    
    /// <summary>
    /// Firebase Auth 오류 메시지 변환
    /// </summary>
    private string GetAuthErrorMessage(FirebaseException e)
    {
        string message = e.Message.ToLower();
        
        if (message.Contains("email-already-in-use") || message.Contains("emailalreadyinuse"))
            return "이미 사용 중인 이메일입니다.";
        else if (message.Contains("invalid-email") || message.Contains("invalidemail"))
            return "올바르지 않은 이메일 형식입니다.";
        else if (message.Contains("weak-password") || message.Contains("weakpassword"))
            return "비밀번호가 너무 약합니다. 6자 이상 입력해주세요.";
        else if (message.Contains("network-request-failed") || message.Contains("networkrequestfailed"))
            return "네트워크 연결을 확인해주세요.";
        else if (message.Contains("too-many-requests") || message.Contains("toomanyrequests"))
            return "너무 많은 요청이 발생했습니다. 잠시 후 다시 시도해주세요.";
        else
            return $"회원가입 실패: {e.Message}";
    }
    
    #endregion
    
}