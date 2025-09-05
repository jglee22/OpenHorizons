using UnityEngine;
using AudioSystem;

/// <summary>
/// 간단한 사운드 테스트 스크립트
/// </summary>
public class SimpleAudioTest : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private bool autoTestOnStart = true;
    [SerializeField] private float testInterval = 1.0f;
    
    private void Start()
    {
        if (autoTestOnStart)
        {
            InvokeRepeating(nameof(TestAudio), 1.0f, testInterval);
        }
    }
    
    private void TestAudio()
    {
        Debug.Log("=== 사운드 시스템 테스트 시작 ===");
        
        // AudioManager 존재 확인
        if (AudioManager.Instance == null)
        {
            Debug.LogError("❌ AudioManager.Instance가 null입니다!");
            return;
        }
        
        Debug.Log("✅ AudioManager.Instance 존재 확인");
        
        // 볼륨 확인
        float sfxVolume = AudioManager.Instance.GetSFXVolume();
        Debug.Log($"🔊 현재 SFX 볼륨: {sfxVolume}");
        
        if (sfxVolume <= 0)
        {
            Debug.LogWarning("⚠️ SFX 볼륨이 0입니다! +키를 눌러 볼륨을 높이세요.");
            AudioManager.Instance.SetSFXVolume(0.5f);
            Debug.Log("🔧 SFX 볼륨을 0.5로 설정했습니다.");
        }
        
        // 간단한 사운드 재생 테스트
        Debug.Log("🎵 테스트 사운드 재생 시도...");
        AudioManager.Instance.PlaySFX("footstep");
        
        Debug.Log("=== 사운드 시스템 테스트 완료 ===");
    }
    
    [ContextMenu("수동 테스트")]
    public void ManualTest()
    {
        TestAudio();
    }
}
