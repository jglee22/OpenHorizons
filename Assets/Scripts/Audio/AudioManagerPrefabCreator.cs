using UnityEngine;
using UnityEditor;

namespace AudioSystem
{
    /// <summary>
    /// AudioManager 프리팹을 올바르게 생성하는 에디터 스크립트
    /// </summary>
    public class AudioManagerPrefabCreator : EditorWindow
    {
        [MenuItem("Tools/Audio System/Create AudioManager Prefab")]
        public static void CreateAudioManagerPrefab()
        {
            // AudioManager 오브젝트 생성
            GameObject audioManagerObj = new GameObject("AudioManager");
            
            // AudioManager 컴포넌트 추가
            AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();
            
            // AudioManagerSetup 컴포넌트 추가
            AudioManagerSetup setup = audioManagerObj.AddComponent<AudioManagerSetup>();
            
            // AudioTestGenerator 컴포넌트 추가
            AudioTestGenerator testGen = audioManagerObj.AddComponent<AudioTestGenerator>();
            
            // 오디오 소스들 생성
            CreateAudioSources(audioManagerObj);
            
            // 프리팹으로 저장
            string prefabPath = "Assets/Audio/AudioManager.prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(audioManagerObj, prefabPath);
            
            if (prefab != null)
            {
                Debug.Log($"AudioManager 프리팹이 성공적으로 생성되었습니다: {prefabPath}");
                
                // 씬에 인스턴스 생성
                GameObject instance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                Selection.activeGameObject = instance;
                
                Debug.Log("AudioManager가 씬에 추가되었습니다!");
            }
            else
            {
                Debug.LogError("AudioManager 프리팹 생성에 실패했습니다!");
            }
            
            // 임시 오브젝트 삭제
            DestroyImmediate(audioManagerObj);
        }
        
        private static void CreateAudioSources(GameObject parent)
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
        }
    }
}
