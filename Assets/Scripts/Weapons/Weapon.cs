using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("무기 설정")]
    public string weaponName = "Sword";
    public float damage = 25f;
    public float range = 2f;
    public float attackSpeed = 1f;
    public float durability = 100f;
    public float maxDurability = 100f;
    
    [Header("무기 타입")]
    public WeaponType weaponType = WeaponType.Sword;
    
    [Header("애니메이션")]
    public string attackAnimationName = "Attack";
    public string blockAnimationName = "Block";
    
    [Header("사운드")]
    public AudioSource audioSource;
    public AudioClip[] attackSounds;
    public AudioClip[] blockSounds;
    
    [Header("이펙트")]
    public ParticleSystem attackEffect;
    public GameObject hitEffect;
    
    // 무기 상태
    private bool isEquipped = false;
    private PlayerCombatController owner;
    
    public enum WeaponType
    {
        Sword,
        Axe,
        Mace,
        Bow,
        Staff,
        Shield
    }
    
    void Start()
    {
        // AudioSource 자동 찾기
        EnsureAudioSource();
        
        // 초기 내구도 설정
        durability = maxDurability;
    }
    
    // AudioSource 안전하게 초기화
    void EnsureAudioSource()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }
    
    public void Equip(PlayerCombatController player)
    {
        owner = player;
        isEquipped = true;
        
        // 무기를 플레이어의 무기 홀더에 부착
        if (player.weaponHolder != null)
        {
            transform.SetParent(player.weaponHolder);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        
        // 무기 활성화
        gameObject.SetActive(true);
        
        Debug.Log($"{weaponName}이 장착되었습니다!");
    }
    
    public void Unequip()
    {
        isEquipped = false;
        owner = null;
        
        // 무기 비활성화
        gameObject.SetActive(false);
        
        Debug.Log($"{weaponName}이 해제되었습니다!");
    }
    
    public void Attack()
    {
        if (!isEquipped || owner == null) return;
        
        // AudioSource 안전하게 초기화
        EnsureAudioSource();
        
        // 내구도 감소
        durability -= 1f;
        durability = Mathf.Max(0, durability);
        
        // 공격 사운드 재생
        if (audioSource != null && attackSounds != null && attackSounds.Length > 0)
        {
            AudioClip randomClip = attackSounds[Random.Range(0, attackSounds.Length)];
            audioSource.PlayOneShot(randomClip);
        }
        
        // 공격 이펙트 재생
        if (attackEffect != null)
        {
            attackEffect.Play();
        }
        
        // 공격 범위 내 적 찾기
        DetectAndDamageEnemies();
        
        // 내구도가 0이 되면 무기 파괴
        if (durability <= 0)
        {
            DestroyWeapon();
        }
        
        Debug.Log($"{weaponName}으로 공격했습니다! 내구도: {durability}/{maxDurability}");
    }
    
    public void Block()
    {
        if (!isEquipped || owner == null) return;
        
        // AudioSource 안전하게 초기화
        EnsureAudioSource();
        
        // 방어 사운드 재생
        if (audioSource != null && blockSounds != null && blockSounds.Length > 0)
        {
            AudioClip randomClip = blockSounds[Random.Range(0, blockSounds.Length)];
            audioSource.PlayOneShot(randomClip);
        }
        
        Debug.Log($"{weaponName}으로 방어했습니다!");
    }
    
    void DetectAndDamageEnemies()
    {
        if (owner == null) return;
        
        // 공격 범위 내 콜라이더 찾기
        Collider[] enemies = Physics.OverlapSphere(transform.position, range, owner.enemyLayer);
        
        foreach (Collider enemy in enemies)
        {
            // 적에게 데미지 주기
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.TakeDamage(damage);
                
                // 히트 이펙트 생성
                if (hitEffect != null)
                {
                    GameObject effect = Instantiate(hitEffect, enemy.transform.position, Quaternion.identity);
                    Destroy(effect, 2f);
                }
            }
        }
    }
    
    void DestroyWeapon()
    {
        Debug.Log($"{weaponName}이 파괴되었습니다!");
        
        // 무기 해제
        if (owner != null)
        {
            owner.UnequipWeapon();
        }
        
        // 무기 파괴
        Destroy(gameObject);
    }
    
    // 내구도 수리
    public void Repair(float amount)
    {
        durability = Mathf.Min(maxDurability, durability + amount);
        Debug.Log($"{weaponName}을 수리했습니다! 내구도: {durability}/{maxDurability}");
    }
    
    // 무기 정보 가져오기
    public string GetWeaponInfo()
    {
        return $"{weaponName}\n데미지: {damage}\n범위: {range}\n내구도: {durability}/{maxDurability}";
    }
    
    // 공격 범위 시각화
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
