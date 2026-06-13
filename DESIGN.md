# Reactive Data Framework — 설계 문서

> 상태: **제안 (Draft)** · 최종 결정: 패키지명, 구현 착수 시점
> 이 문서가 확정되면 단계별로 구현한다. 확정 전까지 기존 코드는 변경하지 않는다.

## 0. 배경과 목표

현재 패키지(`com.achieve.skill-creator`)는 스킬 전용으로,
`SkillDefinition → SkillDataLoader → SkillDatabase → SkillSystem` 흐름이
스킬에 하드코딩되어 있다. 새 도메인(Monster / Item / Map / Quest)을 추가하려면
System / Loader / Database를 매번 복사해야 한다.

핵심 통찰:

> **도메인은 무한하지만, 동작(primitive)은 유한하다.**
> `도메인 × 동작` 매트릭스를 매번 새로 까는 대신, 동작 풀을 전 도메인이 공유하면
> 두 번째 도메인부터는 거의 공짜다. 곱셈을 덧셈으로 바꾼다.

목표:

1. **데이터 레이어 제네릭화** — Skill/Monster/Item/Map/Quest가 한 벌의 `DataBase<T>`를 공유
2. **소스 무관** — JSON(에디터)·CSV·Remote 어디서든 읽기. 사용자가 `IDataLoader<T>`만 구현하면 새 소스 추가
3. **Newtonsoft.Json 채택** — 제네릭 직렬화 지원 → `DataFile` 래퍼 클래스 제거
4. **동작 자동등록** — `[Effect]/[Trigger]/[Condition]` 어트리뷰트를 리플렉션으로 수집, 배선 코드 0줄
5. **사용자 확장 1순위** — 새 동작은 함수 1개, 새 소스는 인터페이스 1개, 새 도메인은 데이터 클래스 1개
6. **데이터는 UIToolkit 에디터가 생성** — 사용자가 JSON/스키마를 손으로 쓰지 않는다.
   UIToolkit 윈도우에서 타입/필드/리액션을 구성하면 코드와 데이터 파일을 생성·편집한다.

### 설계 원칙

- **`typeof(T)` / 런타임 타입 디스패치 지양.** `Dictionary<Type, object>` 같은 타입-키
  서비스 로케이터를 쓰지 않는다. 도메인 묶음은 **명시적 컴포지션**(각 `DataBase<T>`를
  직접 보유·주입)으로 표현하고, 크로스 도메인 참조가 필요한 effect는 수동등록 시
  필요한 `DataBase<T>`를 **클로저로 캡처**한다. (제네릭 자체는 사용하되, 타입을 런타임
  키로 쓰는 패턴만 피한다.)
- **데이터 입력은 UI 우선.** 원본 포맷(JSON/CSV)은 산출물일 뿐, 1차 저작 도구는
  UIToolkit 에디터다.

비목표:

- AI/스폰/인벤토리/씬로딩 같은 **도메인 시스템 로직**은 패키지가 강제하지 않는다.
  통합 경계(`IEntity`, `IWorld`)만 제공하고 구현은 사용자 게임이 한다.

---

## 1. 레이어 구조

```
┌─────────────────────────────────────────────┐
│  Data Layer (제네릭 · 패키지가 한 번만 작성)    │
│  IData · IDataLoader<T> · DataBase<T>         │
│  JsonDataLoader · CsvDataLoader · DataHub     │
├─────────────────────────────────────────────┤
│  Reaction Layer (제네릭 · 패키지가 한 번만 작성)│
│  Reaction · EntityData · ReactionContext      │
│  ReactionEngine                               │
│  EffectRegistry · ConditionRegistry ·         │
│  TriggerRegistry  ← Attribute 자동수집 + 수동등록│
├─────────────────────────────────────────────┤
│  Behavior Pool (게임이 작성 · 도메인 무관)     │
│  [Effect("damage")] [Condition("hp_below")]…  │
├─────────────────────────────────────────────┤
│  Domain Data (코드젠 or 수동 · 데이터)         │
│  SkillData · MonsterData · ItemData · …        │
└─────────────────────────────────────────────┘
```

위 두 레이어는 패키지 제공, 바뀌지 않는다. 아래 두 레이어만 사용자가 채운다.

---

## 2. Data Layer

