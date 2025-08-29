using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상호작용 프롬프트 UI 관리
/// </summary>
public class InteractionPromptUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI promptText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Image iconImage;
    
    [Header("애니메이션 설정")]
    [SerializeField] private float fadeInDuration = 0.2f;
    [SerializeField] private float fadeOutDuration = 0.1f;
    [SerializeField] private bool useScaleAnimation = true;
    [SerializeField] private float scaleAnimationDuration = 0.2f;
    
    [Header("스타일 설정")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private Sprite defaultIcon;
    
    private RectTransform rectTransform;
    private Vector3 originalScale;
    private bool isVisible = false;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalScale = rectTransform.localScale;
        
        // 컴포넌트 자동 할당
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (promptText == null) promptText = GetComponentInChildren<TextMeshProUGUI>();
        if (backgroundImage == null) backgroundImage = GetComponent<Image>();
        if (iconImage == null) iconImage = GetComponentInChildren<Image>();
        
        // 초기 상태 설정
        SetVisible(false, false);
    }
    
    /// <summary>
    /// 프롬프트 표시
    /// </summary>
    public void ShowPrompt(string text, Sprite icon = null)
    {
        if (promptText != null)
        {
            promptText.text = text;
        }
        
        if (iconImage != null)
        {
            iconImage.sprite = icon != null ? icon : defaultIcon;
            iconImage.gameObject.SetActive(iconImage.sprite != null);
        }
        
        SetVisible(true, true);
    }
    
    /// <summary>
    /// 프롬프트 숨기기
    /// </summary>
    public void HidePrompt()
    {
        SetVisible(false, true);
    }
    
    /// <summary>
    /// 가시성 설정
    /// </summary>
    private void SetVisible(bool visible, bool animate)
    {
        if (isVisible == visible) return;
        
        isVisible = visible;
        
        if (animate)
        {
            if (visible)
            {
                ShowWithAnimation();
            }
            else
            {
                HideWithAnimation();
            }
        }
        else
        {
            SetImmediateVisibility(visible);
        }
    }
    
    /// <summary>
    /// 애니메이션과 함께 표시
    /// </summary>
    private void ShowWithAnimation()
    {
        // 페이드 인
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }
        
        // 스케일 애니메이션
        if (useScaleAnimation)
        {
            rectTransform.localScale = Vector3.zero;
            StartCoroutine(ScaleIn());
        }
    }
    
    /// <summary>
    /// 애니메이션과 함께 숨기기
    /// </summary>
    private void HideWithAnimation()
    {
        // 페이드 아웃
        if (canvasGroup != null)
        {
            StartCoroutine(FadeOut());
        }
        
        // 스케일 애니메이션
        if (useScaleAnimation)
        {
            StartCoroutine(ScaleOut());
        }
    }
    
    /// <summary>
    /// 즉시 가시성 설정
    /// </summary>
    private void SetImmediateVisibility(bool visible)
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
        }
        
        if (useScaleAnimation)
        {
            rectTransform.localScale = visible ? originalScale : Vector3.zero;
        }
    }
    
    /// <summary>
    /// 프롬프트 텍스트 업데이트
    /// </summary>
    public void UpdatePromptText(string newText)
    {
        if (promptText != null)
        {
            promptText.text = newText;
        }
    }
    
    /// <summary>
    /// 아이콘 업데이트
    /// </summary>
    public void UpdateIcon(Sprite newIcon)
    {
        if (iconImage != null)
        {
            iconImage.sprite = newIcon != null ? newIcon : defaultIcon;
            iconImage.gameObject.SetActive(iconImage.sprite != null);
        }
    }
    
    /// <summary>
    /// 색상 테마 설정
    /// </summary>
    public void SetColorTheme(Color background, Color text)
    {
        if (backgroundImage != null)
        {
            backgroundColor = background;
            backgroundImage.color = backgroundColor;
        }
        
        if (promptText != null)
        {
            textColor = text;
            promptText.color = textColor;
        }
    }
    
    /// <summary>
    /// 페이드 인 코루틴
    /// </summary>
    private System.Collections.IEnumerator FadeIn()
    {
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            }
            yield return null;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }
    
    /// <summary>
    /// 페이드 아웃 코루틴
    /// </summary>
    private System.Collections.IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            if (canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            }
            yield return null;
        }
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
    }
    
    /// <summary>
    /// 스케일 인 코루틴
    /// </summary>
    private System.Collections.IEnumerator ScaleIn()
    {
        float elapsed = 0f;
        while (elapsed < scaleAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scaleAnimationDuration;
            float scale = Mathf.Lerp(0f, 1f, progress);
            rectTransform.localScale = originalScale * scale;
            yield return null;
        }
        rectTransform.localScale = originalScale;
    }
    
    /// <summary>
    /// 스케일 아웃 코루틴
    /// </summary>
    private System.Collections.IEnumerator ScaleOut()
    {
        float elapsed = 0f;
        while (elapsed < scaleAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / scaleAnimationDuration;
            float scale = Mathf.Lerp(1f, 0f, progress);
            rectTransform.localScale = originalScale * scale;
            yield return null;
        }
        rectTransform.localScale = Vector3.zero;
    }
    
    private void OnDestroy()
    {
        // 코루틴 정리
        StopAllCoroutines();
    }
}
