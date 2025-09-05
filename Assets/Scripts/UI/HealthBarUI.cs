using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("UI 요소")]
    public Slider healthSlider;
    public Image healthFill;
    public Text healthText;
    
    [Header("색상 설정")]
    public Color healthyColor = Color.green;
    public Color warningColor = Color.yellow;
    public Color dangerColor = Color.red;
    
    [Header("애니메이션 설정")]
    public float smoothSpeed = 5f;
    public bool showHealthText = true;
    
    private float targetHealth;
    private float currentDisplayHealth;
    
    void Start()
    {
        // UI 요소 자동 찾기
        if (healthSlider == null)
        {
            healthSlider = GetComponent<Slider>();
        }
        
        if (healthFill == null && healthSlider != null)
        {
            healthFill = healthSlider.fillRect.GetComponent<Image>();
        }
        
        if (healthText == null)
        {
            healthText = GetComponentInChildren<Text>();
        }
        
        // 초기 설정
        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = 100f;
            healthSlider.value = 100f;
        }
        
        currentDisplayHealth = 100f;
        targetHealth = 100f;
    }
    
    void Update()
    {
        // 부드러운 체력 바 애니메이션
        if (Mathf.Abs(currentDisplayHealth - targetHealth) > 0.1f)
        {
            currentDisplayHealth = Mathf.Lerp(currentDisplayHealth, targetHealth, smoothSpeed * Time.deltaTime);
            UpdateHealthBar();
        }
    }
    
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        if (maxHealth <= 0) return;
        
        targetHealth = (currentHealth / maxHealth) * 100f;
        
        // 체력 텍스트 업데이트
        if (healthText != null && showHealthText)
        {
            healthText.text = $"{Mathf.RoundToInt(currentHealth)} / {Mathf.RoundToInt(maxHealth)}";
        }
    }
    
    void UpdateHealthBar()
    {
        if (healthSlider == null) return;
        
        healthSlider.value = currentDisplayHealth;
        
        // 체력에 따른 색상 변경
        if (healthFill != null)
        {
            float healthPercentage = currentDisplayHealth / 100f;
            
            if (healthPercentage > 0.6f)
            {
                healthFill.color = healthyColor;
            }
            else if (healthPercentage > 0.3f)
            {
                healthFill.color = warningColor;
            }
            else
            {
                healthFill.color = dangerColor;
            }
        }
    }
    
    // 체력 바 깜빡임 효과 (피해를 받았을 때)
    public void FlashHealthBar()
    {
        if (healthFill != null)
        {
            StartCoroutine(FlashCoroutine());
        }
    }
    
    private System.Collections.IEnumerator FlashCoroutine()
    {
        Color originalColor = healthFill.color;
        Color flashColor = Color.white;
        
        for (int i = 0; i < 3; i++)
        {
            healthFill.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            healthFill.color = originalColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // 체력 바 숨기기/보이기
    public void SetHealthBarVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
