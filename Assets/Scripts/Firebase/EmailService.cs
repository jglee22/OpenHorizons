using UnityEngine;
using System;
using System.Collections;
using System.Net;
using System.Net.Mail;
using System.Text;

/// <summary>
/// 이메일 전송 서비스 (SMTP)
/// </summary>
public class EmailService : MonoBehaviour
{
    [Header("SMTP 설정")]
    public string smtpServer = "smtp.gmail.com";
    public int smtpPort = 587;
    public string senderEmail = "your-email@gmail.com";
    public string senderPassword = "your-app-password";
    public bool enableSSL = true;
    
    [Header("디버그 설정")]
    public bool enableDebugLogs = true;
    public bool useSimulation = true; // 실제 전송 대신 시뮬레이션 사용
    
    /// <summary>
    /// 인증번호 이메일 전송
    /// </summary>
    public async System.Threading.Tasks.Task<bool> SendVerificationEmail(string toEmail, string verificationCode, int expirySeconds)
    {
        if (useSimulation)
        {
            return await SendSimulationEmail(toEmail, verificationCode, expirySeconds);
        }
        else
        {
            return await SendRealEmail(toEmail, verificationCode, expirySeconds);
        }
    }
    
    /// <summary>
    /// 시뮬레이션 이메일 전송
    /// </summary>
    private async System.Threading.Tasks.Task<bool> SendSimulationEmail(string toEmail, string verificationCode, int expirySeconds)
    {
        try
        {
            if (enableDebugLogs)
            {
                Debug.Log("=== 이메일 전송 시뮬레이션 ===");
                Debug.Log($"받는 사람: {toEmail}");
                Debug.Log($"제목: [OpenHorizons] 이메일 인증번호");
                Debug.Log($"내용: 인증번호: {verificationCode}");
                if (expirySeconds < 60) Debug.Log($"만료시간: {expirySeconds}초 후");
                else Debug.Log($"만료시간: {expirySeconds/60}분 후");
                Debug.Log($"===============================");
            }
            
            // 실제 이메일 전송을 위한 지연 시뮬레이션
            await System.Threading.Tasks.Task.Delay(1000);
            
            return true;
        }
        catch (Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"시뮬레이션 이메일 전송 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 실제 이메일 전송 (SMTP)
    /// </summary>
    private async System.Threading.Tasks.Task<bool> SendRealEmail(string toEmail, string verificationCode, int expirySeconds)
    {
        try
        {
            // SMTP 클라이언트 설정
            using (var client = new SmtpClient(smtpServer, smtpPort))
            {
                client.EnableSsl = enableSSL;
                client.Credentials = new NetworkCredential(senderEmail, senderPassword);
                
                // 이메일 메시지 생성
                var message = new MailMessage(senderEmail, toEmail)
                {
                    Subject = "[OpenHorizons] 이메일 인증번호",
                    Body = CreateEmailBody(verificationCode, expirySeconds),
                    IsBodyHtml = true
                };
                
                // 이메일 전송
                await client.SendMailAsync(message);
                
                if (enableDebugLogs)
                    Debug.Log($"인증번호 이메일이 {toEmail}로 전송되었습니다.");
                
                return true;
            }
        }
        catch (Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"실제 이메일 전송 실패: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 이메일 본문 생성
    /// </summary>
    private string CreateEmailBody(string verificationCode, int expirySeconds)
    {
        var body = new StringBuilder();
        body.AppendLine("<html>");
        body.AppendLine("<body style='font-family: Arial, sans-serif;'>");
        body.AppendLine("<div style='max-width: 600px; margin: 0 auto; padding: 20px;'>");
        body.AppendLine("<h2 style='color: #333;'>OpenHorizons 이메일 인증</h2>");
        body.AppendLine("<p>안녕하세요! OpenHorizons 회원가입을 위한 인증번호입니다.</p>");
        body.AppendLine("<div style='background-color: #f5f5f5; padding: 20px; text-align: center; margin: 20px 0;'>");
        body.AppendLine($"<h1 style='color: #007bff; font-size: 32px; margin: 0;'>{verificationCode}</h1>");
        body.AppendLine("</div>");
        if (expirySeconds < 60)
            body.AppendLine($"<p>이 인증번호는 <strong>{expirySeconds}초</strong> 후에 만료됩니다.</p>");
        else
            body.AppendLine($"<p>이 인증번호는 <strong>{expirySeconds/60}분</strong> 후에 만료됩니다.</p>");
        body.AppendLine("<p>인증번호를 입력하여 회원가입을 완료해주세요.</p>");
        body.AppendLine("<hr style='margin: 30px 0;'>");
        body.AppendLine("<p style='color: #666; font-size: 12px;'>이 이메일은 자동으로 발송되었습니다. 회신하지 마세요.</p>");
        body.AppendLine("</div>");
        body.AppendLine("</body>");
        body.AppendLine("</html>");
        
        return body.ToString();
    }
}
