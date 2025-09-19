using UnityEngine;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

/// <summary>
/// 이메일 전송 서비스
/// </summary>
public class EmailService : MonoBehaviour
{
    [Header("이메일 설정")]
    public string senderEmail = "your-email@gmail.com";
    public string senderPassword = "your-app-password";
    public string smtpServer = "smtp.gmail.com";
    public int smtpPort = 587;
    public bool useSimulation = false;
    
    [Header("디버그")]
    public bool enableDebugLogs = true;
    
    /// <summary>
    /// 인증번호 이메일 전송
    /// </summary>
    public async Task<bool> SendVerificationEmail(string recipientEmail, string verificationCode)
    {
        if (useSimulation)
        {
            return await SendSimulationEmail(recipientEmail, verificationCode);
        }
        else
        {
            return await SendRealEmail(recipientEmail, verificationCode);
        }
    }
    
    /// <summary>
    /// 시뮬레이션 이메일 전송 (테스트용)
    /// </summary>
    private async Task<bool> SendSimulationEmail(string recipientEmail, string verificationCode)
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log($"[EmailService] 시뮬레이션 이메일 전송: {recipientEmail}");
            
            // 1초 대기 (실제 전송 시뮬레이션)
            await System.Threading.Tasks.Task.Delay(1000);
            
            if (enableDebugLogs)
            {
                Debug.Log("=".PadRight(50, '='));
                Debug.Log($"📧 이메일 전송 시뮬레이션");
                Debug.Log($"받는 사람: {recipientEmail}");
                Debug.Log($"인증번호: {verificationCode}");
                Debug.Log("=".PadRight(50, '='));
            }
            
            return true;
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"[EmailService] 시뮬레이션 이메일 전송 오류: {e}");
            return false;
        }
    }
    
    /// <summary>
    /// 실제 이메일 전송
    /// </summary>
    private async Task<bool> SendRealEmail(string recipientEmail, string verificationCode)
    {
        try
        {
            if (enableDebugLogs)
                Debug.Log($"[EmailService] 실제 이메일 전송 시작: {recipientEmail}");
            
            // SMTP 클라이언트 설정
            using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
            {
                smtpClient.EnableSsl = true;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                
                // 이메일 메시지 생성
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(senderEmail, "OpenHorizons");
                mailMessage.To.Add(recipientEmail);
                mailMessage.Subject = "[OpenHorizons] 이메일 인증번호";
                
                // HTML 이메일 본문
                string htmlBody = CreateEmailTemplate(verificationCode);
                mailMessage.Body = htmlBody;
                mailMessage.IsBodyHtml = true;
                
                // 이메일 전송
                await System.Threading.Tasks.Task.Run(() => smtpClient.Send(mailMessage));
                
                if (enableDebugLogs)
                    Debug.Log($"[EmailService] 이메일 전송 완료: {recipientEmail}");
                
                return true;
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"[EmailService] 이메일 전송 오류: {e}");
            return false;
        }
    }
    
    /// <summary>
    /// 이메일 HTML 템플릿 생성
    /// </summary>
    private string CreateEmailTemplate(string verificationCode)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 20px; }}
        .container {{ max-width: 600px; margin: 0 auto; background-color: white; border-radius: 10px; overflow: hidden; box-shadow: 0 4px 6px rgba(0,0,0,0.1); }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 40px 30px; }}
        .verification-code {{ background-color: #f8f9fa; border: 2px dashed #dee2e6; border-radius: 8px; padding: 20px; text-align: center; margin: 20px 0; }}
        .code {{ font-size: 32px; font-weight: bold; color: #495057; letter-spacing: 5px; font-family: 'Courier New', monospace; }}
        .footer {{ background-color: #f8f9fa; padding: 20px; text-align: center; color: #6c757d; font-size: 14px; }}
        .warning {{ background-color: #fff3cd; border: 1px solid #ffeaa7; border-radius: 5px; padding: 15px; margin: 20px 0; color: #856404; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎮 OpenHorizons</h1>
            <p>이메일 인증번호</p>
        </div>
        <div class='content'>
            <h2>안녕하세요!</h2>
            <p>OpenHorizons 회원가입을 위한 인증번호를 발송해드립니다.</p>
            
            <div class='verification-code'>
                <p style='margin: 0 0 10px 0; color: #6c757d;'>인증번호</p>
                <div class='code'>{verificationCode}</div>
            </div>
            
            <div class='warning'>
                <strong>⚠️ 주의사항:</strong>
                <ul style='margin: 10px 0 0 0; padding-left: 20px;'>
                    <li>이 인증번호는 5분 후 만료됩니다.</li>
                    <li>타인에게 인증번호를 공유하지 마세요.</li>
                    <li>본인이 요청하지 않은 인증번호라면 무시하세요.</li>
                </ul>
            </div>
            
            <p>인증번호를 입력하여 회원가입을 완료해주세요.</p>
        </div>
        <div class='footer'>
            <p>© 2024 OpenHorizons. All rights reserved.</p>
            <p>이 이메일은 자동으로 발송되었습니다.</p>
        </div>
    </div>
</body>
</html>";
    }
    
    /// <summary>
    /// 이메일 설정 검증
    /// </summary>
    [ContextMenu("이메일 설정 검증")]
    public void ValidateEmailSettings()
    {
        if (string.IsNullOrEmpty(senderEmail) || senderEmail == "your-email@gmail.com")
        {
            Debug.LogError("❌ 발신자 이메일을 설정해주세요!");
            return;
        }
        
        if (string.IsNullOrEmpty(senderPassword) || senderPassword == "your-app-password")
        {
            Debug.LogError("❌ 앱 비밀번호를 설정해주세요!");
            return;
        }
        
        if (useSimulation)
        {
            Debug.Log("✅ 시뮬레이션 모드로 설정되어 있습니다.");
        }
        else
        {
            Debug.Log("✅ 실제 이메일 전송 모드로 설정되어 있습니다.");
        }
        
        Debug.Log($"📧 발신자: {senderEmail}");
        Debug.Log($"🔒 SMTP 서버: {smtpServer}:{smtpPort}");
    }
}