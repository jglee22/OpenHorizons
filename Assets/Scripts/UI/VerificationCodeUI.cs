using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

/// <summary>
/// 인증번호 입력 UI 관리
/// </summary>
public class VerificationCodeUI : MonoBehaviour
{
    [Header("UI 요소")]
    public GameObject verificationCodePanel;
    public TMP_InputField codeInputField;
    public Button verifyButton;
    public Button resendButton;
    public Button backButton;
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI emailText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI attemptsText;
    
    [Header("설정")]
    public bool enableDebugLogs = true;
    
    private VerificationCodeManager codeManager;
    private string currentEmail;
    private string currentPassword;
    private bool isProcessing = false;
    private float timer = 0f;
    private bool isTimerActive = false;
    
    private void Start()
    {
        // VerificationCodeManager 찾기
        codeManager = FindObjectOfType<VerificationCodeManager>();
        if (codeManager == null)
        {
            Debug.LogError("VerificationCodeManager를 찾을 수 없습니다!");
            return;
        }
        
        // 이벤트 구독
        codeManager.OnCodeSent += OnCodeSent;
        codeManager.OnCodeVerified += OnCodeVerified;
        codeManager.OnCodeExpired += OnCodeExpired;
        codeManager.OnCodeInvalid += OnCodeInvalid;
        
        // UI 초기화
        InitializeUI();
    }
    
