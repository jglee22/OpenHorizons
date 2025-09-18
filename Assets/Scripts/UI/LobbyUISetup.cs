using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 로비 UI 자동 설정 스크립트
/// </summary>
public class LobbyUISetup : MonoBehaviour
{
    [ContextMenu("로비 UI 자동 생성")]
    public void CreateLobbyUI()
    {
        // Canvas 생성
        GameObject canvas = CreateCanvas();
        
        // 로그인 패널 생성
        GameObject loginPanel = CreateLoginPanel(canvas);
        
        // 회원가입 패널 생성
        GameObject signupPanel = CreateSignupPanel(canvas);
        
        // 메인 메뉴 패널 생성
        GameObject mainMenuPanel = CreateMainMenuPanel(canvas);
        
        // 이메일 인증 패널 생성
        GameObject emailVerificationPanel = CreateEmailVerificationPanel(canvas);
        
        // 인증번호 패널 생성
        GameObject verificationCodePanel = CreateVerificationCodePanel(canvas);
        
        // LobbyUIManager 설정
        SetupLobbyUIManager(loginPanel, signupPanel, mainMenuPanel, emailVerificationPanel, verificationCodePanel);
        
        Debug.Log("✅ 로비 UI가 성공적으로 생성되었습니다!");
    }
    
    private GameObject CreateCanvas()
    {
        GameObject canvas = new GameObject("Canvas");
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.AddComponent<CanvasScaler>();
        canvas.AddComponent<GraphicRaycaster>();
        
        // Canvas Scaler 설정
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        return canvas;
    }
    
    private GameObject CreateLoginPanel(GameObject parent)
    {
        GameObject panel = CreatePanel("LoginPanel", parent, new Vector2(800, 900));
        panel.SetActive(true);
        
        // 제목
        CreateText("LoginTitle", panel, "로그인", new Vector2(0, 350), new Vector2(500, 100), 48, Color.white);
        
        // 이메일 입력
        CreateText("EmailLabel", panel, "이메일", new Vector2(-300, 220), new Vector2(150, 50), 24, Color.white);
        GameObject emailInput = CreateInputField("EmailInput", panel, new Vector2(0, 220), new Vector2(450, 60), "이메일을 입력하세요");
        
        // 비밀번호 입력
        CreateText("PasswordLabel", panel, "비밀번호", new Vector2(-300, 120), new Vector2(150, 50), 24, Color.white);
        GameObject passwordInput = CreateInputField("PasswordInput", panel, new Vector2(0, 120), new Vector2(450, 60), "비밀번호를 입력하세요");
        passwordInput.GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.Password;
        
        // 자동 로그인 체크박스
        GameObject rememberMeToggle = CreateToggle("RememberMeToggle", panel, new Vector2(0, 20), new Vector2(300, 30), "자동 로그인");
        
        // 로그인 버튼
        CreateButton("LoginButton", panel, "로그인", new Vector2(0, -146), new Vector2(400, 70), Color.blue);
        
        // 회원가입 버튼
        CreateButton("GoToSignupButton", panel, "회원가입", new Vector2(0, -253), new Vector2(400, 70), Color.green);
        
        // 상태 텍스트
        CreateText("LoginStatusText", panel, "", new Vector2(0, -361), new Vector2(700, 50), 22, Color.red);
        
        return panel;
    }
    
