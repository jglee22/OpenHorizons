using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AudioSystem
{
    /// <summary>
    /// 어드레서블 시스템을 사용한 발자국 사운드 매니저
    /// </summary>
    public class AddressableFootstepManager : MonoBehaviour
    {
        [Header("어드레서블 설정")]
        [SerializeField] private bool loadOnStart = true;
        [SerializeField] private bool enableCaching = true;
        
        [Header("현재 지형")]
        [SerializeField] private GroundType currentGroundType = GroundType.Grass;
        
        [Header("사운드 설정")]
        [SerializeField] private bool enableRandomVariation = true;
        [SerializeField] private float pitchVariation = 0.1f;
        [SerializeField] private float volumeVariation = 0.1f;
        
        [Header("어드레서블 레이블")]
        [SerializeField] private string grassWalkLabel = "footstep_grass_walk";
        [SerializeField] private string grassRunLabel = "footstep_grass_run";
        [SerializeField] private string grassJumpLabel = "footstep_grass_jump";
        
        [SerializeField] private string dirtWalkLabel = "footstep_dirt_walk";
        [SerializeField] private string dirtRunLabel = "footstep_dirt_run";
        [SerializeField] private string dirtLandLabel = "footstep_dirt_land";
        
        [SerializeField] private string gravelWalkLabel = "footstep_gravel_walk";
        [SerializeField] private string gravelRunLabel = "footstep_gravel_run";
        [SerializeField] private string gravelJumpLabel = "footstep_gravel_jump";
        
        [SerializeField] private string metalWalkLabel = "footstep_metal_walk";
        [SerializeField] private string metalRunLabel = "footstep_metal_run";
        [SerializeField] private string metalJumpLabel = "footstep_metal_jump";
        
        [SerializeField] private string woodWalkLabel = "footstep_wood_walk";
        [SerializeField] private string woodRunLabel = "footstep_wood_run";
        [SerializeField] private string woodJumpLabel = "footstep_wood_jump";
        
        [SerializeField] private string stoneWalkLabel = "footstep_stone_walk";
        [SerializeField] private string stoneRunLabel = "footstep_stone_run";
        [SerializeField] private string stoneJumpLabel = "footstep_stone_jump";
        
        [SerializeField] private string sandWalkLabel = "footstep_sand_walk";
        [SerializeField] private string sandRunLabel = "footstep_sand_run";
        [SerializeField] private string sandJumpLabel = "footstep_sand_jump";
        
        [SerializeField] private string snowWalkLabel = "footstep_snow_walk";
        [SerializeField] private string snowRunLabel = "footstep_snow_run";
        [SerializeField] private string snowJumpLabel = "footstep_snow_jump";
        
        [SerializeField] private string waterWalkLabel = "footstep_water_walk";
        [SerializeField] private string waterRunLabel = "footstep_water_run";
        [SerializeField] private string waterJumpLabel = "footstep_water_jump";
        
        [SerializeField] private string tileWalkLabel = "footstep_tile_walk";
        [SerializeField] private string tileRunLabel = "footstep_tile_run";
        [SerializeField] private string tileJumpLabel = "footstep_tile_jump";
        
        [SerializeField] private string mudWalkLabel = "footstep_mud_walk";
        [SerializeField] private string mudRunLabel = "footstep_mud_run";
        [SerializeField] private string mudJumpLabel = "footstep_mud_jump";
        
        [SerializeField] private string leavesWalkLabel = "footstep_leaves_walk";
        [SerializeField] private string leavesRunLabel = "footstep_leaves_run";
        [SerializeField] private string leavesJumpLabel = "footstep_leaves_jump";
        
        public enum GroundType
        {
            Grass,
            Dirt,
            Gravel,
            Metal,
            Wood,
            Stone,
            Sand,
            Snow,
            Water,
            Tile,
            Mud,
            Leaves
        }
        
        // 캐시된 사운드 클립들
        private Dictionary<string, List<AudioClip>> soundCache = new Dictionary<string, List<AudioClip>>();
        
        // 로딩 상태
        private bool isInitialized = false;
        private int loadingCount = 0;
        private int totalLoadingCount = 0;
        
        private void Start()
        {
            if (loadOnStart)
            {
                InitializeFootstepSounds();
            }
        }
        
        /// <summary>
        /// 발자국 사운드 초기화
        /// </summary>
        public async void InitializeFootstepSounds()
        {
            Debug.Log("AddressableFootstepManager: 발자국 사운드 초기화 시작...");
            
            isInitialized = false;
            loadingCount = 0;
            totalLoadingCount = 0;
            
            // 모든 레이블 수집
            List<string> allLabels = GetAllLabels();
            totalLoadingCount = allLabels.Count;
            
            // 병렬로 모든 사운드 로드
            List<Task> loadTasks = new List<Task>();
            
            foreach (string label in allLabels)
            {
                loadTasks.Add(LoadSoundsByLabel(label));
            }
            
            // 모든 로딩 완료 대기
            await Task.WhenAll(loadTasks);
            
            isInitialized = true;
            Debug.Log($"AddressableFootstepManager: 발자국 사운드 초기화 완료! ({loadingCount}/{totalLoadingCount})");
        }
        
        private List<string> GetAllLabels()
        {
            return new List<string>
            {
                grassWalkLabel, grassRunLabel, grassJumpLabel,
                dirtWalkLabel, dirtRunLabel, dirtLandLabel,
                gravelWalkLabel, gravelRunLabel, gravelJumpLabel,
                metalWalkLabel, metalRunLabel, metalJumpLabel,
                woodWalkLabel, woodRunLabel, woodJumpLabel,
                stoneWalkLabel, stoneRunLabel, stoneJumpLabel,
                sandWalkLabel, sandRunLabel, sandJumpLabel,
                snowWalkLabel, snowRunLabel, snowJumpLabel,
                waterWalkLabel, waterRunLabel, waterJumpLabel,
                tileWalkLabel, tileRunLabel, tileJumpLabel,
                mudWalkLabel, mudRunLabel, mudJumpLabel,
                leavesWalkLabel, leavesRunLabel, leavesJumpLabel
            };
        }
        
        private async Task LoadSoundsByLabel(string label)
        {
            try
            {
                var handle = Addressables.LoadAssetsAsync<AudioClip>(label, null);
                var result = await handle.Task;
                
                if (result != null && result.Count > 0)
                {
                    soundCache[label] = new List<AudioClip>(result);
                    loadingCount++;
                    // Debug.Log($"AddressableFootstepManager: {label} - {result.Count}개 사운드 로드 완료");
                }
                else
                {
                    Debug.LogWarning($"AddressableFootstepManager: {label}에서 사운드를 찾을 수 없습니다.");
                }
                
                // 메모리 해제 (선택사항)
                if (!enableCaching)
                {
                    Addressables.Release(handle);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"AddressableFootstepManager: {label} 로드 실패 - {e.Message}");
            }
        }
        
        /// <summary>
        /// 발자국 사운드 재생
        /// </summary>
        public void PlayFootstepSound(bool isRunning = false)
        {
            if (!isInitialized)
            {
                Debug.LogWarning("AddressableFootstepManager: 아직 초기화되지 않았습니다!");
                return;
            }
            
            string label = GetFootstepLabel(isRunning);
            PlayRandomSound(label);
        }
        
        /// <summary>
        /// 점프 사운드 재생
        /// </summary>
        public void PlayJumpSound()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("AddressableFootstepManager: 아직 초기화되지 않았습니다!");
                return;
            }
            
            string label = GetJumpLabel();
            PlayRandomSound(label);
        }
        
        /// <summary>
        /// 착지 사운드 재생
        /// </summary>
        public void PlayLandSound()
        {
            if (!isInitialized)
            {
                Debug.LogWarning("AddressableFootstepManager: 아직 초기화되지 않았습니다!");
                return;
            }
            
            string label = GetLandLabel();
            PlayRandomSound(label);
        }
        
        private string GetFootstepLabel(bool isRunning)
        {
            switch (currentGroundType)
            {
                case GroundType.Grass: return isRunning ? grassRunLabel : grassWalkLabel;
                case GroundType.Dirt: return isRunning ? dirtRunLabel : dirtWalkLabel;
                case GroundType.Gravel: return isRunning ? gravelRunLabel : gravelWalkLabel;
                case GroundType.Metal: return isRunning ? metalRunLabel : metalWalkLabel;
                case GroundType.Wood: return isRunning ? woodRunLabel : woodWalkLabel;
                case GroundType.Stone: return isRunning ? stoneRunLabel : stoneWalkLabel;
                case GroundType.Sand: return isRunning ? sandRunLabel : sandWalkLabel;
                case GroundType.Snow: return isRunning ? snowRunLabel : snowWalkLabel;
                case GroundType.Water: return isRunning ? waterRunLabel : waterWalkLabel;
                case GroundType.Tile: return isRunning ? tileRunLabel : tileWalkLabel;
                case GroundType.Mud: return isRunning ? mudRunLabel : mudWalkLabel;
                case GroundType.Leaves: return isRunning ? leavesRunLabel : leavesWalkLabel;
                default: return grassWalkLabel;
            }
        }
        
        private string GetJumpLabel()
        {
            switch (currentGroundType)
            {
                case GroundType.Grass: return grassJumpLabel;
                case GroundType.Dirt: return dirtLandLabel; // Dirt는 Land 사운드 사용
                case GroundType.Gravel: return gravelJumpLabel;
                case GroundType.Metal: return metalJumpLabel;
                case GroundType.Wood: return woodJumpLabel;
                case GroundType.Stone: return stoneJumpLabel;
                case GroundType.Sand: return sandJumpLabel;
                case GroundType.Snow: return snowJumpLabel;
                case GroundType.Water: return waterJumpLabel;
                case GroundType.Tile: return tileJumpLabel;
                case GroundType.Mud: return mudJumpLabel;
                case GroundType.Leaves: return leavesJumpLabel;
                default: return grassJumpLabel;
            }
        }
        
        private string GetLandLabel()
        {
            switch (currentGroundType)
            {
                case GroundType.Dirt: return dirtLandLabel;
                default: return GetJumpLabel(); // 다른 지형은 Jump 사운드 사용
            }
        }
        
        private void PlayRandomSound(string label)
        {
            if (soundCache.ContainsKey(label) && soundCache[label].Count > 0)
            {
                List<AudioClip> sounds = soundCache[label];
                AudioClip selectedSound = sounds[Random.Range(0, sounds.Count)];
                
                float volume = 1.0f;
                float pitch = 1.0f;
                
                if (enableRandomVariation)
                {
                    volume = 1.0f + Random.Range(-volumeVariation, volumeVariation);
                    pitch = 1.0f + Random.Range(-pitchVariation, pitchVariation);
                }
                
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(selectedSound, volume);
                }
                else
                {
                    AudioSource.PlayClipAtPoint(selectedSound, transform.position, volume);
                }
            }
            else
            {
                Debug.LogWarning($"AddressableFootstepManager: {label} 사운드가 로드되지 않았습니다.");
            }
        }
        
        /// <summary>
        /// 지형 변경
        /// </summary>
        public void SetGroundType(GroundType newGroundType)
        {
            currentGroundType = newGroundType;
            Debug.Log($"AddressableFootstepManager: 지형이 {newGroundType}로 변경되었습니다.");
        }
        
        /// <summary>
        /// 특정 레이블의 사운드 수동 로드
        /// </summary>
        public async Task LoadSpecificSounds(string label)
        {
            await LoadSoundsByLabel(label);
        }
        
        /// <summary>
        /// 캐시된 사운드 해제
        /// </summary>
        public void ReleaseSounds()
        {
            soundCache.Clear();
            isInitialized = false;
            Debug.Log("AddressableFootstepManager: 사운드 캐시가 해제되었습니다.");
        }
        
        [ContextMenu("테스트 발자국 소리")]
        public void TestFootstepSound()
        {
            PlayFootstepSound(false);
        }
        
        [ContextMenu("테스트 달리기 소리")]
        public void TestRunSound()
        {
            PlayFootstepSound(true);
        }
        
        [ContextMenu("테스트 점프 소리")]
        public void TestJumpSound()
        {
            PlayJumpSound();
        }
        
        [ContextMenu("사운드 초기화")]
        public void TestInitialize()
        {
            InitializeFootstepSounds();
        }
    }
}
