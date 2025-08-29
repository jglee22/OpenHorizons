# 🌲 LowpolyNatureWorldBuilder 사용 가이드

## 📋 개요

LowpolyNatureWorldBuilder는 LowpolyNatureBundle 에셋들을 위한 **스마트 배치 시스템**입니다. 
단순한 랜덤 배치가 아닌, 지형 분석과 충돌 방지를 통한 자연스러운 월드 생성을 제공합니다.

## ✨ 주요 기능

### 🌸 계절별 테마 시스템
- **봄 (Spring)**: 생동감 넘치는 초록색 나무들
- **가을 (Autumn)**: 따뜻한 주황/노란색 나무들
- **겨울 (Winter)**: 차가운 하늘색/흰색 나무들
- **혼합 (Mixed)**: 모든 계절의 요소를 조합

### 🌲 생태계별 생성
- **숲 생태계**: 나무와 관목을 집중적으로 배치
- **산악 생태계**: 높은 지형에 바위와 통나무 배치
- **초원 생태계**: 평평한 지형에 관목과 소품 배치

### 🎯 스마트 배치 시스템
- **그리드 기반 공간 관리**: 5m x 5m 셀로 월드 분할
- **지형 분석**: 높이와 경사에 따른 자동 분류
- **충돌 방지**: 오브젝트 간 최소 거리 보장
- **자연스러운 그룹화**: 숲, 산맥, 초원 등 패턴 기반 배치

## 🚀 빠른 시작

### 1단계: 월드 빌더 생성
```
1. Unity 씬에서 빈 GameObject 생성
2. "LowpolyNatureWorldBuilder" 이름 설정
3. LowpolyNatureWorldBuilder 스크립트 추가
```

### 2단계: 프리팹 할당
Inspector에서 다음 배열들에 해당하는 프리팹들을 할당하세요:

#### 🌱 계절별 나무
```
Spring Trees: SpringTree_01 ~ SpringTree_07 (7개)
Autumn Trees: AutumnTree_01 ~ AutumnTree_06 (6개)
Winter Trees: WinterTree_01 ~ WinterTree_06 (6개)
```

#### 🏔️ 지형 요소
```
Rocks: Rock_01 ~ Rock_06 (6개)
Trunks: Trunk_01 ~ Trunk_03 (3개)
Fallen Trees: FallenTree_01 ~ FallenTree_04 (4개)
```

#### 🌿 식물 및 장식
```
Bushes: Bush_01 ~ Bush_04 (4개)
Bamboos: Bamboo_01 ~ Bamboo_03 (3개)
Mushrooms: Mushroom_01 ~ Mushroom_16 (16개)
Potted Plants: PottedPlants_01 (1개)
```

#### 🏗️ 구조물 및 소품
```
Wooden Fences: WoodenFence_01 ~ WoodenFence_03 (3개)
Brick Walls: BrickWall_01 ~ BrickWall_02 (2개)
Furniture: WoodenTable_01, WoodenStool_01
Props: BonFire_01, WateringKettle_01
```

### 3단계: 월드 생성
Inspector 하단의 버튼들을 사용하여 월드를 생성하세요:

#### 🌸 계절별 테마
- **🌸 봄 테마 월드 생성**: 생동감 넘치는 초록색 월드
- **🍂 가을 테마 월드 생성**: 따뜻한 주황/노란색 월드
- **❄️ 겨울 테마 월드 생성**: 차가운 하늘색/흰색 월드
- **🌈 혼합 테마 월드 생성**: 모든 계절의 요소 조합

#### 🌲 생태계별 생성
- **🌲 숲 생태계 생성**: 나무와 관목 집중 배치
- **🏔️ 산악 생태계 생성**: 바위와 통나무 집중 배치
- **🌾 초원 생태계 생성**: 관목과 소품 집중 배치

#### 🔧 유틸리티
- **🌍 스마트 월드 생성**: 현재 설정으로 전체 월드 생성
- **🗑️ 월드 정리**: 생성된 모든 오브젝트 제거
- **⚡ 월드 최적화**: 성능 최적화 설정 적용

## ⚙️ 설정 옵션

### 🌍 월드 설정
```
World Size: 월드 크기 (기본값: 100x100)
Grid Size: 그리드 셀 크기 (기본값: 5m)
Ground Layer: 지형 레이어 (기본값: Default)
```

### 🎨 테마 설정
```
Current Season: 현재 계절 테마
Tree Density: 나무 밀도 (0.1 ~ 0.5)
Rock Density: 바위 밀도 (0.1 ~ 0.4)
Decoration Density: 장식 밀도 (0.2 ~ 0.6)
```

