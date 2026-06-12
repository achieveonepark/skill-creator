# Basic Setup 샘플

SkillForge 를 씬에서 구동하는 최소 예제입니다.

## 포함 파일

- `SampleBattleUnit.cs` — `IBattleUnit` 최소 구현 (MonoBehaviour)
- `SampleUnitRegistry.cs` — `IUnitRegistry` 구현 (광역 타겟팅)
- `SampleCombatBootstrap.cs` — JSON 로드 → `SkillSystem` 구성 → 스킬 사용
- `skills.json`, `buffs.json` — 예제 데이터

## 사용 방법

1. 빈 GameObject 에 `SampleUnitRegistry` 와 `SampleCombatBootstrap` 을 추가합니다.
2. 씬에 `SampleBattleUnit` 을 여러 개 배치하고 `Team` 값을 다르게 설정합니다.
3. `skills.json`, `buffs.json` 을 `StreamingAssets/GameData/` 에 복사합니다.
4. Bootstrap 의 `Caster` / `Target` 을 연결합니다.
5. 플레이 후 **스페이스바**로 `fire_slash` 를 사용합니다. 콘솔에서 실행 로그를 확인하세요.
