using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;

[CustomEditor(typeof(EnemyAI))]
public class EnemyAnimatorFixerEditor : Editor
{
    private EnemyAI enemyAI;
    private Animator animator;
    private AnimatorController controller;
    
    // 애니메이션 파라미터 이름들
    private readonly string[] boolParameters = { "IsWalking" };
    private readonly string[] triggerParameters = { "Attack", "Hit", "Death" };
    private readonly string[] floatParameters = { "Speed" };
    
    // 애니메이션 상태 이름들
    private readonly string[] stateNames = { "Idle", "Walk", "Run", "Attack01", "Attack02", "GetHit", "Victory", "Die" };
    
    void OnEnable()
    {
        enemyAI = (EnemyAI)target;
        animator = enemyAI.GetComponent<Animator>();
        
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            controller = animator.runtimeAnimatorController as AnimatorController;
        }
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("애니메이터 자동 설정", EditorStyles.boldLabel);
        
        if (animator == null)
        {
            EditorGUILayout.HelpBox("Animator 컴포넌트를 찾을 수 없습니다!", MessageType.Error);
            return;
        }
        
        if (controller == null)
        {
            EditorGUILayout.HelpBox("AnimatorController를 찾을 수 없습니다!", MessageType.Error);
            return;
        }
        
        EditorGUILayout.Space(5);
        