```csharp
public interface IData
{
    string Id { get; }
}

// 어디서 읽든 동일한 경계 (동기)
public interface IDataLoader<T> where T : IData
{
    IReadOnlyList<T> Load();
}

// id 인덱스. 모든 도메인이 이 한 클래스를 공유한다.
public sealed class DataBase<T> where T : IData
{
    readonly Dictionary<string, T> _map = new();

    public void Load(IDataLoader<T> loader)
    {
        _map.Clear();
        foreach (var d in loader.Load())
            if (!string.IsNullOrEmpty(d.Id)) _map[d.Id] = d;
    }

    public T Get(string id) => _map.TryGetValue(id, out var v) ? v : default;
    public bool Has(string id) => id != null && _map.ContainsKey(id);
    public IReadOnlyCollection<T> All => _map.Values;
}
```

### Loader 구현체 (패키지 제공)

```csharp
public sealed class JsonDataLoader<T> : IDataLoader<T> where T : IData
{
    readonly Func<string> _read;   // 파일 / StreamingAssets / Resources 등
    public JsonDataLoader(Func<string> read) => _read = read;
    public IReadOnlyList<T> Load() =>
        JsonConvert.DeserializeObject<List<T>>(_read());   // Newtonsoft, 래퍼 불필요
}

public sealed class CsvDataLoader<T> : IDataLoader<T> where T : IData
{
    readonly Func<string> _read;
    readonly ICsvMapper<T> _mapper;
    public CsvDataLoader(Func<string> read, ICsvMapper<T> mapper) { _read = read; _mapper = mapper; }
    public IReadOnlyList<T> Load() { /* 행 분해 후 _mapper.Map(row) */ }
}

public interface ICsvMapper<T> where T : IData
{
    T Map(IReadOnlyDictionary<string, string> row);
}
```

> 사용자가 자기 포맷(예: 바이너리, ScriptableObject, 사내 포맷)을 쓰고 싶으면
> `IDataLoader<T>`만 구현하면 된다. 이것이 "에디터는 JSON, 실상은 다른 걸로도" 요구의 해결.

### 도메인 묶음 — 명시적 컴포지션 (typeof 미사용)

`Dictionary<Type, object>` 기반 허브는 쓰지 않는다. 각 `DataBase<T>`를 명시적으로
보유하고, 게임이 자기 컨텍스트 클래스에 필드로 노출한다.

```csharp
// 게임이 직접 작성하는 조립 지점 (typeof 없음, IDE 자동완성 됨)
public sealed class GameDataContext
{
    public readonly DataBase<SkillData>   Skills   = new();
    public readonly DataBase<MonsterData> Monsters = new();
    public readonly DataBase<ItemData>    Items    = new();
}

// 사용:
//   ctx.Skills.Get("fire_slash")
//   ctx.Monsters.Get("goblin")
```

크로스 도메인 참조가 필요한 effect는 **수동등록 + 클로저 캡처**로 푼다 (타입 키 불필요):

```csharp
// 예: 아이템을 드롭하는 effect가 ItemData를 참조해야 할 때
EffectRegistry.Register("drop_item", c =>
{
    var item = items.Get(c.Params.Get<string>("itemId")); // items 를 클로저로 캡처
    c.World.SpawnLoot(item, c.Source.Position);
});
```

> 자동등록(`[Effect]`)으로 만드는 effect는 도메인 데이터를 모르는 순수 동작만 담고,
> 도메인 간 결합이 필요한 effect만 수동등록으로 두는 것을 권장한다.

---

## 3. Reaction 모델 — 모든 도메인의 공통 언어

```csharp
[Serializable]
public class Reaction
{
    public string trigger;            // "on_use" | "on_equip" | "on_hp_below" ...
    public ParamBag triggerParams;    // 임계값 등 트리거 인자
    public ConditionDef[] conditions; // AND
    public EffectDef[] effects;
}

[Serializable] public class EffectDef    { public string type; public ParamBag @params; }
[Serializable] public class ConditionDef { public string type; public ParamBag @params; }

// 도메인 무관 기본 엔티티 데이터. 그대로 쓰거나 상속한다.
[Serializable]
public class EntityData : IData
{
    public string id;
    public string Id => id;
    public Dictionary<string, float> stats = new();
    public Reaction[] reactions = Array.Empty<Reaction>();
}
```

`ParamBag`은 임의 키-값(`Dictionary<string, JToken>` 래퍼)으로, effect/condition이
필요한 인자를 타입 변환해서 꺼낸다: `ctx.Params.Get<float>("power")`.

