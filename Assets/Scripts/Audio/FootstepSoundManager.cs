using UnityEngine;
using AudioSystem;
using System.Collections.Generic;

/// <summary>
/// 발자국 사운드를 관리하는 매니저
/// </summary>
public class FootstepSoundManager : MonoBehaviour
{
    [Header("발자국 사운드 설정")]
    [SerializeField] private GroundType currentGroundType = GroundType.Grass;
    [SerializeField] private bool enableRandomVariation = true;
    [SerializeField] private float pitchVariation = 0.1f;
    [SerializeField] private float volumeVariation = 0.1f;
    
    [Header("사운드 클립들")]
    [SerializeField] private AudioClip[] grassWalkSounds;
    [SerializeField] private AudioClip[] grassRunSounds;
    [SerializeField] private AudioClip[] grassJumpSounds;
    
    [SerializeField] private AudioClip[] dirtWalkSounds;
    [SerializeField] private AudioClip[] dirtRunSounds;
    [SerializeField] private AudioClip[] dirtLandSounds;
    
    [SerializeField] private AudioClip[] gravelWalkSounds;
    [SerializeField] private AudioClip[] gravelRunSounds;
    [SerializeField] private AudioClip[] gravelJumpSounds;
    
    [SerializeField] private AudioClip[] metalWalkSounds;
    [SerializeField] private AudioClip[] metalRunSounds;
    [SerializeField] private AudioClip[] metalJumpSounds;
    
    [SerializeField] private AudioClip[] woodWalkSounds;
    [SerializeField] private AudioClip[] woodRunSounds;
    [SerializeField] private AudioClip[] woodJumpSounds;
    
    [SerializeField] private AudioClip[] stoneWalkSounds;
    [SerializeField] private AudioClip[] stoneRunSounds;
    [SerializeField] private AudioClip[] stoneJumpSounds;
    
    [SerializeField] private AudioClip[] sandWalkSounds;
    [SerializeField] private AudioClip[] sandRunSounds;
    [SerializeField] private AudioClip[] sandJumpSounds;
    
    [SerializeField] private AudioClip[] snowWalkSounds;
    [SerializeField] private AudioClip[] snowRunSounds;
    [SerializeField] private AudioClip[] snowJumpSounds;
    
    [SerializeField] private AudioClip[] waterWalkSounds;
    [SerializeField] private AudioClip[] waterRunSounds;
    [SerializeField] private AudioClip[] waterJumpSounds;
    
    [SerializeField] private AudioClip[] tileWalkSounds;
    [SerializeField] private AudioClip[] tileRunSounds;
    [SerializeField] private AudioClip[] tileJumpSounds;
    
    [SerializeField] private AudioClip[] mudWalkSounds;
    [SerializeField] private AudioClip[] mudRunSounds;
    [SerializeField] private AudioClip[] mudJumpSounds;
    
    [SerializeField] private AudioClip[] leavesWalkSounds;
    [SerializeField] private AudioClip[] leavesRunSounds;
    [SerializeField] private AudioClip[] leavesJumpSounds;
    
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
    
    private void Start()
    {
        LoadFootstepSounds();
    }
    
    private void LoadFootstepSounds()
    {
        // Inspector에서 수동으로 할당된 사운드가 있으면 사용
        if (grassWalkSounds != null && grassWalkSounds.Length > 0)
        {
            Debug.Log("FootstepSoundManager: Inspector에서 할당된 사운드를 사용합니다.");
            return;
        }
        
        // Grass 사운드 로드
        grassWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Grass/Footsteps_Grass_Walk");
        grassRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Grass/Footsteps_Grass_Run");
        grassJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Grass/Footsteps_Grass_Jump");
        
        // Dirt 사운드 로드
        dirtWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_DirtyGround/Footsteps_DirtyGround_Walk");
        dirtRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_DirtyGround/Footsteps_DirtyGround_Run");
        dirtLandSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_DirtyGround/Footsteps_DirtyGround_Land");
        
        // Gravel 사운드 로드
        gravelWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Gravel/Footsteps_Gravel_Walk");
        gravelRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Gravel/Footsteps_Gravel_Run");
        gravelJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Gravel/Footsteps_Gravel_Jump");
        
        // Metal 사운드 로드
        metalWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Metal/Footsteps_Metal_Walk");
        metalRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Metal/Footsteps_Metal_Run");
        metalJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Metal/Footsteps_Metal_Jump");
        
        // Wood 사운드 로드
        woodWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Wood/Footsteps_Wood_Walk");
        woodRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Wood/Footsteps_Wood_Run");
        woodJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Wood/Footsteps_Wood_Jump");
        
        // Rock 사운드 로드
        stoneWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Rock/Footsteps_Rock_Walk");
        stoneRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Rock/Footsteps_Rock_Run");
        stoneJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Rock/Footsteps_Rock_Jump");
        
        // Sand 사운드 로드
        sandWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Sand/Footsteps_Sand_Walk");
        sandRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Sand/Footsteps_Sand_Run");
        sandJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Sand/Footsteps_Sand_Jump");
        
        // Snow 사운드 로드
        snowWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Snow/Footsteps_Snow_Walk");
        snowRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Snow/Footsteps_Snow_Run");
        snowJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Snow/Footsteps_Snow_Jump");
        
        // Water 사운드 로드
        waterWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Water/Footsteps_Water_Walk");
        waterRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Water/Footsteps_Water_Run");
        waterJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Water/Footsteps_Water_Jump");
        
        // Tile 사운드 로드
        tileWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Tile/Footsteps_Tile_Walk");
        tileRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Tile/Footsteps_Tile_Run");
        tileJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Tile/Footsteps_Tile_Jump");
        
        // Mud 사운드 로드
        mudWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Mud/Footsteps_Mud_Walk");
        mudRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Mud/Footsteps_Mud_Run");
        mudJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Mud/Footsteps_Mud_Jump");
        
        // Leaves 사운드 로드
        leavesWalkSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Leaves/Footsteps_Leaves_Walk");
        leavesRunSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Leaves/Footsteps_Leaves_Run");
        leavesJumpSounds = LoadAudioClipsFromPath("Assets/Footsteps - Essentials/Footsteps_Leaves/Footsteps_Leaves_Jump");
        
        Debug.Log("FootstepSoundManager: 발자국 사운드들이 로드되었습니다!");
    }
    