    private GameObject CreateSignupPanel(GameObject parent)
    {
        GameObject panel = CreatePanel("SignupPanel", parent, new Vector2(800, 1000));
        panel.SetActive(false);
        
        // 제목
        CreateText("SignupTitle", panel, "회원가입", new Vector2(0, 400), new Vector2(500, 100), 48, Color.white);
        
        // 이메일 입력
        CreateText("SignupEmailLabel", panel, "이메일", new Vector2(-300, 280), new Vector2(150, 50), 24, Color.white);
        GameObject emailInput = CreateInputField("SignupEmailInput", panel, new Vector2(0, 280), new Vector2(450, 60), "이메일을 입력하세요");
        
        // 비밀번호 입력
        CreateText("SignupPasswordLabel", panel, "비밀번호", new Vector2(-300, 180), new Vector2(150, 50), 24, Color.white);
        GameObject passwordInput = CreateInputField("SignupPasswordInput", panel, new Vector2(0, 180), new Vector2(450, 60), "비밀번호를 입력하세요");
        passwordInput.GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.Password;
        
        // 비밀번호 확인 입력
        CreateText("ConfirmPasswordLabel", panel, "비밀번호 확인", new Vector2(-300, 80), new Vector2(200, 50), 24, Color.white);
        GameObject confirmPasswordInput = CreateInputField("SignupConfirmPasswordInput", panel, new Vector2(0, 80), new Vector2(450, 60), "비밀번호를 다시 입력하세요");
        confirmPasswordInput.GetComponent<TMP_InputField>().contentType = TMP_InputField.ContentType.Password;
        
        // 회원가입 버튼
        CreateButton("SignupButton", panel, "회원가입", new Vector2(0, -20), new Vector2(400, 70), Color.blue);
        
        // 로그인 버튼
        CreateButton("GoToLoginButton", panel, "로그인으로 돌아가기", new Vector2(0, -120), new Vector2(400, 70), Color.gray);
        
        // 상태 텍스트
        CreateText("SignupStatusText", panel, "", new Vector2(0, -200), new Vector2(700, 50), 22, Color.red);
        
        return panel;
    }
    
    private GameObject CreateMainMenuPanel(GameObject parent)
    {
        GameObject panel = CreatePanel("MainMenuPanel", parent, new Vector2(800, 600));
        panel.SetActive(false);
        
        // 환영 메시지
        CreateText("WelcomeText", panel, "환영합니다!", new Vector2(0, 200), new Vector2(600, 100), 36, Color.white);
        
        // 게임 시작 버튼
        CreateButton("StartGameButton", panel, "게임 시작", new Vector2(0, 80), new Vector2(400, 70), Color.green);
        
        // 설정 버튼
        CreateButton("SettingsButton", panel, "설정", new Vector2(0, -20), new Vector2(400, 70), Color.blue);
        
        // 로그아웃 버튼
        CreateButton("LogoutButton", panel, "로그아웃", new Vector2(0, -120), new Vector2(400, 70), Color.red);
        
        return panel;
    }
    
