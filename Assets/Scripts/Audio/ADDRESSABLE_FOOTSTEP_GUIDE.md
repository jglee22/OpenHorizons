# 🎯 어드레서블 발자국 사운드 시스템 가이드

## 🚀 어드레서블 시스템의 장점

### **메모리 효율성**
- ✅ **필요한 사운드만 로드**
- ✅ **메모리 사용량 최적화**
- ✅ **자동 메모리 해제**

### **로딩 성능**
- ✅ **비동기 로딩**
- ✅ **병렬 처리**
- ✅ **캐싱 지원**

### **관리 편의성**
- ✅ **레이블 기반 관리**
- ✅ **자동 그룹화**
- ✅ **에디터 도구 지원**

## 🛠️ 설정 방법

### **Step 1: 어드레서블 패키지 설치**
1. **Window → Package Manager** 열기
2. **Unity Registry** 선택
3. **Addressables** 검색 및 설치

### **Step 2: 어드레서블 그룹 생성**
1. **Window → Asset Management → Addressables → Groups** 열기
2. **Create → New Group** 클릭
3. **그룹 이름**: "Footstep Sounds"

### **Step 3: 발자국 사운드 설정**
1. **Tools → Audio System → Setup Addressable Footstep Sounds** 실행
2. **소스 경로**: `Assets/Footsteps - Essentials`
3. **그룹 이름**: `Footstep Sounds`
4. **레이블 생성**: ✅ 체크
5. **어드레서블 설정 시작** 클릭

### **Step 4: 레이블 확인**
설정 완료 후 다음 레이블들이 생성됩니다:
```
footstep_grass_walk
footstep_grass_run
footstep_grass_jump
footstep_dirt_walk
footstep_dirt_run
footstep_dirt_land
footstep_gravel_walk
footstep_gravel_run
footstep_gravel_jump
... (총 36개 레이블)
```

## 🎮 사용 방법

### **Step 1: AudioManager 생성**
1. **빈 오브젝트** 생성
2. **AudioManagerSpawner** 스크립트 추가
3. **게임 실행** 또는 **Context Menu**에서 "AudioManager 생성"

### **Step 2: AddressableFootstepManager 설정**
1. **AudioManager** 오브젝트 선택
2. **AddressableFootstepManager** 컴포넌트 확인
3. **Load On Start**: ✅ 체크 (자동 로드)
4. **Enable Caching**: ✅ 체크 (캐싱 활성화)

### **Step 3: 지형 설정**
1. **Current Ground Type** 드롭다운에서 지형 선택
2. **게임 실행** 후 **WASD**로 이동 테스트

## 🔧 고급 설정

### **레이블 커스터마이징**
```csharp
// AddressableFootstepManager에서 레이블 변경
[SerializeField] private string grassWalkLabel = "footstep_grass_walk";
[SerializeField] private string grassRunLabel = "footstep_grass_run";
```

### **수동 로딩**
```csharp
// 특정 레이블의 사운드만 로드
await addressableFootstepManager.LoadSpecificSounds("footstep_grass_walk");
```

### **캐시 관리**
```csharp
// 모든 사운드 해제
addressableFootstepManager.ReleaseSounds();
```

## 📊 성능 최적화

### **로딩 전략**
1. **Load On Start**: 게임 시작 시 모든 사운드 로드
2. **Lazy Loading**: 필요할 때만 로드
3. **Hybrid**: 자주 사용하는 사운드만 미리 로드

### **메모리 관리**
1. **Enable Caching**: 자주 사용하는 사운드 캐싱
2. **Release Sounds**: 사용하지 않는 사운드 해제
3. **Label-based Loading**: 특정 지형의 사운드만 로드

## 🎵 테스트 방법

### **자동 테스트**
1. **게임 실행**
2. **WASD**로 이동 (걷기 소리)
3. **Shift + WASD**로 달리기 (달리기 소리)
4. **Space**로 점프 (점프 소리)

### **수동 테스트**
1. **AddressableFootstepManager** 컴포넌트에서 **Context Menu** 실행:
   - "테스트 발자국 소리"
   - "테스트 달리기 소리"
   - "테스트 점프 소리"
   - "사운드 초기화"

### **디버깅**
1. **Console**에서 로딩 상태 확인
2. **Addressables Groups** 창에서 설정 확인
3. **Profiler**에서 메모리 사용량 모니터링

## 🔍 문제 해결

### **사운드가 로드되지 않는 경우**
1. **어드레서블 설정** 확인
2. **레이블 이름** 확인
3. **그룹 할당** 확인
4. **Console**에서 에러 메시지 확인

### **메모리 사용량이 높은 경우**
1. **Enable Caching** 해제
2. **불필요한 사운드** 해제
3. **레이블별 로딩** 사용

### **로딩이 느린 경우**
1. **병렬 로딩** 확인
2. **네트워크 상태** 확인
3. **캐시 설정** 확인

## 📁 파일 구조

```
Assets/
├── Footsteps - Essentials/          # 원본 사운드
│   ├── Footsteps_Grass/
│   ├── Footsteps_Dirt/
│   └── ...
├── Scripts/Audio/
│   ├── AddressableFootstepManager.cs
│   ├── AddressableFootstepSetup.cs
│   └── ...
└── AddressableAssetsData/           # 어드레서블 설정
    ├── AddressableAssetSettings.asset
    └── ...
```

## 🎯 우선순위 시스템

PlayerController는 다음 순서로 사운드 시스템을 사용합니다:

1. **AddressableFootstepManager** (최우선)
2. **FootstepSoundManager** (백업)
3. **AudioManager** (기본)

## 🚀 확장 가능성

### **추가 기능**
- **지형 감지**: 자동으로 지형 타입 변경
- **거리 기반 로딩**: 플레이어 근처의 사운드만 로드
- **동적 지형**: 런타임에 지형 변경
- **사운드 믹싱**: 지형별 사운드 믹싱

### **최적화 기법**
- **오브젝트 풀링**: AudioSource 재사용
- **LOD 시스템**: 거리에 따른 사운드 품질 조절
- **압축 설정**: 사운드 파일 압축 최적화

**이제 어드레서블 시스템으로 효율적인 발자국 사운드를 사용할 수 있습니다!** 🎵✨
