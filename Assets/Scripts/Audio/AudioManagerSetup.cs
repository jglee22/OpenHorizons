using UnityEngine;

namespace AudioSystem
{
    /// <summary>
    /// AudioManager를 자동으로 설정하는 헬퍼 스크립트
    /// </summary>
    public class AudioManagerSetup : MonoBehaviour
    {
        [Header("AudioManager 설정")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private bool createAudioSources = true;
        
        private void Awake()
        {
            if (autoSetup)
            {
                SetupAudioManager();
            }
        }
        
        private void SetupAudioManager()
        {
            AudioManager audioManager = GetComponent<AudioManager>();
            if (audioManager == null)
            {
                audioManager = gameObject.AddComponent<AudioManager>();
            }
            
            // if (createAudioSources)
            // {
            //     CreateAudioSources(audioManager);
            // }
        }
        
        private void CreateAudioSources(AudioManager audioManager)
        {
            // Music Source 생성
            GameObject musicObj = new GameObject("MusicSource");
            musicObj.transform.SetParent(transform);
            AudioSource musicSource = musicObj.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = false;
            musicSource.volume = 0.7f;
            
            // SFX Source 생성
            GameObject sfxObj = new GameObject("SFXSource");
            sfxObj.transform.SetParent(transform);
            AudioSource sfxSource = sfxObj.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.playOnAwake = false;
            sfxSource.volume = 1.0f;
            
            // UI Source 생성
            GameObject uiObj = new GameObject("UISource");
            uiObj.transform.SetParent(transform);
            AudioSource uiSource = uiObj.AddComponent<AudioSource>();
            uiSource.loop = false;
            uiSource.playOnAwake = false;
            uiSource.volume = 0.8f;
            
            // AudioManager에 연결 (리플렉션 사용)
            var audioManagerType = typeof(AudioManager);
            var musicSourceField = audioManagerType.GetField("musicSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sfxSourceField = audioManagerType.GetField("sfxSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var uiSourceField = audioManagerType.GetField("uiSource", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (musicSourceField != null) musicSourceField.SetValue(audioManager, musicSource);
            if (sfxSourceField != null) sfxSourceField.SetValue(audioManager, sfxSource);
            if (uiSourceField != null) uiSourceField.SetValue(audioManager, uiSource);
            
            Debug.Log("AudioManager 오디오 소스들이 자동으로 생성되었습니다!");
        }
    }
}
