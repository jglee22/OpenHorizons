using UnityEngine;

/// <summary>
/// 애니메이션 이벤트를 받아서 PlayerCombatController로 전달하는 스크립트
/// Rio 오브젝트에 붙여서 사용
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    [Header("참조")]
    public PlayerCombatController playerCombatController;
    
    void Start()
    {
        // PlayerCombatController 자동 찾기
        if (playerCombatController == null)
        {
            playerCombatController = GetComponentInParent<PlayerCombatController>();
        }
        
        if (playerCombatController == null)
        {
            Debug.LogError("PlayerCombatController를 찾을 수 없습니다! Player 오브젝트에 PlayerCombatController가 있는지 확인해주세요.");
        }
    }
    
    /// <summary>
    /// 애니메이션 이벤트: 공격 히트
    /// </summary>
    public void OnAttackHit()
    {
        if (playerCombatController != null)
        {
            playerCombatController.OnAttackHit();
        }
        else
        {
            Debug.LogWarning("PlayerCombatController가 없습니다!");
        }
    }
    
    /// <summary>
    /// 애니메이션 이벤트: 공격 1 히트
    /// </summary>
    public void OnAttack1Hit()
    {
        OnAttackHit();
    }
    
    /// <summary>
    /// 애니메이션 이벤트: 공격 2 히트
    /// </summary>
    public void OnAttack2Hit()
    {
        OnAttackHit();
    }
    
    /// <summary>
    /// 애니메이션 이벤트: 공격 3 히트
    /// </summary>
    public void OnAttack3Hit()
    {
        OnAttackHit();
    }
    
    /// <summary>
    /// 애니메이션 이벤트: 공격 시작
    /// </summary>
    public void StartAttack()
    {
        if (playerCombatController != null)
        {
            // 활성 무기 콜라이더 찾기
            WeaponCollider activeCollider = playerCombatController.GetActiveWeaponCollider();
            if (activeCollider != null)
            {
                activeCollider.StartAttack();
            }
            else
            {
                Debug.LogWarning("활성 무기 콜라이더를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("PlayerCombatController가 없습니다!");
        }
    }
    
    /// <summary>
    /// 애니메이션 이벤트: 공격 종료
    /// </summary>
    public void EndAttack()
    {
        if (playerCombatController != null)
        {
            // 활성 무기 콜라이더 찾기
            WeaponCollider activeCollider = playerCombatController.GetActiveWeaponCollider();
            if (activeCollider != null)
            {
                activeCollider.EndAttack();
            }
            else
            {
                Debug.LogWarning("활성 무기 콜라이더를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("PlayerCombatController가 없습니다!");
        }
    }
    
    /// <summary>
    /// 애니메이션 이벤트: 데미지 처리
    /// </summary>
    public void ProcessHit()
    {
        Debug.Log("🎬 AnimationEventReceiver.ProcessHit 호출됨!");
        
        if (playerCombatController != null)
        {
            // 활성 무기 콜라이더 찾기
            WeaponCollider activeCollider = playerCombatController.GetActiveWeaponCollider();
            if (activeCollider != null)
            {
                Debug.Log($"✅ 활성 무기 콜라이더 찾음: {activeCollider.gameObject.name}");
                activeCollider.ProcessHit();
            }
            else
            {
                Debug.LogWarning("❌ 활성 무기 콜라이더를 찾을 수 없습니다!");
            }
        }
        else
        {
            Debug.LogWarning("❌ PlayerCombatController가 없습니다!");
        }
    }
}
