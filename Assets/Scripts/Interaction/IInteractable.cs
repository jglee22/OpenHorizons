using UnityEngine;

/// <summary>
/// 상호작용 가능한 모든 오브젝트가 구현해야 하는 인터페이스
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 상호작용 가능한지 확인
    /// </summary>
    bool CanInteract { get; }
    
    /// <summary>
    /// 상호작용 실행
    /// </summary>
    void Interact();
    
    /// <summary>
    /// 상호작용 프롬프트 텍스트 반환
    /// </summary>
    string GetInteractionPrompt();
    
    /// <summary>
    /// 상호작용 범위 내에 있는지 확인
    /// </summary>
    bool IsInRange(Vector3 playerPosition);
}
