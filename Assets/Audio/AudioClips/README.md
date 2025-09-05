# 사운드 파일 가이드

이 폴더에는 게임에서 사용할 사운드 파일들을 저장합니다.

## 필요한 사운드 파일들:

### 플레이어 사운드
- `footstep.wav` - 발걸음 소리
- `jump.wav` - 점프 소리
- `land.wav` - 착지 소리
- `run.wav` - 달리기 소리

### 전투 사운드
- `attack.wav` - 공격 소리
- `hit.wav` - 피해 소리
- `block.wav` - 방어 소리
- `weapon_equip.wav` - 무기 장착 소리
- `weapon_unequip.wav` - 무기 해제 소리

### 아이템 사운드
- `item_pickup.wav` - 아이템 획득 소리
- `item_use.wav` - 아이템 사용 소리
- `inventory_open.wav` - 인벤토리 열기 소리
- `inventory_close.wav` - 인벤토리 닫기 소리

### UI 사운드
- `button_click.wav` - 버튼 클릭 소리
- `button_hover.wav` - 버튼 호버 소리
- `notification.wav` - 알림 소리

### 배경음악
- `background_music.wav` - 배경음악
- `combat_music.wav` - 전투음악

## 사운드 파일 형식:
- **포맷**: WAV 또는 OGG
- **샘플레이트**: 44.1kHz
- **비트레이트**: 16bit
- **채널**: 모노 (효과음), 스테레오 (음악)

## 사운드 파일 추가 방법:
1. 이 폴더에 사운드 파일을 복사
2. Unity에서 파일을 선택
3. Import Settings에서 적절한 설정 적용
4. AudioManager의 해당 필드에 드래그 앤 드롭
