# 👣 발자국 사운드 설정 가이드

## 🚨 문제: Inspector에서 사운드 배열이 0으로 표시됨

### **해결 방법 1: 수동 할당 (추천)**

#### **Step 1: AudioManager 오브젝트 선택**
1. **Hierarchy**에서 **AudioManager** 오브젝트 선택
2. **Inspector**에서 **FootstepSoundManager** 컴포넌트 확인

#### **Step 2: Grass 사운드 할당**
1. **Grass Walk Sounds** 배열 크기를 **10**으로 설정
2. **Project** 창에서 `Assets/Footsteps - Essentials/Footsteps_Grass/Footsteps_Grass_Walk` 폴더 열기
3. **WAV 파일들**을 **Grass Walk Sounds** 배열에 드래그 앤 드롭

#### **Step 3: Grass Run Sounds 할당**
1. **Grass Run Sounds** 배열 크기를 **10**으로 설정
2. `Assets/Footsteps - Essentials/Footsteps_Grass/Footsteps_Grass_Run` 폴더에서 파일들 드래그

#### **Step 4: Grass Jump Sounds 할당**
1. **Grass Jump Sounds** 배열 크기를 **10**으로 설정
2. `Assets/Footsteps - Essentials/Footsteps_Grass/Footsteps_Grass_Jump` 폴더에서 파일들 드래그

### **해결 방법 2: Resources 폴더 사용**

#### **Step 1: Resources 폴더에 복사**
1. **Project** 창에서 `Assets/Footsteps - Essentials` 폴더 선택
2. **Ctrl+C**로 복사
3. **Assets/Resources** 폴더로 이동
4. **Ctrl+V**로 붙여넣기

#### **Step 2: 폴더 구조 확인**
```
Assets/Resources/Footsteps - Essentials/
├── Footsteps_Grass/
│   ├── Footsteps_Grass_Walk/
│   ├── Footsteps_Grass_Run/
│   └── Footsteps_Grass_Jump/
├── Footsteps_DirtyGround/
│   ├── Footsteps_DirtyGround_Walk/
│   ├── Footsteps_DirtyGround_Run/
│   └── Footsteps_DirtyGround_Land/
└── ... (다른 지형들)
```

### **해결 방법 3: 자동 복사 스크립트 사용**

#### **Step 1: 스크립트 실행**
1. **AudioManager** 오브젝트 선택
2. **FootstepSoundCopy** 컴포넌트 추가
3. **Context Menu**에서 "발자국 사운드를 Resources로 복사" 실행

## 🎯 빠른 테스트

### **최소 설정 (Grass만)**
1. **Grass Walk Sounds**에 3-5개 파일 할당
2. **Grass Run Sounds**에 3-5개 파일 할당
3. **Grass Jump Sounds**에 3-5개 파일 할당
4. **게임 실행** 후 **WASD**로 이동 테스트

### **전체 설정 (모든 지형)**
각 지형별로 Walk, Run, Jump 사운드를 할당:
- **Dirt** (흙바닥)
- **Gravel** (자갈)
- **Metal** (금속)
- **Wood** (나무)
- **Stone** (바위)
- **Sand** (모래)
- **Snow** (눈)
- **Water** (물)
- **Tile** (타일)
- **Mud** (진흙)
- **Leaves** (잎)

## 🔧 문제 해결

### **사운드가 여전히 0개인 경우**
1. **Console**에서 에러 메시지 확인
2. **파일 경로**가 올바른지 확인
3. **WAV 파일**이 제대로 Import되었는지 확인

### **사운드가 재생되지 않는 경우**
1. **Current Ground Type**이 **Grass**로 설정되어 있는지 확인
2. **볼륨**이 0이 아닌지 확인
3. **Enable Random Variation**이 체크되어 있는지 확인

## 🎵 테스트 방법

### **수동 테스트**
1. **FootstepSoundManager** 컴포넌트에서 **Context Menu** 실행:
   - "테스트 발자국 소리"
   - "테스트 달리기 소리"
   - "테스트 점프 소리"

### **게임 테스트**
1. **게임 실행**
2. **WASD**로 이동 (걷기 소리)
3. **Shift + WASD**로 달리기 (달리기 소리)
4. **Space**로 점프 (점프 소리)

## 📁 파일 구조 예시

```
Assets/
├── Footsteps - Essentials/          # 원본 사운드 폴더
│   ├── Footsteps_Grass/
│   │   ├── Footsteps_Grass_Walk/
│   │   │   ├── grass_walk_01.wav
│   │   │   ├── grass_walk_02.wav
│   │   │   └── ...
│   │   ├── Footsteps_Grass_Run/
│   │   └── Footsteps_Grass_Jump/
│   └── ...
└── Resources/                       # Resources 폴더 (선택사항)
    └── Footsteps - Essentials/
        └── ... (동일한 구조)
```

**이제 Inspector에서 사운드 배열에 파일들을 할당해보세요!** 🎵✨
