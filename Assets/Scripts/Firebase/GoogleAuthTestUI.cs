using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

/// <summary>
/// Google 인증 테스트 UI
/// </summary>
public class GoogleAuthTestUI : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button googleSignInButton;
    public Button googleSignUpButton;
    public Button signOutButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI userInfoText;
    
    [Header("Firebase 매니저")]
    public FirebaseGoogleAuth googleAuth;
    
    private void Start()
    {
        if (googleAuth == null)
            googleAuth = FindObjectOfType<FirebaseGoogleAuth>();
        
        SetupUI();
    }
    
    private void SetupUI()
    {
        if (googleSignInButton != null)
            googleSignInButton.onClick.AddListener(OnGoogleSignInClicked);
        if (googleSignUpButton != null)
            googleSignUpButton.onClick.AddListener(OnGoogleSignUpClicked);
        if (signOutButton != null)
            signOutButton.onClick.AddListener(OnSignOutClicked);
        
        UpdateUserInfo();
    }
    
    private async void OnGoogleSignInClicked()
    {
        if (googleAuth == null) return;
        
        string email = emailInput?.text?.Trim();
        string password = passwordInput?.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("이메일과 비밀번호를 입력해주세요.");
            return;
        }
        
        ShowMessage("Google 로그인 중...");
        bool success = await googleAuth.SignInWithGoogleEmail(email, password);
        
        if (success)
        {
            ShowMessage("Google 로그인 성공!");
            UpdateUserInfo();
        }
        else
        {
            ShowMessage("Google 로그인 실패");
        }
    }
    
    private async void OnGoogleSignUpClicked()
    {
        if (googleAuth == null) return;
        
        string email = emailInput?.text?.Trim();
        string password = passwordInput?.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("이메일과 비밀번호를 입력해주세요.");
            return;
        }
        
        ShowMessage("Google 회원가입 중...");
        bool success = await googleAuth.SignUpWithGoogleEmail(email, password);
        
        if (success)
        {
            ShowMessage("Google 회원가입 성공!");
            UpdateUserInfo();
        }
        else
        {
            ShowMessage("Google 회원가입 실패");
        }
    }
    
    private async void OnSignOutClicked()
    {
        if (googleAuth == null) return;
        
        ShowMessage("로그아웃 중...");
        bool success = await googleAuth.SignOut();
        
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
    
    private void UpdateUserInfo()
    {
        if (userInfoText != null && googleAuth != null)
        {
            if (googleAuth.IsSignedIn)
            {
                userInfoText.text = $"이메일: {googleAuth.UserEmail}\n사용자ID: {googleAuth.UserId}";
            }
            else
            {
                userInfoText.text = "로그인되지 않음";
            }
        }
    }
    
    private void ShowMessage(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"Google Auth UI: {message}");
    }
    
}