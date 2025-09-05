# 🧪 어드레서블 테스트 가이드

## 🚀 빠른 테스트 방법

### **Step 1: AudioManager 생성**
1. **빈 오브젝트** 생성
2. **AudioManagerSpawner** 스크립트 추가
3. **게임 실행** 또는 **Context Menu**에서 "AudioManager 생성"

### **Step 2: 자동 테스트 실행**
게임 실행 시 **자동으로 테스트**가 실행됩니다:

```
=== 어드레서블 테스트 시작 ===
1. 어드레서블 설정 확인 중...
2. 사운드 로드 테스트 중...
3. 사운드 재생 테스트 중...
4. 메모리 해제 테스트 중...
=== 어드레서블 테스트 완료 ===
```

### **Step 3: 수동 테스트**
게임 실행 후 다음 키들을 사용하여 테스트:

```
L키 - 사운드 로드 테스트
P키 - 사운드 재생 테스트
R키 - 메모리 해제 테스트
```

## 🔧 상세 테스트 방법

### **1. 어드레서블 설정 확인**

#### **자동 확인**
- 게임 실행 시 자동으로 어드레서블 설정 확인
- **Console**에서 결과 확인

#### **수동 확인**
1. **Window → Asset Management → Addressables → Groups** 열기
2. **Footstep Sounds** 그룹 확인
3. **레이블** 목록 확인

### **2. 사운드 로드 테스트**

#### **자동 로드**
- 게임 시작 시 `footstep_grass_walk` 레이블로 자동 로드
- **Console**에서 로딩 진행률 및 결과 확인

#### **수동 로드**
1. **L키** 누르기
2. **Console**에서 로딩 결과 확인
3. **Inspector**에서 로드된 사운드 개수 확인

### **3. 사운드 재생 테스트**

#### **자동 재생**
- 로드된 사운드들을 순차적으로 재생
- 각 사운드의 길이만큼 대기 후 다음 사운드 재생

#### **수동 재생**
1. **P키** 누르기
2. 로드된 사운드 중 랜덤하게 선택하여 재생
3. **스피커/헤드폰**에서 사운드 확인

### **4. 메모리 해제 테스트**

#### **자동 해제**
- 테스트 완료 후 자동으로 메모리 해제
- **Console**에서 해제 결과 확인

#### **수동 해제**
1. **R키** 누르기
2. **Console**에서 해제 결과 확인
3. **Inspector**에서 로드된 사운드 개수 0으로 변경 확인

## 🎯 테스트 시나리오

### **시나리오 1: 기본 테스트**
1. **게임 실행**
2. **자동 테스트** 완료 대기
3. **Console**에서 모든 테스트 통과 확인
4. **사운드 재생** 확인

### **시나리오 2: 수동 테스트**
1. **게임 실행**
2. **L키**로 사운드 로드
3. **P키**로 사운드 재생 (여러 번)
4. **R키**로 메모리 해제
5. **P키**로 재생 시도 (실패해야 함)

### **시나리오 3: 에러 테스트**
1. **잘못된 레이블** 설정
2. **L키**로 로드 시도
3. **Console**에서 에러 메시지 확인

## 🔍 문제 해결

### **어드레서블 설정이 없는 경우**
```
❌ 어드레서블 설정을 찾을 수 없습니다!
Window > Asset Management > Addressables > Groups를 먼저 열어주세요.
```
**해결방법:**
1. **Window → Asset Management → Addressables → Groups** 열기
2. **Create → New Group** 클릭
3. **그룹 이름**: "Footstep Sounds"

### **발자국 그룹이 없는 경우**
```
⚠️ 발자국 관련 그룹을 찾을 수 없습니다.
Tools > Audio System > Setup Addressable Footstep Sounds를 실행하세요.
```
**해결방법:**
1. **Tools → Audio System → Setup Addressable Footstep Sounds** 실행
2. **어드레서블 설정 시작** 클릭

### **사운드 로드 실패**
```
❌ footstep_grass_walk 로드 실패: ...
```
**해결방법:**
1. **어드레서블 설정** 확인
2. **레이블 이름** 확인
3. **사운드 파일** 경로 확인

### **사운드가 재생되지 않는 경우**
1. **볼륨** 확인
2. **AudioManager** 설정 확인
3. **스피커/헤드폰** 연결 확인

## 📊 성능 모니터링

### **메모리 사용량**
- **Profiler** 창에서 메모리 사용량 모니터링
- **로드 전후** 메모리 변화 확인
- **해제 후** 메모리 복구 확인

### **로딩 시간**
- **Console**에서 로딩 시간 확인
- **병렬 로딩** 효과 확인
- **캐싱** 효과 확인

### **에러 로그**
- **Console**에서 에러 메시지 확인
- **경고 메시지** 확인
- **디버그 정보** 확인

## 🎵 실제 사용 테스트

### **AddressableFootstepManager 테스트**
1. **AudioManager** 오브젝트 선택
2. **AddressableFootstepManager** 컴포넌트 확인
3. **Context Menu**에서 테스트 실행:
   - "테스트 발자국 소리"
   - "테스트 달리기 소리"
   - "테스트 점프 소리"

### **PlayerController 연동 테스트**
1. **게임 실행**
2. **WASD**로 이동 (걷기 소리)
3. **Shift + WASD**로 달리기 (달리기 소리)
4. **Space**로 점프 (점프 소리)

## 🚀 고급 테스트

### **다양한 레이블 테스트**
```csharp
// AddressableTestController에서 레이블 변경
[SerializeField] private string testLabel = "footstep_grass_walk";
```

### **대용량 로딩 테스트**
```csharp
// 여러 레이블 동시 로딩
private string[] testLabels = {
    "footstep_grass_walk",
    "footstep_grass_run",
    "footstep_grass_jump"
};
```

### **성능 벤치마크**
```csharp
// 로딩 시간 측정
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
// ... 로딩 코드 ...
stopwatch.Stop();
Debug.Log($"로딩 시간: {stopwatch.ElapsedMilliseconds}ms");
```

**이제 어드레서블 시스템을 완전히 테스트할 수 있습니다!** 🎯✨
