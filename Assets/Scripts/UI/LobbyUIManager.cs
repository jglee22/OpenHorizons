using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase.Auth;
using System.Threading.Tasks;

/// <summary>
/// 로비 씬의 로그인/회원가입 UI 관리
/// </summary>
public class LobbyUIManager : MonoBehaviour
{
    [Header("UI 패널")]
    public GameObject loginPanel;
    public GameObject signupPanel;
    public GameObject mainMenuPanel;
    
    [Header("로그인 UI")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public Toggle rememberMeToggle;
    public Button loginButton;
    public Button goToSignupButton;
    public TextMeshProUGUI loginStatusText;
    
    [Header("회원가입 UI")]
    public TMP_InputField signupEmailInput;
    public TMP_InputField signupPasswordInput;
    public TMP_InputField signupConfirmPasswordInput;
    public Button signupButton;
    public Button goToLoginButton;
    public TextMeshProUGUI signupStatusText;
    
    [Header("메인 메뉴 UI")]
    public TextMeshProUGUI welcomeText;
    public Button logoutButton;
    public Button startGameButton;
    public Button settingsButton;
    
    [Header("설정")]
    public bool enableDebugLogs = true;
    public bool enableAutoLogin = true;
    
    // 자격 증명 저장을 위한 키
    private const string REMEMBER_ME_KEY = "RememberMe";
    private const string SAVED_EMAIL_KEY = "SavedEmail";
    private const string SAVED_PASSWORD_KEY = "SavedPassword";
    
    private FirebaseAuthManager authManager;
    private bool isProcessing = false;
    
    // 탭 네비게이션을 위한 입력 필드 순서
    private TMP_InputField[] loginInputFields;
    private TMP_InputField[] signupInputFields;
    
    private void Start()
    {
        // Firebase Auth Manager 찾기
        authManager = FindObjectOfType<FirebaseAuthManager>();
        if (authManager == null)
        {
            Debug.LogError("FirebaseAuthManager를 찾을 수 없습니다!");
            return;
        }
        
        // UI 초기화
        InitializeUI();
        
        // 탭 네비게이션 초기화
        InitializeTabNavigation();
        
        // 현재 로그인 상태 확인
        CheckCurrentAuthState();
        
        // 자동 로그인 시도
        if (enableAutoLogin)
        {
            TryAutoLogin();
        }
    }
    
    private void InitializeUI()
    {
        // 버튼 이벤트 연결
        if (loginButton != null)
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        if (goToSignupButton != null)
            goToSignupButton.onClick.AddListener(OnGoToSignupClicked);
        if (signupButton != null)
            signupButton.onClick.AddListener(OnSignupButtonClicked);
        if (goToLoginButton != null)
            goToLoginButton.onClick.AddListener(OnGoToLoginClicked);
        if (logoutButton != null)
            logoutButton.onClick.AddListener(OnLogoutButtonClicked);
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnSettingsClicked);
        
        // 패스워드 입력 필드 설정
        if (loginPasswordInput != null)
            loginPasswordInput.contentType = TMP_InputField.ContentType.Password;
        if (signupPasswordInput != null)
            signupPasswordInput.contentType = TMP_InputField.ContentType.Password;
        if (signupConfirmPasswordInput != null)
            signupConfirmPasswordInput.contentType = TMP_InputField.ContentType.Password;
        
        // 초기 패널 상태 설정
        ShowLoginPanel();
    }
    
    private async void CheckCurrentAuthState()
    {
        if (authManager == null) return;
        
        try
        {
            var user = FirebaseAuth.DefaultInstance.CurrentUser;
            if (user != null)
            {
                if (enableDebugLogs)
                    Debug.Log($"현재 로그인된 사용자: {user.Email}");
                
                ShowMainMenuPanel();
                UpdateWelcomeText(user.Email);
            }
            else
            {
                if (enableDebugLogs)
                    Debug.Log("현재 로그인된 사용자가 없습니다.");
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"인증 상태 확인 오류: {e}");
        }
    }
    
