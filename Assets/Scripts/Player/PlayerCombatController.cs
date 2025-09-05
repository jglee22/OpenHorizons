using UnityEngine;
using AudioSystem;

[RequireComponent(typeof(PlayerController))]
public class PlayerCombatController : MonoBehaviour
{
    [Header("전투 설정")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float attackDamage = 25f;
    public float attackRange = 2f;
    public float attackCooldown = 1f;
    public float blockDuration = 2f;
    
    [Header("콤보 시스템")]
    public int maxComboCount = 3; // 최대 콤보 수
    public float comboResetTime = 2f; // 콤보 리셋 시간
    public float comboDamageMultiplier = 1.2f; // 콤보 데미지 배수
    
    [Header("무기 설정")]
    public Transform weaponHolder;
    public Weapon currentWeapon;
    public LayerMask enemyLayer = 1 << 8; // Enemy 레이어
    
    [Header("애니메이션")]
    public Animator animator;
    
    [Header("UI")]
    public HealthBarUI healthBarUI;
    
    // 전투 상태
    private bool isAttacking = false;
    private bool isBlocking = false;
    private bool isDead = false;
    private float lastAttackTime = 0f;
    private float blockStartTime = 0f;
    
    // 콤보 시스템 변수
    private int currentComboCount = 0;
    private float lastComboTime = 0f;
    
    // 애니메이션 해시
    private readonly int attackTriggerHash = Animator.StringToHash("Attack");
    private readonly int blockTriggerHash = Animator.StringToHash("Block");
    private readonly int hitTriggerHash = Animator.StringToHash("Hit");
    private readonly int deathTriggerHash = Animator.StringToHash("Death");
    private readonly int isBlockingHash = Animator.StringToHash("IsBlocking");
    private readonly int isAttackingHash = Animator.StringToHash("IsAttacking");
    
    // 콤보 애니메이션 해시
    private readonly int attack1TriggerHash = Animator.StringToHash("Attack1");
    private readonly int attack2TriggerHash = Animator.StringToHash("Attack2");
    private readonly int attack3TriggerHash = Animator.StringToHash("Attack3");
    private readonly int comboCountHash = Animator.StringToHash("ComboCount");
    private readonly int attackTypeHash = Animator.StringToHash("AttackType");
    
    // 컴포넌트 참조
    private PlayerController playerController;
    private CharacterController characterController;
    
    void Start()
    {
        // 컴포넌트 초기화
        playerController = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        
        // Animator 자동 찾기 (자식 오브젝트에서)
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogError("자식 오브젝트에서 Animator를 찾을 수 없습니다! Rio 오브젝트에 Animator가 있는지 확인해주세요.");
        }
        else
        {
            Debug.Log($"PlayerCombatController에서 Animator를 찾았습니다: {animator.name}");
        }
        
        // 체력 초기화
        currentHealth = maxHealth;
        
        // 무기 홀더 자동 생성
        if (weaponHolder == null)
        {
            GameObject weaponHolderObj = new GameObject("WeaponHolder");
            weaponHolderObj.transform.SetParent(transform);
            weaponHolderObj.transform.localPosition = new Vector3(0.5f, 1.2f, 0.3f);
            weaponHolder = weaponHolderObj.transform;
        }
        
        // HealthBar UI 자동 찾기
        if (healthBarUI == null)
        {
            healthBarUI = FindObjectOfType<HealthBarUI>();
        }
        
        // UI 업데이트
        UpdateHealthUI();
    }
    
    void Update()
    {
        if (isDead) return;
        
        // 콤보 리셋 타이머
        if (Time.time - lastComboTime > comboResetTime)
        {
            currentComboCount = 0;
        }
        
        HandleCombatInput();
        UpdateCombatState();
    }
    
