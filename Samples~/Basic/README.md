# Basic Sample — AchReactive

`on_use` 리액션으로 스킬을 실행하는 최소 예시입니다.

## 구성

| 파일 | 역할 |
|---|---|
| `SampleEntity.cs` | `IEntity` 구현 — `StatSheet` 노출, hp 로 생사 판정 |
| `SampleWorld.cs` | `IWorld` 구현 — 반경 조회/스폰 |
| `SampleBootstrap.cs` | 레지스트리 초기화 + 데이터 적재 + Space 키로 스킬 실행 |
| `skills.json` | reaction 형식 스킬 데이터(최상위 배열) |

## 실행 방법

1. 빈 씬에 GameObject 두 개를 만들고 각각 `SampleEntity` 를 붙입니다(caster / target).
   - target 의 `hpMax` 와 `hp` 를 같게(예: 100) 설정합니다.
2. 또 다른 GameObject 에 `SampleBootstrap` 을 붙입니다.
   - `skillsJson` 에 이 폴더의 `skills.json` 을 `TextAsset` 으로 드래그합니다.
   - `caster` / `target` 에 위 두 엔티티를 연결합니다.
3. 플레이 후 **Space** 키를 누르면 콘솔에 데미지 로그와 남은 HP 가 출력됩니다.

## 동작 흐름

```
Space → engine.Run("on_use", fire_slash, ctx)
        → 조건 alive 통과
        → effect damage 실행 (atk*power - def 만큼 hp 감소)
        → effect log 출력
```

`damage`, `heal` 같은 기본 effect 는 패키지가 제공하므로 별도 코드가 필요 없습니다.
새 동작이 필요하면 `[Effect("이름")]` 정적 메서드 하나만 추가하면 자동 등록됩니다.
