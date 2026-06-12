using System.Collections.Generic;
using UnityEngine;

namespace SkillCreator
{
    /// <summary>
    /// 조건 목록을 AND 로 검사한다. 하나라도 실패하면 false.
    /// </summary>
    public sealed class ConditionEvaluator
    {
        public bool Evaluate(List<ConditionDefinition> conditions, IBattleUnit caster, List<IBattleUnit> targets)
        {
            if (conditions == null)
                return true;

            for (int i = 0; i < conditions.Count; i++)
            {
                if (!EvaluateSingle(conditions[i], caster, targets))
                    return false;
            }

            return true;
        }

        private bool EvaluateSingle(ConditionDefinition condition, IBattleUnit caster, List<IBattleUnit> targets)
        {
            switch (condition.type)
            {
                case ConditionType.Always:
                    return true;

                case ConditionType.Alive:
                    return targets.Exists(t => t != null && t.IsAlive);

                case ConditionType.Chance:
                    return Random.value <= condition.value;

                case ConditionType.HpBelow:
                    return targets.Exists(t => t != null && t.HpRatio <= condition.value);

                case ConditionType.HpAbove:
                    return targets.Exists(t => t != null && t.HpRatio >= condition.value);

                case ConditionType.HasBuff:
                    return targets.Exists(t => t != null && t.Buffs.Has(condition.key));

                case ConditionType.NotHasBuff:
                    return targets.TrueForAll(t => t == null || !t.Buffs.Has(condition.key));

                case ConditionType.TargetCountAbove:
                    return targets.Count >= condition.value;

                case ConditionType.TargetCountBelow:
                    return targets.Count <= condition.value;

                default:
                    Debug.LogWarning($"[Skill Creator] 지원하지 않는 조건 타입: {condition.type}");
                    return false;
            }
        }
    }
}
