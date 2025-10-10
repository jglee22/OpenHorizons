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
    public float attackHitDelay = 0.4f; // 공격 히트 지연 시간
    
    [Header("무기 설정")]
    public Transform weaponHolder;
    public Weapon currentWeapon;
    public LayerMask enemyLayer = 1 << 9; // Enemy 레이어 (9번)
    
    [Header("무기 콜라이더")]
    public WeaponCollider weaponCollider; // 무기 콜라이더 참조
    public WeaponCollider fistCollider; // 주먹 콜라이더 참조
    
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
    private bool hasAttacked = false; // 중복 공격 방지
    
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
        
        // 무기 콜라이더 자동 찾기
        if (weaponCollider == null)
        {
            weaponCollider = GetComponentInChildren<WeaponCollider>();
        }
        
        if (fistCollider == null)
        {
            // 주먹 콜라이더 자동 생성
            CreateFistCollider();
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
        if (IsTouchOverUI()) return; // UI 위에서는 전투 입력 무시
        // 공격 입력 (마우스 왼쪽 클릭) - 콤보 시스템
        if (Input.GetMouseButtonDown(0) && CanAttack())
        {
            Debug.Log($"공격 입력 감지! isAttacking: {isAttacking}, lastAttackTime: {lastAttackTime}, currentTime: {Time.time}");
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

    // UI 위 터치/클릭 여부 확인
    bool IsTouchOverUI()
    {
        if (UnityEngine.EventSystems.EventSystem.current == null) return false;

        // 마우스(에디터/PC)
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return true;

        // 터치(모바일)
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        }
        return false;
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
    
    public bool CanAttack()
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
        
        Debug.Log($"맨손 공격 범위 내 적 수: {enemies.Length}");
        
        foreach (Collider enemy in enemies)
        {
            Debug.Log($"맨손 공격으로 발견된 적: {enemy.name}");
            
            // 적에게 데미지 주기
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(attackDamage);
                Debug.Log($"적에게 {attackDamage} 데미지를 입혔습니다!");
            }
            else
            {
                Debug.LogWarning($"EnemyAI 컴포넌트를 찾을 수 없습니다: {enemy.name}");
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
        // 콤보 공격 범위 (원형)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // 맨손 공격 범위 (앞쪽)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + transform.forward * attackRange, attackRange);
        
        // 공격 방향 표시
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * attackRange * 2);
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
            
            // 무기의 WeaponCollider 찾기
            weaponCollider = currentWeapon.GetComponent<WeaponCollider>();
            if (weaponCollider != null)
            {
                weaponCollider.enemyLayer = enemyLayer;
                weaponCollider.damage = attackDamage;
                Debug.Log($"무기 WeaponCollider를 찾았습니다: {weaponCollider.gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"무기 {currentWeapon.weaponName}에 WeaponCollider가 없습니다!");
            }
            
            // 무기 장착 시 FistCollider 비활성화
            if (fistCollider != null)
            {
                fistCollider.gameObject.SetActive(false);
                Debug.Log("무기 장착으로 인해 FistCollider 비활성화");
            }
        }
        else
        {
            weaponCollider = null; // 맨손일 때는 무기 콜라이더 비활성화
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
        
        // 무기 해제 시 FistCollider 활성화
        if (fistCollider != null)
        {
            fistCollider.gameObject.SetActive(true);
            Debug.Log("무기 해제로 인해 FistCollider 활성화");
        }
        
        // 무기 콜라이더 참조 해제
        weaponCollider = null;
        
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
    public void PerformComboAttack()
    {
        // 이미 공격 중이면 무시
        if (isAttacking)
        {
            Debug.Log("이미 공격 중이므로 무시합니다!");
            return;
        }
        
        // 콤보 카운트 증가
        currentComboCount++;
        if (currentComboCount > maxComboCount)
        {
            currentComboCount = 1; // 최대 콤보 도달 시 1로 리셋
        }
        
        // 공격 상태 설정 (즉시 설정)
        isAttacking = true;
        hasAttacked = false; // 공격 플래그 리셋
        lastAttackTime = Time.time;
        lastComboTime = Time.time;
        
        Debug.Log($"공격 시작! 콤보: {currentComboCount}, isAttacking: {isAttacking}");
        
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
        
        // 무기 콜라이더에 데미지 설정
        WeaponCollider activeCollider = GetActiveWeaponCollider();
        Debug.Log($"GetActiveWeaponCollider 결과: {activeCollider?.gameObject.name ?? "null"}");
        
        if (activeCollider != null)
        {
            activeCollider.SetDamage(comboDamage);
            activeCollider.enemyLayer = enemyLayer; // 레이어 마스크 전달
            activeCollider.StartAttack();
            Debug.Log($"무기 콜라이더 활성화! 무기: {activeCollider.gameObject.name}, 데미지: {comboDamage}, 레이어: {enemyLayer}, 위치: {activeCollider.transform.position}");
        }
        else
        {
            Debug.LogWarning("활성 무기 콜라이더를 찾을 수 없습니다! 공격을 실행할 수 없습니다.");
            Debug.Log($"weaponCollider: {weaponCollider?.gameObject.name ?? "null"}, fistCollider: {fistCollider?.gameObject.name ?? "null"}, currentWeapon: {currentWeapon?.weaponName ?? "null"}");
        }
        
        // 공격 사운드 재생
        if (AudioManager.Instance != null)
        {
            string attackSound = currentComboCount == 1 ? "attack" : $"attack_combo_{currentComboCount}";
            AudioManager.Instance.PlaySFX(attackSound);
        }
        
        Debug.Log($"콤보 공격! {currentComboCount}/{maxComboCount} - 데미지: {comboDamage}");
        
        // 공격 애니메이션 완료 후 상태 리셋
        StartCoroutine(ResetAttackState());
    }
    
    // 실제 공격 실행 (WeaponCollider에서 처리하므로 제거됨)
    // void ExecuteAttack(float damage) - WeaponCollider의 OnTriggerEnter에서 처리
    
    // 애니메이션 이벤트용 공격 실행 (애니메이션 중간에 호출)
    // Rio 오브젝트의 AnimationEventReceiver에서 호출됨
    public void OnAttackHit()
    {
        // WeaponCollider의 OnTriggerEnter에서 처리하므로 여기서는 아무것도 하지 않음
        Debug.Log("OnAttackHit 호출됨 - WeaponCollider에서 처리 중");
    }
    
    // 지연된 공격 실행 (WeaponCollider에서 처리하므로 제거됨)
    // System.Collections.IEnumerator DelayedAttack - WeaponCollider의 OnTriggerEnter에서 처리
    
    // 주먹 콜라이더 자동 생성
    void CreateFistCollider()
    {
        // 주먹 콜라이더 오브젝트 생성
        GameObject fistColliderObj = new GameObject("FistCollider");
        fistColliderObj.transform.SetParent(transform);
        fistColliderObj.transform.localPosition = new Vector3(0.5f, 1.2f, 0.8f); // 플레이어 앞쪽
        
        // Rigidbody 추가 (OnTriggerEnter 작동을 위해 필요)
        Rigidbody rb = fistColliderObj.AddComponent<Rigidbody>();
        rb.isKinematic = true; // 물리 시뮬레이션 비활성화
        
        // 콜라이더 추가
        BoxCollider boxCollider = fistColliderObj.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        
        // WeaponCollider 스크립트 추가
        fistCollider = fistColliderObj.AddComponent<WeaponCollider>();
        fistCollider.damage = attackDamage;
        fistCollider.enemyLayer = enemyLayer;
        
        Debug.Log("주먹 콜라이더를 자동으로 생성했습니다! (Rigidbody 포함)");
    }
    
    // 활성 무기 콜라이더 가져오기
    public WeaponCollider GetActiveWeaponCollider()
    {
        // 무기가 있으면 무기 콜라이더, 없으면 주먹 콜라이더
        if (currentWeapon != null && weaponCollider != null)
        {
            return weaponCollider;
        }
        else if (fistCollider != null && fistCollider.gameObject.activeInHierarchy)
        {
            return fistCollider;
        }
        
        return null;
    }
    
    // 공격 종료 (애니메이션 이벤트에서 호출)
    public void OnAttackEnd()
    {
        WeaponCollider activeCollider = GetActiveWeaponCollider();
        if (activeCollider != null)
        {
            activeCollider.EndAttack();
        }
    }
    
    // 공격 상태 리셋
    System.Collections.IEnumerator ResetAttackState()
    {
        // 공격 애니메이션 시간만큼 대기 (대략 1초)
        yield return new WaitForSeconds(1f);
        
        // 무기 콜라이더 공격 종료
        WeaponCollider activeCollider = GetActiveWeaponCollider();
        if (activeCollider != null)
        {
            activeCollider.EndAttack();
        }
        
        isAttacking = false;
        hasAttacked = false; // 공격 플래그 리셋
        animator.SetBool(isAttackingHash, false);
    }
}