    private void Update()
    {
        // 타이머 업데이트
        if (isTimerActive)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                timer = 0;
                isTimerActive = false;
                UpdateTimerDisplay();
                UpdateStatusText("인증번호가 만료되었습니다. 재전송해주세요.", Color.red);
            }
            else
            {
                UpdateTimerDisplay();
            }
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (codeManager != null)
        {
            codeManager.OnCodeSent -= OnCodeSent;
            codeManager.OnCodeVerified -= OnCodeVerified;
            codeManager.OnCodeExpired -= OnCodeExpired;
            codeManager.OnCodeInvalid -= OnCodeInvalid;
        }
    }
    
    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        if (verifyButton != null)
            verifyButton.onClick.AddListener(OnVerifyButtonClicked);
        if (resendButton != null)
            resendButton.onClick.AddListener(OnResendButtonClicked);
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
        if (codeInputField != null)
            codeInputField.onValueChanged.AddListener(OnCodeInputChanged);
    }
    
    /// <summary>
    /// 인증번호 입력 패널 표시
    /// </summary>
    public async void ShowVerificationPanel(string email, string password)
    {
        currentEmail = email;
        currentPassword = password;
        
        if (verificationCodePanel != null)
            verificationCodePanel.SetActive(true);
        
        if (emailText != null)
            emailText.text = $"인증번호가 {email}로 전송되었습니다.";
        
        // 입력 필드 초기화
        if (codeInputField != null)
            codeInputField.text = "";
        
        // 상태 초기화
        UpdateStatusText("인증번호 전송 중...", Color.yellow);
        UpdateAttemptsDisplay();
        
        // 인증번호 전송
        if (codeManager != null)
        {
            bool sent = await codeManager.SendVerificationCode(email);
            if (sent)
            {
                UpdateStatusText("인증번호가 전송되었습니다. 받은 번호를 입력해주세요.", Color.white);
                // 타이머 시작
                StartTimer();
            }
            else
            {
                UpdateStatusText("인증번호 전송에 실패했습니다. 잠시 후 다시 시도해주세요.", Color.red);
            }
        }
    }
    
    /// <summary>
    /// 인증번호 입력 패널 숨기기
    /// </summary>
    public void HideVerificationPanel()
    {
        if (verificationCodePanel != null)
            verificationCodePanel.SetActive(false);
        
        isTimerActive = false;
        timer = 0f;
    }
    
    /// <summary>
    /// 인증 확인 버튼 클릭
    /// </summary>
    private async void OnVerifyButtonClicked()
    {
        if (isProcessing) return;
        
        string inputCode = codeInputField?.text?.Trim();
        if (string.IsNullOrEmpty(inputCode))
        {
            UpdateStatusText("인증번호를 입력해주세요.", Color.red);
            return;
        }
        
        if (inputCode.Length != 6)
        {
            UpdateStatusText("인증번호는 6자리입니다.", Color.red);
            return;
        }
        
        isProcessing = true;
        SetButtonsEnabled(false);
        UpdateStatusText("인증번호 확인 중...", Color.yellow);
        
        try
        {
            bool success = await codeManager.VerifyCode(currentEmail, inputCode);
            if (success)
            {
                UpdateStatusText("인증이 완료되었습니다!", Color.green);
                await System.Threading.Tasks.Task.Delay(2000);
                HideVerificationPanel();
            }
        }
        catch (System.Exception e)
        {
            UpdateStatusText($"오류가 발생했습니다: {e.Message}", Color.red);
            if (enableDebugLogs)
                Debug.LogError($"인증 확인 오류: {e}");
        }
        finally
        {
            isProcessing = false;
            SetButtonsEnabled(true);
        }
    }
    
    /// <summary>
    /// 재전송 버튼 클릭
    /// </summary>
    private async void OnResendButtonClicked()
    {
        if (isProcessing) return;
        
        isProcessing = true;
        SetButtonsEnabled(false);
        UpdateStatusText("인증번호 재전송 중...", Color.yellow);
        
        try
        {
            bool success = await codeManager.SendVerificationCode(currentEmail);
            if (success)
            {
                UpdateStatusText("인증번호가 재전송되었습니다.", Color.green);
                StartTimer();
                UpdateAttemptsDisplay();
            }
            else
            {
                UpdateStatusText("재전송에 실패했습니다. 잠시 후 다시 시도해주세요.", Color.red);
            }
        }
        catch (System.Exception e)
        {
            UpdateStatusText($"오류가 발생했습니다: {e.Message}", Color.red);
            if (enableDebugLogs)
                Debug.LogError($"재전송 오류: {e}");
        }
        finally
        {
            isProcessing = false;
            SetButtonsEnabled(true);
        }
    }
    
    /// <summary>
    /// 뒤로가기 버튼 클릭
    /// </summary>
    private void OnBackButtonClicked()
    {
        HideVerificationPanel();
    }
    
    /// <summary>
    /// 인증번호 입력 변경
    /// </summary>
    private void OnCodeInputChanged(string value)
    {
        // 숫자만 입력 허용
        if (codeInputField != null)
        {
            string filtered = System.Text.RegularExpressions.Regex.Replace(value, @"[^0-9]", "");
            if (filtered != value)
            {
                codeInputField.text = filtered;
            }
        }
    }
    
    /// <summary>
    /// 타이머 시작
    /// </summary>
    private void StartTimer()
    {
        int seconds = 600;
        if (codeManager != null)
        {
            seconds = codeManager.GetCurrentExpirySeconds();
        }
        timer = seconds;
        isTimerActive = true;
    }
    
    /// <summary>
    /// 타이머 표시 업데이트
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.FloorToInt(timer % 60);
            timerText.text = $"남은 시간: {minutes:00}:{seconds:00}";
            
            if (timer <= 60)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// 상태 텍스트 업데이트
    /// </summary>
    private void UpdateStatusText(string message, Color color)
    {
        if (statusText != null)
        {
            statusText.text = message;
            statusText.color = color;
        }
    }
    
    /// <summary>
    /// 시도 횟수 표시 업데이트
    /// </summary>
    private void UpdateAttemptsDisplay()
    {
        if (attemptsText != null && codeManager != null)
        {
            int remaining = codeManager.GetRemainingAttempts(currentEmail);
            attemptsText.text = $"남은 시도: {remaining}회";
            
            if (remaining <= 1)
            {
                attemptsText.color = Color.red;
            }
            else
            {
                attemptsText.color = Color.white;
            }
        }
    }
    
    /// <summary>
    /// 버튼들 활성화/비활성화
    /// </summary>
    private void SetButtonsEnabled(bool enabled)
    {
        if (verifyButton != null)
            verifyButton.interactable = enabled;
        if (resendButton != null)
            resendButton.interactable = enabled;
        if (backButton != null)
            backButton.interactable = enabled;
    }
    
    #region 이벤트 핸들러
    
    private void OnCodeSent(string email)
    {
        if (enableDebugLogs)
            Debug.Log($"인증번호가 {email}로 전송되었습니다.");
    }
    
    private void OnCodeVerified(string email)
    {
        if (enableDebugLogs)
            Debug.Log($"인증번호가 {email}에서 검증되었습니다.");
        
        // 인증 성공 시 UI 업데이트
        UpdateStatusText("인증이 완료되었습니다!", Color.green);
    }
    
    private void OnCodeExpired(string message)
    {
        if (enableDebugLogs)
            Debug.LogWarning($"인증번호 만료: {message}");
        
        UpdateStatusText(message, Color.red);
        isTimerActive = false;
    }
    
    private void OnCodeInvalid(string message)
    {
        if (enableDebugLogs)
            Debug.LogWarning($"인증번호 오류: {message}");
        
        UpdateStatusText(message, Color.red);
        UpdateAttemptsDisplay();
    }
    
    #endregion
}
