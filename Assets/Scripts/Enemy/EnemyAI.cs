using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("적 설정")]
    public float attackDamage = 15f;
    public float attackRange = 1.5f;
    public float detectionRange = 8f;
    public float attackCooldown = 2f;
    public float moveSpeed = 3f;
    
    [Header("AI 설정")]
    public Transform player;
    public LayerMask playerLayer = 1 << 7; // Player 레이어 (7번)
    public float rotationSpeed = 5f;
    
    [Header("애니메이션")]
    public Animator animator;
    
    [Header("사운드")]
    public AudioSource audioSource;
    public AudioClip[] attackSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] deathSounds;
    
    [Header("피격 이펙트")]
    public GameObject hitEffectPrefab; // 피격 이펙트 프리팹
    public Transform hitEffectPoint; // 피격 이펙트 생성 위치
    public float hitFlashDuration = 0.2f; // 피격 시 빨간색 깜빡임 시간
    
    // AI 상태
    public enum EnemyState
    {
        Idle,
        Patrolling,
        Chasing,
        Attacking,
        Dead
    }
    
    public EnemyState currentState = EnemyState.Idle;
    
    // 내부 변수
    private NavMeshAgent navAgent;
    private EnemyHealth enemyHealth;
    private float lastAttackTime = 0f;
    private Vector3 startPosition;
    private bool isAttacking = false;
    private float attackStartTime = 0f;
    
    // 피격 피드백 변수
    private Renderer[] enemyRenderers;
    private Color[] originalColors;
    private bool isHitFlashing = false;
    
    // 애니메이션 해시
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int attackTriggerHash = Animator.StringToHash("Attack");
    private readonly int hitTriggerHash = Animator.StringToHash("Hit");
    private readonly int deathTriggerHash = Animator.StringToHash("Death");
    private readonly int speedHash = Animator.StringToHash("Speed");
    
    void Start()
    {
        // 컴포넌트 초기화
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }
        
        // NavMeshAgent가 NavMesh 위에 있는지 확인
        if (!IsOnNavMesh())
        {
            Debug.LogWarning($"[EnemyAI] {gameObject.name}이 NavMesh 위에 있지 않습니다. NavMesh 위로 이동합니다.");
            MoveToNearestNavMesh();
        }
        
        // Animator 자동 찾기
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // AudioSource 자동 찾기
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        // 플레이어 자동 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        // EnemyHealth 컴포넌트 찾기
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            Debug.LogError("EnemyHealth 컴포넌트를 찾을 수 없습니다!");
        }
        else
        {
            // EnemyHealth 이벤트 구독
            enemyHealth.OnEnemyDeath += OnEnemyDeath;
        }
        
        // NavMeshAgent 설정
        navAgent.speed = moveSpeed;
        navAgent.stoppingDistance = attackRange * 0.8f;
        
        startPosition = transform.position;
        
        // 렌더러 초기화 (피격 피드백용)
        InitializeRenderers();
        
        // 초기 상태 설정
        ChangeState(EnemyState.Idle);
    }
    
    void Update()
    {
        if (enemyHealth != null && enemyHealth.isDead) return;
        
        UpdateAI();
        UpdateAnimations();
    }
    
    void UpdateAI()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdleState(distanceToPlayer);
                break;
                
            case EnemyState.Patrolling:
                HandlePatrollingState(distanceToPlayer);
                break;
                
            case EnemyState.Chasing:
                HandleChasingState(distanceToPlayer);
                break;
                
            case EnemyState.Attacking:
                HandleAttackingState(distanceToPlayer);
                break;
        }
    }
    
    void HandleIdleState(float distanceToPlayer)
    {
        // 플레이어가 감지 범위 내에 있으면 추적 시작
        if (distanceToPlayer <= detectionRange)
        {
            ChangeState(EnemyState.Chasing);
        }
        else
        {
            // 일정 시간 후 순찰 시작
            if (Random.Range(0f, 1f) < 0.01f) // 1% 확률로 순찰 시작
            {
                ChangeState(EnemyState.Patrolling);
            }
        }
    }
    
    void HandlePatrollingState(float distanceToPlayer)
    {
        // 플레이어가 감지 범위 내에 있으면 추적 시작
        if (distanceToPlayer <= detectionRange)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // 목표 지점에 도달했으면 새로운 목표 설정
        if (!navAgent.pathPending && navAgent.remainingDistance < 0.5f)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 5f;
            randomDirection += startPosition;
            randomDirection.y = startPosition.y;
            
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
            {
                if (IsNavMeshAgentValid())
                {
                    navAgent.SetDestination(hit.position);
                }
            }
        }
    }
    
    void HandleChasingState(float distanceToPlayer)
    {
        // 플레이어가 공격 범위 내에 있으면 공격
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // 플레이어가 감지 범위를 벗어나면 순찰로 돌아감
        if (distanceToPlayer > detectionRange * 1.5f)
        {
            ChangeState(EnemyState.Patrolling);
            return;
        }
        
        // 플레이어를 추적
        if (IsNavMeshAgentValid())
        {
            navAgent.SetDestination(player.position);
        }
    }
    
    void HandleAttackingState(float distanceToPlayer)
    {
        // 공격 중에는 이동 정지
        if (IsNavMeshAgentValid())
        {
            navAgent.ResetPath();
        }
        
        // 플레이어를 바라보기
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        
        // 공격 애니메이션이 진행 중이면 기다림
        if (isAttacking)
        {
            // 공격 애니메이션 완료 확인 (약 1초 후)
            if (Time.time - attackStartTime >= 1.0f)
            {
                isAttacking = false;
            }
            return;
        }
        
        // 공격 범위를 벗어나면 추적으로 돌아감
        if (distanceToPlayer > attackRange * 1.2f)
        {
            ChangeState(EnemyState.Chasing);
            return;
        }
        
        // 공격 쿨다운 확인
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Attack();
        }
    }
    
    void ChangeState(EnemyState newState)
    {
        currentState = newState;
        
        switch (newState)
        {
            case EnemyState.Idle:
                if (IsNavMeshAgentValid())
                {
                    navAgent.ResetPath();
                }
                break;
                
            case EnemyState.Patrolling:
                // 순찰 시작
                Vector3 randomDirection = Random.insideUnitSphere * 5f;
                randomDirection += startPosition;
                randomDirection.y = startPosition.y;
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
                {
                    if (IsNavMeshAgentValid())
                    {
                        navAgent.SetDestination(hit.position);
                    }
                }
                break;
                
            case EnemyState.Chasing:
                if (IsNavMeshAgentValid())
                {
                    navAgent.SetDestination(player.position);
                }
                break;
                
            case EnemyState.Attacking:
                if (IsNavMeshAgentValid())
                {
                    navAgent.ResetPath();
                }
                break;
        }
        
        Debug.Log($"적 AI 상태 변경: {newState}");
    }
    
    void Attack()
    {
        lastAttackTime = Time.time;
        attackStartTime = Time.time;
        isAttacking = true;
        
        // 공격 애니메이션
        if (animator != null)
        {
            animator.SetTrigger(attackTriggerHash);
        }
        
        // 공격 사운드
        if (audioSource != null && attackSounds.Length > 0)
        {
            AudioClip randomClip = attackSounds[Random.Range(0, attackSounds.Length)];
            audioSource.PlayOneShot(randomClip);
        }
        
        // 플레이어에게 데미지 주기
        PlayerCombatController playerCombat = player.GetComponent<PlayerCombatController>();
        if (playerCombat != null)
        {
            playerCombat.TakeDamage(attackDamage);
        }
        
        Debug.Log("적이 공격했습니다!");
    }
    
    public void TakeDamage(float damage)
    {
        if (enemyHealth == null || enemyHealth.isDead) return;
        
        // EnemyHealth를 통해 데미지 처리
        enemyHealth.TakeDamage(damage);
        
        // 피해 애니메이션
        if (animator != null)
        {
            animator.SetTrigger(hitTriggerHash);
        }
        
        // 피해 사운드
        if (audioSource != null && hitSounds.Length > 0)
        {
            AudioClip randomClip = hitSounds[Random.Range(0, hitSounds.Length)];
            audioSource.PlayOneShot(randomClip);
        }
        
        // 피격 이펙트 생성
        CreateHitEffect();
        
        // 피격 시 빨간색 깜빡임
        StartCoroutine(HitFlashEffect());
        
        // 피해를 받으면 플레이어를 추적
        if (currentState == EnemyState.Idle || currentState == EnemyState.Patrolling)
        {
            ChangeState(EnemyState.Chasing);
        }
        
        Debug.Log($"적이 {damage} 데미지를 받았습니다! 남은 체력: {enemyHealth.currentHealth}");
    }
    
    public void OnEnemyDeath()
    {
        currentState = EnemyState.Dead;
        
        // 퀘스트 리포팅은 EnemyHealth.Die()에서 처리됨
        
        // 사망 애니메이션
        if (animator != null)
        {
            animator.SetTrigger(deathTriggerHash);
        }
        
        // 사망 사운드
        if (audioSource != null && deathSounds.Length > 0)
        {
            AudioClip randomClip = deathSounds[Random.Range(0, deathSounds.Length)];
            audioSource.PlayOneShot(randomClip);
        }
        
        // NavMeshAgent 비활성화
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
        
        // 콜라이더 비활성화
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        Debug.Log("적이 사망했습니다!");
        
        // 일정 시간 후 GameObject 제거
        Debug.Log("DestroyAfterDelay 코루틴 시작");
        StartCoroutine(DestroyAfterDelay(3f));
        
        // 대안: Invoke 사용
        Invoke(nameof(DestroyEnemy), 3f);
    }
    
    /// <summary>
    /// NavMesh 위에 있는지 확인
    /// </summary>
    private bool IsOnNavMesh()
    {
        if (navAgent == null) return false;
        
        NavMeshHit hit;
        return NavMesh.SamplePosition(transform.position, out hit, 0.1f, NavMesh.AllAreas);
    }
    
    /// <summary>
    /// 가장 가까운 NavMesh 위치로 이동
    /// </summary>
    private void MoveToNearestNavMesh()
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 10f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            Debug.Log($"[EnemyAI] {gameObject.name}을 NavMesh 위로 이동했습니다: {hit.position}");
        }
        else
        {
            Debug.LogError($"[EnemyAI] {gameObject.name} 주변에 NavMesh를 찾을 수 없습니다!");
        }
    }
    
    /// <summary>
    /// NavMeshAgent가 활성화되고 NavMesh 위에 있는지 확인
    /// </summary>
    private bool IsNavMeshAgentValid()
    {
        return navAgent != null && navAgent.enabled && navAgent.isOnNavMesh;
    }
    
    /// <summary>
    /// 일정 시간 후 GameObject 제거
    /// </summary>
    private System.Collections.IEnumerator DestroyAfterDelay(float delay)
    {
        Debug.Log($"DestroyAfterDelay 시작 - {delay}초 대기 중...");
        yield return new WaitForSeconds(delay);
        
        Debug.Log("대기 완료 - GameObject 제거 시도");
        
        // 사망 애니메이션이 완료된 후 GameObject 제거
        if (gameObject != null)
        {
            Debug.Log($"적이 완전히 제거됩니다: {gameObject.name}");
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("GameObject가 이미 null입니다!");
        }
    }
    
    /// <summary>
    /// 적 제거 (Invoke용)
    /// </summary>
    private void DestroyEnemy()
    {
        Debug.Log("Invoke로 적 제거 시도");
        if (gameObject != null)
        {
            Debug.Log($"적이 완전히 제거됩니다 (Invoke): {gameObject.name}");
            Destroy(gameObject);
        }
    }
    
    void OnDestroy()
    {
        // 이벤트 구독 해제
        if (enemyHealth != null)
        {
            enemyHealth.OnEnemyDeath -= OnEnemyDeath;
        }
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // 이동 속도에 따른 애니메이션
        float speed = navAgent.velocity.magnitude;
        animator.SetFloat(speedHash, speed);
        
        // 상태에 따른 애니메이션
        bool isWalking = (currentState == EnemyState.Patrolling || currentState == EnemyState.Chasing) && 
                        (currentState != EnemyState.Attacking) && 
                        (currentState != EnemyState.Dead);
        
        animator.SetBool(isWalkingHash, isWalking);
    }
    
    /// <summary>
    /// 렌더러 초기화 (피격 피드백용)
    /// </summary>
    void InitializeRenderers()
    {
        enemyRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[enemyRenderers.Length];
        
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i].material != null)
            {
                originalColors[i] = enemyRenderers[i].material.color;
            }
        }
    }
    
    /// <summary>
    /// 피격 이펙트 생성
    /// </summary>
    void CreateHitEffect()
    {
        if (hitEffectPrefab != null)
        {
            Vector3 effectPosition = hitEffectPoint != null ? hitEffectPoint.position : transform.position + Vector3.up;
            GameObject effect = Instantiate(hitEffectPrefab, effectPosition, Quaternion.identity);
            
            // 이펙트 자동 삭제 (3초 후)
            Destroy(effect, 3f);
        }
        else
        {
            // 기본 이펙트 (파티클 시스템 없이)
            Debug.Log("피격 이펙트!");
        }
    }
    
    /// <summary>
    /// 피격 시 빨간색 깜빡임 효과
    /// </summary>
    System.Collections.IEnumerator HitFlashEffect()
    {
        if (isHitFlashing || enemyRenderers == null) yield break;
        
        isHitFlashing = true;
        
        // 빨간색으로 변경
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i].material != null)
            {
                enemyRenderers[i].material.color = Color.red;
            }
        }
        
        // 깜빡임 시간만큼 대기
        yield return new WaitForSeconds(hitFlashDuration);
        
        // 원래 색상으로 복원
        for (int i = 0; i < enemyRenderers.Length; i++)
        {
            if (enemyRenderers[i].material != null)
            {
                enemyRenderers[i].material.color = originalColors[i];
            }
        }
        
        isHitFlashing = false;
    }
    
    // 감지 범위 시각화
    void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        
        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
