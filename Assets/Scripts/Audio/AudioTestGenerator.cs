using UnityEngine;
using System.Collections;

namespace AudioSystem
{
    /// <summary>
    /// 테스트용 사운드를 생성하는 스크립트
    /// 실제 사운드 파일이 없을 때 사용
    /// </summary>
    public class AudioTestGenerator : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool generateTestSounds = true;
        [SerializeField] private float testSoundDuration = 0.5f;
        [SerializeField] private float testSoundFrequency = 440f; // A4 음
        
        private void Start()
        {
            if (generateTestSounds)
            {
                GenerateTestSounds();
            }
        }
        
        private void GenerateTestSounds()
        {
            AudioManager audioManager = FindObjectOfType<AudioManager>();
            if (audioManager == null)
            {
                Debug.LogWarning("AudioManager를 찾을 수 없습니다!");
                return;
            }
            
            Debug.Log("AudioTestGenerator: 테스트 사운드 생성을 시작합니다...");
            
            // 테스트용 오디오 클립 생성
            AudioClip footstepClip = CreateTestClip("Footstep", 0.1f, 200f);
            AudioClip jumpClip = CreateTestClip("Jump", 0.3f, 600f);
            AudioClip landClip = CreateTestClip("Land", 0.2f, 300f);
            AudioClip runClip = CreateTestClip("Run", 0.1f, 250f);
            AudioClip attackClip = CreateTestClip("Attack", 0.4f, 800f);
            AudioClip hitClip = CreateTestClip("Hit", 0.3f, 400f);
            AudioClip blockClip = CreateTestClip("Block", 0.2f, 500f);
            AudioClip weaponEquipClip = CreateTestClip("WeaponEquip", 0.5f, 700f);
            AudioClip weaponUnequipClip = CreateTestClip("WeaponUnequip", 0.3f, 300f);
            AudioClip itemPickupClip = CreateTestClip("ItemPickup", 0.2f, 600f);
            AudioClip itemUseClip = CreateTestClip("ItemUse", 0.3f, 500f);
            AudioClip inventoryOpenClip = CreateTestClip("InventoryOpen", 0.2f, 400f);
            AudioClip inventoryCloseClip = CreateTestClip("InventoryClose", 0.2f, 350f);
            AudioClip buttonClickClip = CreateTestClip("ButtonClick", 0.1f, 800f);
            AudioClip buttonHoverClip = CreateTestClip("ButtonHover", 0.1f, 600f);
            AudioClip notificationClip = CreateTestClip("Notification", 0.5f, 500f);
            
            // AudioManager에 할당 (리플렉션 사용)
            AssignAudioClip(audioManager, "footstepSound", footstepClip);
            AssignAudioClip(audioManager, "jumpSound", jumpClip);
            AssignAudioClip(audioManager, "landSound", landClip);
            AssignAudioClip(audioManager, "runSound", runClip);
            AssignAudioClip(audioManager, "attackSound", attackClip);
            AssignAudioClip(audioManager, "hitSound", hitClip);
            AssignAudioClip(audioManager, "blockSound", blockClip);
            AssignAudioClip(audioManager, "weaponEquipSound", weaponEquipClip);
            AssignAudioClip(audioManager, "weaponUnequipSound", weaponUnequipClip);
            AssignAudioClip(audioManager, "itemPickupSound", itemPickupClip);
            AssignAudioClip(audioManager, "itemUseSound", itemUseClip);
            AssignAudioClip(audioManager, "inventoryOpenSound", inventoryOpenClip);
            AssignAudioClip(audioManager, "inventoryCloseSound", inventoryCloseClip);
            AssignAudioClip(audioManager, "buttonClickSound", buttonClickClip);
            AssignAudioClip(audioManager, "buttonHoverSound", buttonHoverClip);
            AssignAudioClip(audioManager, "notificationSound", notificationClip);
            
            Debug.Log("AudioTestGenerator: 테스트용 사운드들이 생성되었습니다!");
            
            // 사운드 재생 테스트
            StartCoroutine(TestSoundPlayback());
        }
        
        private AudioClip CreateTestClip(string name, float duration, float frequency)
        {
            int sampleRate = 44100;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] samplesArray = new float[samples];
            
            for (int i = 0; i < samples; i++)
            {
                float time = (float)i / sampleRate;
                // 사인파 + 감쇠로 자연스러운 소리 생성
                float amplitude = Mathf.Exp(-time * 3f); // 감쇠
                samplesArray[i] = amplitude * Mathf.Sin(2f * Mathf.PI * frequency * time);
            }
            
            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(samplesArray, 0);
            
            return clip;
        }
        
        private void AssignAudioClip(AudioManager audioManager, string fieldName, AudioClip clip)
        {
            var field = typeof(AudioManager).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(audioManager, clip);
                Debug.Log($"AudioTestGenerator: {fieldName}에 테스트 사운드 할당 완료");
            }
            else
            {
                Debug.LogWarning($"AudioManager에서 '{fieldName}' 필드를 찾을 수 없습니다!");
            }
        }
        
        private IEnumerator TestSoundPlayback()
        {
            yield return new WaitForSeconds(0.5f);
            
            Debug.Log("AudioTestGenerator: 사운드 재생 테스트를 시작합니다...");
            
            // 간단한 사운드 재생 테스트
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX("footstep");
                Debug.Log("AudioTestGenerator: 발걸음 사운드 재생 테스트");
                
                yield return new WaitForSeconds(0.2f);
                
                AudioManager.Instance.PlaySFX("jump");
                Debug.Log("AudioTestGenerator: 점프 사운드 재생 테스트");
            }
            else
            {
                Debug.LogError("AudioTestGenerator: AudioManager.Instance가 null입니다!");
            }
        }
    }
}
