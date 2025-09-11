using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 무기나 주먹에 붙여서 사용하는 콜라이더 스크립트
/// OnTrigger 이벤트로 정확한 공격 감지
/// </summary>
public class WeaponCollider : MonoBehaviour
{
    [Header("공격 설정")]
    public float damage = 25f;
    public float knockbackForce = 5f;
    public LayerMask enemyLayer = 1 << 9; // Enemy 레이어 (9번, PlayerCombatController에서 설정됨)
    
    [Header("이펙트")]
    public GameObject hitEffectPrefab;
    public Transform hitEffectPoint;
    
    [Header("사운드")]
    public AudioClip[] hitSounds;
    
    // 참조
    private PlayerCombatController playerCombatController;
    private AudioSource audioSource;
    private bool isAttacking = false;
    private float lastHitTime = 0f;
    private float hitCooldown = 0.1f; // 중복 히트 방지
    private HashSet<Collider> hitTargets = new HashSet<Collider>(); // 이번 공격에서 이미 히트한 적들
    
    void Start()
    {
        // PlayerCombatController 찾기
        playerCombatController = GetComponentInParent<PlayerCombatController>();
        if (playerCombatController == null)
        {
            playerCombatController = FindObjectOfType<PlayerCombatController>();
        }
        
        // AudioSource 설정
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 콜라이더가 Trigger인지 확인
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Debug.Log($"콜라이더 정보 - 이름: {gameObject.name}, IsTrigger: {col.isTrigger}, 크기: {col.bounds.size}, 위치: {transform.position}");
            if (!col.isTrigger)
            {
                Debug.LogWarning($"{gameObject.name}의 콜라이더가 Trigger가 아닙니다! isTrigger를 true로 설정해주세요.");
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name}에 콜라이더가 없습니다!");
        }
        
        // 콜라이더 기본적으로 비활성화 (애니메이션 이벤트로만 활성화)
        Collider startCollider = GetComponent<Collider>();
        if (startCollider != null)
        {
            startCollider.enabled = false;
        }
        
        // 레이어 마스크 디버그
        Debug.Log($"WeaponCollider 시작! 무기: {gameObject.name}, 적 레이어: {enemyLayer}, 레이어: {gameObject.layer}, 콜라이더 비활성화");
    }
    
    /// <summary>
    /// 공격 시작 (애니메이션 이벤트에서 호출)
    /// </summary>
    public void StartAttack()
    {
        isAttacking = true;
        lastHitTime = 0f;
        hitTargets.Clear(); // 히트 목록 초기화
        
        // 콜라이더 활성화
        Collider attackCollider = GetComponent<Collider>();
        if (attackCollider != null)
        {
            attackCollider.enabled = true;
        }
        
        Debug.Log($"🔥 StartAttack 호출됨! 무기: {gameObject.name}, 데미지: {damage}, 콜라이더 활성화, isAttacking: {isAttacking}");
    }
    
    /// <summary>
    /// 공격 종료 (애니메이션 이벤트에서 호출)
    /// </summary>
    public void EndAttack()
    {
        isAttacking = false;
        hitTargets.Clear(); // 히트 목록 초기화
        
        // 콜라이더 비활성화
        Collider endCollider = GetComponent<Collider>();
        if (endCollider != null)
        {
            endCollider.enabled = false;
        }
        
        Debug.Log($"무기 공격 종료! 무기: {gameObject.name}, 콜라이더 비활성화");
    }
    
    void OnTriggerEnter(Collider other)
    {
        // OnTriggerEnter에서는 아무것도 하지 않음
        // 애니메이션 이벤트로만 데미지 처리
        Debug.Log($"OnTriggerEnter 호출됨! 대상: {other.name} (애니메이션 이벤트로만 처리)");
    }
    
    /// <summary>
    /// OnTriggerStay - 지속적인 충돌 방지
    /// </summary>
    void OnTriggerStay(Collider other)
    {
        // OnTriggerStay에서는 아무것도 하지 않음 (중복 방지)
        // 애니메이션 이벤트로만 히트 처리
    }
    
    /// <summary>
    /// 애니메이션 이벤트: 데미지 처리
    /// </summary>
    public void ProcessHit()
    {
        Debug.Log($"⚔️ ProcessHit 호출됨! isAttacking: {isAttacking}, 무기: {gameObject.name}");
        
        if (!isAttacking)
        {
            Debug.Log("❌ 공격 중이 아니므로 데미지 처리 무시");
            return;
        }
        
        // 콜라이더 범위 내의 적들 찾기
        Collider[] enemies = Physics.OverlapSphere(transform.position, 2f, enemyLayer);
        
        Debug.Log($"🎯 애니메이션 이벤트로 데미지 처리! 범위 내 적 수: {enemies.Length}, 위치: {transform.position}");
        
        foreach (Collider enemy in enemies)
        {
            // 이미 이번 공격에서 히트한 적인지 확인
            if (hitTargets.Contains(enemy))
            {
                Debug.Log($"이미 히트한 적이므로 무시: {enemy.name}");
                continue;
            }
            
            // 쿨다운 확인
            if (Time.time - lastHitTime < hitCooldown) 
            {
                Debug.Log($"쿨다운 중이므로 무시 (남은 시간: {hitCooldown - (Time.time - lastHitTime):F2}초)");
                continue;
            }
            
            // 적에게 데미지 주기
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                // 데미지 적용
                enemyAI.TakeDamage(damage);
                
                // 넉백 효과
                ApplyKnockback(enemy);
                
                // 히트 이펙트
                CreateHitEffect(enemy.transform.position);
                
                // 히트 사운드
                PlayHitSound();
                
                // 쿨다운 설정
                lastHitTime = Time.time;
                
                // 히트 목록에 추가 (중복 방지)
                hitTargets.Add(enemy);
                
                Debug.Log($"애니메이션 이벤트로 {damage} 데미지를 입혔습니다! 대상: {enemy.name}");
            }
            else
            {
                Debug.LogWarning($"EnemyAI 컴포넌트를 찾을 수 없습니다! 대상: {enemy.name}");
            }
        }
    }
    
    /// <summary>
    /// 넉백 효과 적용
    /// </summary>
    void ApplyKnockback(Collider target)
    {
        Rigidbody targetRb = target.GetComponent<Rigidbody>();
        if (targetRb != null)
        {
            Vector3 knockbackDirection = (target.transform.position - transform.position).normalized;
            knockbackDirection.y = 0.3f; // 약간 위로
            targetRb.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
        }
    }
    
    /// <summary>
    /// 히트 이펙트 생성
    /// </summary>
    void CreateHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            Vector3 effectPos = hitEffectPoint != null ? hitEffectPoint.position : position;
            GameObject effect = Instantiate(hitEffectPrefab, effectPos, Quaternion.identity);
            
            // 3초 후 파괴
            Destroy(effect, 3f);
        }
    }
    
    /// <summary>
    /// 히트 사운드 재생
    /// </summary>
    void PlayHitSound()
    {
        if (audioSource != null && hitSounds.Length > 0)
        {
            AudioClip randomClip = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(randomClip);
        }
    }
    
    /// <summary>
    /// 데미지 설정 (외부에서 호출)
    /// </summary>
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
    }
    
    /// <summary>
    /// 공격 활성화/비활성화
    /// </summary>
    public void SetAttackActive(bool active)
    {
        isAttacking = active;
    }
}
