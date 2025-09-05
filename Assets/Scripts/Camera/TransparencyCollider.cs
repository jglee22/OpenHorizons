using UnityEngine;

public class TransparencyCollider : MonoBehaviour
{
    [Header("투명화 콜라이더 설정")]
    public LayerMask transparencyLayer = 8; // Transparency 레이어 (8번)
    public bool createOnStart = true;
    
    void Start()
    {
        if (createOnStart)
        {
            CreateTransparencyCollider();
        }
    }
    
    [ContextMenu("투명화 콜라이더 생성")]
    public void CreateTransparencyCollider()
    {
        // 기존 투명화 콜라이더가 있는지 확인
        Transform existingCollider = transform.Find("TransparencyCollider");
        if (existingCollider != null)
        {
            Debug.Log("이미 투명화 콜라이더가 존재합니다.");
            return;
        }
        
        // 새로운 투명화 콜라이더 생성
        GameObject transparencyCollider = new GameObject("TransparencyCollider");
        transparencyCollider.transform.SetParent(transform);
        transparencyCollider.transform.localPosition = Vector3.zero;
        transparencyCollider.transform.localRotation = Quaternion.identity;
        transparencyCollider.transform.localScale = Vector3.one;
        
        // 레이어 설정
        transparencyCollider.layer = (int)Mathf.Log(transparencyLayer.value, 2);
        
        // 콜라이더 복사
        Collider originalCollider = GetComponent<Collider>();
        if (originalCollider != null)
        {
            if (originalCollider is BoxCollider)
            {
                BoxCollider boxCollider = transparencyCollider.AddComponent<BoxCollider>();
                BoxCollider originalBox = (BoxCollider)originalCollider;
                boxCollider.center = originalBox.center;
                boxCollider.size = originalBox.size;
                boxCollider.isTrigger = true; // 트리거로 설정
            }
            else if (originalCollider is SphereCollider)
            {
                SphereCollider sphereCollider = transparencyCollider.AddComponent<SphereCollider>();
                SphereCollider originalSphere = (SphereCollider)originalCollider;
                sphereCollider.center = originalSphere.center;
                sphereCollider.radius = originalSphere.radius;
                sphereCollider.isTrigger = true; // 트리거로 설정
            }
            else if (originalCollider is CapsuleCollider)
            {
                CapsuleCollider capsuleCollider = transparencyCollider.AddComponent<CapsuleCollider>();
                CapsuleCollider originalCapsule = (CapsuleCollider)originalCollider;
                capsuleCollider.center = originalCapsule.center;
                capsuleCollider.radius = originalCapsule.radius;
                capsuleCollider.height = originalCapsule.height;
                capsuleCollider.direction = originalCapsule.direction;
                capsuleCollider.isTrigger = true; // 트리거로 설정
            }
        }
        
        Debug.Log($"투명화 콜라이더를 생성했습니다: {transparencyCollider.name}");
    }
    
    [ContextMenu("투명화 콜라이더 제거")]
    public void RemoveTransparencyCollider()
    {
        Transform transparencyCollider = transform.Find("TransparencyCollider");
        if (transparencyCollider != null)
        {
            DestroyImmediate(transparencyCollider.gameObject);
            Debug.Log("투명화 콜라이더를 제거했습니다.");
        }
        else
        {
            Debug.Log("투명화 콜라이더가 존재하지 않습니다.");
        }
    }
}
