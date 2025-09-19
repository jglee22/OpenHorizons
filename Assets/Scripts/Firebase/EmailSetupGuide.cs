using UnityEngine;

/// <summary>
/// 이메일 설정 가이드
/// </summary>
public class EmailSetupGuide : MonoBehaviour
{
    [ContextMenu("Gmail 설정 가이드")]
    public void ShowGmailSetupGuide()
    {
        Debug.Log("=".PadRight(60, '='));
        Debug.Log("📧 Gmail 이메일 전송 설정 가이드");
        Debug.Log("=".PadRight(60, '='));
        Debug.Log("");
        Debug.Log("1️⃣ Gmail 2단계 인증 활성화:");
        Debug.Log("   - Gmail 계정 설정 → 보안 → 2단계 인증 켜기");
        Debug.Log("");
        Debug.Log("2️⃣ 앱 비밀번호 생성:");
        Debug.Log("   - Google 계정 → 보안 → 2단계 인증 → 앱 비밀번호");
        Debug.Log("   - 앱 선택: '메일'");
        Debug.Log("   - 기기 선택: '기타(사용자 지정 이름)'");
        Debug.Log("   - 생성된 16자리 비밀번호 복사");
        Debug.Log("");
        Debug.Log("3️⃣ EmailService 설정:");
        Debug.Log("   - senderEmail: 본인 Gmail 주소");
        Debug.Log("   - senderPassword: 생성된 앱 비밀번호 (16자리)");
        Debug.Log("   - useSimulation: false로 설정");
        Debug.Log("");
        Debug.Log("4️⃣ 테스트:");
        Debug.Log("   - Unity에서 회원가입 테스트");
        Debug.Log("   - 실제 이메일로 인증번호 수신 확인");
        Debug.Log("");
        Debug.Log("⚠️ 주의사항:");
        Debug.Log("   - 일반 Gmail 비밀번호가 아닌 앱 비밀번호 사용");
        Debug.Log("   - 앱 비밀번호는 16자리 (공백 없이)");
        Debug.Log("   - 2단계 인증이 활성화되어야 함");
        Debug.Log("=".PadRight(60, '='));
    }
    
    [ContextMenu("이메일 설정 테스트")]
    public void TestEmailSettings()
    {
        EmailService emailService = FindObjectOfType<EmailService>();
        if (emailService != null)
        {
            emailService.ValidateEmailSettings();
        }
        else
        {
            Debug.LogError("EmailService를 찾을 수 없습니다!");
        }
    }
}
