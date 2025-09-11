using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace AudioSystem
{
    /// <summary>
    /// 어드레서블 시스템 테스트 컨트롤러
    /// </summary>
    public class AddressableTestController : MonoBehaviour
    {
        [Header("테스트 설정")]
        [SerializeField] private bool autoTestOnStart = true;
        [SerializeField] private bool showDebugInfo = true;
        
        [Header("테스트 레이블")]
        [SerializeField] private string testLabel = "footstep_grass_walk";
        [SerializeField] private int maxTestCount = 5;
        
        [Header("키보드 테스트")]
        [SerializeField] private KeyCode testLoadKey = KeyCode.L;
        [SerializeField] private KeyCode testPlayKey = KeyCode.P;
        [SerializeField] private KeyCode testReleaseKey = KeyCode.R;
        
        private List<AudioClip> loadedClips = new List<AudioClip>();
        private bool isLoaded = false;
        
        private void Start()
        {
            if (autoTestOnStart)
            {
                StartCoroutine(AutoTestSequence());
            }
        }
        
        private void Update()
        {
            HandleTestInput();
        }
        
        private void HandleTestInput()
        {
            if (Input.GetKeyDown(testLoadKey))
            {
                TestLoadSounds();
            }
            
            if (Input.GetKeyDown(testPlayKey))
            {
                TestPlaySound();
            }
            
            if (Input.GetKeyDown(testReleaseKey))
            {
                TestReleaseSounds();
            }
        }
        
        private System.Collections.IEnumerator AutoTestSequence()
        {
            Debug.Log("=== 어드레서블 테스트 시작 ===");
            
            yield return new WaitForSeconds(1f);
            
            // 1. 어드레서블 설정 확인
            yield return StartCoroutine(TestAddressableSettings());
            
            yield return new WaitForSeconds(1f);
            
            // 2. 사운드 로드 테스트
            yield return StartCoroutine(TestLoadSoundsCoroutine());
            
            yield return new WaitForSeconds(1f);
            
            // 3. 사운드 재생 테스트
            yield return StartCoroutine(TestPlaySoundsCoroutine());
            
            yield return new WaitForSeconds(1f);
            
            // 4. 메모리 해제 테스트
            yield return StartCoroutine(TestReleaseSoundsCoroutine());
            
            Debug.Log("=== 어드레서블 테스트 완료 ===");
        }
        
        private System.Collections.IEnumerator TestAddressableSettings()
        {
            Debug.Log("1. 어드레서블 설정 확인 중...");
            
#if UNITY_EDITOR
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("❌ 어드레서블 설정을 찾을 수 없습니다!");
                Debug.LogError("Window > Asset Management > Addressables > Groups를 먼저 열어주세요.");
                yield break;
            }
#else
            Debug.LogWarning("⚠️ 어드레서블 설정 확인은 에디터에서만 가능합니다.");
            yield break;
#endif
            
#if UNITY_EDITOR
            Debug.Log("✅ 어드레서블 설정 발견!");
            Debug.Log($"   - 그룹 수: {settings.groups.Count}");
            Debug.Log($"   - 레이블 수: {settings.GetLabels().Count}");
            
            // 발자국 관련 그룹 찾기
            var footstepGroups = new List<string>();
            foreach (var group in settings.groups)
            {
                if (group.name.ToLower().Contains("footstep") || group.name.ToLower().Contains("sound"))
                {
                    footstepGroups.Add(group.name);
                    Debug.Log($"   - 발자국 그룹: {group.name} ({group.entries.Count}개 항목)");
                }
            }
            
            if (footstepGroups.Count == 0)
            {
                Debug.LogWarning("⚠️ 발자국 관련 그룹을 찾을 수 없습니다.");
                Debug.LogWarning("Tools > Audio System > Setup Addressable Footstep Sounds를 실행하세요.");
            }
#endif
            
            yield return null;
        }
        
        private System.Collections.IEnumerator TestLoadSoundsCoroutine()
        {
            Debug.Log("2. 사운드 로드 테스트 중...");
            
            var handle = Addressables.LoadAssetsAsync<AudioClip>(testLabel, null);
            
            while (!handle.IsDone)
            {
                Debug.Log($"   로딩 진행률: {handle.PercentComplete * 100:F1}%");
                yield return new WaitForSeconds(0.1f);
            }
            
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedClips = new List<AudioClip>(handle.Result);
                isLoaded = true;
                Debug.Log($"✅ {testLabel} 로드 성공! ({loadedClips.Count}개 사운드)");
            }
            else
            {
                Debug.LogError($"❌ {testLabel} 로드 실패: {handle.OperationException}");
            }
            
            yield return null;
        }
        
        private System.Collections.IEnumerator TestPlaySoundsCoroutine()
        {
            Debug.Log("3. 사운드 재생 테스트 중...");
            
            if (!isLoaded || loadedClips.Count == 0)
            {
                Debug.LogWarning("⚠️ 로드된 사운드가 없습니다.");
                yield break;
            }
            
            for (int i = 0; i < Mathf.Min(maxTestCount, loadedClips.Count); i++)
            {
                var clip = loadedClips[i];
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(clip);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(clip, transform.position);
                }
                
                yield return new WaitForSeconds(clip.length + 0.1f);
            }
            
            Debug.Log("✅ 사운드 재생 테스트 완료!");
            yield return null;
        }
        
        private System.Collections.IEnumerator TestReleaseSoundsCoroutine()
        {
            Debug.Log("4. 메모리 해제 테스트 중...");
            
            if (loadedClips.Count > 0)
            {
                Debug.Log($"   해제할 사운드: {loadedClips.Count}개");
                loadedClips.Clear();
                isLoaded = false;
                Debug.Log("✅ 사운드 메모리 해제 완료!");
            }
            else
            {
                Debug.Log("⚠️ 해제할 사운드가 없습니다.");
            }
            
            yield return null;
        }
        
        [ContextMenu("수동 로드 테스트")]
        public async void TestLoadSounds()
        {
            Debug.Log($"수동 로드 테스트: {testLabel}");
            
            try
            {
                var handle = Addressables.LoadAssetsAsync<AudioClip>(testLabel, null);
                var result = await handle.Task;
                
                if (result != null && result.Count > 0)
                {
                    loadedClips = new List<AudioClip>(result);
                    isLoaded = true;
                    Debug.Log($"✅ {testLabel} 로드 성공! ({loadedClips.Count}개 사운드)");
                }
                else
                {
                    Debug.LogWarning($"⚠️ {testLabel}에서 사운드를 찾을 수 없습니다.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ 로드 실패: {e.Message}");
            }
        }
        
        [ContextMenu("사운드 재생 테스트")]
        public void TestPlaySound()
        {
            if (!isLoaded || loadedClips.Count == 0)
            {
                Debug.LogWarning("⚠️ 로드된 사운드가 없습니다. 먼저 로드하세요.");
                return;
            }
            
            var randomClip = loadedClips[Random.Range(0, loadedClips.Count)];
            Debug.Log($"재생 중: {randomClip.name}");
            
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(randomClip);
            }
            else
            {
                AudioSource.PlayClipAtPoint(randomClip, transform.position);
            }
        }
        
        [ContextMenu("메모리 해제 테스트")]
        public void TestReleaseSounds()
        {
            if (loadedClips.Count > 0)
            {
                Debug.Log($"메모리 해제: {loadedClips.Count}개 사운드");
                loadedClips.Clear();
                isLoaded = false;
                Debug.Log("✅ 메모리 해제 완료!");
            }
            else
            {
                Debug.Log("⚠️ 해제할 사운드가 없습니다.");
            }
        }
        
        // private void OnGUI()
        // {
        //     if (!showDebugInfo) return;
            
        //     GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        //     GUILayout.Label("=== 어드레서블 테스트 ===");
        //     GUILayout.Label($"L키: 사운드 로드 테스트");
        //     GUILayout.Label($"P키: 사운드 재생 테스트");
        //     GUILayout.Label($"R키: 메모리 해제 테스트");
        //     GUILayout.Label("");
        //     GUILayout.Label($"현재 상태: {(isLoaded ? "로드됨" : "로드 안됨")}");
        //     GUILayout.Label($"로드된 사운드: {loadedClips.Count}개");
        //     GUILayout.Label($"테스트 레이블: {testLabel}");
        //     GUILayout.EndArea();
        // }
    }
}
