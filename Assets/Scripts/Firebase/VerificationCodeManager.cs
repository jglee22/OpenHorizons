using UnityEngine;
using Firebase;
using Firebase.Database;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 인증번호 관리 시스템
/// </summary>
public class VerificationCodeManager : MonoBehaviour
{
    [Header("인증번호 설정")]
    public int codeLength = 6;
    public int codeExpiryMinutes = 10;
    public int maxAttempts = 3;
    public int resendCooldownMinutes = 1;
    
    [Header("디버그/테스트 설정")]
    public bool useDebugShortExpiry = false; // 테스트용 짧은 만료 시간 사용
    public int debugExpirySeconds = 20;      // 디버그 만료 시간(초)
    
    [Header("디버그 설정")]
    public bool enableDebugLogs = true;
    
    private DatabaseReference databaseRef;
    private Dictionary<string, VerificationCodeData> pendingCodes = new Dictionary<string, VerificationCodeData>();
    private EmailService emailService;
    
    // 이벤트
    public System.Action<string> OnCodeSent;
    public System.Action<string> OnCodeVerified;
    public System.Action<string> OnCodeExpired;
    public System.Action<string> OnCodeInvalid;
    
    private void Start()
    {
        // Firebase Database 참조 초기화
        databaseRef = FirebaseDatabase.DefaultInstance.RootReference;
        
        // 이메일 서비스 찾기 또는 생성
        emailService = FindObjectOfType<EmailService>();
        if (emailService == null)
        {
            GameObject emailServiceObj = new GameObject("EmailService");
            emailService = emailServiceObj.AddComponent<EmailService>();
        }
    }
    
