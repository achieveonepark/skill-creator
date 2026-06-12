# Changelog

이 프로젝트의 모든 주요 변경 사항을 기록합니다.
형식은 [Keep a Changelog](https://keepachangelog.com/ko/1.0.0/)를 따르며,
버전은 [Semantic Versioning](https://semver.org/lang/ko/)을 따릅니다.

## [0.1.0] - 2026-06-12

### Added
- 데이터 정의: `SkillDefinition`, `BuffDefinition`, `TargetingDefinition`, `ConditionDefinition`, `EffectDefinition`, `StatModifierDefinition`
- 런타임: `SkillDatabase`, `SkillDataLoader`, `SkillRunner`, `TargetResolver`, `ConditionEvaluator`, `EffectExecutor`, `BuffController`, `SkillSystem` 파사드
- 통합 경계 인터페이스: `IBattleUnit`, `IUnitRegistry`, `IVfxPlayer`, `ISfxPlayer`, `ICombatLogger`
- 지원 구현체: `StatSheet`, `CooldownStore`, `BuffContainer`, `BuffInstance`
- 검증: `DataValidator`, `ValidationReport`
- 에디터 도구: `SkillEditorWindow`, `BuffEditorWindow`, `PreviewSimulatorWindow`, `JsonExporter`
- 샘플: `Basic Setup` (예제 유닛/레지스트리/부트스트랩, skills.json, buffs.json)
