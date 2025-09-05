using UnityEngine;

public class AnimationDebugger : MonoBehaviour
{
    [Header("디버그 설정")]
    public bool showDebugInfo = true;
    public KeyCode debugToggleKey = KeyCode.F1;
    
    private Animator animator;
    private PlayerController playerController;
    
    void Start()
    {
        // 자식 오브젝트에서 Animator 찾기
        animator = GetComponentInChildren<Animator>();
        playerController = GetComponent<PlayerController>();
        
        if (animator == null)
        {
            Debug.LogError("자식 오브젝트에서 Animator를 찾을 수 없습니다! Rio 오브젝트에 Animator가 있는지 확인해주세요.");
        }
        else
        {
            Debug.Log($"Animator를 찾았습니다: {animator.name}");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(debugToggleKey))
        {
            showDebugInfo = !showDebugInfo;
        }
        
        if (showDebugInfo)
        {
            DebugAnimationInfo();
        }
    }
    
    void DebugAnimationInfo()
    {
        if (animator == null)
        {
            Debug.LogError("Animator가 없습니다!");
            return;
        }
        
        // 현재 애니메이션 상태 정보
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        string currentStateName = GetStateName(stateInfo.shortNameHash);
        
        Debug.Log($"=== 애니메이션 디버그 정보 ===");
        Debug.Log($"현재 상태: {currentStateName}");
        Debug.Log($"정규화된 시간: {stateInfo.normalizedTime:F2}");
        Debug.Log($"애니메이션 길이: {stateInfo.length:F2}초");
        Debug.Log($"애니메이션 속도: {stateInfo.speed:F2}");
        
        // 파라미터 값들
        Debug.Log($"=== 파라미터 값들 ===");
        Debug.Log($"Speed: {animator.GetFloat("Speed"):F2}");
        Debug.Log($"IsWalking: {animator.GetBool("IsWalking")}");
        Debug.Log($"IsRunning: {animator.GetBool("IsRunning")}");
        Debug.Log($"IsJumping: {animator.GetBool("IsJumping")}");
        Debug.Log($"IsGrounded: {animator.GetBool("IsGrounded")}");
        Debug.Log($"IsBlocking: {animator.GetBool("IsBlocking")}");
        Debug.Log($"IsAttacking: {animator.GetBool("IsAttacking")}");
        
        // 트리거 상태
        Debug.Log($"=== 트리거 상태 ===");
        Debug.Log($"Jump: {animator.GetBool("Jump")}");
        Debug.Log($"Attack: {animator.GetBool("Attack")}");
        Debug.Log($"Block: {animator.GetBool("Block")}");
        Debug.Log($"Hit: {animator.GetBool("Hit")}");
        Debug.Log($"Death: {animator.GetBool("Death")}");
        
        // Animator Controller 정보
        if (animator.runtimeAnimatorController != null)
        {
            Debug.Log($"=== Animator Controller ===");
            Debug.Log($"Controller: {animator.runtimeAnimatorController.name}");
            Debug.Log($"Layer Count: {animator.layerCount}");
        }
        else
        {
            Debug.LogError("Animator Controller가 없습니다!");
        }
        
        Debug.Log("================================");
    }
    
    string GetStateName(int stateHash)
    {
        // 일반적인 상태 해시들
        switch (stateHash)
        {
            case 0: return "Empty";
            case 2081823275: return "HumanF@Idle01";
            case 2081823276: return "HumanF@Walk01_Forward";
            case 2081823277: return "HumanF@Run01_Forward";
            case 2081823278: return "HumanF@Jump01";
            case 2081823279: return "Attack1";
            case 2081823280: return "Attack2";
            case 2081823281: return "Attack3";
            case 2081823282: return "Block";
            case 2081823283: return "Hit";
            case 2081823284: return "Death";
            default: return $"Unknown State ({stateHash})";
        }
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));
        GUILayout.Label("=== 애니메이션 디버그 ===", GUI.skin.label);
        
        if (animator != null)
        {
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            GUILayout.Label($"현재 상태: {GetStateName(stateInfo.shortNameHash)}");
            GUILayout.Label($"정규화된 시간: {stateInfo.normalizedTime:F2}");
            GUILayout.Label($"애니메이션 길이: {stateInfo.length:F2}초");
            
            GUILayout.Space(10);
            GUILayout.Label("=== 파라미터 ===");
            GUILayout.Label($"Speed: {animator.GetFloat("Speed"):F2}");
            GUILayout.Label($"IsWalking: {animator.GetBool("IsWalking")}");
            GUILayout.Label($"IsRunning: {animator.GetBool("IsRunning")}");
            GUILayout.Label($"IsBlocking: {animator.GetBool("IsBlocking")}");
            GUILayout.Label($"IsAttacking: {animator.GetBool("IsAttacking")}");
        }
        else
        {
            GUILayout.Label("Animator가 없습니다!");
        }
        
        GUILayout.Space(10);
        GUILayout.Label("=== 조작법 ===");
        GUILayout.Label("마우스 왼쪽: 공격");
        GUILayout.Label("마우스 오른쪽: 방어");
        GUILayout.Label("T키: 피해 애니메이션");
        GUILayout.Label("Y키: 사망 애니메이션");
        GUILayout.Label("F1키: 디버그 토글");
        
        GUILayout.EndArea();
    }
}