    private void InitializeTabNavigation()
    {
        // 로그인 입력 필드 순서 설정
        loginInputFields = new TMP_InputField[]
        {
            loginEmailInput,
            loginPasswordInput
        };
        
        // 회원가입 입력 필드 순서 설정
        signupInputFields = new TMP_InputField[]
        {
            signupEmailInput,
            signupPasswordInput,
            signupConfirmPasswordInput
        };
        
        // Tab 키 이벤트 리스너 추가
        SetupTabNavigation(loginInputFields);
        SetupTabNavigation(signupInputFields);
    }
    
    private void SetupTabNavigation(TMP_InputField[] inputFields)
    {
        for (int i = 0; i < inputFields.Length; i++)
        {
            if (inputFields[i] != null)
            {
                int currentIndex = i;
                inputFields[i].onEndEdit.AddListener((value) =>
                {
                    if (Input.GetKeyDown(KeyCode.Tab))
                    {
                        FocusNextInputField(inputFields, currentIndex);
                    }
                });
            }
        }
    }
    
    private void FocusNextInputField(TMP_InputField[] inputFields, int currentIndex)
    {
        int nextIndex = (currentIndex + 1) % inputFields.Length;
        if (inputFields[nextIndex] != null)
        {
            SelectInputField(inputFields[nextIndex]);
        }
    }
    
    private void SelectInputField(TMP_InputField inputField)
    {
        if (inputField != null)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
    }
    
    private async void TryAutoLogin()
    {
        if (authManager == null) return;
        
        // 자동 로그인 설정 확인
        bool rememberMe = PlayerPrefs.GetInt(REMEMBER_ME_KEY, 0) == 1;
        if (!rememberMe) return;
        
        // 저장된 자격 증명 가져오기
        string savedEmail = DecryptString(PlayerPrefs.GetString(SAVED_EMAIL_KEY, ""));
        string savedPassword = DecryptString(PlayerPrefs.GetString(SAVED_PASSWORD_KEY, ""));
        
        if (string.IsNullOrEmpty(savedEmail) || string.IsNullOrEmpty(savedPassword))
            return;
        
        if (enableDebugLogs)
            Debug.Log("자동 로그인 시도 중...");
        
        // 자동 로그인 시도
        bool success = await authManager.SignInWithEmail(savedEmail, savedPassword);
        if (success)
        {
            if (enableDebugLogs)
                Debug.Log("자동 로그인 성공!");
            
            ShowMainMenuPanel();
            UpdateWelcomeText(savedEmail);
        }
        else
        {
            if (enableDebugLogs)
                Debug.Log("자동 로그인 실패");
            
            // 저장된 자격 증명 삭제
            ClearSavedCredentials();
        }
    }
    
    #region 패널 관리
    
    public void ShowLoginPanel()
    {
        SetPanelActive(loginPanel, true);
        SetPanelActive(signupPanel, false);
        SetPanelActive(mainMenuPanel, false);
        
        ClearInputFields();
        ClearStatusTexts();
        
        // 첫 번째 입력 필드에 포커스
        if (loginEmailInput != null)
        {
            SelectInputField(loginEmailInput);
        }
    }
    
    public void ShowSignupPanel()
    {
        SetPanelActive(loginPanel, false);
        SetPanelActive(signupPanel, true);
        SetPanelActive(mainMenuPanel, false);
        
        ClearInputFields();
        ClearStatusTexts();
        
        // 첫 번째 입력 필드에 포커스
        if (signupEmailInput != null)
        {
            SelectInputField(signupEmailInput);
        }
    }
    
    public void ShowMainMenuPanel()
    {
        SetPanelActive(loginPanel, false);
        SetPanelActive(signupPanel, false);
        SetPanelActive(mainMenuPanel, true);
    }
    
    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
    
    private void HideAllPanels()
    {
        SetPanelActive(loginPanel, false);
        SetPanelActive(signupPanel, false);
        SetPanelActive(mainMenuPanel, false);
    }
    
    #endregion
    
    #region 버튼 이벤트
    
