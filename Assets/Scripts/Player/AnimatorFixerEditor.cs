using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

public class AnimatorFixerEditor : MonoBehaviour
{
#if UNITY_EDITOR
    [CustomEditor(typeof(AnimatorFixerEditor))]
    public class AnimatorFixerEditorClass : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animator Controller 수정", EditorStyles.boldLabel);
            
            if (GUILayout.Button("🔧 Animator Controller 수정하기", GUILayout.Height(40)))
            {
                FixAnimatorController();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("이 버튼을 클릭하면 PlayerAnimator.controller가 자동으로 수정됩니다.\n\n수정 내용:\n• 필요한 파라미터 추가\n• 전환 조건 설정\n• 기본 상태 설정\n• 전투 애니메이션 추가", MessageType.Info);
            
            EditorGUILayout.Space();
            if (GUILayout.Button("⚔️ 전투 애니메이션 추가하기", GUILayout.Height(40)))
            {
                AddCombatAnimations();
            }
        }
        
        private void FixAnimatorController()
        {
            // PlayerAnimator.controller 찾기
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animators/PlayerAnimator.controller");
            if (controller == null)
            {
                EditorUtility.DisplayDialog("오류", "PlayerAnimator.controller를 찾을 수 없습니다!\nAssets/Animators/ 폴더에 PlayerAnimator.controller 파일이 있는지 확인해주세요.", "확인");
                return;
            }
            
            Debug.Log("Animator Controller 수정을 시작합니다...");
            
            // 파라미터 확인 및 수정
            bool hasSpeed = false;
            bool hasIsWalking = false;
            bool hasIsRunning = false;
            bool hasIsJumping = false;
            bool hasIsGrounded = false;
            bool hasJump = false;
            
            foreach (var param in controller.parameters)
            {
                switch (param.name)
                {
                    case "Speed":
                        hasSpeed = true;
                        break;
                    case "IsWalking":
                        hasIsWalking = true;
                        break;
                    case "IsRunning":
                        hasIsRunning = true;
                        break;
                    case "IsJumping":
                        hasIsJumping = true;
                        break;
                    case "IsGrounded":
                        hasIsGrounded = true;
                        break;
                    case "Jump":
                        hasJump = true;
                        break;
                }
            }
            
            // 누락된 파라미터 추가
            if (!hasSpeed) controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            if (!hasIsWalking) controller.AddParameter("IsWalking", AnimatorControllerParameterType.Bool);
            if (!hasIsRunning) controller.AddParameter("IsRunning", AnimatorControllerParameterType.Bool);
            if (!hasIsJumping) controller.AddParameter("IsJumping", AnimatorControllerParameterType.Bool);
            if (!hasIsGrounded) controller.AddParameter("IsGrounded", AnimatorControllerParameterType.Bool);
            if (!hasJump) controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            
            // 상태 머신 가져오기
            var stateMachine = controller.layers[0].stateMachine;
            
            // 기본 상태 찾기 (더 유연하게)
            AnimatorState idleState = null;
            AnimatorState walkState = null;
            AnimatorState runState = null;
            AnimatorState jumpState = null;
            
            foreach (var state in stateMachine.states)
            {
                string stateName = state.state.name.ToLower();
                
                // Idle 상태 찾기 (여러 가능한 이름)
                if (stateName.Contains("idle") || stateName == "idle")
                {
                    idleState = state.state;
                    Debug.Log($"Idle 상태를 찾았습니다: {state.state.name}");
                }
                // Walk 상태 찾기
                else if (stateName.Contains("walk") || stateName.Contains("walk01"))
                {
                    walkState = state.state;
                    Debug.Log($"Walk 상태를 찾았습니다: {state.state.name}");
                }
                // Run 상태 찾기
                else if (stateName.Contains("run") || stateName.Contains("run01"))
                {
                    runState = state.state;
                    Debug.Log($"Run 상태를 찾았습니다: {state.state.name}");
                }
                // Jump 상태 찾기
                else if (stateName.Contains("jump") || stateName.Contains("jump01"))
                {
                    jumpState = state.state;
                    Debug.Log($"Jump 상태를 찾았습니다: {state.state.name}");
                }
            }
            
            // 상태 찾기 결과 확인
            Debug.Log($"=== 상태 찾기 결과 ===");
            Debug.Log($"Idle 상태: {(idleState != null ? "찾음" : "찾지 못함")}");
            Debug.Log($"Walk 상태: {(walkState != null ? "찾음" : "찾지 못함")}");
            Debug.Log($"Run 상태: {(runState != null ? "찾음" : "찾지 못함")}");
            Debug.Log($"Jump 상태: {(jumpState != null ? "찾음" : "찾지 못함")}");
            
            // 사용 가능한 모든 상태 이름 출력
            Debug.Log("=== 사용 가능한 상태들 ===");
            foreach (var state in stateMachine.states)
            {
                Debug.Log($"- {state.state.name}");
            }
            
            // 기본 상태 설정
            if (idleState != null)
            {
                stateMachine.defaultState = idleState;
                Debug.Log("기본 상태를 Idle로 설정했습니다.");
            }
            else
            {
                Debug.LogError("Idle 상태를 찾을 수 없습니다! 상태 이름을 확인해주세요.");
            }
            
            // 기존 상태들 완전 제거 (새로 생성하기 위해)
            Debug.Log("=== 기존 상태들 완전 제거 시작 ===");
            
            // 기존 상태들을 리스트로 수집
            List<AnimatorState> statesToRemove = new List<AnimatorState>();
            
            if (idleState != null) statesToRemove.Add(idleState);
            if (walkState != null) statesToRemove.Add(walkState);
            if (runState != null) statesToRemove.Add(runState);
            if (jumpState != null) statesToRemove.Add(jumpState);
            
            // 상태들 제거
            foreach (var state in statesToRemove)
            {
                Debug.Log($"기존 상태 '{state.name}'을 제거합니다.");
                stateMachine.RemoveState(state);
            }
            
            // Any State 전환도 제거
            Debug.Log($"Any State 전환 {stateMachine.anyStateTransitions.Length}개를 제거합니다.");
            while (stateMachine.anyStateTransitions.Length > 0)
            {
                stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[0]);
            }
            
