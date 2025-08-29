# 월드 배치 가이드 - Open Horizons

## 🏗️ 월드 빌더 시스템 개요

Open Horizons 프로젝트는 월드 배치를 자동화하고 편리하게 할 수 있는 두 가지 월드 빌더를 제공합니다:

1. **WorldBuilder** - 일반적인 월드 생성 (지형, 구조물, 환경)
2. **POLYARTWorldBuilder** - POLYART Ancient Village 에셋 전용

## 🚀 빠른 시작

### 1. 기본 월드 생성
```
1. 빈 GameObject 생성 → "WorldBuilder" 이름 설정
2. WorldBuilder 스크립트 추가
3. Inspector에서 "Generate World" 버튼 클릭
```

### 2. POLYART 월드 생성
```
1. 빈 GameObject 생성 → "POLYARTWorldBuilder" 이름 설정
2. POLYARTWorldBuilder 스크립트 추가
3. POLYART 에셋들을 해당 배열에 할당
4. Inspector에서 "Build POLYART World" 버튼 클릭
```

## 🎯 월드 빌더 기능

### WorldBuilder (일반 월드)
- **지형 생성**: Perlin 노이즈 기반 자연스러운 지형
- **구조물 배치**: 랜덤 위치에 건물들 자동 배치
- **환경 오브젝트**: 나무, 바위, 풀 등 자연 요소
- **경로 생성**: 연결된 경로 시스템
- **스폰 포인트**: 플레이어 시작 지점

### POLYARTWorldBuilder (POLYART 전용)
- **건물 배치**: 고대 마을 건물들 자동 배치
- **장식 요소**: 분수, 조각상, 가로등 등
- **자연 요소**: 나무, 꽃, 바위 등
- **경로 시스템**: 마을 내 연결된 길
- **마을 중심부**: 중심부 우선 배치 옵션

## ⚙️ 설정 옵션

### 월드 크기 설정
```csharp
worldSize = new Vector2(100, 100);  // X, Z 크기
worldHeight = 10f;                   // Y 높이
```

### 배치 밀도 조절
```csharp
buildingCount = 8;       // 건물 개수
decorationCount = 20;    // 장식 개수
natureCount = 30;        // 자연 오브젝트 개수
pathCount = 4;           // 경로 개수
```

### 거리 설정
```csharp
minBuildingDistance = 15f;     // 건물 간 최소 거리
minDecorationDistance = 5f;    // 장식 간 최소 거리
minNatureDistance = 3f;        // 자연 오브젝트 간 최소 거리
```

## 🎨 고급 기능

### 1. 랜덤 시드 시스템
```csharp
useRandomSeed = true;    // 자동 랜덤 시드
seed = 12345;            // 고정 시드 (원하는 레이아웃 재현)
```

### 2. 지형 맞춤 배치
```csharp
alignToGround = true;    // 지형에 맞춰 자동 배치
groundOffset = 0.1f;     // 지면에서의 오프셋
```

### 3. 중심부 우선 배치
```csharp
// POLYARTWorldBuilder에서만 사용 가능
"Generate Village Center" - 마을 중심부 우선 생성
```

## 🔧 사용 팁

### 1. 단계별 월드 구성
```
1단계: "Generate Simple World" - 기본 지형만 생성
2단계: "Generate Full World" - 모든 요소 포함
3단계: 수동으로 특정 영역 조정
```

### 2. POLYART 에셋 최적화
```
1. 에셋을 카테고리별로 분류 (건물, 장식, 자연, 경로)
2. 각 배열에 적절한 프리팹 할당
3. "Optimize World" 실행으로 성능 최적화
```

### 3. 레이아웃 조정
```
1. 시드 값을 변경하여 다른 레이아웃 생성
2. 거리 설정으로 밀도 조절
3. 개별 오브젝트 수동 이동/회전
```

## 📁 파일 구조

```
Assets/Scripts/Setup/
├── WorldBuilder.cs              # 일반 월드 빌더
├── POLYARTWorldBuilder.cs       # POLYART 전용 빌더
└── WorldBuildingGuide.md        # 이 가이드 문서
```

## 🎮 컨텍스트 메뉴

### WorldBuilder
- **Generate World**: 전체 월드 생성
- **Generate Simple World**: 기본 지형만 생성
- **Generate Full World**: 모든 요소 포함 생성
- **Clear World**: 생성된 월드 정리

### POLYARTWorldBuilder
- **Build POLYART World**: POLYART 월드 구성
- **Generate Village Center**: 마을 중심부 생성
- **Clear World**: 배치된 월드 정리
- **Optimize World**: 월드 최적화

## ⚠️ 주의사항

### 1. 성능 고려사항
- 너무 많은 오브젝트 생성 시 성능 저하 가능
- 환경 오브젝트 밀도는 적절히 조절
- LOD 시스템 활용 권장

### 2. 메모리 관리
- 불필요한 오브젝트는 "Clear World"로 정리
- 프리팹 배열에 null 값이 없도록 주의
- 에셋 크기와 복잡도 고려

### 3. URP 호환성
- 모든 생성된 머티리얼은 URP 셰이더 사용
- URPShaderForcer와 함께 사용 권장
- 핑크색 문제 발생 시 자동 해결

## 🔍 문제 해결

### 1. 오브젝트가 겹침
```
- minDistance 값 증가
- 시드 변경으로 다른 레이아웃 생성
- 수동으로 개별 오브젝트 조정
```

### 2. 지형에 맞지 않음
```
- alignToGround = true 확인
- groundOffset 값 조정
- 지형 콜라이더 존재 확인
```

### 3. 성능 문제
```
- 오브젝트 개수 줄이기
- "Optimize World" 실행
- LOD 그룹 추가
- 오클루전 컬링 설정
```

## 🚀 다음 단계

월드 구성이 완료되면 다음 단계로 진행할 수 있습니다:

1. **수집품 시스템**: 월드 곳곳에 수집 가능한 아이템 배치
2. **퍼즐 시스템**: 특정 건물이나 장소에 퍼즐 요소 추가
3. **스토리 진행**: 환경 스토리텔링 요소 배치
4. **AI 시스템**: NPC나 적 캐릭터 배치
5. **퀘스트 시스템**: 퀘스트 관련 오브젝트 배치

## 💡 창작 팁

- **테마별 구역**: 마을, 숲, 폐허 등 테마별로 구역 분할
- **높이 변화**: 건물과 지형의 높이 차이로 입체감 연출
- **경로 연결**: 주요 건물들을 경로로 연결하여 탐험 흥미 증대
- **시각적 포인트**: 중심부나 특별한 장소에 시각적 포인트 배치
- **환경 스토리**: 오브젝트 배치로 환경 스토리텔링 구현

이 가이드를 활용하여 멋진 오픈 월드를 구성해보세요! 🎮✨
