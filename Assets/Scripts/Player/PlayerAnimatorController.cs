using UnityEngine;
using UnityEditor;
using System.IO;

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("Animation Clips")]
    public AnimationClip idleAnimation;
    public AnimationClip walkAnimation;
    public AnimationClip runAnimation;
    public AnimationClip jumpAnimation;
    public AnimationClip fallAnimation;
    
    [Header("Animator Settings")]
    public RuntimeAnimatorController animatorController;
    
    private Animator animator;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        
        // Animator Controller가 없으면 자동 생성
        if (animatorController == null)
        {
            CreateAnimatorController();
        }
        
        if (animator != null && animatorController != null)
        {
            animator.runtimeAnimatorController = animatorController;
        }
    }
    
    [ContextMenu("Create Animator Controller")]
    public void CreateAnimatorController()
    {
#if UNITY_EDITOR
        // Animations 폴더 생성
        string animationsPath = "Assets/Animations";
        if (!Directory.Exists(animationsPath))
        {
            Directory.CreateDirectory(animationsPath);
        }
        
        // Animator Controller 생성
        string controllerPath = animationsPath + "/PlayerAnimatorController.controller";
        UnityEditor.Animations.AnimatorController controller = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
        
        // 파라미터 추가
        controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
        controller.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
        
        // 기본 상태들 생성
        var idleState = controller.layers[0].stateMachine.AddState("Idle");
        var walkState = controller.layers[0].stateMachine.AddState("Walk");
        var runState = controller.layers[0].stateMachine.AddState("Run");
        var jumpState = controller.layers[0].stateMachine.AddState("Jump");
        var fallState = controller.layers[0].stateMachine.AddState("Fall");
        
        // 기본 상태를 Idle로 설정
        controller.layers[0].stateMachine.defaultState = idleState;
        
        // 전환 조건 설정
        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "Speed");
        
        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "Speed");
        
        var walkToRun = walkState.AddTransition(runState);
        walkToRun.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 5f, "Speed");
        
        var runToWalk = runState.AddTransition(walkState);
        runToWalk.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 5f, "Speed");
        
        // 점프 전환
        var anyToJump = controller.layers[0].stateMachine.AddAnyStateTransition(jumpState);
        anyToJump.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "Jump");
        
        var jumpToFall = jumpState.AddTransition(fallState);
        jumpToFall.hasExitTime = true;
        jumpToFall.exitTime = 0.5f;
        
        var fallToIdle = fallState.AddTransition(idleState);
        fallToIdle.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, "IsGrounded");
        
        // Animator Controller 저장
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        
        animatorController = controller;
        
        Debug.Log("Animator Controller가 생성되었습니다: " + controllerPath);
#endif
    }
    
    [ContextMenu("Load Default Animations")]
    public void LoadDefaultAnimations()
    {
#if UNITY_EDITOR
        // Kevin Iglesias 애니메이션에서 기본 애니메이션들 로드
        string basePath = "Assets/Kevin Iglesias/Human Animations/Animations/Male/Movement";
        
        // Idle 애니메이션 찾기
        string[] idleFiles = Directory.GetFiles(basePath, "*Idle*.fbx", SearchOption.AllDirectories);
        if (idleFiles.Length > 0)
        {
            idleAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(idleFiles[0]);
        }
        
        // Walk 애니메이션 찾기
        string[] walkFiles = Directory.GetFiles(basePath + "/Walk", "*.fbx");
        if (walkFiles.Length > 0)
        {
            walkAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(walkFiles[0]);
        }
        
        // Run 애니메이션 찾기
        string[] runFiles = Directory.GetFiles(basePath + "/Run", "*.fbx");
        if (runFiles.Length > 0)
        {
            runAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(runFiles[0]);
        }
        
        // Jump 애니메이션 찾기
        string[] jumpFiles = Directory.GetFiles(basePath + "/Jump", "*.fbx");
        if (jumpFiles.Length > 0)
        {
            jumpAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(jumpFiles[0]);
        }
        
        Debug.Log("기본 애니메이션들이 로드되었습니다.");
#endif
    }
}