`SkillData` / `MonsterData`는 `EntityData`를 상속해 도메인 필드를 추가하거나,
순수하게 `reactions`만으로 표현한다.

### 예: 도메인이 달라도 형태는 같다

```json
// 스킬 — 사용 시 피해 + 버프
{ "id": "fire_slash",
  "reactions": [{ "trigger": "on_use",
    "effects": [{ "type": "damage",   "params": { "power": 1.8 } },
                { "type": "add_buff", "params": { "buffId": "burn" } }] }] }

// 몬스터 — HP 30% 이하에서 광폭화
{ "id": "goblin_boss",
  "reactions": [{ "trigger": "on_hp_below", "triggerParams": { "threshold": 0.3 },
    "effects": [{ "type": "add_buff", "params": { "buffId": "enrage" } }] }] }

// 아이템 — 장착 시 오라
{ "id": "ember_ring",
  "reactions": [{ "trigger": "on_equip",
    "effects": [{ "type": "add_buff", "params": { "buffId": "fire_aura" } }] }] }

// 맵 — 구역 진입 시 보스 스폰 (퀘스트 진행 중일 때만)
{ "id": "dungeon_01",
  "reactions": [{ "trigger": "on_enter_zone",
    "conditions": [{ "type": "quest_active", "params": { "questId": "q_001" } }],
    "effects": [{ "type": "spawn", "params": { "entityId": "goblin_boss" } }] }] }
```

`add_buff`는 스킬이 쓰든 아이템이 쓰든 **같은 코드 하나**다.

---

## 4. Registry — Attribute 자동수집 + 사용자 확장

```csharp
[AttributeUsage(AttributeTargets.Method)]
public sealed class EffectAttribute : Attribute
{
    public string Name { get; }
    public EffectAttribute(string name) => Name = name;
}

public delegate void EffectFn(ReactionContext ctx);

public static class EffectRegistry
{
    static readonly Dictionary<string, EffectFn> _map = new();

    // ① 자동수집: 모든 어셈블리의 [Effect] 메서드를 스캔해 등록
    public static void AutoRegister()
    {
        foreach (var (attr, method) in ReflectionScan.Find<EffectAttribute>())
            _map[attr.Name] = (EffectFn)Delegate.CreateDelegate(typeof(EffectFn), method);
    }

    // ② 수동등록: 리플렉션이 싫거나 DI/클로저가 필요한 사용자용
    public static void Register(string name, EffectFn fn) => _map[name] = fn;

    public static bool TryGet(string name, out EffectFn fn) => _map.TryGetValue(name, out fn);
}
```

`TriggerRegistry`, `ConditionRegistry`도 동일 패턴.

### 사용자 확장 — 두 길 중 택1

```csharp
// 길 A — Attribute (권장). IL2CPP에서는 link.xml로 보존.
public static class MyEffects
{
    [Effect("teleport")]
    public static void Teleport(ReactionContext c) =>
        c.Source.Position = c.World.RandomPoint();
}

// 길 B — 수동등록 (DI/클로저/런타임 상태 필요할 때)
EffectRegistry.Register("teleport", c => myTeleporter.Move(c.Source));
```

> AOT(IL2CPP) 주의: `AutoRegister`는 리플렉션 기반이라 메서드가 stripping될 수 있다.
> `link.xml` 가이드를 문서에 포함하고, 부담스러우면 길 B(수동등록)를 안내한다.

---

## 5. 실행 엔진

```csharp
public sealed class ReactionContext
{
    public IEntity Source;                 // 시전자 / 소유자
    public IEntity Target;                 // 단일 대상 (없을 수 있음)
    public IReadOnlyList<IEntity> Targets; // 광역 대상
    public IWorld World;                   // 스폰 / 조회 등 게임 경계
    public ParamBag Params;                // 현재 effect 인자 (엔진이 주입)
    // 다른 도메인 데이터가 필요하면 effect를 수동등록하고 해당 DataBase<T>를 클로저로 캡처
}

public sealed class ReactionEngine
{
    public void Run(string trigger, EntityData data, ReactionContext ctx)
    {
        foreach (var r in data.reactions)
        {
            if (r.trigger != trigger) continue;
            if (!TriggerMatch(r, ctx)) continue;      // on_hp_below 임계값 비교 등
            if (!AllConditions(r.conditions, ctx)) continue;
            foreach (var e in r.effects)
            {
                ctx.Params = e.@params;
                if (EffectRegistry.TryGet(e.type, out var fn)) fn(ctx);
            }
        }
    }
}
```

