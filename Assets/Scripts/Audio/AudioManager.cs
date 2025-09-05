using UnityEngine;
using System.Collections.Generic;

namespace AudioSystem
{
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;
        
        [Header("Music")]
        [SerializeField] private AudioClip backgroundMusic;
        [SerializeField] private AudioClip combatMusic;
        
        [Header("SFX - Player")]
        [SerializeField] private AudioClip footstepSound;
        [SerializeField] private AudioClip jumpSound;
        [SerializeField] private AudioClip landSound;
        [SerializeField] private AudioClip runSound;
        
        [Header("SFX - Combat")]
        [SerializeField] private AudioClip attackSound;
        [SerializeField] private AudioClip hitSound;
        [SerializeField] private AudioClip blockSound;
        [SerializeField] private AudioClip weaponEquipSound;
        [SerializeField] private AudioClip weaponUnequipSound;
        
        [Header("SFX - Items")]
        [SerializeField] private AudioClip itemPickupSound;
        [SerializeField] private AudioClip itemUseSound;
        [SerializeField] private AudioClip inventoryOpenSound;
        [SerializeField] private AudioClip inventoryCloseSound;
        
        [Header("SFX - UI")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip buttonHoverSound;
        [SerializeField] private AudioClip notificationSound;
        
        [Header("Settings")]
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float sfxVolume = 1.0f;
        [SerializeField] private float uiVolume = 0.8f;
        
        // Singleton
        public static AudioManager Instance { get; private set; }
        
        // Audio clip dictionary for easy access
        private Dictionary<string, AudioClip> audioClips;
        
        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioManager();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        private void InitializeAudioManager()
        {
            // Create audio sources if not assigned
            if (musicSource == null)
            {
                GameObject musicObj = new GameObject("MusicSource");
                musicObj.transform.SetParent(transform);
                musicSource = musicObj.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.volume = musicVolume;
            }
            
            if (sfxSource == null)
            {
                GameObject sfxObj = new GameObject("SFXSource");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }
            
            if (uiSource == null)
            {
                GameObject uiObj = new GameObject("UISource");
                uiObj.transform.SetParent(transform);
                uiSource = uiObj.AddComponent<AudioSource>();
                uiSource.loop = false;
                uiSource.playOnAwake = false;
                uiSource.volume = uiVolume;
            }
            
            // Initialize audio clips dictionary
            audioClips = new Dictionary<string, AudioClip>
            {
                // Player sounds
                {"footstep", footstepSound},
                {"jump", jumpSound},
                {"land", landSound},
                {"run", runSound},
                
                // Combat sounds
                {"attack", attackSound},
                {"hit", hitSound},
                {"block", blockSound},
                {"weapon_equip", weaponEquipSound},
                {"weapon_unequip", weaponUnequipSound},
                
                // Item sounds
                {"item_pickup", itemPickupSound},
                {"item_use", itemUseSound},
                {"inventory_open", inventoryOpenSound},
                {"inventory_close", inventoryCloseSound},
                
                // UI sounds
                {"button_click", buttonClickSound},
                {"button_hover", buttonHoverSound},
                {"notification", notificationSound}
            };
            
            // Set initial volumes
            SetMusicVolume(musicVolume);
            SetSFXVolume(sfxVolume);
            SetUIVolume(uiVolume);
        }
        
        #region Music Methods
        public void PlayBackgroundMusic()
        {
            if (backgroundMusic != null)
            {
                musicSource.clip = backgroundMusic;
                musicSource.Play();
            }
        }
        
        public void PlayCombatMusic()
        {
            if (combatMusic != null)
            {
                musicSource.clip = combatMusic;
                musicSource.Play();
            }
        }
        
        public void StopMusic()
        {
            musicSource.Stop();
        }
        
        public void PauseMusic()
        {
            musicSource.Pause();
        }
        
        public void ResumeMusic()
        {
            musicSource.UnPause();
        }
        #endregion
        
        #region SFX Methods
        public void PlaySFX(string soundName, float volume = 1.0f)
        {
            Debug.Log($"AudioManager: PlaySFX 호출 - {soundName}, 볼륨: {volume}");
            
            if (sfxSource == null)
            {
                Debug.LogError("AudioManager: sfxSource가 null입니다!");
                return;
            }
            
            if (audioClips.ContainsKey(soundName) && audioClips[soundName] != null)
            {
                Debug.Log($"AudioManager: {soundName} 사운드 재생 중... (볼륨: {volume})");
                sfxSource.PlayOneShot(audioClips[soundName], volume);
                Debug.Log($"AudioManager: sfxSource.isPlaying = {sfxSource.isPlaying}");
            }
            else
            {
                Debug.LogWarning($"AudioManager: Audio clip '{soundName}' not found! (audioClips.Count: {audioClips?.Count ?? 0})");
            }
        }
        
        public void PlaySFX(AudioClip clip, float volume = 1.0f)
        {
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }
        
        public void PlayUISFX(string soundName, float volume = 1.0f)
        {
            if (audioClips.ContainsKey(soundName) && audioClips[soundName] != null)
            {
                uiSource.PlayOneShot(audioClips[soundName], volume);
            }
            else
            {
                Debug.LogWarning($"UI Audio clip '{soundName}' not found!");
            }
        }
        
        public void PlayUISFX(AudioClip clip, float volume = 1.0f)
        {
            if (clip != null)
            {
                uiSource.PlayOneShot(clip, volume);
            }
        }
        #endregion
        
        #region Volume Control
        public void SetMusicVolume(float volume)
        {
            musicVolume = Mathf.Clamp01(volume);
            musicSource.volume = musicVolume;
        }
        
        public void SetSFXVolume(float volume)
        {
            sfxVolume = Mathf.Clamp01(volume);
            sfxSource.volume = sfxVolume;
        }
        
        public void SetUIVolume(float volume)
        {
            uiVolume = Mathf.Clamp01(volume);
            uiSource.volume = uiVolume;
        }
        
        public float GetMusicVolume() => musicVolume;
        public float GetSFXVolume() => sfxVolume;
        public float GetUIVolume() => uiVolume;
        #endregion
        
        #region Utility Methods
        public bool IsPlaying(string soundName)
        {
            return audioClips.ContainsKey(soundName) && 
                   audioClips[soundName] != null && 
                   sfxSource.isPlaying;
        }
        
        public void StopAllSFX()
        {
            sfxSource.Stop();
            uiSource.Stop();
        }
        
        public void StopAllAudio()
        {
            musicSource.Stop();
            sfxSource.Stop();
            uiSource.Stop();
        }
        #endregion
    }
}