        // 현재 상태 표시
        EditorGUILayout.LabelField("현재 상태:", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Animator: {animator.name}");
        EditorGUILayout.LabelField($"Controller: {controller.name}");
        EditorGUILayout.LabelField($"Controller Path: {UnityEditor.AssetDatabase.GetAssetPath(controller)}");
        
        // 컨트롤러 직접 열기 버튼
        if (GUILayout.Button("Animator Controller 열기", GUILayout.Height(25)))
        {
            UnityEditor.AssetDatabase.OpenAsset(controller);
        }
        
        EditorGUILayout.Space(5);
        
        // 파라미터 상태 확인
        EditorGUILayout.LabelField("파라미터 상태:", EditorStyles.boldLabel);
        
        bool allParametersExist = true;
        
        // Bool 파라미터 확인
        EditorGUILayout.LabelField("Bool 파라미터:");
        foreach (string paramName in boolParameters)
        {
            bool exists = HasParameter(paramName, AnimatorControllerParameterType.Bool);
            EditorGUILayout.LabelField($"  {paramName}: {(exists ? "✓" : "✗")}");
            if (!exists) allParametersExist = false;
        }
        
        // Trigger 파라미터 확인
        EditorGUILayout.LabelField("Trigger 파라미터:");
        foreach (string paramName in triggerParameters)
        {
            bool exists = HasParameter(paramName, AnimatorControllerParameterType.Trigger);
            EditorGUILayout.LabelField($"  {paramName}: {(exists ? "✓" : "✗")}");
            if (!exists) allParametersExist = false;
        }
        
        // Float 파라미터 확인
        EditorGUILayout.LabelField("Float 파라미터:");
        foreach (string paramName in floatParameters)
        {
            bool exists = HasParameter(paramName, AnimatorControllerParameterType.Float);
            EditorGUILayout.LabelField($"  {paramName}: {(exists ? "✓" : "✗")}");
            if (!exists) allParametersExist = false;
        }
        
        EditorGUILayout.Space(5);
        
        // 상태 확인
        EditorGUILayout.LabelField("애니메이션 상태:");
        foreach (string stateName in stateNames)
        {
            bool exists = HasState(stateName);
            EditorGUILayout.LabelField($"  {stateName}: {(exists ? "✓" : "✗")}");
            if (!exists) allParametersExist = false;
        }
        
        EditorGUILayout.Space(10);
        
        // 자동 설정 버튼
        if (allParametersExist)
        {
            EditorGUILayout.HelpBox("모든 파라미터와 상태가 올바르게 설정되어 있습니다!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("일부 파라미터나 상태가 누락되었습니다. 자동 설정을 실행하세요.", MessageType.Warning);
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("애니메이터 자동 설정", GUILayout.Height(30)))
        {
            SetupAnimator();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("완전 초기화 및 재설정", GUILayout.Height(30)))
        {
            ResetAnimatorCompletely();
        }
        
        EditorGUILayout.Space(5);
        
        if (GUILayout.Button("파라미터만 추가", GUILayout.Height(25)))
        {
            AddParameters();
        }
        
        if (GUILayout.Button("스테이트 생성", GUILayout.Height(25)))
        {
            CreateStates();
        }
        
        if (GUILayout.Button("전환 설정", GUILayout.Height(25)))
        {
            SetupTransitions();
        }
        
        EditorGUILayout.Space(10);
        
        // 수동 설정 가이드
        EditorGUILayout.HelpBox(
            "자동 설정이 안될 경우 수동 설정 방법:\n" +
            "1. 'Animator Controller 열기' 버튼 클릭\n" +
            "2. Parameters 탭에서 다음 파라미터 추가:\n" +
            "   - IsWalking (Bool)\n" +
            "   - IsRunning (Bool)\n" +
            "   - Attack (Trigger)\n" +
            "   - Hit (Trigger)\n" +
            "   - Death (Trigger)\n" +
            "   - Speed (Float)\n" +
            "3. States 탭에서 전환 설정:\n" +
            "   - Idle ↔ Walk (IsWalking)\n" +
            "   - Walk ↔ Run (IsRunning)\n" +
            "   - 모든 상태 → Attack01 (Attack)\n" +
            "   - 모든 상태 → GetHit (Hit)\n" +
            "   - 모든 상태 → Die (Death)",
            MessageType.Info
        );
        
        EditorGUILayout.Space(5);
        
        // 도움말
        EditorGUILayout.HelpBox(
            "자동 설정 기능:\n" +
            "1. 필요한 파라미터들을 자동으로 추가\n" +
            "2. 애니메이션 상태 간 전환을 자동으로 설정\n" +
            "3. EnemyAI 스크립트와 호환되도록 구성",
            MessageType.Info
        );
    }
    
    private bool HasParameter(string parameterName, AnimatorControllerParameterType type)
    {
        if (controller == null) return false;
        
        foreach (var param in controller.parameters)
        {
            if (param.name == parameterName && param.type == type)
                return true;
        }
        return false;
    }
    
    private bool HasState(string stateName)
    {
        if (controller == null) return false;
        
        foreach (var layer in controller.layers)
        {
            foreach (var state in layer.stateMachine.states)
            {
                if (state.state.name == stateName)
                    return true;
            }
        }
        return false;
    }
    
    private void SetupAnimator()
    {
        if (controller == null)
        {
            Debug.LogError("AnimatorController를 찾을 수 없습니다!");
            return;
        }
        
        Undo.RecordObject(controller, "Setup Enemy Animator");
        
        // 파라미터 추가
        AddParameters();
        
        // 스테이트 생성
        CreateStates();
        
        // 전환 설정
        SetupTransitions();
        
        // 강제 저장 및 새로고침
        EditorUtility.SetDirty(controller);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        
        // 잠시 대기 (Unity가 처리할 시간을 줌)
        System.Threading.Thread.Sleep(100);
        
        Debug.Log("애니메이터 설정이 완료되었습니다!");
        Debug.Log($"컨트롤러 경로: {UnityEditor.AssetDatabase.GetAssetPath(controller)}");
    }
    
    /// <summary>
    /// 애니메이터를 완전히 초기화하고 재설정하는 메서드
    /// </summary>
    private void ResetAnimatorCompletely()
    {
        if (controller == null)
        {
            Debug.LogError("AnimatorController를 찾을 수 없습니다!");
            return;
        }
        
        Undo.RecordObject(controller, "Reset Enemy Animator Completely");
        
        var layer = controller.layers[0];
        var stateMachine = layer.stateMachine;
        
        // === 1단계: 모든 파라미터 제거 ===
        RemoveAllParameters();
        
        // === 2단계: 모든 스테이트 제거 ===
        RemoveAllStates(stateMachine);
        
        // === 3단계: 강제 저장 ===
        EditorUtility.SetDirty(controller);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        System.Threading.Thread.Sleep(200);
        
        // === 4단계: 새로 설정 ===
        SetupAnimator();
        
        Debug.Log("애니메이터가 완전히 초기화되고 재설정되었습니다!");
    }
    
    /// <summary>
    /// 모든 파라미터를 제거하는 메서드
    /// </summary>
    private void RemoveAllParameters()
    {
        if (controller == null) return;
        
        var parameters = new List<AnimatorControllerParameter>();
        foreach (var param in controller.parameters)
        {
            parameters.Add(param);
        }
        
        foreach (var param in parameters)
        {
            controller.RemoveParameter(param);
        }
        
        Debug.Log("모든 파라미터가 제거되었습니다.");
    }
    
    /// <summary>
    /// 모든 스테이트를 제거하는 메서드
    /// </summary>
    private void RemoveAllStates(AnimatorStateMachine stateMachine)
    {
        if (stateMachine == null) return;
        
        var states = new List<ChildAnimatorState>();
        foreach (var state in stateMachine.states)
        {
            states.Add(state);
        }
        
        foreach (var state in states)
        {
            stateMachine.RemoveState(state.state);
        }
        
        Debug.Log("모든 스테이트가 제거되었습니다.");
    }
    
    private void AddParameters()
    {
        if (controller == null) return;
        
        bool parametersAdded = false;
        
        // Bool 파라미터 추가
        foreach (string paramName in boolParameters)
        {
            if (!HasParameter(paramName, AnimatorControllerParameterType.Bool))
            {
                controller.AddParameter(paramName, AnimatorControllerParameterType.Bool);
                Debug.Log($"Bool 파라미터 '{paramName}' 추가됨");
                parametersAdded = true;
            }
        }
        
        // Trigger 파라미터 추가
        foreach (string paramName in triggerParameters)
        {
            if (!HasParameter(paramName, AnimatorControllerParameterType.Trigger))
            {
                controller.AddParameter(paramName, AnimatorControllerParameterType.Trigger);
                Debug.Log($"Trigger 파라미터 '{paramName}' 추가됨");
                parametersAdded = true;
            }
        }
        
        // Float 파라미터 추가
        foreach (string paramName in floatParameters)
        {
            if (!HasParameter(paramName, AnimatorControllerParameterType.Float))
            {
                controller.AddParameter(paramName, AnimatorControllerParameterType.Float);
                Debug.Log($"Float 파라미터 '{paramName}' 추가됨");
                parametersAdded = true;
            }
        }
        
        // 파라미터가 추가되었으면 저장
        if (parametersAdded)
        {
            EditorUtility.SetDirty(controller);
            UnityEditor.AssetDatabase.SaveAssets();
            Debug.Log("파라미터 저장 완료");
        }
    }
    
    /// <summary>
    /// 필요한 애니메이션 스테이트들을 생성하는 메서드
    /// </summary>
    private void CreateStates()
    {
        if (controller == null) return;
        
        var layer = controller.layers[0];
        var stateMachine = layer.stateMachine;
        
        // 기본 스테이트들 생성
        CreateStateIfNotExists(stateMachine, "Idle");
        CreateStateIfNotExists(stateMachine, "Walk");
        CreateStateIfNotExists(stateMachine, "Run");
        CreateStateIfNotExists(stateMachine, "Attack01");
        CreateStateIfNotExists(stateMachine, "Attack02");
        CreateStateIfNotExists(stateMachine, "GetHit");
        CreateStateIfNotExists(stateMachine, "Die");
        CreateStateIfNotExists(stateMachine, "Victory");
        
        // Idle을 기본 상태로 설정
        var idleState = FindState(stateMachine, "Idle");
        if (idleState != null)
        {
            stateMachine.defaultState = idleState;
        }
        
        Debug.Log("애니메이션 스테이트들이 생성되었습니다!");
    }
    
    /// <summary>
    /// 스테이트가 존재하지 않으면 생성하는 메서드
    /// </summary>
    private void CreateStateIfNotExists(AnimatorStateMachine stateMachine, string stateName)
    {
        if (FindState(stateMachine, stateName) != null)
        {
            Debug.Log($"스테이트 '{stateName}' 이미 존재함");
            return;
        }
        
        // 새 스테이트 생성
        var newState = stateMachine.AddState(stateName);
        Debug.Log($"스테이트 '{stateName}' 생성됨");
    }
    
    private void SetupTransitions()
    {
        if (controller == null) return;
        
        var layer = controller.layers[0];
        var stateMachine = layer.stateMachine;
        
        // === 기존 트랜지션 완전 제거 ===
        RemoveAllTransitions(stateMachine);
        
        // 기본 상태 설정
        var idleState = FindState(stateMachine, "Idle");
        if (idleState != null)
        {
            stateMachine.defaultState = idleState;
        }
        
        // === 기본 이동 전환 (필수) ===
        // Idle ↔ Walk (양방향)
        SetupTransition(stateMachine, "Idle", "Walk", "IsWalking", true);
        SetupTransition(stateMachine, "Walk", "Idle", "IsWalking", false);
        
        // === 공격 전환 (필수) ===
        // 이동 상태에서 공격 가능
        SetupTransition(stateMachine, "Idle", "Attack01", "Attack", true);
        SetupTransition(stateMachine, "Walk", "Attack01", "Attack", true);
        
        // 공격 완료 후 원래 상태로 복귀
        SetupTransition(stateMachine, "Attack01", "Idle", "Attack", false);
        SetupTransition(stateMachine, "Attack01", "Walk", "Attack", false, "IsWalking", true);
        
        // === 피해 전환 (필수) ===
        // 모든 상태에서 피해 가능
        SetupTransition(stateMachine, "Idle", "GetHit", "Hit", true);
        SetupTransition(stateMachine, "Walk", "GetHit", "Hit", true);
        SetupTransition(stateMachine, "Attack01", "GetHit", "Hit", true);
        
        // 피해 완료 후 원래 상태로 복귀
        SetupTransition(stateMachine, "GetHit", "Idle", "Hit", false);
        SetupTransition(stateMachine, "GetHit", "Walk", "Hit", false, "IsWalking", true);
        
        // === 사망 전환 (필수) ===
        // 모든 상태에서 사망 가능
        SetupTransition(stateMachine, "Idle", "Die", "Death", true);
        SetupTransition(stateMachine, "Walk", "Die", "Death", true);
        SetupTransition(stateMachine, "Attack01", "Die", "Death", true);
        SetupTransition(stateMachine, "GetHit", "Die", "Death", true);
        
        // 전환 설정 후 강제 저장
        EditorUtility.SetDirty(controller);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        
        Debug.Log("애니메이션 전환이 설정되었습니다!");
        Debug.Log("전환 설정 저장 완료");
    }
    
    /// <summary>
    /// 모든 트랜지션을 제거하는 메서드
    /// </summary>
    private void RemoveAllTransitions(AnimatorStateMachine stateMachine)
    {
        if (stateMachine == null) return;
        
        // 각 상태의 모든 트랜지션 제거
        foreach (var state in stateMachine.states)
        {
            if (state.state != null)
            {
                // 상태에서 나가는 트랜지션 제거
                var transitions = new List<AnimatorStateTransition>();
                foreach (var transition in state.state.transitions)
                {
                    transitions.Add(transition);
                }
                
                foreach (var transition in transitions)
                {
                    state.state.RemoveTransition(transition);
                }
            }
        }
        
        // Any State 트랜지션 제거
        var anyStateTransitions = new List<AnimatorStateTransition>();
        foreach (var transition in stateMachine.anyStateTransitions)
        {
            anyStateTransitions.Add(transition);
        }
        
        foreach (var transition in anyStateTransitions)
        {
            stateMachine.RemoveAnyStateTransition(transition);
        }
        
        Debug.Log("기존 트랜지션이 모두 제거되었습니다.");
    }
    
    private AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
    {
        foreach (var state in stateMachine.states)
        {
            if (state.state.name == stateName)
                return state.state;
        }
        return null;
    }
    
    private void SetupTransition(AnimatorStateMachine stateMachine, string fromState, string toState, 
        string parameterName, bool parameterValue, string secondParameterName = null, bool secondParameterValue = false)
    {
        var from = FindState(stateMachine, fromState);
        var to = FindState(stateMachine, toState);
        
        if (from == null || to == null) return;
        
        // 중복 전환 확인
        if (HasTransition(from, to, parameterName, parameterValue, secondParameterName, secondParameterValue))
        {
            Debug.Log($"전환 {fromState} → {toState} 이미 존재함");
            return;
        }
        
        // 새 전환 생성 (올바른 API 사용)
        var newTransition = from.AddTransition(to);
        newTransition.hasExitTime = false;
        newTransition.exitTime = 0.9f;
        newTransition.duration = 0.1f;
        
        // 조건 설정 - Trigger 파라미터는 If 모드만 사용
        if (secondParameterName != null)
        {
            // 두 번째 파라미터가 Bool인지 확인
            if (IsBoolParameter(secondParameterName))
            {
                newTransition.AddCondition(secondParameterValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, secondParameterName);
            }
            else
            {
                newTransition.AddCondition(AnimatorConditionMode.If, 0, secondParameterName);
            }
            
            // 첫 번째 파라미터 처리
            if (IsBoolParameter(parameterName))
            {
                newTransition.AddCondition(parameterValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, parameterName);
            }
            else
            {
                newTransition.AddCondition(AnimatorConditionMode.If, 0, parameterName);
            }
        }
        else
        {
            // 단일 파라미터 처리
            if (IsBoolParameter(parameterName))
            {
                newTransition.AddCondition(parameterValue ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, parameterName);
            }
            else
            {
                // Trigger 파라미터는 If 모드만 사용
                newTransition.AddCondition(AnimatorConditionMode.If, 0, parameterName);
            }
        }
        
        Debug.Log($"전환 생성: {fromState} → {toState} (조건: {parameterName}={parameterValue})");
    }
    
    /// <summary>
    /// 파라미터가 Bool 타입인지 확인하는 메서드
    /// </summary>
    private bool IsBoolParameter(string parameterName)
    {
        if (controller == null) return false;
        
        foreach (var param in controller.parameters)
        {
            if (param.name == parameterName)
            {
                return param.type == AnimatorControllerParameterType.Bool;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 중복 전환 확인 메서드
    /// </summary>
    private bool HasTransition(AnimatorState from, AnimatorState to, string parameterName, bool parameterValue, 
        string secondParameterName = null, bool secondParameterValue = false)
    {
        if (from == null || to == null) return false;
        
        foreach (var transition in from.transitions)
        {
            if (transition.destinationState == to)
            {
                // 조건 개수 확인
                int conditionCount = 0;
                if (secondParameterName != null) conditionCount = 2;
                else conditionCount = 1;
                
                if (transition.conditions.Length == conditionCount)
                {
                    // 조건 매칭 확인
                    bool matches = true;
                    foreach (var condition in transition.conditions)
                    {
                        if (condition.parameter == parameterName)
                        {
                            if (condition.mode == AnimatorConditionMode.If && !parameterValue) matches = false;
                            if (condition.mode == AnimatorConditionMode.IfNot && parameterValue) matches = false;
                        }
                        else if (secondParameterName != null && condition.parameter == secondParameterName)
                        {
                            if (condition.mode == AnimatorConditionMode.If && !secondParameterValue) matches = false;
                            if (condition.mode == AnimatorConditionMode.IfNot && secondParameterValue) matches = false;
                        }
                        else
                        {
                            matches = false;
                        }
                    }
                    
                    if (matches) return true;
                }
            }
        }
        
        return false;
    }
    
    private void SetupAnyStateTransition(AnimatorStateMachine stateMachine, string toState, string triggerName)
    {
        var to = FindState(stateMachine, toState);
        if (to == null) return;
        
        // Any State에서 해당 상태로의 전환 생성
        var transition = stateMachine.AddAnyStateTransition(to);
        transition.hasExitTime = false;
        transition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
    }
}
