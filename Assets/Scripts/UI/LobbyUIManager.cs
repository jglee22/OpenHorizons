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
    
    [Header("이메일 인증 UI")]
    public GameObject emailVerificationPanel;
    public TextMeshProUGUI verificationStatusText;
    public Button resendVerificationButton;
    public Button checkVerificationButton;
    public Button backToLoginButton;
    
    [Header("인증번호 UI")]
    public GameObject verificationCodePanel;
    public TMP_InputField codeInputField;
    public Button verifyCodeButton;
    public Button resendCodeButton;
    public Button backToSignupButton;
    public TextMeshProUGUI codeStatusText;
    public TextMeshProUGUI codeEmailText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI attemptsText;
    
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
        
        // 이메일 인증 이벤트 구독
        authManager.OnEmailVerificationSent += OnEmailVerificationSent;
        authManager.OnEmailVerificationFailed += OnEmailVerificationFailed;
        authManager.OnEmailVerified += OnEmailVerified;
        
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
                    Debug.Log($"이미 로그인된 사용자: {user.Email}");
                ShowMainMenuPanel();
                UpdateWelcomeText(user.Email);
            }
            else
            {
                ShowLoginPanel();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"인증 상태 확인 중 오류: {e.Message}");
            ShowLoginPanel();
        }
    }
    
    #region 탭 네비게이션
    
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
        
        // 각 입력 필드에 탭 이벤트 추가
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
                inputFields[i].onSelect.AddListener((string value) => OnInputFieldSelected(currentIndex, inputFields));
            }
        }
    }
    
    private void OnInputFieldSelected(int currentIndex, TMP_InputField[] inputFields)
    {
        // 현재 선택된 입력 필드 인덱스 저장
        currentSelectedInputIndex = currentIndex;
        currentInputFieldArray = inputFields;
    }
    
    private int currentSelectedInputIndex = -1;
    private TMP_InputField[] currentInputFieldArray;
    
    private void HandleTabNavigation()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (currentInputFieldArray != null && currentSelectedInputIndex >= 0)
            {
                // Shift + Tab: 이전 필드로 이동
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                {
                    int previousIndex = currentSelectedInputIndex - 1;
                    if (previousIndex < 0)
                        previousIndex = currentInputFieldArray.Length - 1;
                    
                    SelectInputField(currentInputFieldArray[previousIndex]);
                }
                // Tab: 다음 필드로 이동
                else
                {
                    int nextIndex = currentSelectedInputIndex + 1;
                    if (nextIndex >= currentInputFieldArray.Length)
                        nextIndex = 0;
                    
                    SelectInputField(currentInputFieldArray[nextIndex]);
                }
            }
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
    
    #endregion
    
    #region UI 패널 관리
    
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
        SetPanelActive(emailVerificationPanel, false);
        SetPanelActive(verificationCodePanel, false);
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
        
        await ProcessLogin(email, password);
    }
    
    private async void OnSignupButtonClicked()
    {
        if (isProcessing) return;
        
        string email = signupEmailInput?.text?.Trim();
        string password = signupPasswordInput?.text;
        string confirmPassword = signupConfirmPasswordInput?.text;
        
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowSignupStatus("이메일과 비밀번호를 입력해주세요.", Color.red);
            return;
        }
        
        if (password != confirmPassword)
        {
            ShowSignupStatus("비밀번호가 일치하지 않습니다.", Color.red);
            return;
        }
        
        if (password.Length < 6)
        {
            ShowSignupStatus("비밀번호는 6자 이상이어야 합니다.", Color.red);
            return;
        }
        
        ProcessSignup(email, password);
    }
    
    private void OnGoToSignupClicked()
    {
        ShowSignupPanel();
    }
    
    private void OnGoToLoginClicked()
    {
        ShowLoginPanel();
    }
    
    private async void OnLogoutButtonClicked()
    {
        if (isProcessing) return;
        
        ProcessLogout();
    }
    
    private void OnStartGameClicked()
    {
        // 메인 게임 씬으로 이동
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main");
    }
    
    private void OnSettingsClicked()
    {
        // 설정 UI 열기 (추후 구현)
        Debug.Log("설정 버튼 클릭");
    }
    
    #endregion
    
    #region Firebase 인증 처리
    
    private async System.Threading.Tasks.Task<bool> ProcessLogin(string email, string password)
    {
        isProcessing = true;
        SetLoginButtonEnabled(false);
        ShowLoginStatus("로그인 중...", Color.yellow);
        
        try
        {
            bool success = await authManager.SignInWithEmail(email, password);
            
            if (success)
            {
                ShowLoginStatus("로그인 성공!", Color.green);
                
                // 자격 증명 저장
                SaveCredentials(email, password);
                
                await System.Threading.Tasks.Task.Delay(1000); // 1초 대기
                ShowMainMenuPanel();
                UpdateWelcomeText(email);
                return true;
            }
            else
            {
                ShowLoginStatus("로그인 실패. 이메일과 비밀번호를 확인해주세요.", Color.red);
                return false;
            }
        }
        catch (System.Exception e)
        {
            ShowLoginStatus($"로그인 오류: {e.Message}", Color.red);
            if (enableDebugLogs)
                Debug.LogError($"로그인 오류: {e}");
            return false;
        }
        finally
        {
            isProcessing = false;
            SetLoginButtonEnabled(true);
        }
    }
    
    private async void ProcessSignup(string email, string password)
    {
        isProcessing = true;
        SetSignupButtonEnabled(false);
        ShowSignupStatus("회원가입 중...", Color.yellow);
        
        try
        {
            // 이메일 형식 검증
            if (!IsValidEmail(email))
            {
                ShowSignupStatus("잘못된 이메일 형식입니다.", Color.red);
                return;
            }
            
            // 비밀번호 길이 검증
            if (password.Length < 6)
            {
                ShowSignupStatus("비밀번호는 6자리 이상이어야 합니다.", Color.red);
                return;
            }
            
            // Firebase에서 이메일 중복 체크
            bool isRegistered = await authManager.IsEmailRegistered(email);
            if (isRegistered)
            {
                ShowSignupStatus("이미 가입된 이메일입니다. 로그인해 주세요.", Color.red);
                await System.Threading.Tasks.Task.Delay(2000);
                ShowLoginPanel(); // 로그인 패널로 전환
                return;
            }
            
            // 인증번호 패널로 이동 (Firebase 계정은 인증 완료 후 생성)
            ShowSignupStatus("인증번호가 전송되었습니다.", Color.green);
            await System.Threading.Tasks.Task.Delay(2000); // 2초 대기
            ShowVerificationCodePanel(email, password);
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
    
    private async void ProcessLogout()
    {
        isProcessing = true;
        SetLogoutButtonEnabled(false);
        
        try
        {
            bool success = await authManager.SignOut();
            
            if (success)
            {
                // 저장된 자격 증명 삭제
                ClearSavedCredentials();
                
                ShowMainMenuPanel();
                ShowLoginPanel();
                ClearInputFields();
                ClearStatusTexts();
            }
            else
            {
                Debug.LogError("로그아웃 실패");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"로그아웃 오류: {e.Message}");
        }
        finally
        {
            isProcessing = false;
            SetLogoutButtonEnabled(true);
        }
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
        if (loginEmailInput != null) loginEmailInput.text = "";
        if (loginPasswordInput != null) loginPasswordInput.text = "";
        if (signupEmailInput != null) signupEmailInput.text = "";
        if (signupPasswordInput != null) signupPasswordInput.text = "";
        if (signupConfirmPasswordInput != null) signupConfirmPasswordInput.text = "";
    }
    
    private void ClearStatusTexts()
    {
        if (loginStatusText != null) loginStatusText.text = "";
        if (signupStatusText != null) signupStatusText.text = "";
    }
    
    #endregion
    
    #region 키보드 단축키
    
    private void Update()
    {
        // 탭 네비게이션 처리
        HandleTabNavigation();
        
        // Enter 키로 로그인/회원가입
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (loginPanel != null && loginPanel.activeInHierarchy)
            {
                OnLoginButtonClicked();
            }
            else if (signupPanel != null && signupPanel.activeInHierarchy)
            {
                OnSignupButtonClicked();
            }
        }
        
        // ESC 키로 패널 전환
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (signupPanel != null && signupPanel.activeInHierarchy)
            {
                OnGoToLoginClicked();
            }
            else if (mainMenuPanel != null && mainMenuPanel.activeInHierarchy)
            {
                OnLogoutButtonClicked();
            }
        }
    }
    
    #endregion
    
    #region 자격 증명 저장 및 자동 로그인
    
    /// <summary>
    /// 자동 로그인 시도
    /// </summary>
    private async void TryAutoLogin()
    {
        if (!IsRememberMeEnabled())
        {
            if (enableDebugLogs)
                Debug.Log("자동 로그인 비활성화됨");
            return;
        }
        
        string savedEmail = GetSavedEmail();
        string savedPassword = GetSavedPassword();
        
        if (string.IsNullOrEmpty(savedEmail) || string.IsNullOrEmpty(savedPassword))
        {
            if (enableDebugLogs)
                Debug.Log("저장된 자격 증명이 없음");
            return;
        }
        
        if (enableDebugLogs)
            Debug.Log("자동 로그인 시도 중...");
        
        // 로그인 패널 표시
        ShowLoginPanel();
        
        // 저장된 자격 증명으로 로그인 시도
        bool success = await ProcessLogin(savedEmail, savedPassword);
        
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
    
    /// <summary>
    /// 자격 증명 저장
    /// </summary>
    private void SaveCredentials(string email, string password)
    {
        if (rememberMeToggle != null && rememberMeToggle.isOn)
        {
            // 암호화된 형태로 저장
            string encryptedEmail = EncryptString(email);
            string encryptedPassword = EncryptString(password);
            
            PlayerPrefs.SetString(SAVED_EMAIL_KEY, encryptedEmail);
            PlayerPrefs.SetString(SAVED_PASSWORD_KEY, encryptedPassword);
            PlayerPrefs.SetInt(REMEMBER_ME_KEY, 1);
            PlayerPrefs.Save();
            
            if (enableDebugLogs)
                Debug.Log("자격 증명 저장됨");
        }
        else
        {
            ClearSavedCredentials();
        }
    }
    
    /// <summary>
    /// 저장된 자격 증명 삭제
    /// </summary>
    private void ClearSavedCredentials()
    {
        PlayerPrefs.DeleteKey(SAVED_EMAIL_KEY);
        PlayerPrefs.DeleteKey(SAVED_PASSWORD_KEY);
        PlayerPrefs.DeleteKey(REMEMBER_ME_KEY);
        PlayerPrefs.Save();
        
        if (enableDebugLogs)
            Debug.Log("저장된 자격 증명 삭제됨");
    }
    
    /// <summary>
    /// 저장된 이메일 가져오기
    /// </summary>
    private string GetSavedEmail()
    {
        string encryptedEmail = PlayerPrefs.GetString(SAVED_EMAIL_KEY, "");
        if (string.IsNullOrEmpty(encryptedEmail))
            return "";
        
        return DecryptString(encryptedEmail);
    }
    
    /// <summary>
    /// 저장된 비밀번호 가져오기
    /// </summary>
    private string GetSavedPassword()
    {
        string encryptedPassword = PlayerPrefs.GetString(SAVED_PASSWORD_KEY, "");
        if (string.IsNullOrEmpty(encryptedPassword))
            return "";
        
        return DecryptString(encryptedPassword);
    }
    
    /// <summary>
    /// 자동 로그인 활성화 여부 확인
    /// </summary>
    private bool IsRememberMeEnabled()
    {
        return PlayerPrefs.GetInt(REMEMBER_ME_KEY, 0) == 1;
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
    
    #region 이메일 인증 처리
    
    /// <summary>
    /// 이메일 인증 패널 표시
    /// </summary>
    private void ShowEmailVerificationPanel(string email)
    {
        HideAllPanels();
        
        if (emailVerificationPanel != null)
        {
            emailVerificationPanel.SetActive(true);
            UpdateVerificationStatusText($"인증 메일이 {email}로 전송되었습니다.\n이메일을 확인하고 인증 링크를 클릭해주세요.");
        }
        
        // 버튼 이벤트 연결
        if (resendVerificationButton != null)
            resendVerificationButton.onClick.AddListener(OnResendVerificationClicked);
        if (checkVerificationButton != null)
            checkVerificationButton.onClick.AddListener(OnCheckVerificationClicked);
        if (backToLoginButton != null)
            backToLoginButton.onClick.AddListener(OnBackToLoginClicked);
    }
    
    /// <summary>
    /// 인증 상태 텍스트 업데이트
    /// </summary>
    private void UpdateVerificationStatusText(string message)
    {
        if (verificationStatusText != null)
        {
            verificationStatusText.text = message;
        }
    }
    
    /// <summary>
    /// 인증 메일 재전송 버튼 클릭
    /// </summary>
    private async void OnResendVerificationClicked()
    {
        if (isProcessing) return;
        
        isProcessing = true;
        SetVerificationButtonsEnabled(false);
        UpdateVerificationStatusText("인증 메일 재전송 중...");
        
        try
        {
            bool success = await authManager.SendEmailVerification();
            if (success)
            {
                UpdateVerificationStatusText("인증 메일이 재전송되었습니다.\n이메일을 확인해주세요.");
            }
            else
            {
                UpdateVerificationStatusText("인증 메일 재전송에 실패했습니다.\n잠시 후 다시 시도해주세요.");
            }
        }
        catch (System.Exception e)
        {
            UpdateVerificationStatusText($"오류가 발생했습니다: {e.Message}");
            if (enableDebugLogs)
                Debug.LogError($"인증 메일 재전송 오류: {e}");
        }
        finally
        {
            isProcessing = false;
            SetVerificationButtonsEnabled(true);
        }
    }
    
    /// <summary>
    /// 인증 상태 확인 버튼 클릭
    /// </summary>
    private async void OnCheckVerificationClicked()
    {
        if (isProcessing) return;
        
        isProcessing = true;
        SetVerificationButtonsEnabled(false);
        UpdateVerificationStatusText("인증 상태 확인 중...");
        
        try
        {
            bool refreshed = await authManager.RefreshUser();
            if (refreshed)
            {
                if (authManager.IsEmailVerified())
                {
                    UpdateVerificationStatusText("이메일 인증이 완료되었습니다!");
                    await System.Threading.Tasks.Task.Delay(2000);
                    
                    // 인증 패널 숨기기
                    if (emailVerificationPanel != null)
                        emailVerificationPanel.SetActive(false);
                    
                    ShowMainMenuPanel();
                    UpdateWelcomeText(authManager.UserEmail);
                }
                else
                {
                    UpdateVerificationStatusText("아직 이메일 인증이 완료되지 않았습니다.\n이메일을 확인하고 인증 링크를 클릭해주세요.");
                }
            }
            else
            {
                UpdateVerificationStatusText("인증 상태 확인에 실패했습니다.\n잠시 후 다시 시도해주세요.");
            }
        }
        catch (System.Exception e)
        {
            UpdateVerificationStatusText($"오류가 발생했습니다: {e.Message}");
            if (enableDebugLogs)
                Debug.LogError($"인증 상태 확인 오류: {e}");
        }
        finally
        {
            isProcessing = false;
            SetVerificationButtonsEnabled(true);
        }
    }
    
    /// <summary>
    /// 로그인으로 돌아가기 버튼 클릭
    /// </summary>
    private void OnBackToLoginClicked()
    {
        ShowLoginPanel();
    }
    
    /// <summary>
    /// 인증 버튼들 활성화/비활성화
    /// </summary>
    private void SetVerificationButtonsEnabled(bool enabled)
    {
        if (resendVerificationButton != null)
            resendVerificationButton.interactable = enabled;
        if (checkVerificationButton != null)
            checkVerificationButton.interactable = enabled;
        if (backToLoginButton != null)
            backToLoginButton.interactable = enabled;
    }
    
    /// <summary>
    /// 이메일 인증 메일 전송 성공 이벤트
    /// </summary>
    private void OnEmailVerificationSent()
    {
        if (enableDebugLogs)
            Debug.Log("이메일 인증 메일이 전송되었습니다.");
    }
    
    /// <summary>
    /// 이메일 인증 메일 전송 실패 이벤트
    /// </summary>
    private void OnEmailVerificationFailed(string errorMessage)
    {
        if (enableDebugLogs)
            Debug.LogError($"이메일 인증 메일 전송 실패: {errorMessage}");
        
        UpdateVerificationStatusText($"인증 메일 전송 실패: {errorMessage}");
    }
    
    /// <summary>
    /// 이메일 인증 완료 이벤트
    /// </summary>
    private void OnEmailVerified()
    {
        if (enableDebugLogs)
            Debug.Log("이메일 인증이 완료되었습니다.");
        
        UpdateVerificationStatusText("이메일 인증이 완료되었습니다!");
        
        // 2초 후 메인 메뉴로 이동
        System.Threading.Tasks.Task.Delay(2000).ContinueWith(_ =>
        {
            // 인증 패널 숨기기
            if (emailVerificationPanel != null)
                emailVerificationPanel.SetActive(false);
            
            // 메인 메뉴 표시
            ShowMainMenuPanel();
            UpdateWelcomeText(authManager.UserEmail);
        });
    }
    
    #endregion
    
    #region 인증번호 처리
    
    /// <summary>
    /// 인증번호 패널 표시
    /// </summary>
    private void ShowVerificationCodePanel(string email, string password)
    {
        HideAllPanels();
        
        if (verificationCodePanel != null)
        {
            verificationCodePanel.SetActive(true);
        }
        
        // VerificationCodeUI 컴포넌트 찾기
        var verificationUI = FindObjectOfType<VerificationCodeUI>();
        if (verificationUI != null)
        {
            verificationUI.ShowVerificationPanel(email, password);
        }
        
        // 버튼 이벤트 연결
        if (verifyCodeButton != null)
            verifyCodeButton.onClick.AddListener(() => OnVerifyCodeButtonClicked(email, password));
        if (resendCodeButton != null)
            resendCodeButton.onClick.AddListener(() => OnResendCodeButtonClicked(email));
        if (backToSignupButton != null)
            backToSignupButton.onClick.AddListener(OnBackToSignupClicked);
    }
    
    /// <summary>
    /// 인증번호 확인 버튼 클릭
    /// </summary>
    private async void OnVerifyCodeButtonClicked(string email, string password)
    {
        if (isProcessing) return;
        
        string inputCode = codeInputField?.text?.Trim();
        if (string.IsNullOrEmpty(inputCode))
        {
            UpdateCodeStatusText("인증번호를 입력해주세요.", Color.red);
            return;
        }
        
        if (inputCode.Length != 6)
        {
            UpdateCodeStatusText("인증번호는 6자리입니다.", Color.red);
            return;
        }
        
        isProcessing = true;
        SetCodeButtonsEnabled(false);
        UpdateCodeStatusText("인증번호 확인 중...", Color.yellow);
        
        try
        {
            var codeManager = FindObjectOfType<VerificationCodeManager>();
            if (codeManager != null)
            {
                bool success = await codeManager.VerifyCode(email, inputCode);
                if (success)
                {
                    UpdateCodeStatusText("인증 완료! 계정을 생성하는 중...", Color.green);
                    
                    // 인증 완료 후 Firebase 계정 생성
                    bool accountCreated = await authManager.SignUpWithEmail(email, password);
                    if (accountCreated)
                    {
                        // 이메일을 등록 목록에 추가
                        await authManager.RegisterEmail(email);
                        
                        UpdateCodeStatusText("회원가입이 완료되었습니다!", Color.green);
                        await System.Threading.Tasks.Task.Delay(2000);
                        
                        // 인증 패널 숨기기
                        if (verificationCodePanel != null)
                            verificationCodePanel.SetActive(false);
                        
                        ShowMainMenuPanel();
                        UpdateWelcomeText(email);
                    }
                    else
                    {
                        // 계정 생성 실패 시 중복 가입 가능성 체크
                        UpdateCodeStatusText("이미 가입된 이메일입니다. 로그인해 주세요.", Color.red);
                        await System.Threading.Tasks.Task.Delay(2000);
                        ShowLoginPanel();
                    }
                }
                else
                {
                    UpdateCodeStatusText("잘못된 인증번호입니다.", Color.red);
                }
            }
        }
        catch (System.Exception e)
        {
            UpdateCodeStatusText($"오류가 발생했습니다: {e.Message}", Color.red);
            if (enableDebugLogs)
                Debug.LogError($"인증번호 확인 오류: {e}");
        }
        finally
        {
            isProcessing = false;
            SetCodeButtonsEnabled(true);
        }
    }
    
    /// <summary>
    /// 인증번호 재전송 버튼 클릭
    /// </summary>
    private async void OnResendCodeButtonClicked(string email)
    {
        if (isProcessing) return;
        
        isProcessing = true;
        SetCodeButtonsEnabled(false);
        UpdateCodeStatusText("인증번호 재전송 중...", Color.yellow);
        
        try
        {
            var codeManager = FindObjectOfType<VerificationCodeManager>();
            if (codeManager != null)
            {
                bool success = await codeManager.SendVerificationCode(email);
                if (success)
                {
                    UpdateCodeStatusText("인증번호가 재전송되었습니다.", Color.green);
                }
                else
                {
                    UpdateCodeStatusText("재전송에 실패했습니다.", Color.red);
                }
            }
        }
        catch (System.Exception e)
        {
            UpdateCodeStatusText($"오류가 발생했습니다: {e.Message}", Color.red);
            if (enableDebugLogs)
                Debug.LogError($"인증번호 재전송 오류: {e}");
        }
        finally
        {
            isProcessing = false;
            SetCodeButtonsEnabled(true);
        }
    }
    
    /// <summary>
    /// 회원가입으로 돌아가기 버튼 클릭
    /// </summary>
    private void OnBackToSignupClicked()
    {
        ShowSignupPanel();
    }
    
    /// <summary>
    /// 인증번호 상태 텍스트 업데이트
    /// </summary>
    private void UpdateCodeStatusText(string message, Color color)
    {
        if (codeStatusText != null)
        {
            codeStatusText.text = message;
            codeStatusText.color = color;
        }
    }
    
    /// <summary>
    /// 인증번호 버튼들 활성화/비활성화
    /// </summary>
    private void SetCodeButtonsEnabled(bool enabled)
    {
        if (verifyCodeButton != null)
            verifyCodeButton.interactable = enabled;
        if (resendCodeButton != null)
            resendCodeButton.interactable = enabled;
        if (backToSignupButton != null)
            backToSignupButton.interactable = enabled;
    }
    
    /// <summary>
    /// 이메일 형식 검증
    /// </summary>
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
}
