# Open Horizons - 오픈 월드 탐험 게임

## 프로젝트 개요
이 프로젝트는 오픈 월드 탐험과 미스터리 요소가 있는 3인칭 시점의 포폴 프로젝트입니다.

## 구현된 기능

### 1. 플레이어 시스템
- **PlayerController.cs**: 3인칭 캐릭터 이동, 점프, 애니메이션 제어
- WASD: 이동
- Space: 점프
- Left Shift: 달리기

### 2. 카메라 시스템
- **ThirdPersonCamera.cs**: 3인칭 카메라 제어
- 우클릭 + 마우스 이동: 카메라 회전
- 마우스 휠: 줌인/아웃
- 부드러운 추적 및 회전

### 3. 상호작용 시스템
- **InteractionSystem.cs**: 오브젝트 감지 및 하이라이트
- **InteractableObject.cs**: 상호작용 가능한 오브젝트 예시
- E키: 상호작용
- 자동 하이라이트 및 UI 프롬프트

### 4. 게임 관리
- **GameManager.cs**: 게임 상태 관리
- ESC: 일시정지/재개
- 플레이어 스폰 및 씬 관리

### 5. 자동 씬 설정 (NEW!)
- **SceneSetup.cs**: 기본 씬 구성을 자동화
- **TestObjectCreator.cs**: 테스트용 상호작용 오브젝트 자동 생성
- **URPMaterialCreator.cs**: URP 프로젝트용 머티리얼 자동 생성
- **URPShaderForcer.cs**: URP 셰이더 강제 적용으로 Built-in 셰이더 문제 해결

## 🚀 빠른 시작 (씬 설정)

### 방법 1: 자동 설정 (권장)
1. Unity에서 빈 GameObject 생성
2. "SceneSetup" 이름으로 설정
3. SceneSetup 스크립트 추가
4. Inspector에서 "Setup Scene Now" 버튼 클릭
5. 또는 Play 모드에서 자동 실행

### 방법 2: 수동 설정
1. 빈 GameObject 생성 후 "Player" 태그 설정
2. CharacterController 컴포넌트 추가
3. PlayerController 스크립트 추가
4. Main Camera에 ThirdPersonCamera 스크립트 추가
5. UI Canvas 및 상호작용 프롬프트 생성

## 상세 설정 방법

### 1. 플레이어 설정
1. 빈 GameObject 생성 후 "Player" 태그 설정
2. CharacterController 컴포넌트 추가
3. PlayerController 스크립트 추가
4. Animator 설정 (선택사항)

### 2. 카메라 설정
1. Main Camera에 ThirdPersonCamera 스크립트 추가
2. Target을 Player로 설정
3. Offset 값 조정

### 3. 상호작용 시스템 설정
1. Player에 InteractionSystem 스크립트 추가
2. 상호작용 가능한 오브젝트에 InteractableObject 스크립트 추가
3. UI 프롬프트 설정 (선택사항)

### 4. 게임 매니저 설정
1. 빈 GameObject 생성 후 "GameManager" 이름 설정
2. GameManager 스크립트 추가
3. UI 요소들 연결

### 5. 테스트 오브젝트 생성
1. TestObjectCreator 스크립트 추가
2. Inspector에서 "Create Test Objects" 버튼 클릭
3. 다양한 상호작용 가능한 오브젝트들이 자동 생성됨

### 6. URP 머티리얼 생성 (URP 프로젝트용)
1. URPMaterialCreator 스크립트 추가
2. Inspector에서 "Create All Materials" 버튼 클릭
3. URP 셰이더를 사용하는 머티리얼들이 자동 생성됨

### 7. URP 셰이더 강제 적용 (Built-in 셰이더 문제 해결)
1. URPShaderForcer 스크립트 추가
2. Inspector에서 "Force URP Shaders Now" 버튼 클릭
3. 모든 Built-in 셰이더가 URP 셰이더로 자동 변경됨

## 🎮 컨트롤

### 플레이어 이동
- **WASD**: 이동
- **Space**: 점프
- **Left Shift**: 달리기

