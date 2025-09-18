using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Threading.Tasks;
using System.Linq;

/// <summary>
/// Firebase 인증 관리자
/// </summary>
public class FirebaseAuthManager : MonoBehaviour
{
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
    
    private void Start()
    {
        // 인증 상태 변경 감지
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
    /// 이메일로 회원가입
    /// </summary>
    public async Task<bool> SignUpWithEmail(string email, string password)
    {
        try
        {
            var result = await FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password);
            
            if (enableDebugLogs)
                Debug.Log($"회원가입 성공: {result.User.Email}");
            
            OnSignInSuccess?.Invoke();
            return true;
        }
        catch (System.Exception e)
        {
            string errorMessage = GetErrorMessage(e);
            Debug.LogError($"회원가입 실패: {errorMessage}");
            OnSignInFailed?.Invoke(errorMessage);
            return false;
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
    
    /// <summary>
    /// 이메일 인증이 필요한 회원가입
    /// </summary>
    public async Task<bool> SignUpWithEmailVerification(string email, string password)
    {
        try
        {
            var result = await FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(email, password);
            
            if (enableDebugLogs)
                Debug.Log($"회원가입 성공: {result.User.Email}");
            
            // 이메일 인증 메일 전송
            bool verificationSent = await SendEmailVerification();
            if (verificationSent)
            {
                if (enableDebugLogs)
                    Debug.Log("인증 메일이 전송되었습니다. 이메일을 확인해주세요.");
            }
            
            return true;
        }
        catch (System.Exception e)
        {
            string errorMessage = GetErrorMessage(e);
            if (enableDebugLogs)
                Debug.LogError($"회원가입 실패: {errorMessage}");
            
            OnSignInFailed?.Invoke(errorMessage);
            return false;
        }
    }

    /// <summary>
    /// 이메일이 이미 가입되어 있는지 확인
    /// </summary>
    public async System.Threading.Tasks.Task<bool> IsEmailRegistered(string email)
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log($"이메일 중복 체크 시작: {email}");
            
            // 임시 하드코딩된 중복 체크 (테스트용)
            string[] knownEmails = { "sfaa1541@naver.com" };
            bool exists = System.Array.Exists(knownEmails, e => e == email);
            
            if (exists)
            {
                if (enableDebugLogs)
                    Debug.Log($"하드코딩된 중복 체크 결과({email}): 이미 가입됨");
                return true;
            }
            
            // Firebase Realtime Database에서 이메일 목록 확인
            var database = FirebaseDatabase.DefaultInstance;
            var emailRef = database.GetReference("registered_emails").Child(email.Replace(".", "_"));
            
            var snapshot = await emailRef.GetValueAsync();
            exists = snapshot.Exists;
            
            if (enableDebugLogs)
                Debug.Log($"이메일 중복 체크 결과({email}): {(exists ? "이미 가입됨" : "미가입")}");
            
            return exists;
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"이메일 중복 체크 실패: {e.Message}");
            // 오류 발생 시 안전하게 false 반환 (가입 진행 허용)
            return false;
        }
    }
    
    /// <summary>
    /// 이메일을 등록된 이메일 목록에 추가
    /// </summary>
    public async System.Threading.Tasks.Task<bool> RegisterEmail(string email)
    {
        try
        {
            var database = FirebaseDatabase.DefaultInstance;
            var emailRef = database.GetReference("registered_emails").Child(email.Replace(".", "_"));
            
            // 간단한 문자열로 저장
            await emailRef.SetValueAsync(email);
            
            if (enableDebugLogs)
                Debug.Log($"이메일 등록 완료: {email}");
            
            return true;
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"이메일 등록 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 기존 이메일들을 수동으로 등록 (테스트용)
    /// </summary>
    [ContextMenu("기존 이메일들 등록")]
    public async void RegisterExistingEmails()
    {
        string[] existingEmails = { "sfaa1541@naver.com" }; // 기존에 가입된 이메일들
        
        foreach (string email in existingEmails)
        {
            await RegisterEmail(email);
        }
        
        Debug.Log("기존 이메일들 등록 완료!");
    }
}