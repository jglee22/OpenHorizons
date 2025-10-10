using UnityEngine;
using UnityEngine.UI;

// 조이스틱/버튼 입력을 PlayerController의 외부 입력 필드로 전달하는 어댑터
public class MobileInputAdapter : MonoBehaviour
{
    [Header("Targets")]
    public PlayerController player;

    [Header("Joystick (Joystick Pack)")]
    // Joystick Pack의 공통 Joystick 컴포넌트 (Fixed/Dynamic/Floating/Variable 모두 동일 인터페이스)
    public Joystick joystick;

    [Header("Buttons")]
    public Button jumpButton;      // 점프 버튼
    public Button attackButton;   // 공격 버튼
    public Toggle runToggle;       // 달리기 토글(선택)

    private void Awake()
    {
        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpClicked);
        }
        
        if (attackButton != null)
        {
            attackButton.onClick.AddListener(OnAttackClicked);
        }
    }

    private void Update()
    {
        if (player == null || !player.useExternalInput) return;

        // 조이스틱 방향 입력 주입
        Vector2 move = Vector2.zero;
        if (joystick != null)
        {
            move.x = joystick.Horizontal;
            move.y = joystick.Vertical;
        }
        player.externalMoveInput = move;

        // 달리기 입력 주입(토글 기반, 미지정 시 false)
        player.externalRunPressed = runToggle != null && runToggle.isOn;
    }

    private void OnJumpClicked()
    {
        if (player == null || !player.useExternalInput) return;
        // 한 프레임 소비되도록 PlayerController에서 처리
        player.externalJumpPressed = true;
    }
    
    private void OnAttackClicked()
    {
        // 공격은 PlayerCombatController에서 처리
        // 마우스 클릭을 시뮬레이션
        if (player != null && player.useExternalInput)
        {
            // 공격 입력을 직접 처리하거나 이벤트로 전달
            var combatController = player.GetComponent<PlayerCombatController>();
            if (combatController != null && combatController.CanAttack())
            {
                combatController.PerformComboAttack();
            }
        }
    }
}


