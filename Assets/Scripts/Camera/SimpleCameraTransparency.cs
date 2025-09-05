using UnityEngine;
using System.Collections.Generic;

public class SimpleCameraTransparency : MonoBehaviour
{
    [Header("투명화 설정")]
    public Transform target; // 플레이어
    public LayerMask transparencyLayers = -1; // 투명화할 레이어
    public float transparencyAlpha = 0.3f; // 투명도
    public float fadeSpeed = 3f; // 투명화 속도
    
    private List<Renderer> transparentObjects = new List<Renderer>();
    private Dictionary<Renderer, Color> originalColors = new Dictionary<Renderer, Color>();
    
    void Start()
    {
        // 자동으로 플레이어 찾기
        if (target == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
        }
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        CheckTransparency();
        UpdateTransparency();
    }
    
    void CheckTransparency()
    {
        // 카메라에서 플레이어로의 방향
        Vector3 direction = (target.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, target.position);
        
        // 레이캐스트 실행 (카메라에서 플레이어 방향으로)
        RaycastHit[] hits = Physics.RaycastAll(transform.position, direction, distance, transparencyLayers);
        
        // 카메라가 오브젝트 안에 있을 때를 위한 추가 검사
        List<Collider> additionalColliders = new List<Collider>();
        if (hits.Length == 0)
        {
            // 카메라 주변의 오브젝트들도 검사
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, 2f, transparencyLayers);
            foreach (Collider collider in nearbyColliders)
            {
                Renderer renderer = collider.GetComponent<Renderer>();
                if (renderer != null && renderer.material != null)
                {
                    // 카메라가 이 오브젝트 안에 있는지 확인
                    if (collider.bounds.Contains(transform.position))
                    {
                        additionalColliders.Add(collider);
                    }
                }
            }
        }
        
        // 새로운 투명화할 오브젝트들
        List<Renderer> newTransparentObjects = new List<Renderer>();
        
        // 레이캐스트로 감지된 오브젝트들
        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                newTransparentObjects.Add(renderer);
                
                // 처음 투명화하는 오브젝트라면 원본 색상 저장
                if (!originalColors.ContainsKey(renderer))
                {
                    originalColors[renderer] = renderer.material.color;
                }
            }
        }
        
        // 추가 검사로 감지된 오브젝트들
        foreach (Collider collider in additionalColliders)
        {
            Renderer renderer = collider.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null && !newTransparentObjects.Contains(renderer))
            {
                newTransparentObjects.Add(renderer);
                
                // 처음 투명화하는 오브젝트라면 원본 색상 저장
                if (!originalColors.ContainsKey(renderer))
                {
                    originalColors[renderer] = renderer.material.color;
                }
            }
        }
        
        // 투명화가 끝난 오브젝트들 복원
        List<Renderer> objectsToRestore = new List<Renderer>();
        foreach (Renderer obj in transparentObjects)
        {
            if (!newTransparentObjects.Contains(obj))
            {
                objectsToRestore.Add(obj);
            }
        }
        
        // 투명화 복원
        foreach (Renderer obj in objectsToRestore)
        {
            RestoreObject(obj);
        }
        
        // 새로운 투명화 대상 업데이트
        transparentObjects = newTransparentObjects;
        
    }
    
    void UpdateTransparency()
    {
        foreach (Renderer renderer in transparentObjects)
        {
            if (renderer != null && renderer.material != null)
            {
                // 투명도 점진적 적용
                Color color = renderer.material.color;
                color.a = Mathf.Lerp(color.a, transparencyAlpha, Time.deltaTime * fadeSpeed);
                renderer.material.color = color;
                
                // URP에서 투명화를 위한 머티리얼 설정
                renderer.material.SetFloat("_Surface", 1); // Transparent
                renderer.material.SetFloat("_Blend", 0); // Alpha
                renderer.material.SetFloat("_AlphaClip", 0);
                renderer.material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                renderer.material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                renderer.material.SetFloat("_ZWrite", 0);
                renderer.material.renderQueue = 3000;
                
            }
        }
    }
    
    void RestoreObject(Renderer renderer)
    {
        if (renderer != null && originalColors.ContainsKey(renderer))
        {
            // 원본 색상으로 복원
            Color originalColor = originalColors[renderer];
            originalColor.a = 1f; // 완전 불투명으로 복원
            renderer.material.color = originalColor;
            
            // URP 머티리얼 설정 복원
            renderer.material.SetFloat("_Surface", 0); // Opaque
            renderer.material.SetFloat("_Blend", 0);
            renderer.material.SetFloat("_AlphaClip", 0);
            renderer.material.SetFloat("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            renderer.material.SetFloat("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            renderer.material.SetFloat("_ZWrite", 1);
            renderer.material.renderQueue = 2000;
            
        }
    }
    
    void OnDestroy()
    {
        // 게임 종료 시 모든 오브젝트 복원
        foreach (Renderer renderer in originalColors.Keys)
        {
            if (renderer != null)
            {
                RestoreObject(renderer);
            }
        }
    }
    
    // 디버그용 레이캐스트 시각화
    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            Gizmos.color = Color.red;
            Vector3 direction = (target.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, target.position);
            Gizmos.DrawRay(transform.position, direction * distance);
        }
    }
}
