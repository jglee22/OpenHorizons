using UnityEngine;
using AudioSystem;

/// <summary>
/// 씬에 AudioManager를 자동으로 생성하는 스크립트
/// </summary>
public class AudioManagerSpawner : MonoBehaviour
{
    [Header("자동 생성 설정")]
    [SerializeField] private bool autoSpawnOnStart = true;
    [SerializeField] private bool destroyAfterSpawn = true;
    
    private void Start()
    {
        if (autoSpawnOnStart)
        {
            SpawnAudioManager();
        }
    }
    
    [ContextMenu("AudioManager 생성")]
    public void SpawnAudioManager()
    {
        // 이미 AudioManager가 있는지 확인
        if (FindObjectOfType<AudioManager>() != null)
        {
            Debug.Log("AudioManager가 이미 존재합니다!");
            return;
        }
        
        // AudioManager 오브젝트 생성
        GameObject audioManagerObj = new GameObject("AudioManager");
        
        // AudioManager 컴포넌트 추가
        AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();
        
        // AudioManagerSetup 컴포넌트 추가
        AudioManagerSetup setup = audioManagerObj.AddComponent<AudioManagerSetup>();
        
        // AudioTestGenerator 컴포넌트 추가
        AudioTestGenerator testGen = audioManagerObj.AddComponent<AudioTestGenerator>();
        
        // AudioTestController 컴포넌트 추가
        AudioTestController audioTestController = audioManagerObj.AddComponent<AudioTestController>();
        
        // FootstepSoundManager 컴포넌트 추가
        FootstepSoundManager footstepManager = audioManagerObj.AddComponent<FootstepSoundManager>();
        
        // AddressableFootstepManager 컴포넌트 추가
        AddressableFootstepManager addressableFootstepManager = audioManagerObj.AddComponent<AddressableFootstepManager>();
        
        // AddressableTestController 컴포넌트 추가
        AddressableTestController addressableTestController = audioManagerObj.AddComponent<AddressableTestController>();
        
        // 오디오 소스들 생성
        CreateAudioSources(audioManagerObj);
        
        Debug.Log("AudioManager가 성공적으로 생성되었습니다!");
        
        // 이 오브젝트 삭제
        if (destroyAfterSpawn)
        {
            Destroy(gameObject);
        }
    }
    
    private void CreateAudioSources(GameObject parent)
    {
        // Music Source
        GameObject musicObj = new GameObject("MusicSource");
        musicObj.transform.SetParent(parent.transform);
        AudioSource musicSource = musicObj.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = 0.7f;
        
        // SFX Source
        GameObject sfxObj = new GameObject("SFXSource");
        sfxObj.transform.SetParent(parent.transform);
        AudioSource sfxSource = sfxObj.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
        sfxSource.volume = 1.0f;
        
        // UI Source
        GameObject uiObj = new GameObject("UISource");
        uiObj.transform.SetParent(parent.transform);
        AudioSource uiSource = uiObj.AddComponent<AudioSource>();
        uiSource.loop = false;
        uiSource.playOnAwake = false;
        uiSource.volume = 0.8f;
        
        Debug.Log("오디오 소스들이 생성되었습니다!");
    }
}