### 통합 경계 (사용자 게임이 구현)

기존 `IBattleUnit` / `IUnitRegistry`의 일반화:

```csharp
public interface IEntity
{
    string Id { get; }
    Vector3 Position { get; set; }
    StatSheet Stats { get; }      // 기존 StatSheet 재사용
}

public interface IWorld
{
    IEntity Spawn(string dataId, Vector3 at);
    void Collect(Vector3 center, float radius, List<IEntity> buffer);
}
```

---

## 6. 사용자가 새 도메인을 추가하는 전체 절차 (몬스터 예)

```
1) Type Designer(UIToolkit)에서 Monster 타입/필드 구성 → Generate
       → MonsterData.cs + MonsterCsvMapper.cs 생성
2) Data Editor(UIToolkit)에서 몬스터 인스턴스 + reactions 작성 → Export(JSON/CSV)
3) 새 동작만 함수로:
     [Effect("enrage")]  [Trigger("on_hp_below")]   ← 없으면 추가, 있으면 생략
4) 끝. System 클래스 0개.
```

런타임 (typeof 없는 명시적 보유):

```csharp
var monsters = new DataBase<MonsterData>();
monsters.Load(new CsvDataLoader<MonsterData>(() => File.ReadAllText(path), new MonsterCsvMapper()));

// 몬스터가 피격당했을 때
engine.Run("on_hit", monsters.Get("goblin_boss"), ctx);
// → reactions의 on_hp_below 발동 → enrage effect 실행
```

`damage`/`heal`/`add_buff`는 스킬이 이미 등록해둔 것을 재사용 → 코드 0줄.

---

## 7. 저작 도구 — UIToolkit 에디터 (JSON 수기 작성 없음)

사용자는 스키마/데이터 JSON을 손으로 쓰지 않는다. 두 개의 UIToolkit 윈도우가 담당한다.

### 7.1 Data Type Designer (타입 → 코드 생성)

```
[Tools/Game Data/Type Designer]  (UIToolkit EditorWindow)
  ┌──────────────────────────────────────────┐
  │ Type Name: [ Monster        ]            │
  │ Base Type: [ EntityData ▾   ]            │
  │ Fields:                                   │
  │   [ hp        ] [ float    ▾ ] [-]       │
  │   [ dropTable ] [ string[] ▾ ] [-]       │
  │   [ + Add Field ]                         │
  │                              [ Generate ] │
  └──────────────────────────────────────────┘
        │  생성
        ▼
generated/MonsterData.cs        (IData / EntityData 파생, Newtonsoft 어트리뷰트)
generated/MonsterCsvMapper.cs   (ICsvMapper<MonsterData>)
```

- 타입 정의는 에셋(`*.gdtype`, UIToolkit이 읽고 쓰는 직렬화된 정의)으로 저장 → 재편집 가능.
- "Generate" 시 C# 파일을 쓰고 `AssetDatabase.Refresh()`로 리컴파일.
- 코드젠을 원치 않으면 손으로 `class MonsterData : EntityData { ... }`를 써도 100% 동작.

### 7.2 Data Editor (인스턴스 → 데이터 파일 생성/편집)

```
[Tools/Game Data/Data Editor]  (UIToolkit EditorWindow)
  - 좌측: 생성된 타입 목록 + 인스턴스 리스트
  - 우측: 선택 인스턴스 폼 (필드 + Reactions 빌더)
        · Reactions: trigger 드롭다운(레지스트리에서 채움)
        ·            conditions / effects 추가 (등록된 type 자동완성)
        ·            params 키-값 인스펙터
  - 하단: Export ▾  (JSON / CSV)
```

- 폼은 생성된 타입의 필드를 리플렉션으로 렌더(에디터 전용이라 AOT 무관).
- trigger/condition/effect 드롭다운은 `*Registry`의 등록 목록을 그대로 노출 →
  오타·미등록 type을 원천 차단.
- 저장 산출물은 `JsonDataLoader`/`CsvDataLoader`가 그대로 읽는 포맷.

> 즉 **저작은 UI, 런타임 소비는 데이터 파일**. 사용자가 JSON을 직접 만질 필요가 없다.

---

## 8. 디렉토리 구조 (목표)