    /// <summary>
    /// 인증번호 생성 및 전송
    /// </summary>
    public async Task<bool> SendVerificationCode(string email)
    {
        try
        {
            // 이메일 형식 검증
            if (!IsValidEmail(email))
            {
                if (enableDebugLogs)
                    Debug.LogError("잘못된 이메일 형식입니다.");
                return false;
            }
            
            // 기존 인증번호가 있는지 확인
            if (pendingCodes.ContainsKey(email))
            {
                var existingCode = pendingCodes[email];
                if (DateTime.Now < existingCode.lastSentTime.AddMinutes(resendCooldownMinutes))
                {
                    if (enableDebugLogs)
                        Debug.LogWarning("재전송 쿨다운 시간이 아직 남았습니다.");
                    return false;
                }
            }
            
            // 인증번호 생성
            string code = GenerateVerificationCode();
            string encryptedCode = EncryptCode(code);
            
            // 인증번호 데이터 생성
            var codeData = new VerificationCodeData
            {
                email = email,
                code = encryptedCode,
                attempts = 0,
                createdAt = DateTime.Now,
                expiresAt = DateTime.Now.AddSeconds(GetCurrentExpirySeconds()),
                lastSentTime = DateTime.Now
            };
            
            // 로컬 캐시에 저장
            pendingCodes[email] = codeData;
            
            // Firebase Database에 저장
            await SaveCodeToDatabase(email, codeData);
            
            // 이메일 전송
            if (emailService != null)
            {
                bool emailSent = await emailService.SendVerificationEmail(email, code);
                if (!emailSent)
                {
                    if (enableDebugLogs)
                        Debug.LogError("이메일 전송에 실패했습니다.");
                    return false;
                }
            }
            else
            {
                if (enableDebugLogs)
                    Debug.LogError("이메일 서비스를 찾을 수 없습니다.");
                return false;
            }
            
            if (enableDebugLogs)
                Debug.Log($"인증번호가 {email}로 전송되었습니다.");
            
            OnCodeSent?.Invoke(email);
            return true;
        }
        catch (Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"인증번호 전송 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 인증번호 검증
    /// </summary>
    public async Task<bool> VerifyCode(string email, string inputCode)
    {
        try
        {
            // 로컬 캐시에서 확인
            if (!pendingCodes.ContainsKey(email))
            {
                // Firebase에서 로드 시도
                var codeData = await LoadCodeFromDatabase(email);
                if (codeData == null)
                {
                    if (enableDebugLogs)
                        Debug.LogError("인증번호를 찾을 수 없습니다.");
                    OnCodeInvalid?.Invoke("인증번호를 찾을 수 없습니다.");
                    return false;
                }
                pendingCodes[email] = codeData;
            }
            
            var verificationData = pendingCodes[email];
            
            // 만료 시간 확인
            if (DateTime.Now > verificationData.expiresAt)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("인증번호가 만료되었습니다.");
                OnCodeExpired?.Invoke("인증번호가 만료되었습니다.");
                await RemoveCodeFromDatabase(email);
                pendingCodes.Remove(email);
                return false;
            }
            
            // 시도 횟수 확인
            if (verificationData.attempts >= maxAttempts)
            {
                if (enableDebugLogs)
                    Debug.LogWarning("최대 시도 횟수를 초과했습니다.");
                OnCodeInvalid?.Invoke("최대 시도 횟수를 초과했습니다.");
                await RemoveCodeFromDatabase(email);
                pendingCodes.Remove(email);
                return false;
            }
            
            // 인증번호 검증
            string decryptedCode = DecryptCode(verificationData.code);
            if (inputCode == decryptedCode)
            {
                if (enableDebugLogs)
                    Debug.Log("인증번호가 올바릅니다.");
                
                // 성공 시 데이터 삭제
                await RemoveCodeFromDatabase(email);
                pendingCodes.Remove(email);
                
                OnCodeVerified?.Invoke(email);
                return true;
            }
            else
            {
                // 실패 시 시도 횟수 증가
                verificationData.attempts++;
                await SaveCodeToDatabase(email, verificationData);
                
                if (enableDebugLogs)
                    Debug.LogWarning($"잘못된 인증번호입니다. 남은 시도: {maxAttempts - verificationData.attempts}");
                
                OnCodeInvalid?.Invoke($"잘못된 인증번호입니다. 남은 시도: {maxAttempts - verificationData.attempts}");
                return false;
            }
        }
        catch (Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"인증번호 검증 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 인증번호 생성
    /// </summary>
    private string GenerateVerificationCode()
    {
        const string chars = "0123456789";
        var random = new System.Random();
        var result = new StringBuilder(codeLength);
        
        for (int i = 0; i < codeLength; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }
        
        return result.ToString();
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
    
    /// <summary>
    /// 인증번호 암호화
    /// </summary>
    private string EncryptCode(string code)
    {
        // 간단한 XOR 암호화
        string key = "OpenHorizons2024";
        string result = "";
        
        for (int i = 0; i < code.Length; i++)
        {
            result += (char)(code[i] ^ key[i % key.Length]);
        }
        
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(result));
    }
    
    /// <summary>
    /// 인증번호 복호화
    /// </summary>
    private string DecryptCode(string encryptedCode)
    {
        try
        {
            string key = "OpenHorizons2024";
            string encrypted = Encoding.UTF8.GetString(Convert.FromBase64String(encryptedCode));
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
    
    /// <summary>
    /// Firebase Database에 인증번호 저장
    /// </summary>
    private async System.Threading.Tasks.Task SaveCodeToDatabase(string email, VerificationCodeData codeData)
    {
        try
        {
            string json = JsonUtility.ToJson(codeData);
            await databaseRef.Child("verification_codes").Child(email.Replace(".", "_")).SetRawJsonValueAsync(json);
        }
        catch (Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"Firebase 저장 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// Firebase Database에서 인증번호 로드
    /// </summary>
    private async System.Threading.Tasks.Task<VerificationCodeData> LoadCodeFromDatabase(string email)
    {
        try
        {
            var snapshot = await databaseRef.Child("verification_codes").Child(email.Replace(".", "_")).GetValueAsync();
            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                return JsonUtility.FromJson<VerificationCodeData>(json);
            }
            return null;
        }
        catch (Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"Firebase 로드 실패: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Firebase Database에서 인증번호 삭제
    /// </summary>
    private async System.Threading.Tasks.Task RemoveCodeFromDatabase(string email)
    {
        try
        {
            await databaseRef.Child("verification_codes").Child(email.Replace(".", "_")).RemoveValueAsync();
        }
        catch (Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"Firebase 삭제 실패: {e.Message}");
        }
    }
    
    /// <summary>
    /// 이메일 전송 (시뮬레이션)
    /// </summary>
    private async System.Threading.Tasks.Task SendEmailWithCode(string email, string code)
    {
        // 실제 구현에서는 이메일 서비스 API를 사용
        // 여기서는 시뮬레이션으로 콘솔에 출력
        if (enableDebugLogs)
        {
            Debug.Log($"=== 이메일 전송 시뮬레이션 ===");
            Debug.Log($"받는 사람: {email}");
            Debug.Log($"제목: [OpenHorizons] 이메일 인증번호");
            Debug.Log($"내용: 인증번호: {code}");
            Debug.Log($"만료시간: {codeExpiryMinutes}분 후");
            Debug.Log($"===============================");
        }
        
        // 실제 이메일 전송을 위한 지연 시뮬레이션
        await System.Threading.Tasks.Task.Delay(1000);
    }
    
    /// <summary>
    /// 남은 시도 횟수 확인
    /// </summary>
    public int GetRemainingAttempts(string email)
    {
        if (pendingCodes.ContainsKey(email))
        {
            return maxAttempts - pendingCodes[email].attempts;
        }
        return maxAttempts;
    }
    
    /// <summary>
    /// 만료 시간까지 남은 시간 확인
    /// </summary>
    public TimeSpan GetRemainingTime(string email)
    {
        if (pendingCodes.ContainsKey(email))
        {
            var remaining = pendingCodes[email].expiresAt - DateTime.Now;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }
        return TimeSpan.Zero;
    }

    /// <summary>
    /// 현재 설정된 인증 만료 시간(초)을 반환
    /// </summary>
    public int GetCurrentExpirySeconds()
    {
        if (useDebugShortExpiry && debugExpirySeconds > 0)
        {
            return debugExpirySeconds;
        }
        return Mathf.Max(1, codeExpiryMinutes * 60);
    }
}

/// <summary>
/// 인증번호 데이터 구조
/// </summary>
[System.Serializable]
public class VerificationCodeData
{
    public string email;
    public string code;
    public int attempts;
    public DateTime createdAt;
    public DateTime expiresAt;
    public DateTime lastSentTime;
}
