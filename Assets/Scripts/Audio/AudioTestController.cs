using UnityEngine;
using AudioSystem;

/// <summary>
/// 사운드 시스템 테스트를 위한 컨트롤러
/// </summary>
public class AudioTestController : MonoBehaviour
{
    [Header("테스트 설정")]
    [SerializeField] private bool enableTestKeys = true;
    [SerializeField] private bool showTestUI = true;
    
    [Header("사운드 테스트")]
    [SerializeField] private KeyCode testFootstepKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode testJumpKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode testAttackKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode testHitKey = KeyCode.Alpha4;
    [SerializeField] private KeyCode testWeaponEquipKey = KeyCode.Alpha5;
    [SerializeField] private KeyCode testItemPickupKey = KeyCode.Alpha6;
    [SerializeField] private KeyCode testInventoryKey = KeyCode.Alpha7;
    [SerializeField] private KeyCode testButtonClickKey = KeyCode.Alpha8;
    [SerializeField] private KeyCode testBackgroundMusicKey = KeyCode.Alpha9;
    [SerializeField] private KeyCode testCombatMusicKey = KeyCode.Alpha0;
    
    [Header("볼륨 테스트")]
    [SerializeField] private KeyCode volumeUpKey = KeyCode.Plus;
    [SerializeField] private KeyCode volumeDownKey = KeyCode.Minus;
    [SerializeField] private KeyCode muteKey = KeyCode.M;
    
    private void Update()
    {
        if (!enableTestKeys) return;
        
        HandleSoundTests();
        HandleVolumeTests();
    }
    
    private void HandleSoundTests()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioTestController: AudioManager를 찾을 수 없습니다!");
            return;
        }
        
        // AudioManager 상태 확인
        if (AudioManager.Instance.GetSFXVolume() <= 0)
        {
            Debug.LogWarning("AudioTestController: SFX 볼륨이 0입니다! +키를 눌러 볼륨을 높이세요.");
        }
        
        // 플레이어 사운드 테스트
        if (Input.GetKeyDown(testFootstepKey))
        {
            Debug.Log("AudioTestController: 발걸음 사운드 재생 시도...");
            AudioManager.Instance.PlaySFX("footstep");
            Debug.Log($"AudioTestController: 현재 SFX 볼륨: {AudioManager.Instance.GetSFXVolume()}");
        }
        
        if (Input.GetKeyDown(testJumpKey))
        {
            AudioManager.Instance.PlaySFX("jump");
            Debug.Log("점프 소리 테스트");
        }
        
        if (Input.GetKeyDown(testAttackKey))
        {
            AudioManager.Instance.PlaySFX("attack");
            Debug.Log("공격 소리 테스트");
        }
        
        if (Input.GetKeyDown(testHitKey))
        {
            AudioManager.Instance.PlaySFX("hit");
            Debug.Log("피해 소리 테스트");
        }
        
        if (Input.GetKeyDown(testWeaponEquipKey))
        {
            AudioManager.Instance.PlaySFX("weapon_equip");
            Debug.Log("무기 장착 소리 테스트");
        }
        
        if (Input.GetKeyDown(testItemPickupKey))
        {
            AudioManager.Instance.PlaySFX("item_pickup");
            Debug.Log("아이템 획득 소리 테스트");
        }
        
        if (Input.GetKeyDown(testInventoryKey))
        {
            AudioManager.Instance.PlayUISFX("inventory_open");
            Debug.Log("인벤토리 열기 소리 테스트");
        }
        
        if (Input.GetKeyDown(testButtonClickKey))
        {
            AudioManager.Instance.PlayUISFX("button_click");
            Debug.Log("버튼 클릭 소리 테스트");
        }
        
        if (Input.GetKeyDown(testBackgroundMusicKey))
        {
            AudioManager.Instance.PlayBackgroundMusic();
            Debug.Log("배경음악 테스트");
        }
        
        if (Input.GetKeyDown(testCombatMusicKey))
        {
            AudioManager.Instance.PlayCombatMusic();
            Debug.Log("전투음악 테스트");
        }
    }
    
    private void HandleVolumeTests()
    {
        if (AudioManager.Instance == null) return;
        
        if (Input.GetKeyDown(volumeUpKey))
        {
            float currentVolume = AudioManager.Instance.GetSFXVolume();
            AudioManager.Instance.SetSFXVolume(Mathf.Min(1.0f, currentVolume + 0.1f));
            Debug.Log($"SFX 볼륨 증가: {AudioManager.Instance.GetSFXVolume():F2}");
        }
        
        if (Input.GetKeyDown(volumeDownKey))
        {
            float currentVolume = AudioManager.Instance.GetSFXVolume();
            AudioManager.Instance.SetSFXVolume(Mathf.Max(0.0f, currentVolume - 0.1f));
            Debug.Log($"SFX 볼륨 감소: {AudioManager.Instance.GetSFXVolume():F2}");
        }
        
        if (Input.GetKeyDown(muteKey))
        {
            AudioManager.Instance.SetSFXVolume(0.0f);
            Debug.Log("SFX 음소거");
        }
    }
    
    // private void OnGUI()
    // {
    //     if (!showTestUI) return;
        
    //     GUILayout.BeginArea(new Rect(10, 10, 300, 400));
    //     GUILayout.Label("=== 사운드 테스트 ===");
    //     GUILayout.Label("1키: 발걸음 소리");
    //     GUILayout.Label("2키: 점프 소리");
    //     GUILayout.Label("3키: 공격 소리");
    //     GUILayout.Label("4키: 피해 소리");
    //     GUILayout.Label("5키: 무기 장착 소리");
    //     GUILayout.Label("6키: 아이템 획득 소리");
    //     GUILayout.Label("7키: 인벤토리 열기 소리");
    //     GUILayout.Label("8키: 버튼 클릭 소리");
    //     GUILayout.Label("9키: 배경음악");
    //     GUILayout.Label("0키: 전투음악");
    //     GUILayout.Label("");
    //     GUILayout.Label("=== 볼륨 테스트 ===");
    //     GUILayout.Label("+키: 볼륨 증가");
    //     GUILayout.Label("-키: 볼륨 감소");
    //     GUILayout.Label("M키: 음소거");
    //     GUILayout.EndArea();
    // }
}
