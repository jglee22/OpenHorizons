using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("적 설정")]
    public float maxHealth = 50f;
    public float currentHealth;
    public float attackDamage = 15f;
    public float attackRange = 1.5f;
    public float detectionRange = 8f;
    public float attackCooldown = 2f;
    public float moveSpeed = 3f;
    
    [Header("AI 설정")]
    public Transform player;
    public LayerMask playerLayer = 1 << 6; // Player 레이어
    public float rotationSpeed = 5f;
    
    [Header("애니메이션")]
    public Animator animator;
    
    [Header("사운드")]
    public AudioSource audioSource;
    public AudioClip[] attackSounds;
    public AudioClip[] hitSounds;
    public AudioClip[] deathSounds;
    
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
    private float lastAttackTime = 0f;
    private Vector3 startPosition;
    private bool isDead = false;
    
    // 애니메이션 해시
    private readonly int isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int isRunningHash = Animator.StringToHash("IsRunning");
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
        
        // NavMeshAgent 설정
        navAgent.speed = moveSpeed;
        navAgent.stoppingDistance = attackRange * 0.8f;
        
        // 체력 초기화
        currentHealth = maxHealth;
        startPosition = transform.position;
        
        // 초기 상태 설정
        ChangeState(EnemyState.Idle);
    }
    
    void Update()
    {
        if (isDead) return;
        
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
                navAgent.SetDestination(hit.position);
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
        navAgent.SetDestination(player.position);
    }
    
    void HandleAttackingState(float distanceToPlayer)
    {
        // 플레이어를 바라보기
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
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
                navAgent.ResetPath();
                break;
                
            case EnemyState.Patrolling:
                // 순찰 시작
                Vector3 randomDirection = Random.insideUnitSphere * 5f;
                randomDirection += startPosition;
                randomDirection.y = startPosition.y;
                
                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomDirection, out hit, 5f, NavMesh.AllAreas))
                {
                    navAgent.SetDestination(hit.position);
                }
                break;
                
            case EnemyState.Chasing:
                navAgent.SetDestination(player.position);
                break;
                
            case EnemyState.Attacking:
                navAgent.ResetPath();
                break;
        }
        
        Debug.Log($"적 AI 상태 변경: {newState}");
    }
    
    void Attack()
    {
        lastAttackTime = Time.time;
        
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
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
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
        
        // 피해를 받으면 플레이어를 추적
        if (currentState == EnemyState.Idle || currentState == EnemyState.Patrolling)
        {
            ChangeState(EnemyState.Chasing);
        }
        
        // 사망 확인
        if (currentHealth <= 0)
        {
            Die();
        }
        
        Debug.Log($"적이 {damage} 데미지를 받았습니다! 남은 체력: {currentHealth}");
    }
    
    void Die()
    {
        isDead = true;
        currentState = EnemyState.Dead;
        
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
        
        // 5초 후 오브젝트 파괴
        Destroy(gameObject, 5f);
        
        Debug.Log("적이 사망했습니다!");
    }
    
    void UpdateAnimations()
    {
        if (animator == null) return;
        
        // 이동 속도에 따른 애니메이션
        float speed = navAgent.velocity.magnitude;
        animator.SetFloat(speedHash, speed);
        
        // 상태에 따른 애니메이션
        bool isWalking = currentState == EnemyState.Patrolling;
        bool isRunning = currentState == EnemyState.Chasing;
        
        animator.SetBool(isWalkingHash, isWalking);
        animator.SetBool(isRunningHash, isRunning);
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