            Debug.Log("기존 상태들을 모두 제거했습니다.");
            
            // 상태 변수들 초기화
            idleState = null;
            walkState = null;
            runState = null;
            jumpState = null;
            
            // 새로운 상태들 생성
            Debug.Log("=== 새로운 상태들 생성 시작 ===");
            
            // Idle 상태 생성
            idleState = stateMachine.AddState("Idle");
            idleState.speed = 1.0f;
            Debug.Log("✅ Idle 상태를 생성했습니다.");
            
            // Walk 상태 생성
            walkState = stateMachine.AddState("Walk");
            walkState.speed = 1.0f;
            Debug.Log("✅ Walk 상태를 생성했습니다.");
            
            // Run 상태 생성
            runState = stateMachine.AddState("Run");
            runState.speed = 1.0f;
            Debug.Log("✅ Run 상태를 생성했습니다.");
            
            // Jump 상태 생성
            jumpState = stateMachine.AddState("Jump");
            jumpState.speed = 1.0f;
            Debug.Log("✅ Jump 상태를 생성했습니다.");
            
            // 기본 상태 설정
            stateMachine.defaultState = idleState;
            Debug.Log("✅ 기본 상태를 Idle로 설정했습니다.");
            
            // 새로운 전환 추가
            Debug.Log("=== 새로운 전환 추가 시작 ===");
            
            // Idle ↔ Walk 전환
            var idleToWalk = idleState.AddTransition(walkState);
            idleToWalk.AddCondition(AnimatorConditionMode.If, 0, "IsWalking");
            idleToWalk.duration = 0.1f;
            Debug.Log("✅ Idle → Walk 전환을 추가했습니다.");
            
            var walkToIdle = walkState.AddTransition(idleState);
            walkToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsWalking");
            walkToIdle.duration = 0.1f;
            Debug.Log("✅ Walk → Idle 전환을 추가했습니다.");
            
            // Idle ↔ Run 전환
            var idleToRun = idleState.AddTransition(runState);
            idleToRun.AddCondition(AnimatorConditionMode.If, 0, "IsRunning");
            idleToRun.duration = 0.1f;
            Debug.Log("✅ Idle → Run 전환을 추가했습니다.");
            
            var runToIdle = runState.AddTransition(idleState);
            runToIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");
            runToIdle.duration = 0.1f;
            Debug.Log("✅ Run → Idle 전환을 추가했습니다.");
            
