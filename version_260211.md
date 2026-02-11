프로젝트 상태 정리 (스네이크의 모험 / Unity 6.3)
1. 프로젝트 개요

Unity 6.3 / URP

3D Snake 게임

이동 방식: FPS 스타일 부드러운 이동 + 트레일 기반 몸통 추적

입력: New Input System (SnakeInputHandler)

구조:

SnakeRoot
 ├─ HeadSocket
 │   └─ HeadPrefab
 │       ├─ Visual
 │       ├─ HitCollider
 │       └─ EatTrigger (Trigger)
 └─ BodyContainer

2. 핵심 스크립트 구성
✔ SnakeMover

전진/회전/회전각 제한 정상 동작

boostHeld(입력 부스트) + 음식 버프 부스트 동시 적용

ApplySpeedBoost(mul, duration) 사용

버프 중복 시 시간 연장, 배수는 고정

DummyBana를 먹었을 때만 부스트 적용

✔ SnakeInputHandler

move.x = 회전

move.y = 전진

boostHeld = 수동 부스트

입력 안정화(Vector2.ClampMagnitude) 적용

SnakeMover와 정상 연동

✔ HeadEatFood

IFoodConsumer.Consume() 기반

Destroy는 FoodPickup 쪽에서만 처리

과일 효과:

Apple

몸통 +1

점수 증가

Banana

일반 과일

부스트 ❌

DummyBana

몸통 +2

부스트 ⭕ (ApplySpeedBoost)

Consume 중복 문제 없음

3. Food 시스템 설계 (중요)
🎯 목표

필드에 항상 N개의 과일 유지 (스테이지별)

과일 종류 비율 유지

플레이어가 원하는 과일을 선택해서 먹을 수 있음

특정 과일이 너무 많거나 적어지지 않음

🍎🍌🍌 과일 종류 정의
FruitType
- Apple
- Banana       // 일반 바나나 (부스트 없음)
- DummyBana    // 특수 바나나

4. FoodSpawner 동작 규칙 (최종 확정)
📌 기본 필드 구성 (예: N = 10)

Apple: 7

Banana: 3

DummyBana: 0

📌 Banana → DummyBana 전환 규칙

Banana를 10개 먹어야만

DummyBana 1개가 필드에 등장

이때 필드 구성은:

Banana 2

DummyBana 1

DummyBana는 항상 최대 1개만 존재

📌 DummyBana 처리

DummyBana를 먹으면:

몸통 +2

부스트 획득

DummyBana가 실제로 스폰되는 순간

dummyPending = false 처리

→ 다시 Banana 10개를 먹기 전까지 Dummy는 나오지 않음

Dummy를 먹고 나면 다시 기본 상태(바나나 3개)로 복귀

📌 중요한 버그 해결 포인트 (기억용)

Dummy가 시작부터 나오던 문제 원인:

dummyPending을 스폰 후 false로 내리지 않았기 때문

해결:

Dummy 스폰 직후 dummyPending = false;

현재 이 문제 완전히 해결됨

5. FoodSpawner 역할 정리

단일 스폰 ❌

필드 재고 관리자 역할

기능:

필드 총 개수 유지

과일 비율 유지

Banana 누적 카운트 관리

Dummy 전환 규칙 관리

Destroy → 지연 후 EnsureFoodCount() 호출

6. 현재 상태 요약

✅ 이동 / 회전 정상
✅ 몸통 증가 정상
✅ 입력 시스템 안정
✅ Food 스폰 / 제거 / 재생성 정상
✅ Banana → DummyBana 전환 로직 정상
✅ Dummy 중복 생성 문제 해결
✅ Dummy는 10개 먹어야만 생성됨