### 📐 배치 규칙
```
Min Tree Distance: 나무 간 최소 거리 (기본값: 4m)
Min Rock Distance: 바위 간 최소 거리 (기본값: 3m)
Min Structure Distance: 구조물 간 최소 거리 (기본값: 8m)
Min Decoration Distance: 장식 간 최소 거리 (기본값: 2m)
```

## 🔍 작동 원리

### 1. 월드 그리드 초기화
```
월드를 5m x 5m 크기의 셀로 분할
각 셀의 위치와 상태 정보 저장
```

### 2. 지형 분석
```
각 셀에서 지형 높이 측정
높이에 따른 지형 타입 자동 분류:
- Water (물): 높이 < 0m
- Plain (평지): 높이 0-2m
- Forest (숲): 높이 2-5m
- Mountain (산): 높이 5-10m
- Valley (계곡): 높이 > 10m
```

### 3. 스마트 배치
```
각 오브젝트 타입별로 적합한 위치 탐색
충돌 방지 및 최소 거리 규칙 적용
지형 타입에 따른 배치 제한
자연스러운 그룹화 및 패턴 생성
```

### 4. 충돌 방지 시스템
```
그리드 기반 빠른 충돌 감지
오브젝트별 최소 거리 보장
지형 타입별 배치 제한
```

## 💡 사용 팁

### 🎯 최적의 설정값
```
Tree Density: 0.3 (적당한 숲 밀도)
Rock Density: 0.2 (자연스러운 바위 분포)
Decoration Density: 0.4 (풍부한 장식 요소)
Min Tree Distance: 4m (자연스러운 나무 간격)
```

### 🌲 생태계별 최적화
```
숲 생태계: Tree Density를 0.4로 증가
산악 생태계: Rock Density를 0.3으로 증가
초원 생태계: Decoration Density를 0.5로 증가
```

### ⚡ 성능 최적화
```
Grid Size를 10m로 증가하여 셀 수 감소
Tree Density를 0.2로 감소하여 오브젝트 수 제한
월드 크기를 50x50으로 축소하여 처리 속도 향상
```

## 🚨 문제 해결

### ❌ 오브젝트가 겹치는 문제
```
Min Distance 값을 증가시켜보세요
Grid Size를 줄여서 더 세밀한 제어를 하세요
```

### ❌ 나무가 너무 많거나 적은 문제
```
Tree Density 값을 조정해보세요
Min Tree Distance 값을 변경해보세요
```

### ❌ 특정 지형에 오브젝트가 배치되지 않는 문제
```
Ground Layer 설정을 확인하세요
지형의 Collider가 제대로 설정되어 있는지 확인하세요
```

### ❌ 성능이 느린 문제
```
월드 크기를 줄여보세요
Grid Size를 늘려보세요
오브젝트 밀도를 줄여보세요
```

## 🔧 고급 기능

### 📊 월드 통계 확인
```
Console에서 월드 생성 로그 확인
생성된 오브젝트 수와 타입별 분포 확인
```

### 🎨 커스텀 테마 생성
```
계절별 프리팹 배열을 조합하여 새로운 테마 생성
밀도 설정을 조정하여 다양한 분위기 연출
```

### 🌍 대규모 월드 생성
```
월드 크기를 200x200 이상으로 설정
Grid Size를 10m로 증가하여 성능 최적화
```

## 📚 예제 시나리오

### 🌸 봄 정원 만들기
```
1. Current Season을 Spring으로 설정
2. Tree Density를 0.4로 증가
3. Decoration Density를 0.5로 증가
4. "🌸 봄 테마 월드 생성" 버튼 클릭
```

### 🍂 가을 숲 만들기
```
1. Current Season을 Autumn으로 설정
2. Tree Density를 0.5로 증가
3. "🌲 숲 생태계 생성" 버튼 클릭
4. "🍂 가을 테마 월드 생성" 버튼 클릭
```

### ❄️ 겨울 산맥 만들기
```
1. Current Season을 Winter으로 설정
2. Rock Density를 0.4로 증가
3. "🏔️ 산악 생태계 생성" 버튼 클릭
4. "❄️ 겨울 테마 월드 생성" 버튼 클릭
```

## 🎉 마무리

LowpolyNatureWorldBuilder를 사용하면 단순한 랜덤 배치가 아닌, 
자연스럽고 아름다운 월드를 자동으로 생성할 수 있습니다.

**핵심은 프리팹 배열에 적절한 에셋들을 할당하고, 
원하는 테마와 생태계 버튼을 클릭하는 것입니다!**

즐거운 월드 빌딩 되세요! 🌲✨
