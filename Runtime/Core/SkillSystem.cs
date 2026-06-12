using System.Collections.Generic;

namespace SkillForge
{
    /// <summary>
    /// SkillForge 의 모든 구성요소를 배선하는 파사드.
    /// 데이터 로드부터 스킬 실행, 버프 진행(Tick)까지 하나의 진입점으로 제공한다.
    /// </summary>
    public sealed class SkillSystem
    {
        public SkillDatabase Database { get; }
        public SkillRunner Runner { get; }
        public BuffController Buffs { get; }
        public TargetResolver Targeting { get; }
        public ConditionEvaluator Conditions { get; }
        public EffectExecutor Effects { get; }

        public SkillSystem(
            IUnitRegistry registry,
            IVfxPlayer vfxPlayer = null,
            ISfxPlayer sfxPlayer = null,
            ICombatLogger logger = null)
        {
            logger = logger ?? NullCombatLogger.Instance;

            Database = new SkillDatabase();
            Targeting = new TargetResolver(registry);
            Conditions = new ConditionEvaluator();

            Buffs = new BuffController(Database, logger);
            Effects = new EffectExecutor(Buffs, vfxPlayer, sfxPlayer, logger);
            Buffs.Initialize(Effects);

            Runner = new SkillRunner(Database, Targeting, Conditions, Effects, logger);
        }

        /// <summary>JSON 문자열로 스킬/버프 데이터를 로드한다.</summary>
        public void LoadFromJson(string skillsJson, string buffsJson)
        {
            if (skillsJson != null)
                Database.LoadSkills(SkillDataLoader.LoadSkillsFromJson(skillsJson));

            if (buffsJson != null)
                Database.LoadBuffs(SkillDataLoader.LoadBuffsFromJson(buffsJson));
        }

        public void LoadSkills(IEnumerable<SkillDefinition> skills) => Database.LoadSkills(skills);

        public void LoadBuffs(IEnumerable<BuffDefinition> buffs) => Database.LoadBuffs(buffs);

        /// <summary>스킬을 즉시 사용한다.</summary>
        public SkillUseResult Use(string skillId, IBattleUnit caster, IBattleUnit inputTarget = null)
        {
            return Runner.Use(skillId, caster, inputTarget);
        }

        /// <summary>버프 지속시간/주기효과를 진행한다. 매 프레임 호출한다.</summary>
        public void Tick(float deltaTime)
        {
            Buffs.Tick(deltaTime);
        }
    }
}
