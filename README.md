# SkillForge

Unity에서 **스킬, 버프, 타겟팅, 조건, 효과를 데이터(JSON) 조합으로 제작하고 검증**하는 콘텐츠 제작 도구입니다.

스킬 하나를 거대한 클래스로 만들지 않고, 작은 기능 조각들을 데이터로 조합합니다. `ScriptableObject` 의존 없이 JSON / Spreadsheet / Remote Data 기반으로 운용할 수 있습니다.

```text
Trigger → Targeting → Condition → Effect → Result
```

> 📖 전체 문서: **https://docs.somiri.dev/skill-creator**

## 설치

Unity Package Manager → **Add package from git URL**:

```
https://github.com/achieveonepark/skill-creator.git
```

또는 `Packages/manifest.json` 에 추가:

```json
{
  "dependencies": {
    "com.skillforge.core": "https://github.com/achieveonepark/skill-creator.git"
  }
}
```

외부 의존성이 없습니다. JSON 직렬화는 Unity 내장 `JsonUtility` 를 사용합니다.

## 빠른 시작

```csharp
// 1) 통합 경계 구현: 본인의 전투 시스템이 IBattleUnit / IUnitRegistry 를 구현
//    (Samples~/BasicSetup 에 예제 구현 포함)

// 2) 시스템 구성
var system = new SkillSystem(myUnitRegistry, myVfxPlayer, mySfxPlayer, UnityCombatLogger.Instance);
system.LoadFromJson(skillsJson, buffsJson);

// 3) 매 프레임 버프 진행
void Update() => system.Tick(Time.deltaTime);

// 4) 스킬 사용 (즉시 실행)
SkillUseResult result = system.Use("fire_slash", caster, target);

// 4-2) 캐스팅 시간을 기다리려면 코루틴
StartCoroutine(system.Runner.UseRoutine("fire_slash", caster, target,
    r => Debug.Log(r.Status)));
```

## 에디터 도구

`Tools/SkillForge` 메뉴:

- **Skill Editor** — 스킬 목록/추가/삭제/복제/편집, JSON 저장, 검증
- **Buff Editor** — 버프 목록/스탯·주기 효과 편집, JSON 저장, 검증
- **Preview Simulator** — 실제 전투 없이 예상 데미지/힐/버프 결과 미리보기

## 지원 타입 (MVP)

| 분류 | 값 |
| --- | --- |
| Targeting | `self`, `single_enemy`, `circle`, `cone` |
| Condition | `always`, `alive`, `chance`, `hp_below`, `hp_above`, `has_buff`, `not_has_buff`, `target_count_above`, `target_count_below` |
| Effect | `damage`, `heal`, `add_buff`, `remove_buff`, `play_vfx`, `play_sfx` |
| StackPolicy | `stack`, `ignore`, `replace` |
| RefreshPolicy | `refresh_duration`, `keep` |
| StatModifier | `flat`, `percent_add`, `percent_mul` |

## 라이선스

MIT