    private async void OnLoginButtonClicked()
    {
        if (isProcessing) return;
        
        string email = loginEmailInput?.text?.Trim();
        string password = loginPasswordInput?.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowLoginStatus("이메일과 비밀번호를 입력해주세요.", Color.red);
            return;
        }
        
        isProcessing = true;
        SetLoginButtonEnabled(false);
        ShowLoginStatus("로그인 중...", Color.yellow);
        
        try
        {
            bool success = await authManager.SignInWithEmail(email, password);
            if (success)
            {
                ShowLoginStatus("로그인 성공!", Color.green);
                
                // 자동 로그인 설정 저장
                if (rememberMeToggle != null && rememberMeToggle.isOn)
                {
                    SaveCredentials(email, password);
                }
                else
                {
                    ClearSavedCredentials();
                }
                
                // 메인 메뉴로 이동
                await System.Threading.Tasks.Task.Delay(1000);
                ShowMainMenuPanel();
                UpdateWelcomeText(email);
            }
            else
            {
                ShowLoginStatus("로그인 실패. 이메일과 비밀번호를 확인해주세요.", Color.red);
            }
        }
        catch (System.Exception e)
        {
            ShowLoginStatus($"로그인 오류: {e.Message}", Color.red);
            if (enableDebugLogs)
                Debug.LogError($"로그인 오류: {e}");
        }
        finally
        {
            isProcessing = false;
            SetLoginButtonEnabled(true);
        }
    }
    
    private void OnGoToSignupClicked()
    {
        ShowSignupPanel();
    }
    
    private async void OnSignupButtonClicked()
    {
        if (isProcessing) return;
        
        string email = signupEmailInput?.text?.Trim();
        string password = signupPasswordInput?.text;
        string confirmPassword = signupConfirmPasswordInput?.text;
        
        // 입력 검증
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            ShowSignupStatus("모든 필드를 입력해주세요.", Color.red);
            return;
        }
        
        if (!IsValidEmail(email))
        {
            ShowSignupStatus("올바른 이메일 형식을 입력해주세요.", Color.red);
            return;
        }
        
        if (password.Length < 6)
        {
            ShowSignupStatus("비밀번호는 6자 이상이어야 합니다.", Color.red);
            return;
        }
        
        if (password != confirmPassword)
        {
            ShowSignupStatus("비밀번호가 일치하지 않습니다.", Color.red);
            return;
        }
        
        isProcessing = true;
        SetSignupButtonEnabled(false);
        ShowSignupStatus("회원가입 중...", Color.yellow);
        
        try
        {
            // 회원가입 시도 (Firebase에서 중복 체크)
            bool success = await authManager.SignUpWithEmail(email, password);
            if (success)
            {
                ShowSignupStatus("회원가입 성공! 로그인해주세요.", Color.green);
                
                // 2초 후 로그인 패널로 이동
                await System.Threading.Tasks.Task.Delay(2000);
                ShowLoginPanel();
            }
            else
            {
                ShowSignupStatus("회원가입 실패. 이미 가입된 이메일이거나 네트워크 오류입니다.", Color.red);
            }
        }
        catch (System.Exception e)
        {
            ShowSignupStatus($"회원가입 오류: {e.Message}", Color.red);
            if (enableDebugLogs)
                Debug.LogError($"회원가입 오류: {e}");
        }
        finally
        {
            isProcessing = false;
            SetSignupButtonEnabled(true);
        }
    }
    
    private void OnGoToLoginClicked()
    {
        ShowLoginPanel();
    }
    
    private async void OnLogoutButtonClicked()
    {
        if (isProcessing) return;
        
        isProcessing = true;
        SetLogoutButtonEnabled(false);
        
        try
        {
            await authManager.SignOut();
            ClearSavedCredentials();
            ShowLoginPanel();
            
            if (enableDebugLogs)
                Debug.Log("로그아웃 완료");
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"로그아웃 오류: {e}");
        }
        finally
        {
            isProcessing = false;
            SetLogoutButtonEnabled(true);
        }
    }
    
    private void OnStartGameClicked()
    {
        if (enableDebugLogs)
            Debug.Log("게임 시작 버튼 클릭");
        
        // TODO: 게임 씬으로 이동
        // SceneManager.LoadScene("GameScene");
    }
    
    private void OnSettingsClicked()
    {
        if (enableDebugLogs)
            Debug.Log("설정 버튼 클릭");
        
        // TODO: 설정 패널 표시
    }
    
    #endregion
    
    #region UI 업데이트
    
    private void ShowLoginStatus(string message, Color color)
    {
        if (loginStatusText != null)
        {
            loginStatusText.text = message;
            loginStatusText.color = color;
        }
    }
    
    private void ShowSignupStatus(string message, Color color)
    {
        if (signupStatusText != null)
        {
            signupStatusText.text = message;
            signupStatusText.color = color;
        }
    }
    
    private void UpdateWelcomeText(string email)
    {
        if (welcomeText != null)
        {
            welcomeText.text = $"환영합니다, {email}님!";
        }
    }
    
    private void SetLoginButtonEnabled(bool enabled)
    {
        if (loginButton != null)
            loginButton.interactable = enabled;
    }
    
    private void SetSignupButtonEnabled(bool enabled)
    {
        if (signupButton != null)
            signupButton.interactable = enabled;
    }
    
    private void SetLogoutButtonEnabled(bool enabled)
    {
        if (logoutButton != null)
            logoutButton.interactable = enabled;
    }
    
    private void ClearInputFields()
    {
        if (loginEmailInput != null)
            loginEmailInput.text = "";
        if (loginPasswordInput != null)
            loginPasswordInput.text = "";
        if (signupEmailInput != null)
            signupEmailInput.text = "";
        if (signupPasswordInput != null)
            signupPasswordInput.text = "";
        if (signupConfirmPasswordInput != null)
            signupConfirmPasswordInput.text = "";
    }
    
    private void ClearStatusTexts()
    {
        if (loginStatusText != null)
            loginStatusText.text = "";
        if (signupStatusText != null)
            signupStatusText.text = "";
    }
    
    #endregion
    
    #region 유틸리티
    
    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
    
    #endregion
    
    #region 자격 증명 관리
    
    private void SaveCredentials(string email, string password)
    {
        PlayerPrefs.SetInt(REMEMBER_ME_KEY, 1);
        PlayerPrefs.SetString(SAVED_EMAIL_KEY, EncryptString(email));
        PlayerPrefs.SetString(SAVED_PASSWORD_KEY, EncryptString(password));
        PlayerPrefs.Save();
        
        if (enableDebugLogs)
            Debug.Log("자격 증명이 저장되었습니다.");
    }
    
    private void ClearSavedCredentials()
    {
        PlayerPrefs.DeleteKey(REMEMBER_ME_KEY);
        PlayerPrefs.DeleteKey(SAVED_EMAIL_KEY);
        PlayerPrefs.DeleteKey(SAVED_PASSWORD_KEY);
        PlayerPrefs.Save();
        
        if (enableDebugLogs)
            Debug.Log("저장된 자격 증명이 삭제되었습니다.");
    }
    
    /// <summary>
    /// 간단한 문자열 암호화 (XOR 기반)
    /// </summary>
    private string EncryptString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";
        
        string key = "OpenHorizons2024";
        string result = "";
        
        for (int i = 0; i < input.Length; i++)
        {
            result += (char)(input[i] ^ key[i % key.Length]);
        }
        
        return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(result));
    }
    
    /// <summary>
    /// 간단한 문자열 복호화 (XOR 기반)
    /// </summary>
    private string DecryptString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "";
        
        try
        {
            byte[] data = System.Convert.FromBase64String(input);
            string encrypted = System.Text.Encoding.UTF8.GetString(data);
            
            string key = "OpenHorizons2024";
            string result = "";
            
            for (int i = 0; i < encrypted.Length; i++)
            {
                result += (char)(encrypted[i] ^ key[i % key.Length]);
            }
            
            return result;
        }
        catch
        {
            return "";
        }
    }
    
    #endregion
}
