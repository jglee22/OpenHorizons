using UnityEngine;
using System;

public class EnemyHealth : MonoBehaviour
{
    [Header("적 체력 설정")]
    public float maxHealth = 50f;
    public float currentHealth;
    
    [Header("사망 설정")]
    public bool isDead = false;
    public float deathDelay = 2f;
    
    [Header("이벤트")]
    public Action<float, float> OnHealthChanged; // (현재체력, 최대체력)
    public Action OnEnemyDeath;
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    // 데미지 받기
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        
        // 체력 변경 이벤트 발생
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        
        Debug.Log($"적이 {damage} 데미지를 받았습니다! 남은 체력: {currentHealth}");
        
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // 사망 처리
    void Die()
    {
        if (isDead) return;
        
        isDead = true;
        
        // 사망 이벤트 발생
        OnEnemyDeath?.Invoke();
        
        Debug.Log("적이 사망했습니다!");
        
        // 사망 애니메이션 재생 (있다면)
        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // 일정 시간 후 오브젝트 비활성화
        Invoke(nameof(DisableEnemy), deathDelay);
    }
    
    // 적 비활성화
    void DisableEnemy()
    {
        gameObject.SetActive(false);
    }
    
    // 체력 회복
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"적이 {amount} 체력을 회복했습니다! 현재 체력: {currentHealth}");
    }
    
    // 체력 비율 가져오기
    public float GetHealthPercentage()
    {
        return currentHealth / maxHealth;
    }
}