    void HandleCombatInput()
    {
        // 공격 입력 (마우스 왼쪽 클릭) - 콤보 시스템
        if (Input.GetMouseButtonDown(0) && CanAttack())
        {
            PerformComboAttack();
        }
        
        // 방어 입력 (마우스 오른쪽 클릭)
        if (Input.GetMouseButtonDown(1) && CanBlock())
        {
            StartBlocking();
        }
        
        // 방어 해제 (마우스 오른쪽 클릭 해제)
        if (Input.GetMouseButtonUp(1))
        {
            StopBlocking();
        }
    }
    
    void UpdateCombatState()
    {
        // 공격 상태 업데이트
        if (isAttacking)
        {
            // 공격 애니메이션이 끝났는지 확인
            if (animator != null && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))
            {
                isAttacking = false;
            }
        }
        
        // 방어 상태 업데이트
        if (isBlocking)
        {
            // 방어 시간 초과 확인
            if (Time.time - blockStartTime > blockDuration)
            {
                StopBlocking();
            }
        }
    }
    
    bool CanAttack()
    {
        return !isAttacking && !isBlocking && !isDead && 
               Time.time - lastAttackTime >= attackCooldown;
    }
    
    bool CanBlock()
    {
        return !isAttacking && !isDead;
    }
    
    void Attack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // 애니메이션 트리거
        if (animator != null)
        {
            animator.SetTrigger(attackTriggerHash);
            animator.SetBool(isAttackingHash, true);
        }
        
        // 무기가 있으면 무기로 공격, 없으면 맨손 공격
        if (currentWeapon != null)
        {
            currentWeapon.Attack();
        }
        else
        {
            DetectAndDamageEnemies();
        }
        
        Debug.Log("플레이어가 공격했습니다!");
    }
    
    void StartBlocking()
    {
        isBlocking = true;
        blockStartTime = Time.time;
        
        // 애니메이션 설정
        if (animator != null)
        {
            animator.SetTrigger(blockTriggerHash);
            animator.SetBool(isBlockingHash, true);
        }
        
        // 무기가 있으면 무기로 방어
        if (currentWeapon != null)
        {
            currentWeapon.Block();
        }
        
        Debug.Log("플레이어가 방어를 시작했습니다!");
    }
    
    void StopBlocking()
    {
        isBlocking = false;
        
        // 애니메이션 해제
        if (animator != null)
        {
            animator.SetBool(isBlockingHash, false);
        }
        
        Debug.Log("플레이어가 방어를 해제했습니다!");
    }
    
    void DetectAndDamageEnemies()
    {
        // 공격 범위 내 콜라이더 찾기
        Collider[] enemies = Physics.OverlapSphere(transform.position + transform.forward * attackRange, attackRange, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            // 적에게 데미지 주기
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(attackDamage);
                Debug.Log($"적에게 {attackDamage} 데미지를 입혔습니다!");
            }
        }
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        // 방어 중이면 데미지 감소
        if (isBlocking)
        {
            damage *= 0.3f; // 70% 데미지 감소
            Debug.Log("방어로 데미지가 감소했습니다!");
        }
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // 피해 애니메이션
        if (animator != null)
        {
            animator.SetTrigger(hitTriggerHash);
        }
        
        // UI 업데이트
        UpdateHealthUI();
        
        // 사망 확인
        if (currentHealth <= 0)
        {
            Die();
        }
        
        Debug.Log($"플레이어가 {damage} 데미지를 받았습니다! 남은 체력: {currentHealth}");
    }
    
    void Die()
    {
        isDead = true;
        
        // 사망 애니메이션
        if (animator != null)
        {
            animator.SetTrigger(deathTriggerHash);
        }
        
        // 플레이어 컨트롤러 비활성화
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        Debug.Log("플레이어가 사망했습니다!");
    }
    
    void UpdateHealthUI()
    {
        if (healthBarUI != null)
        {
            healthBarUI.UpdateHealth(currentHealth, maxHealth);
        }
    }
    
    // 공격 범위 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackRange);
    }
    
    // 공격 상태 확인 (다른 스크립트에서 사용)
    public bool IsAttacking()
    {
        return isAttacking;
    }
    
    public bool IsBlocking()
    {
        return isBlocking;
    }
    
    public bool IsDead()
    {
        return isDead;
    }
    
    // 체력 회복
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        UpdateHealthUI();
        Debug.Log($"체력을 {amount} 회복했습니다! 현재 체력: {currentHealth}");
    }
    
    // 무기 장착
    public void EquipWeapon(Weapon weapon)
    {
        // 기존 무기 해제
        if (currentWeapon != null)
        {
            currentWeapon.Unequip();
        }
        
        // 새 무기 장착
        currentWeapon = weapon;
        if (currentWeapon != null)
        {
            currentWeapon.Equip(this);
        }
        
        Debug.Log($"무기를 장착했습니다: {weapon?.weaponName ?? "맨손"}");
    }
    
    // 무기 해제
    public void UnequipWeapon()
    {
        if (currentWeapon != null)
        {
            currentWeapon.Unequip();
            currentWeapon = null;
        }
        
        Debug.Log("무기를 해제했습니다!");
    }
    
    // 무기 정보 가져오기
    public string GetWeaponInfo()
    {
        if (currentWeapon != null)
        {
            return currentWeapon.GetWeaponInfo();
        }
        return "맨손";
    }
    
    // 콤보 공격 시스템
    void PerformComboAttack()
    {
        // 콤보 카운트 증가
        currentComboCount++;
        if (currentComboCount > maxComboCount)
        {
            currentComboCount = 1; // 최대 콤보 도달 시 1로 리셋
        }
        
        // 공격 상태 설정
        isAttacking = true;
        lastAttackTime = Time.time;
        lastComboTime = Time.time;
        
        // 콤보에 따른 애니메이션 트리거 설정
        switch (currentComboCount)
        {
            case 1:
                animator.SetTrigger(attack1TriggerHash);
                Debug.Log("Attack 1 애니메이션 실행!");
                break;
            case 2:
                animator.SetTrigger(attack2TriggerHash);
                Debug.Log("Attack 2 애니메이션 실행!");
                break;
            case 3:
                animator.SetTrigger(attack3TriggerHash);
                Debug.Log("Attack 3 애니메이션 실행!");
                break;
        }
        
        // 애니메이션 파라미터 설정
        animator.SetInteger(comboCountHash, currentComboCount);
        animator.SetInteger(attackTypeHash, currentComboCount);
        animator.SetBool(isAttackingHash, true);
        
        // 콤보 데미지 계산
        float comboDamage = attackDamage * Mathf.Pow(comboDamageMultiplier, currentComboCount - 1);
        
        // 공격 사운드 재생
        if (AudioManager.Instance != null)
        {
            string attackSound = currentComboCount == 1 ? "attack" : $"attack_combo_{currentComboCount}";
            AudioManager.Instance.PlaySFX(attackSound);
        }
        
        Debug.Log($"콤보 공격! {currentComboCount}/{maxComboCount} - 데미지: {comboDamage}");
        
        // 실제 공격 실행
        ExecuteAttack(comboDamage);
        
        // 공격 애니메이션 완료 후 상태 리셋
        StartCoroutine(ResetAttackState());
    }
    
    // 실제 공격 실행
    void ExecuteAttack(float damage)
    {
        // 공격 범위 내의 적 찾기
        Collider[] enemies = Physics.OverlapSphere(transform.position, attackRange, enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            // 적에게 데미지 주기
            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
                Debug.Log($"적에게 {damage} 데미지를 입혔습니다!");
            }
        }
    }
    
    // 공격 상태 리셋
    System.Collections.IEnumerator ResetAttackState()
    {
        // 공격 애니메이션 시간만큼 대기 (대략 1초)
        yield return new WaitForSeconds(1f);
        
        isAttacking = false;
        animator.SetBool(isAttackingHash, false);
    }
}