            // Walk ↔ Run 전환
            var walkToRun = walkState.AddTransition(runState);
            walkToRun.AddCondition(AnimatorConditionMode.If, 0, "IsRunning");
            walkToRun.duration = 0.1f;
            Debug.Log("✅ Walk → Run 전환을 추가했습니다.");
            
            var runToWalk = runState.AddTransition(walkState);
            runToWalk.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");
            runToWalk.duration = 0.1f;
            Debug.Log("✅ Run → Walk 전환을 추가했습니다.");
            
            // Jump → Idle 전환 (Exit Time 있음)
            var jumpToIdle = jumpState.AddTransition(idleState);
            jumpToIdle.duration = 0.1f;
            jumpToIdle.hasExitTime = true;
            jumpToIdle.exitTime = 0.8f;
            Debug.Log("✅ Jump → Idle 전환을 추가했습니다.");
            
            // Any State → Jump 전환
            var anyToJump = stateMachine.AddAnyStateTransition(jumpState);
            anyToJump.AddCondition(AnimatorConditionMode.If, 0, "Jump");
            anyToJump.duration = 0.1f;
            Debug.Log("✅ Any State → Jump 전환을 추가했습니다.");
            
            // 변경사항 저장
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            
            Debug.Log("=== Animator Controller 수정 완료 ===");
            Debug.Log("✅ 모든 전환이 성공적으로 추가되었습니다!");
            Debug.Log("이제 Play 모드에서 애니메이션을 테스트해보세요.");
            