    private AudioClip[] LoadAudioClipsFromPath(string path)
    {
        // Resources 폴더에서 로드 시도
        Object[] objects = Resources.LoadAll<AudioClip>(path);
        if (objects != null && objects.Length > 0)
        {
            Debug.Log($"FootstepSoundManager: {path}에서 {objects.Length}개의 사운드를 로드했습니다.");
            return objects as AudioClip[];
        }
        
        // Resources 로드 실패 시 빈 배열 반환
        Debug.LogWarning($"FootstepSoundManager: {path}에서 사운드를 로드할 수 없습니다. Resources 폴더에 사운드가 있는지 확인하세요.");
        return new AudioClip[0];
    }
    
    public void PlayFootstepSound(bool isRunning = false)
    {
        AudioClip[] sounds = GetFootstepSounds(isRunning);
        if (sounds != null && sounds.Length > 0)
        {
            AudioClip selectedSound = sounds[Random.Range(0, sounds.Length)];
            PlaySoundWithVariation(selectedSound);
        }
    }
    
    public void PlayJumpSound()
    {
        AudioClip[] sounds = GetJumpSounds();
        if (sounds != null && sounds.Length > 0)
        {
            AudioClip selectedSound = sounds[Random.Range(0, sounds.Length)];
            PlaySoundWithVariation(selectedSound);
        }
    }
    
    public void PlayLandSound()
    {
        AudioClip[] sounds = GetLandSounds();
        if (sounds != null && sounds.Length > 0)
        {
            AudioClip selectedSound = sounds[Random.Range(0, sounds.Length)];
            PlaySoundWithVariation(selectedSound);
        }
    }
    
    private AudioClip[] GetFootstepSounds(bool isRunning)
    {
        switch (currentGroundType)
        {
            case GroundType.Grass: return isRunning ? grassRunSounds : grassWalkSounds;
            case GroundType.Dirt: return isRunning ? dirtRunSounds : dirtWalkSounds;
            case GroundType.Gravel: return isRunning ? gravelRunSounds : gravelWalkSounds;
            case GroundType.Metal: return isRunning ? metalRunSounds : metalWalkSounds;
            case GroundType.Wood: return isRunning ? woodRunSounds : woodWalkSounds;
            case GroundType.Stone: return isRunning ? stoneRunSounds : stoneWalkSounds;
            case GroundType.Sand: return isRunning ? sandRunSounds : sandWalkSounds;
            case GroundType.Snow: return isRunning ? snowRunSounds : snowWalkSounds;
            case GroundType.Water: return isRunning ? waterRunSounds : waterWalkSounds;
            case GroundType.Tile: return isRunning ? tileRunSounds : tileWalkSounds;
            case GroundType.Mud: return isRunning ? mudRunSounds : mudWalkSounds;
            case GroundType.Leaves: return isRunning ? leavesRunSounds : leavesWalkSounds;
            default: return grassWalkSounds;
        }
    }
    
    private AudioClip[] GetJumpSounds()
    {
        switch (currentGroundType)
        {
            case GroundType.Grass: return grassJumpSounds;
            case GroundType.Dirt: return dirtLandSounds; // Dirt는 Land 사운드 사용
            case GroundType.Gravel: return gravelJumpSounds;
            case GroundType.Metal: return metalJumpSounds;
            case GroundType.Wood: return woodJumpSounds;
            case GroundType.Stone: return stoneJumpSounds;
            case GroundType.Sand: return sandJumpSounds;
            case GroundType.Snow: return snowJumpSounds;
            case GroundType.Water: return waterJumpSounds;
            case GroundType.Tile: return tileJumpSounds;
            case GroundType.Mud: return mudJumpSounds;
            case GroundType.Leaves: return leavesJumpSounds;
            default: return grassJumpSounds;
        }
    }
    
    private AudioClip[] GetLandSounds()
    {
        switch (currentGroundType)
        {
            case GroundType.Dirt: return dirtLandSounds;
            default: return GetJumpSounds(); // 다른 지형은 Jump 사운드 사용
        }
    }
    
    private void PlaySoundWithVariation(AudioClip clip)
    {
        if (clip == null) return;
        
        float volume = 1.0f;
        float pitch = 1.0f;
        
        if (enableRandomVariation)
        {
            volume = 1.0f + Random.Range(-volumeVariation, volumeVariation);
            pitch = 1.0f + Random.Range(-pitchVariation, pitchVariation);
        }
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(clip, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clip, transform.position, volume);
        }
    }
    
    public void SetGroundType(GroundType newGroundType)
    {
        currentGroundType = newGroundType;
        Debug.Log($"FootstepSoundManager: 지형이 {newGroundType}로 변경되었습니다.");
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
}
