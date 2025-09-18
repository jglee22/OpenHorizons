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
    
    private void OnGUI()
    {
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 14;
        labelStyle.normal.textColor = Color.white;
        
        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.yellow;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== Google 인증 테스트 UI ===", headerStyle);
        GUILayout.Space(5);
        
        if (googleAuth != null && googleAuth.IsSignedIn)
        {
            GUILayout.Label("=== 로그인된 사용자 정보 ===", headerStyle);
            GUILayout.Label($"이메일: {googleAuth.UserEmail}", labelStyle);
            GUILayout.Label($"사용자ID: {googleAuth.UserId}", labelStyle);
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== 테스트 키 ===", headerStyle);
        GUILayout.Label("F10: 구글 로그인 테스트", labelStyle);
        GUILayout.Label("F11: 이메일 회원가입", labelStyle);
        GUILayout.Label("F12: 이메일 로그인", labelStyle);
        GUILayout.Label("ESC: 로그아웃", labelStyle);
        
        GUILayout.Space(10);
        GUILayout.Label("=== 사용법 ===", headerStyle);
        GUILayout.Label("1. 이메일과 비밀번호 입력", labelStyle);
        GUILayout.Label("2. Google 회원가입 또는 로그인 버튼 클릭", labelStyle);
        GUILayout.Label("3. 또는 키보드 단축키 사용", labelStyle);
        
        GUILayout.EndArea();
    }
}