            EditorUtility.DisplayDialog("완료", "Animator Controller 수정이 완료되었습니다!\n\n추가된 전환:\n• Idle ↔ Walk\n• Idle ↔ Run\n• Walk ↔ Run\n• Jump ↔ Idle\n• Any State → Jump\n\n이제 Play 모드에서 애니메이션을 테스트해보세요!", "확인");
        }
        
        private AnimatorState GetOrCreateState(AnimatorStateMachine stateMachine, string stateName)
        {
            // 기존 상태 찾기
            foreach (var state in stateMachine.states)
            {
                if (state.state != null && state.state.name == stateName)
                {
                    Debug.Log($"기존 {stateName} 상태를 찾았습니다.");
                    return state.state;
                }
            }
            
            // 새 상태 생성
            Debug.Log($"새로운 {stateName} 상태를 생성합니다.");
            var newState = stateMachine.AddState(stateName);
            
            // 상태 생성 확인
            if (newState != null)
            {
                Debug.Log($"✅ {stateName} 상태가 성공적으로 생성되었습니다!");
            }
            else
            {
                Debug.LogError($"❌ {stateName} 상태 생성에 실패했습니다!");
            }
            
            return newState;
        }
        
        private void AddCombatAnimations()
        {
            // PlayerAnimator.controller 찾기
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/Animators/PlayerAnimator.controller");
            if (controller == null)
            {
                EditorUtility.DisplayDialog("오류", "PlayerAnimator.controller를 찾을 수 없습니다!\nAssets/Animators/ 폴더에 PlayerAnimator.controller 파일이 있는지 확인해주세요.", "확인");
                return;
            }
            
            Debug.Log("전투 애니메이션 추가를 시작합니다...");
            
            // 기존 전투 관련 파라미터 모두 제거 (완전히 강력하게)
            Debug.Log("기존 전투 파라미터들을 제거합니다...");
            
            // 제거할 파라미터들 수집 (이름과 함께)
            List<string> parametersToRemove = new List<string>();
            foreach (var param in controller.parameters)
            {
                string paramName = param.name;
                
                // 전투 관련 파라미터들 식별 (더 포괄적으로)
                if (paramName == "Attack" || paramName == "Attack1" || paramName == "Attack2" || paramName == "Attack3" ||
                    paramName == "Block" || paramName == "Hit" || paramName == "Death" || 
                    paramName == "IsBlocking" || paramName == "IsAttacking" || 
                    paramName == "ComboCount" || paramName == "AttackType" ||
                    paramName.StartsWith("ComboCount") || paramName.StartsWith("AttackType") ||
                    paramName.StartsWith("Attack1") || paramName.StartsWith("Attack2") || paramName.StartsWith("Attack3") ||
                    paramName.Contains("Attack1") || paramName.Contains("Attack2") || paramName.Contains("Attack3") ||
                    paramName.Contains("ComboCount") || paramName.Contains("AttackType"))
                {
                    parametersToRemove.Add(paramName);
                }
            }
            
            // 수집된 파라미터들 제거 (반복적으로)
            int maxAttempts = 10;
            int attempt = 0;
            while (parametersToRemove.Count > 0 && attempt < maxAttempts)
            {
                attempt++;
                Debug.Log($"제거 시도 {attempt}/{maxAttempts} - {parametersToRemove.Count}개 파라미터 남음");
                
                List<string> stillToRemove = new List<string>();
                foreach (string paramName in parametersToRemove)
                {
                    var param = controller.parameters.FirstOrDefault(p => p.name == paramName);
                    if (param != null)
                    {
                        Debug.Log($"파라미터 '{paramName}'을 제거합니다.");
                        controller.RemoveParameter(param);
                    }
                    else
                    {
                        stillToRemove.Add(paramName);
                    }
                }
                
                parametersToRemove = stillToRemove;
                
                // 즉시 저장
                EditorUtility.SetDirty(controller);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                // 잠시 대기
                System.Threading.Thread.Sleep(50);
            }
            
            Debug.Log($"파라미터 제거 완료 - {attempt}번 시도 후 {parametersToRemove.Count}개 남음");
            
            // 전투 파라미터 새로 추가 (중복 방지)
            Debug.Log("새로운 파라미터들을 추가합니다...");
            
            // 파라미터 추가 전에 현재 상태 확인
            Debug.Log($"현재 파라미터 개수: {controller.parameters.Length}");
            foreach (var param in controller.parameters)
            {
                Debug.Log($"  - {param.name} ({param.type})");
            }
            
            // 파라미터 추가 (Attack 제외, Attack1, 2, 3만 사용)
            AddParameterIfNotExists(controller, "Attack1", AnimatorControllerParameterType.Trigger);
            AddParameterIfNotExists(controller, "Attack2", AnimatorControllerParameterType.Trigger);
            AddParameterIfNotExists(controller, "Attack3", AnimatorControllerParameterType.Trigger);
            AddParameterIfNotExists(controller, "Block", AnimatorControllerParameterType.Trigger);
            AddParameterIfNotExists(controller, "Hit", AnimatorControllerParameterType.Trigger);
            AddParameterIfNotExists(controller, "Death", AnimatorControllerParameterType.Trigger);
            AddParameterIfNotExists(controller, "IsBlocking", AnimatorControllerParameterType.Bool);
            AddParameterIfNotExists(controller, "IsAttacking", AnimatorControllerParameterType.Bool);
            AddParameterIfNotExists(controller, "ComboCount", AnimatorControllerParameterType.Int);
            AddParameterIfNotExists(controller, "AttackType", AnimatorControllerParameterType.Int);
            
            // 최종 상태 확인
            Debug.Log($"최종 파라미터 개수: {controller.parameters.Length}");
            foreach (var param in controller.parameters)
            {
                Debug.Log($"  - {param.name} ({param.type})");
            }
            
            // 상태 머신 가져오기
            var stateMachine = controller.layers[0].stateMachine;
            
            // 기존 전투 상태들 제거를 건너뛰고 새로 생성 (안전하게)
            Debug.Log("=== 전투 상태들 새로 생성 시작 (기존 제거 건너뛰기) ===");
            
            // Any State 전환만 제거 (상태는 건드리지 않음)
            Debug.Log($"Any State 전환 {stateMachine.anyStateTransitions.Length}개를 제거합니다.");
            while (stateMachine.anyStateTransitions.Length > 0)
            {
                stateMachine.RemoveAnyStateTransition(stateMachine.anyStateTransitions[0]);
            }
            
            Debug.Log("Any State 전환을 제거했습니다. 기존 상태들은 그대로 두고 새로 생성합니다.");
            
            // 전투 애니메이션 클립들 찾기 (정확한 경로)
            AnimationClip attackClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/DoubleL/One Hand Up/Attack_A/OneHand_Up_Attack_1.fbx");
            AnimationClip attack1Clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/DoubleL/One Hand Up/Attack_A/OneHand_Up_Attack_1.fbx");
            AnimationClip attack2Clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/DoubleL/One Hand Up/Attack_A/OneHand_Up_Attack_2.fbx");
            AnimationClip attack3Clip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/DoubleL/One Hand Up/Attack_A/OneHand_Up_Attack_3.fbx");
            AnimationClip blockIdleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/DoubleL/One Hand Up/Sheild/Idle/OneHand_Up_Shield_Block_Idle.fbx");
            AnimationClip blockHitClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/DoubleL/One Hand Up/Sheild/Hit/OneHand_Up_Shield_Block_Hit_1.fbx");
            AnimationClip hitClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/DoubleL/Hit/Hit_F_1.fbx");
            AnimationClip deathClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Kevin Iglesias/Human Animations/Animations/Male/Combat/HumanM@Knockdown01 - Fall.fbx");
            
            // 애니메이션 클립 찾기 디버그
            Debug.Log($"Attack 클립: {(attackClip != null ? "찾음" : "없음")}");
            Debug.Log($"BlockIdle 클립: {(blockIdleClip != null ? "찾음" : "없음")}");
            Debug.Log($"BlockHit 클립: {(blockHitClip != null ? "찾음" : "없음")}");
            Debug.Log($"Hit 클립: {(hitClip != null ? "찾음" : "없음")}");
            Debug.Log($"Death 클립: {(deathClip != null ? "찾음" : "없음")}");
            
            // 대체 경로로 시도
            if (blockIdleClip == null)
            {
                blockIdleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/DoubleL/One Hand Up/Sheild/OneHand_Up_Shield_Block.fbx");
                Debug.Log($"대체 BlockIdle 클립: {(blockIdleClip != null ? "찾음" : "없음")}");
            }
            
            if (blockHitClip == null)
            {
                blockHitClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/DoubleL/One Hand Up/Sheild/OneHand_Up_Shield_Hit.fbx");
                Debug.Log($"대체 BlockHit 클립: {(blockHitClip != null ? "찾음" : "없음")}");
            }
            
            if (deathClip == null)
            {
                deathClip = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/DoubleL/Hit/Hit_F_2.fbx");
                Debug.Log($"대체 Death 클립: {(deathClip != null ? "찾음" : "없음")}");
            }
            
            // 전투 상태들 생성 또는 업데이트 (안전하게)
            Debug.Log("=== 전투 상태들 생성/업데이트 시작 ===");
            
            // 콤보 공격 상태들 생성
            var attack1State = GetOrCreateState(stateMachine, "Attack1");
            if (attack1Clip != null)
            {
                attack1State.motion = attack1Clip;
                Debug.Log("✅ Attack1 상태를 생성/업데이트했습니다. (애니메이션 연결됨)");
            }
            else
            {
                Debug.LogWarning("Attack1 애니메이션 클립을 찾을 수 없습니다!");
            }
            attack1State.speed = 1.2f;
            
            var attack2State = GetOrCreateState(stateMachine, "Attack2");
            if (attack2Clip != null)
            {
                attack2State.motion = attack2Clip;
                Debug.Log("✅ Attack2 상태를 생성/업데이트했습니다. (애니메이션 연결됨)");
            }
            else
            {
                Debug.LogWarning("Attack2 애니메이션 클립을 찾을 수 없습니다!");
            }
            attack2State.speed = 1.2f;
            
            var attack3State = GetOrCreateState(stateMachine, "Attack3");
            if (attack3Clip != null)
            {
                attack3State.motion = attack3Clip;
                Debug.Log("✅ Attack3 상태를 생성/업데이트했습니다. (애니메이션 연결됨)");
            }
            else
            {
                Debug.LogWarning("Attack3 애니메이션 클립을 찾을 수 없습니다!");
            }
            attack3State.speed = 1.2f;
            
            // 방어 상태 생성
            var blockState = GetOrCreateState(stateMachine, "BlockIdle");
            if (blockIdleClip != null)
            {
                blockState.motion = blockIdleClip;
                Debug.Log("✅ BlockIdle 상태를 생성/업데이트했습니다. (애니메이션 연결됨)");
                Debug.Log($"BlockIdle 애니메이션 루프 상태: {blockIdleClip.isLooping}");
            }
            else
            {
                Debug.LogWarning("BlockIdle 애니메이션 클립을 찾을 수 없습니다! 기본 상태로 생성합니다.");
            }
            blockState.speed = 1.0f;
            
            // 방어 중 피해 상태 생성
            var blockHitState = GetOrCreateState(stateMachine, "BlockHit");
            if (blockHitClip != null)
            {
                blockHitState.motion = blockHitClip;
                Debug.Log("✅ BlockHit 상태를 생성/업데이트했습니다. (애니메이션 연결됨)");
            }
            else
            {
                Debug.LogWarning("BlockHit 애니메이션 클립을 찾을 수 없습니다! 기본 상태로 생성합니다.");
            }
            blockHitState.speed = 1.0f;
            
            // 피해 상태 생성
            var hitState = GetOrCreateState(stateMachine, "Hit");
            if (hitClip != null)
            {
                hitState.motion = hitClip;
                Debug.Log("✅ Hit 상태를 생성/업데이트했습니다. (애니메이션 연결됨)");
            }
            else
            {
                Debug.LogWarning("Hit 애니메이션 클립을 찾을 수 없습니다!");
            }
            hitState.speed = 1.0f;
            
            // 사망 상태 생성
            var deathState = GetOrCreateState(stateMachine, "Death");
            if (deathClip != null)
            {
                deathState.motion = deathClip;
                Debug.Log("✅ Death 상태를 생성/업데이트했습니다. (애니메이션 연결됨)");
            }
            else
            {
                Debug.LogWarning("Death 애니메이션 클립을 찾을 수 없습니다! 기본 상태로 생성합니다.");
            }
            deathState.speed = 1.0f;
            
            // Idle 상태 찾기 (더 유연하게)
            AnimatorState idleState = null;
            foreach (var state in stateMachine.states)
            {
                string stateName = state.state.name.ToLower();
                if (stateName.Contains("idle") || stateName == "idle")
                {
                    idleState = state.state;
                    Debug.Log($"Idle 상태를 찾았습니다: {state.state.name}");
                    break;
                }
            }
            
            if (idleState == null)
            {
                Debug.LogError("Idle 상태를 찾을 수 없습니다! 기본 상태를 Idle로 생성합니다.");
                idleState = stateMachine.AddState("Idle");
                stateMachine.defaultState = idleState;
            }
            
            // 생성된 상태들 확인
            Debug.Log($"=== 생성된 전투 상태들 ===");
            Debug.Log($"Attack1 상태: 생성됨");
            Debug.Log($"Attack2 상태: 생성됨");
            Debug.Log($"Attack3 상태: 생성됨");
            Debug.Log($"BlockIdle 상태: 생성됨");
            Debug.Log($"BlockHit 상태: 생성됨");
            Debug.Log($"Hit 상태: 생성됨");
            Debug.Log($"Death 상태: 생성됨");
            Debug.Log($"Idle 상태: {(idleState != null ? "찾음" : "찾지 못함")}");
            
            // 전투 전환 추가 (중복 방지)
            Debug.Log("=== 전투 전환 추가 시작 (중복 방지) ===");
            
            // Idle → Attack1, 2, 3 전환
            AddTransitionIfNotExists(idleState, attack1State, "Attack1", AnimatorConditionMode.If);
            AddTransitionIfNotExists(idleState, attack2State, "Attack2", AnimatorConditionMode.If);
            AddTransitionIfNotExists(idleState, attack3State, "Attack3", AnimatorConditionMode.If);
            
            // Attack1, 2, 3 → Idle 전환
            AddTransitionIfNotExists(attack1State, idleState, null, AnimatorConditionMode.If, 0.1f, true, 0.9f);
            AddTransitionIfNotExists(attack2State, idleState, null, AnimatorConditionMode.If, 0.1f, true, 0.9f);
            AddTransitionIfNotExists(attack3State, idleState, null, AnimatorConditionMode.If, 0.1f, true, 0.9f);
            
            // Idle → BlockIdle 전환 (방어 모드 진입)
            AddTransitionIfNotExists(idleState, blockState, "IsBlocking", AnimatorConditionMode.If);
            
            // BlockIdle → Idle 전환 (방어 모드 해제) - 더 엄격한 조건
            AddTransitionIfNotExists(blockState, idleState, "IsBlocking", AnimatorConditionMode.IfNot, 0.5f, true, 0.8f);
            
            // BlockIdle → BlockHit 전환 (방어 중 피해)
            AddTransitionIfNotExists(blockState, blockHitState, "Hit", AnimatorConditionMode.If);
            
            // BlockHit → BlockIdle 전환 (방어 중 피해 후 방어 상태로)
            AddTransitionIfNotExists(blockHitState, blockState, "IsBlocking", AnimatorConditionMode.If, 0.1f, true, 0.9f);
            
            // BlockHit → Idle 전환 (방어 중 피해 후 방어 해제)
            AddTransitionIfNotExists(blockHitState, idleState, "IsBlocking", AnimatorConditionMode.IfNot, 0.1f, true, 0.9f);
            
            // Idle → Hit 전환 (일반 피해)
            AddTransitionIfNotExists(idleState, hitState, "Hit", AnimatorConditionMode.If);
            
            // Hit → Idle 전환 (피해 후 기본 상태로)
            AddTransitionIfNotExists(hitState, idleState, null, AnimatorConditionMode.If, 0.1f, true, 0.9f);
            
            // Any State → Death 전환 (수동으로 추가)
            if (deathState != null)
            {
                // Any State 전환 중복 체크
                bool hasAnyToDeath = false;
                foreach (var transition in stateMachine.anyStateTransitions)
                {
                    if (transition.destinationState == deathState)
                    {
                        hasAnyToDeath = true;
                        break;
                    }
                }
                
                if (!hasAnyToDeath)
                {
                    var anyToDeath = stateMachine.AddAnyStateTransition(deathState);
                    anyToDeath.AddCondition(AnimatorConditionMode.If, 0, "Death");
                    anyToDeath.duration = 0.1f;
                    anyToDeath.hasExitTime = false;
                    Debug.Log("✅ Any State → Death 전환을 추가했습니다.");
                }
                else
                {
                    Debug.Log("Any State → Death 전환이 이미 존재합니다. 건너뜁니다.");
                }
            }
            
            // 변경사항 즉시 저장
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            Debug.Log("전투 애니메이션 추가가 완료되었습니다!");
            EditorUtility.DisplayDialog("완료", "전투 애니메이션이 추가되었습니다!\n\n추가된 애니메이션:\n• Attack1 (콤보 1)\n• Attack2 (콤보 2)\n• Attack3 (콤보 3)\n• BlockIdle\n• BlockHit\n• Hit\n• Death\n\n이제 Play 모드에서 콤보 전투를 테스트해보세요!", "확인");
        }
        
        // 콤보 공격 상태들 전환 추가 메서드
        private void AddComboTransitions(AnimatorStateMachine stateMachine, AnimatorState attack1State, AnimatorState attack2State, AnimatorState attack3State)
        {
            Debug.Log("콤보 공격 상태들 전환을 추가합니다...");
            
            // Idle 상태 찾기
            var idleState = stateMachine.states.FirstOrDefault(s => s.state.name == "Idle" || s.state.name == "HumanF@Idle01");
            if (idleState.state == null)
            {
                Debug.LogWarning("Idle 상태를 찾을 수 없습니다!");
                return;
            }
            
            // Idle → Attack1 전환
            if (attack1State != null)
            {
                var idleToAttack1 = idleState.state.AddTransition(attack1State);
                idleToAttack1.AddCondition(AnimatorConditionMode.If, 0, "Attack1");
                idleToAttack1.duration = 0.1f;
                idleToAttack1.hasExitTime = false;
                Debug.Log("Idle → Attack1 전환을 추가했습니다.");
                
                // Attack1 → Idle 전환
                var attack1ToIdle = attack1State.AddTransition(idleState.state);
                attack1ToIdle.hasExitTime = true;
                attack1ToIdle.exitTime = 0.9f;
                attack1ToIdle.duration = 0.1f;
                Debug.Log("Attack1 → Idle 전환을 추가했습니다.");
            }
            
            // Idle → Attack2 전환
            if (attack2State != null)
            {
                var idleToAttack2 = idleState.state.AddTransition(attack2State);
                idleToAttack2.AddCondition(AnimatorConditionMode.If, 0, "Attack2");
                idleToAttack2.duration = 0.1f;
                idleToAttack2.hasExitTime = false;
                Debug.Log("Idle → Attack2 전환을 추가했습니다.");
                
                // Attack2 → Idle 전환
                var attack2ToIdle = attack2State.AddTransition(idleState.state);
                attack2ToIdle.hasExitTime = true;
                attack2ToIdle.exitTime = 0.9f;
                attack2ToIdle.duration = 0.1f;
                Debug.Log("Attack2 → Idle 전환을 추가했습니다.");
            }
            
            // Idle → Attack3 전환
            if (attack3State != null)
            {
                var idleToAttack3 = idleState.state.AddTransition(attack3State);
                idleToAttack3.AddCondition(AnimatorConditionMode.If, 0, "Attack3");
                idleToAttack3.duration = 0.1f;
                idleToAttack3.hasExitTime = false;
                Debug.Log("Idle → Attack3 전환을 추가했습니다.");
                
                // Attack3 → Idle 전환
                var attack3ToIdle = attack3State.AddTransition(idleState.state);
                attack3ToIdle.hasExitTime = true;
                attack3ToIdle.exitTime = 0.9f;
                attack3ToIdle.duration = 0.1f;
                Debug.Log("Attack3 → Idle 전환을 추가했습니다.");
            }
            
            Debug.Log("✅ Idle 상태에서 Attack1, 2, 3으로 가는 3개의 독립적인 전환이 생성되었습니다.");
            Debug.Log("   - Idle → Attack1 (Attack1 트리거)");
            Debug.Log("   - Idle → Attack2 (Attack2 트리거)");
            Debug.Log("   - Idle → Attack3 (Attack3 트리거)");
            Debug.Log("   이는 Unity Animator의 정상적인 동작입니다!");
            
            Debug.Log("콤보 공격 상태들 전환이 완료되었습니다!");
        }
        
        // 트랜지션 중복 체크 메서드
        private bool HasTransition(AnimatorState fromState, AnimatorState toState, string conditionParameter = null)
        {
            if (fromState == null || toState == null) return false;
            
            foreach (var transition in fromState.transitions)
            {
                if (transition.destinationState == toState)
                {
                    // 조건이 지정되지 않았으면 무조건 중복으로 간주
                    if (string.IsNullOrEmpty(conditionParameter))
                    {
                        return true;
                    }
                    
                    // 조건이 지정되었으면 조건도 확인
                    foreach (var condition in transition.conditions)
                    {
                        if (condition.parameter == conditionParameter)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        
        // 안전한 트랜지션 추가 메서드
        private void AddTransitionIfNotExists(AnimatorState fromState, AnimatorState toState, string conditionParameter, AnimatorConditionMode conditionMode, float duration = 0.1f, bool hasExitTime = false, float exitTime = 0.9f)
        {
            if (fromState == null || toState == null)
            {
                Debug.LogError($"트랜지션 추가 실패 - From: {(fromState != null ? "있음" : "없음")}, To: {(toState != null ? "있음" : "없음")}");
                return;
            }
            
            // 중복 체크
            if (HasTransition(fromState, toState, conditionParameter))
            {
                Debug.Log($"트랜지션 {fromState.name} → {toState.name} (조건: {conditionParameter})이 이미 존재합니다. 건너뜁니다.");
                return;
            }
            
            // 트랜지션 추가
            var transition = fromState.AddTransition(toState);
            if (!string.IsNullOrEmpty(conditionParameter))
            {
                transition.AddCondition(conditionMode, 0, conditionParameter);
            }
            transition.duration = duration;
            transition.hasExitTime = hasExitTime;
            if (hasExitTime)
            {
                transition.exitTime = exitTime;
            }
            
            Debug.Log($"✅ 트랜지션 {fromState.name} → {toState.name} (조건: {conditionParameter})을 추가했습니다.");
        }
        
        // 파라미터 중복 추가 방지 메서드
        private void AddParameterIfNotExists(AnimatorController controller, string parameterName, AnimatorControllerParameterType parameterType)
        {
            // 이미 존재하는 파라미터인지 확인 (정확한 이름만 확인)
            bool exists = false;
            foreach (var param in controller.parameters)
            {
                if (param.name == parameterName)
                {
                    exists = true;
                    break;
                }
            }
            
            if (exists)
            {
                Debug.Log($"파라미터 '{parameterName}'이 이미 존재합니다. 건너뜁니다.");
                return;
            }
            
            // 파라미터 추가
            controller.AddParameter(parameterName, parameterType);
            Debug.Log($"✅ 파라미터 '{parameterName}'을 추가했습니다.");
            
            // 즉시 저장
            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // 잠시 대기
            System.Threading.Thread.Sleep(10);
        }
    }
#endif
}