```
Runtime/
  Data/      IData, IDataLoader, DataBase, JsonDataLoader, CsvDataLoader, ICsvMapper
  Reaction/  Reaction, EntityData, ParamBag, ReactionContext, ReactionEngine
  Registry/  EffectRegistry, ConditionRegistry, TriggerRegistry, *Attribute, ReflectionScan
  Entity/    IEntity, IWorld, StatSheet(이전), CooldownStore(이전), Buff 시스템(이전)
  Skill/     SkillData + 기본 [Effect]/[Condition]/[Trigger] 풀 (damage/heal/add_buff/alive/hp_below…)
Editor/
  CodeGen/   TypeDesignerWindow(UIToolkit), CodeWriter, Templates, *.uxml/*.uss
  DataTool/  DataEditorWindow(UIToolkit), ReactionBuilder, *.uxml/*.uss
  Tools/     기존 에디터 창 재배치
Samples~/
  Skill/  Monster/  Item/   (도메인별 예제)
```

---

## 9. 기존 Skill 코드의 흡수 매핑

| 현재 | 이후 |
|---|---|
| `SkillDefinition` | `SkillData : EntityData` |
| `EffectExecutor`의 damage/heal/add_buff/… | `[Effect("...")]` 함수들로 분해 |
| `ConditionEvaluator`의 alive/hp_below/chance | `[Condition("...")]` 함수들로 분해 |
| `TargetResolver` (self/single/circle/cone) | `[Trigger]`/타겟팅 헬퍼로 재배치 |
| `SkillDataLoader` + `SkillDataFile`/`BuffDataFile` | **삭제** (Newtonsoft + `DataBase<T>`가 대체) |
| 기존 IMGUI 에디터 창(Skill/Buff/Preview) | UIToolkit `Type Designer` + `Data Editor`로 통합/대체 |
| `SkillSystem.Use("fire_slash", caster)` | `engine.Run("on_use", skill, ctx)` |
| `StatSheet`, `CooldownStore`, Buff(Instance/Container/Controller) | **유지**, `Entity/`로 이동 |
| `IBattleUnit` / `IUnitRegistry` | `IEntity` / `IWorld`로 일반화 |
| `DataValidator` | 스키마 + 레지스트리 기준으로 확장 |

> 마이그레이션은 **데이터 호환성**에 주의. 기존 `skills.json`(`{ "skills": [...] }`)은
> 새 형식(`reactions` 기반)으로 변환 필요. 변환 스크립트 또는 하위호환 로더를 제공한다.

---

## 10. 의존성

- **Newtonsoft.Json**: `com.unity.nuget.newtonsoft-json` (UPM 공식)을 `package.json` 의존성에 추가.
  현재의 "외부 의존성 0" 원칙은 깨지지만, 제네릭 직렬화/`DataFile` 제거/표현력 향상의 대가로 수용.

---

## 11. 패키지 메타 (확정)

- **같은 repo · 한 패키지**(코어 + 스킬 기본 도메인).
- **패키지명**: `com.achieve.reactive` (확정)
- **displayName**: `AchReactive` (확정)
- **문서 경로**: `docs.somiri.dev/reactive` (확정)
- **repo 이름**: `AchReactive` (확정)
- M8에서 일괄 완료.

---

## 12. 구현 단계 (제안)

1. **M1 — Data Layer**: `IData`, `IDataLoader<T>`, `DataBase<T>`, Json/Csv 로더, `ICsvMapper` + Newtonsoft 의존성 (typeof 미사용)
2. **M2 — Reaction Layer**: `Reaction`, `EntityData`, `ParamBag`, `ReactionContext`, `ReactionEngine`
3. **M3 — Registry**: 3종 Registry + Attribute + `ReflectionScan` + 수동등록 API
4. **M4 — Skill 이식**: 기존 effect/condition을 기본 동작 풀로 분해, `StatSheet`/Cooldown/Buff `Entity/`로 이동
5. **M5 — 통합 경계**: `IEntity`/`IWorld`로 일반화, 샘플 갱신
6. **M6 — UIToolkit 저작 도구**: `Type Designer`(코드 생성) + `Data Editor`(데이터 생성/편집)
7. **M7 — 도메인 샘플**: Monster/Item 예제, 마이그레이션 가이드
8. **M8 — 문서/이름 변경**: 패키지명(`com.achieve.game-data`)·docs 경로(`game-data`) 갱신, AOT(link.xml) 가이드 포함

각 단계는 독립 커밋. M1~M3까지는 기존 스킬 코드와 **공존** 가능하므로 안전하게 진행한다.
