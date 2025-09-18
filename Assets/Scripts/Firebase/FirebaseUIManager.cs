using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using System.Threading.Tasks;

/// <summary>
/// Firebase UI 관리자
/// </summary>
public class FirebaseUIManager : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button signUpButton;
    public Button signInButton;
    public Button signOutButton;
    public Button guestButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI userInfoText;
    
    [Header("Firebase 매니저")]
    public FirebaseAuthManager authManager;
    
    private void Start()
    {
        if (authManager == null)
            authManager = FindObjectOfType<FirebaseAuthManager>();
        
        SetupUI();
    }
    
    private void SetupUI()
    {
        if (signUpButton != null)
            signUpButton.onClick.AddListener(OnSignUpClicked);
        if (signInButton != null)
            signInButton.onClick.AddListener(OnSignInClicked);
        if (signOutButton != null)
            signOutButton.onClick.AddListener(OnSignOutClicked);
        if (guestButton != null)
            guestButton.onClick.AddListener(OnGuestClicked);
        
        UpdateUserInfo();
    }
    
    private async void OnSignUpClicked()
    {
        if (authManager == null) return;
        
        string email = emailInput?.text?.Trim();
        string password = passwordInput?.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("이메일과 비밀번호를 입력해주세요.");
            return;
        }
        
        ShowMessage("회원가입 중...");
        bool success = await authManager.SignUpWithEmail(email, password);
        
        if (success)
        {
            ShowMessage("회원가입 성공!");
            UpdateUserInfo();
        }
        else
        {
            ShowMessage("회원가입 실패");
        }
    }
    
    private async void OnSignInClicked()
    {
        if (authManager == null) return;
        
        string email = emailInput?.text?.Trim();
        string password = passwordInput?.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("이메일과 비밀번호를 입력해주세요.");
            return;
        }
        
        ShowMessage("로그인 중...");
        bool success = await authManager.SignInWithEmail(email, password);
        
        if (success)
        {
            ShowMessage("로그인 성공!");
            UpdateUserInfo();
        }
        else
        {
            ShowMessage("로그인 실패");
        }
    }
    
    private async void OnSignOutClicked()
    {
        if (authManager == null) return;
        
        ShowMessage("로그아웃 중...");
        bool success = await authManager.SignOut();
        
        if (success)
        {
            ShowMessage("로그아웃 완료");
            UpdateUserInfo();
        }
        else
        {
            ShowMessage("로그아웃 실패");
        }
    }
    
    private async void OnGuestClicked()
    {
        if (authManager == null) return;
        
        ShowMessage("게스트 로그인 중...");
        bool success = await authManager.SignInAnonymously();
        
        if (success)
        {
            ShowMessage("게스트 로그인 성공!");
            UpdateUserInfo();
        }
        else
        {
            ShowMessage("게스트 로그인 실패");
        }
    }
    
    private void UpdateUserInfo()
    {
        if (userInfoText != null && authManager != null)
        {
            string userInfo = $"사용자: {authManager.UserEmail ?? "게스트"}\nID: {authManager.UserId}";
            userInfoText.text = userInfo;
        }
    }
    
    private void ShowMessage(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"Firebase UI: {message}");
    }
}