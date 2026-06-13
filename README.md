# AchReactive

Unity용 **도메인 무관 리액션 프레임워크**. 스킬·몬스터·아이템 등 어떤 도메인이든 Trigger → Condition → Effect 파이프라인 하나로 표현합니다.

> 📖 전체 문서: **https://docs.somiri.dev/reactive**

## 설치

Unity Package Manager → **Add package from git URL**:

```
https://github.com/achieveonepark/AchReactive.git
```

또는 `Packages/manifest.json` 에 추가:

```json
{
  "dependencies": {
    "com.achieve.reactive": "https://github.com/achieveonepark/AchReactive.git"
  }
}
```

요구 사항: **Unity 2021.3 이상**. 의존성은 `com.unity.nuget.newtonsoft-json` 하나이며 자동으로 설치됩니다.

## 빠른 시작

```csharp
// 1. 등록
EffectRegistry.AutoRegister();
ConditionRegistry.AutoRegister();
TriggerRegistry.AutoRegister();

// 2. 데이터 로드
var skills = new DataBase<SkillData>();
skills.Load(new JsonDataLoader<SkillData>(() => skillsJson.text));

// 3. 실행
var ctx = new ReactionContext { Source = caster, Target = target };
engine.Run("on_use", skills.Get("fire_slash"), ctx);
```

## 에디터 도구

`Tools/AchReactive` 메뉴:

- **Type Designer** — 도메인 데이터 클래스 + CsvMapper 코드 자동 생성
- **Data Editor** — 리액션 빌더 (트리거/컨디션/이펙트 드롭다운) → JSON 내보내기

## 라이선스

MIT
