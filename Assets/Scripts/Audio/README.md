# 사운드 시스템 가이드

## 개요
이 사운드 시스템은 Unity 게임에서 오디오를 관리하는 완전한 솔루션입니다.

## 주요 컴포넌트

### 1. AudioManager
- **역할**: 모든 사운드 재생을 중앙에서 관리
- **특징**: 
  - Singleton 패턴으로 전역 접근 가능
  - 음악, 효과음, UI 사운드 분리 관리
  - 볼륨 조절 기능
  - 사운드 재생/정지/일시정지 기능

### 2. SoundEffect
- **역할**: 개별 사운드 효과 관리
- **특징**:
  - 3D/2D 사운드 지원
  - 피치/볼륨 랜덤화
  - 거리 기반 감쇠 설정

### 3. AudioManagerSetup
- **역할**: AudioManager 자동 설정
- **특징**: 오디오 소스 자동 생성 및 연결

### 4. AudioTestGenerator
- **역할**: 테스트용 사운드 생성
- **특징**: 실제 사운드 파일 없이도 테스트 가능

## 사용 방법

### 1. AudioManager 설정
```csharp
// 사운드 재생
AudioManager.Instance.PlaySFX("attack");
AudioManager.Instance.PlayUISFX("button_click");

// 음악 재생
AudioManager.Instance.PlayBackgroundMusic();
AudioManager.Instance.PlayCombatMusic();

// 볼륨 조절
AudioManager.Instance.SetSFXVolume(0.8f);
AudioManager.Instance.SetMusicVolume(0.5f);
```

### 2. 사운드 파일 추가
1. `Assets/Audio/AudioClips/` 폴더에 사운드 파일 추가
2. AudioManager 컴포넌트의 해당 필드에 드래그 앤 드롭
3. Import Settings에서 적절한 설정 적용

### 3. 프리팹 사용
1. `Assets/Audio/AudioManager.prefab`을 씬에 드래그
2. AudioTestGenerator가 자동으로 테스트 사운드 생성
3. 실제 사운드 파일로 교체

## 지원하는 사운드 타입

### 플레이어 사운드
- `footstep` - 발걸음
- `jump` - 점프
- `land` - 착지
- `run` - 달리기

### 전투 사운드
- `attack` - 공격
- `hit` - 피해
- `block` - 방어
- `weapon_equip` - 무기 장착
- `weapon_unequip` - 무기 해제

### 아이템 사운드
- `item_pickup` - 아이템 획득
- `item_use` - 아이템 사용
- `inventory_open` - 인벤토리 열기
- `inventory_close` - 인벤토리 닫기

### UI 사운드
- `button_click` - 버튼 클릭
- `button_hover` - 버튼 호버
- `notification` - 알림

## 통합된 시스템들

### PlayerController
- 이동, 점프, 공격, 방어 시 사운드 재생
- 발걸음 소리 자동 처리

### CombatTestManager
- 무기 획득, 장착, 해제 시 사운드 재생

### InventoryUI
- 인벤토리 열기/닫기 시 사운드 재생

## 설정 팁

### 사운드 파일 최적화
- **효과음**: WAV, 44.1kHz, 16bit, 모노
- **음악**: OGG, 44.1kHz, 16bit, 스테레오
- **길이**: 효과음은 0.1-2초, 음악은 30초-5분

### 성능 최적화
- 자주 사용하는 사운드는 미리 로드
- 사용하지 않는 사운드는 언로드
- 오디오 소스 풀링 고려

### 디버깅
- AudioManager의 Debug 로그 활성화
- 사운드 재생 상태 확인
- 볼륨 레벨 체크

## 문제 해결

### 사운드가 재생되지 않는 경우
1. AudioManager가 씬에 있는지 확인
2. 사운드 파일이 할당되었는지 확인
3. 볼륨이 0이 아닌지 확인
4. 오디오 소스가 올바르게 설정되었는지 확인

### 성능 문제
1. 동시에 재생되는 사운드 수 제한
2. 사운드 파일 크기 최적화
3. 불필요한 사운드 정리

## 확장 가능성

### 추가 기능
- 오디오 믹서 그룹 지원
- 사운드 이펙트 (리버브, 에코 등)
- 동적 음악 시스템
- 음성 대화 시스템

### 커스터마이징
- 새로운 사운드 타입 추가
- 사운드 이벤트 시스템
- 지역별 사운드 설정