    private GameObject CreatePanel(string name, GameObject parent, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent.transform, false);
        
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        
        // Anchor 설정 (중앙 정렬)
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Image image = panel.AddComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        return panel;
    }
    
    private void CreateText(string name, GameObject parent, string text, Vector2 position, Vector2 size, int fontSize, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        
        // Anchor 설정 (중앙 정렬)
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = color;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontStyle = FontStyles.Bold;
    }
    
    private GameObject CreateInputField(string name, GameObject parent, Vector2 position, Vector2 size, string placeholder)
    {
        GameObject inputObj = new GameObject(name);
        inputObj.transform.SetParent(parent.transform, false);
        
        RectTransform rect = inputObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        
        // Anchor 설정 (중앙 정렬)
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Image image = inputObj.AddComponent<Image>();
        image.color = Color.white;
        
        TMP_InputField inputField = inputObj.AddComponent<TMP_InputField>();
        
        // Placeholder 생성
        GameObject placeholderObj = new GameObject("Placeholder");
        placeholderObj.transform.SetParent(inputObj.transform, false);
        RectTransform placeholderRect = placeholderObj.AddComponent<RectTransform>();
        placeholderRect.anchorMin = Vector2.zero;
        placeholderRect.anchorMax = Vector2.one;
        placeholderRect.offsetMin = Vector2.zero;
        placeholderRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI placeholderText = placeholderObj.AddComponent<TextMeshProUGUI>();
        placeholderText.text = placeholder;
        placeholderText.fontSize = 22;
        placeholderText.color = Color.gray;
        placeholderText.fontStyle = FontStyles.Italic;
        
        // Text 생성
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(inputObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.fontSize = 22;
        textComponent.color = Color.black;
        
        inputField.textComponent = textComponent;
        inputField.placeholder = placeholderText;
        
        return inputObj;
    }
    
    private void CreateButton(string name, GameObject parent, string text, Vector2 position, Vector2 size, Color color)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform, false);
        
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;
        
        // Anchor 설정 (중앙 정렬)
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Image image = buttonObj.AddComponent<Image>();
        image.color = color;
        
        Button button = buttonObj.AddComponent<Button>();
        
        // 버튼 텍스트
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 24;
        textComponent.color = Color.white;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.fontStyle = FontStyles.Bold;
    }
    
    private void SetupLobbyUIManager(GameObject loginPanel, GameObject signupPanel, GameObject mainMenuPanel, GameObject emailVerificationPanel, GameObject verificationCodePanel)
    {
        GameObject managerObj = new GameObject("LobbyUIManager");
        LobbyUIManager manager = managerObj.AddComponent<LobbyUIManager>();
        
        // 인증번호 매니저 & UI 자동 생성
        GameObject codeManagerObj = new GameObject("VerificationCodeManager");
        var codeManager = codeManagerObj.AddComponent<VerificationCodeManager>();
        GameObject codeUIObj = new GameObject("VerificationCodeUI");
        var codeUI = codeUIObj.AddComponent<VerificationCodeUI>();
        
        // 이메일 서비스 자동 생성
        GameObject emailServiceObj = new GameObject("EmailService");
        var emailService = emailServiceObj.AddComponent<EmailService>();
        
        // VerificationCodeUI에 UI 패널 연결
        codeUI.verificationCodePanel = verificationCodePanel;
        codeUI.codeInputField = verificationCodePanel.transform.Find("CodeInputField")?.GetComponent<TMP_InputField>();
        codeUI.verifyButton = verificationCodePanel.transform.Find("VerifyCodeButton")?.GetComponent<Button>();
        codeUI.resendButton = verificationCodePanel.transform.Find("ResendCodeButton")?.GetComponent<Button>();
        codeUI.backButton = verificationCodePanel.transform.Find("BackToSignupButton")?.GetComponent<Button>();
        codeUI.statusText = verificationCodePanel.transform.Find("CodeStatusText")?.GetComponent<TextMeshProUGUI>();
        codeUI.emailText = verificationCodePanel.transform.Find("CodeEmailText")?.GetComponent<TextMeshProUGUI>();
        codeUI.timerText = verificationCodePanel.transform.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        codeUI.attemptsText = verificationCodePanel.transform.Find("AttemptsText")?.GetComponent<TextMeshProUGUI>();

        // UI 요소들 연결
        manager.loginPanel = loginPanel;
        manager.signupPanel = signupPanel;
        manager.mainMenuPanel = mainMenuPanel;
        manager.emailVerificationPanel = emailVerificationPanel;
        manager.verificationCodePanel = verificationCodePanel;
        
        // 로그인 UI 요소들
        manager.loginEmailInput = loginPanel.transform.Find("EmailInput")?.GetComponent<TMP_InputField>();
        manager.loginPasswordInput = loginPanel.transform.Find("PasswordInput")?.GetComponent<TMP_InputField>();
        manager.loginButton = loginPanel.transform.Find("LoginButton")?.GetComponent<Button>();
        manager.goToSignupButton = loginPanel.transform.Find("GoToSignupButton")?.GetComponent<Button>();
        manager.loginStatusText = loginPanel.transform.Find("LoginStatusText")?.GetComponent<TextMeshProUGUI>();
        
        // 회원가입 UI 요소들
        manager.signupEmailInput = signupPanel.transform.Find("SignupEmailInput")?.GetComponent<TMP_InputField>();
        manager.signupPasswordInput = signupPanel.transform.Find("SignupPasswordInput")?.GetComponent<TMP_InputField>();
        manager.signupConfirmPasswordInput = signupPanel.transform.Find("SignupConfirmPasswordInput")?.GetComponent<TMP_InputField>();
        manager.signupButton = signupPanel.transform.Find("SignupButton")?.GetComponent<Button>();
        manager.goToLoginButton = signupPanel.transform.Find("GoToLoginButton")?.GetComponent<Button>();
        manager.signupStatusText = signupPanel.transform.Find("SignupStatusText")?.GetComponent<TextMeshProUGUI>();
        
        // 메인 메뉴 UI 요소들
        manager.welcomeText = mainMenuPanel.transform.Find("WelcomeText")?.GetComponent<TextMeshProUGUI>();
        manager.logoutButton = mainMenuPanel.transform.Find("LogoutButton")?.GetComponent<Button>();
        manager.startGameButton = mainMenuPanel.transform.Find("StartGameButton")?.GetComponent<Button>();
        manager.settingsButton = mainMenuPanel.transform.Find("SettingsButton")?.GetComponent<Button>();
        
        // 자동 로그인 체크박스
        manager.rememberMeToggle = loginPanel.transform.Find("RememberMeToggle")?.GetComponent<Toggle>();
        
        // 이메일 인증 UI 요소들
        manager.verificationStatusText = emailVerificationPanel.transform.Find("VerificationStatusText")?.GetComponent<TextMeshProUGUI>();
        manager.resendVerificationButton = emailVerificationPanel.transform.Find("ResendVerificationButton")?.GetComponent<Button>();
        manager.checkVerificationButton = emailVerificationPanel.transform.Find("CheckVerificationButton")?.GetComponent<Button>();
        manager.backToLoginButton = emailVerificationPanel.transform.Find("BackToLoginButton")?.GetComponent<Button>();
        
        // 인증번호 UI 요소들
        manager.codeInputField = verificationCodePanel.transform.Find("CodeInputField")?.GetComponent<TMP_InputField>();
        manager.verifyCodeButton = verificationCodePanel.transform.Find("VerifyCodeButton")?.GetComponent<Button>();
        manager.resendCodeButton = verificationCodePanel.transform.Find("ResendCodeButton")?.GetComponent<Button>();
        manager.backToSignupButton = verificationCodePanel.transform.Find("BackToSignupButton")?.GetComponent<Button>();
        manager.codeStatusText = verificationCodePanel.transform.Find("CodeStatusText")?.GetComponent<TextMeshProUGUI>();
        manager.codeEmailText = verificationCodePanel.transform.Find("CodeEmailText")?.GetComponent<TextMeshProUGUI>();
        manager.timerText = verificationCodePanel.transform.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        manager.attemptsText = verificationCodePanel.transform.Find("AttemptsText")?.GetComponent<TextMeshProUGUI>();
        
        Debug.Log("✅ LobbyUIManager 설정 완료!");
    }
    
    private GameObject CreateToggle(string name, GameObject parent, Vector2 anchoredPosition, Vector2 sizeDelta, string labelText)
    {
        GameObject toggleObj = new GameObject(name);
        RectTransform rectTransform = toggleObj.AddComponent<RectTransform>();
        rectTransform.SetParent(parent.transform, false);
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        
        // Toggle 컴포넌트 추가
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.targetGraphic = toggleObj.AddComponent<Image>();
        toggle.targetGraphic.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        // 체크마크 이미지 생성
        GameObject checkmark = new GameObject("Checkmark");
        RectTransform checkmarkRect = checkmark.AddComponent<RectTransform>();
        checkmarkRect.SetParent(toggleObj.transform, false);
        checkmarkRect.anchorMin = new Vector2(0.2f, 0.2f);
        checkmarkRect.anchorMax = new Vector2(0.8f, 0.8f);
        checkmarkRect.offsetMin = Vector2.zero;
        checkmarkRect.offsetMax = Vector2.zero;
        
        Image checkmarkImage = checkmark.AddComponent<Image>();
        checkmarkImage.color = Color.white;
        // 간단한 체크마크 스프라이트 생성
        checkmarkImage.sprite = CreateSimpleCheckmarkSprite();
        toggle.graphic = checkmarkImage;
        
        // 라벨 텍스트 생성
        GameObject labelObj = new GameObject("Label");
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.SetParent(toggleObj.transform, false);
        labelRect.anchorMin = new Vector2(0f, 0f);
        labelRect.anchorMax = new Vector2(1f, 1f);
        labelRect.offsetMin = new Vector2(30f, 0f);
        labelRect.offsetMax = new Vector2(0f, 0f);
        
        TextMeshProUGUI labelTextComponent = labelObj.AddComponent<TextMeshProUGUI>();
        labelTextComponent.text = labelText;
        labelTextComponent.fontSize = 18;
        labelTextComponent.color = Color.white;
        labelTextComponent.alignment = TextAlignmentOptions.Left;
        
        return toggleObj;
    }
    
    private Sprite CreateCheckmarkSprite()
    {
        // 간단한 체크마크 스프라이트 생성
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // 체크마크 그리기 (간단한 X 모양)
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                if ((x == y) || (x + y == 31))
                {
                    pixels[y * 32 + x] = Color.white;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    private Sprite CreateSimpleCheckmarkSprite()
    {
        // 단순한 원형 체크마크 스프라이트 생성
        Texture2D texture = new Texture2D(32, 32);
        Color[] pixels = new Color[32 * 32];
        
        // 모든 픽셀을 투명하게 초기화
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }
        
        // 중앙에 작은 원형 그리기
        int centerX = 16;
        int centerY = 16;
        int radius = 8;
        
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float distance = Mathf.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));
                if (distance <= radius)
                {
                    pixels[y * 32 + x] = Color.white;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }
    
    private GameObject CreateEmailVerificationPanel(GameObject parent)
    {
        GameObject panel = CreatePanel("EmailVerificationPanel", parent, new Vector2(800, 600));
        panel.SetActive(false);
        
        // 제목 텍스트
        CreateText("TitleText", panel, "이메일 인증", new Vector2(0, 200), new Vector2(600, 60), 36, Color.white);
        
        // 상태 텍스트
        CreateText("VerificationStatusText", panel, "인증 메일이 전송되었습니다.\n이메일을 확인하고 인증 링크를 클릭해주세요.", new Vector2(0, 100), new Vector2(700, 200), 24, Color.white);
        
        // 재전송 버튼
        CreateButton("ResendVerificationButton", panel, "재전송", new Vector2(-150, -50), new Vector2(200, 60), Color.blue);
        
        // 인증 확인 버튼
        CreateButton("CheckVerificationButton", panel, "인증 확인", new Vector2(150, -50), new Vector2(200, 60), Color.green);
        
        // 로그인으로 돌아가기 버튼
        CreateButton("BackToLoginButton", panel, "로그인으로", new Vector2(0, -150), new Vector2(200, 50), Color.gray);
        
        return panel;
    }
    
    private GameObject CreateVerificationCodePanel(GameObject parent)
    {
        GameObject panel = CreatePanel("VerificationCodePanel", parent, new Vector2(800, 600));
        panel.SetActive(false);
        
        // 제목 텍스트
        CreateText("TitleText", panel, "이메일 인증번호", new Vector2(0, 200), new Vector2(600, 60), 36, Color.white);
        
        // 이메일 텍스트
        CreateText("CodeEmailText", panel, "인증번호가 이메일로 전송되었습니다.", new Vector2(0, 150), new Vector2(700, 50), 20, Color.white);
        
        // 인증번호 입력 필드
        GameObject codeInputObj = CreateInputField("CodeInputField", panel, new Vector2(0, 50), new Vector2(300, 60), "인증번호 6자리");
        TMP_InputField codeInput = codeInputObj.GetComponent<TMP_InputField>();
        codeInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        codeInput.characterLimit = 6;
        
        // 상태 텍스트
        CreateText("CodeStatusText", panel, "인증번호를 입력해주세요.", new Vector2(0, 0), new Vector2(700, 50), 18, Color.white);
        
        // 타이머 텍스트
        CreateText("TimerText", panel, "남은 시간: 10:00", new Vector2(0, -50), new Vector2(300, 30), 16, Color.white);
        
        // 시도 횟수 텍스트
        CreateText("AttemptsText", panel, "남은 시도: 3회", new Vector2(0, -80), new Vector2(300, 30), 16, Color.white);
        
        // 인증 확인 버튼
        CreateButton("VerifyCodeButton", panel, "인증 확인", new Vector2(0, -150), new Vector2(200, 60), Color.green);
        
        // 재전송 버튼
        CreateButton("ResendCodeButton", panel, "재전송", new Vector2(-150, -220), new Vector2(150, 50), Color.blue);
        
        // 회원가입으로 돌아가기 버튼
        CreateButton("BackToSignupButton", panel, "회원가입으로", new Vector2(150, -220), new Vector2(150, 50), Color.gray);
        
        return panel;
    }
}
