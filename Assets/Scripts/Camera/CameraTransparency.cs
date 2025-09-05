using UnityEngine;
using System.Collections.Generic;

public class CameraTransparency : MonoBehaviour
{
    [Header("투명화 설정")]
    public Transform target; // 플레이어
    public LayerMask transparencyLayers = -1; // 투명화할 레이어
    public float transparencyAlpha = 0.3f; // 투명도 (0=완전투명, 1=완전불투명)
    public float fadeSpeed = 5f; // 투명화 속도
    
    [Header("레이캐스트 설정")]
    public float raycastDistance = 10f; // 레이캐스트 거리
    public Vector3 raycastOffset = Vector3.zero; // 레이캐스트 시작점 오프셋
    
    private Camera cam;
    private List<Renderer> transparentObjects = new List<Renderer>();
    private Dictionary<Renderer, Material> originalMaterials = new Dictionary<Renderer, Material>();
    
    void Start()
    {
        cam = GetComponent<Camera>();
        
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
        
        // 레이캐스트 시작점 (카메라 위치 + 오프셋)
        Vector3 raycastStart = transform.position + raycastOffset;
        
        // 레이캐스트 실행
        RaycastHit[] hits = Physics.RaycastAll(raycastStart, direction, distance, transparencyLayers);
        
        // 새로운 투명화할 오브젝트들
        List<Renderer> newTransparentObjects = new List<Renderer>();
        
        foreach (RaycastHit hit in hits)
        {
            Renderer renderer = hit.collider.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                newTransparentObjects.Add(renderer);
                
                // 처음 투명화하는 오브젝트라면 원본 머티리얼 저장
                if (!originalMaterials.ContainsKey(renderer))
                {
                    originalMaterials[renderer] = new Material(renderer.material);
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
                float targetAlpha = transparencyAlpha;
                color.a = Mathf.Lerp(color.a, targetAlpha, Time.deltaTime * fadeSpeed);
                renderer.material.color = color;
                
                // 투명화를 위한 머티리얼 설정
                renderer.material.SetFloat("_Mode", 3); // Transparent 모드
                renderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                renderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                renderer.material.SetInt("_ZWrite", 0);
                renderer.material.DisableKeyword("_ALPHATEST_ON");
                renderer.material.EnableKeyword("_ALPHABLEND_ON");
                renderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                renderer.material.renderQueue = 3000;
            }
        }
    }
    
    void RestoreObject(Renderer renderer)
    {
        if (renderer != null && originalMaterials.ContainsKey(renderer))
        {
            // 원본 머티리얼로 복원
            renderer.material = originalMaterials[renderer];
        }
    }
    
    void OnDestroy()
    {
        // 게임 종료 시 모든 오브젝트 복원
        foreach (Renderer renderer in originalMaterials.Keys)
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
            Vector3 raycastStart = transform.position + raycastOffset;
            Vector3 direction = (target.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, target.position);
            Gizmos.DrawRay(raycastStart, direction * distance);
        }
    }
}