### 카메라 제어
- **우클릭 + 마우스 이동**: 카메라 회전
- **마우스 휠**: 줌인/아웃

### 상호작용
- **E키**: 오브젝트와 상호작용

### 게임 제어
- **ESC**: 일시정지/재개

## 다음 단계
- POLYART Ancient Village 에셋을 활용한 월드 구축
- 수집품 시스템 구현
- 퍼즐 시스템 구현
- 스토리 진행 시스템 구현

## 주의사항
- Unity 6000.0.56f1 이상 필요
- CharacterController 컴포넌트가 필요
- 적절한 레이어 설정 필요
- SceneSetup 스크립트는 Editor 스크립트이므로 Unity Editor에서만 작동

## 🔧 URP 프로젝트 설정

### URP 머티리얼 문제 해결
URP 프로젝트에서 Built-in 셰이더를 사용하면 핑크색으로 표시되는 문제가 발생할 수 있습니다.

#### 해결 방법 1: 자동 머티리얼 생성
1. URPMaterialCreator 스크립트를 씬에 추가
2. Inspector에서 "Create All Materials" 버튼 클릭
3. URP 셰이더를 사용하는 머티리얼들이 자동 생성됨

#### 해결 방법 2: URP 셰이더 강제 적용 (가장 강력한 방법)
1. URPShaderForcer 스크립트를 씬에 추가
2. Inspector에서 "Force URP Shaders Now" 버튼 클릭
3. 모든 Built-in 셰이더가 URP 셰이더로 자동 변경됨
4. 기존 색상과 텍스처는 보존됨

#### 해결 방법 3: 수동 머티리얼 설정
1. Project 창에서 Materials 폴더 생성
2. 우클릭 → Create → Material
3. Shader를 "Universal Render Pipeline/Lit"으로 설정
4. 원하는 색상 설정

#### 해결 방법 4: 코드에서 URP 머티리얼 생성
```csharp
// URP 머티리얼을 코드로 생성
Material urpMaterial = URPMaterialCreator.CreateURPMaterial(Color.blue);
renderer.material = urpMaterial;
```

### URP 셰이더 종류
- **Universal Render Pipeline/Lit**: 가장 완전한 기능을 제공하는 셰이더
- **Universal Render Pipeline/Simple Lit**: 간단한 조명 효과를 제공하는 셰이더
- **Universal Render Pipeline/Unlit**: 조명 효과가 없는 단순한 셰이더

### URP 문제 진단
SceneSetup 스크립트의 Inspector에서 "Test URP Shader Detection" 버튼을 클릭하여 URP 셰이더 감지 상태를 확인할 수 있습니다.

## 문제 해결

### 일반적인 문제들
1. **플레이어가 움직이지 않음**: CharacterController가 추가되었는지 확인
2. **카메라가 플레이어를 따라가지 않음**: ThirdPersonCamera의 Target 설정 확인
3. **상호작용이 작동하지 않음**: InteractionSystem이 Player에 추가되었는지 확인
4. **UI가 보이지 않음**: Canvas와 EventSystem이 생성되었는지 확인

### URP 관련 문제들
1. **오브젝트가 핑크색으로 표시됨**: URP 셰이더를 사용하는 머티리얼로 변경
2. **셰이더 오류**: URP 패키지가 올바르게 설치되었는지 확인
3. **머티리얼이 보이지 않음**: URP 설정에서 머티리얼이 올바르게 렌더링되는지 확인
4. **여전히 Built-in 셰이더 사용**: URPShaderForcer를 사용하여 강제로 변경

### 디버깅 팁
- Console 창에서 로그 메시지 확인
- SceneSetup 스크립트의 Inspector에서 각 옵션 활성화/비활성화
- Gizmos를 활성화하여 오브젝트 위치 및 범위 시각화
- URP 머티리얼 문제는 URPMaterialCreator를 사용하여 해결
- Built-in 셰이더 문제는 URPShaderForcer를 사용하여 강제 해결
- "Test URP Shader Detection"으로 URP 셰이더 감지 상태 확인
- "Check Current Shaders"로 현재 씬의 셰이더 상태 확인
