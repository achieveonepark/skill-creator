using System.Collections.Generic;
using UnityEngine;

namespace SkillForge
{
    /// <summary>
    /// 타겟팅 정의를 기반으로 실제 대상 목록을 계산한다.
    /// circle / cone 은 <see cref="IUnitRegistry"/> 를 통해 주변 유닛을 조회한다.
    /// </summary>
    public sealed class TargetResolver
    {
        private readonly IUnitRegistry _registry;
        private readonly List<IBattleUnit> _scratch = new List<IBattleUnit>();

        public TargetResolver(IUnitRegistry registry)
        {
            _registry = registry;
        }

        public List<IBattleUnit> Resolve(TargetingDefinition definition, IBattleUnit caster, IBattleUnit inputTarget)
        {
            if (definition == null)
                return new List<IBattleUnit>();

            switch (definition.type)
            {
                case TargetingType.Self:
                    return new List<IBattleUnit> { caster };

                case TargetingType.SingleEnemy:
                    return inputTarget != null
                        ? new List<IBattleUnit> { inputTarget }
                        : new List<IBattleUnit>();

                case TargetingType.Circle:
                    return FindCircleTargets(caster, definition.radius, definition.maxTargets);

                case TargetingType.Cone:
                    return FindConeTargets(caster, inputTarget, definition.range, definition.angle, definition.maxTargets);

                default:
                    return new List<IBattleUnit>();
            }
        }

        private List<IBattleUnit> FindCircleTargets(IBattleUnit caster, float radius, int maxTargets)
        {
            var result = new List<IBattleUnit>();
            if (_registry == null)
                return result;

            _scratch.Clear();
            _registry.Collect(caster.Position, radius, _scratch);

            for (int i = 0; i < _scratch.Count; i++)
            {
                IBattleUnit unit = _scratch[i];
                if (unit == null || ReferenceEquals(unit, caster) || !unit.IsAlive)
                    continue;
                if (!caster.IsEnemyOf(unit))
                    continue;

                result.Add(unit);
                if (maxTargets > 0 && result.Count >= maxTargets)
                    break;
            }

            return result;
        }

        private List<IBattleUnit> FindConeTargets(IBattleUnit caster, IBattleUnit inputTarget, float range, float angle, int maxTargets)
        {
            var result = new List<IBattleUnit>();
            if (_registry == null)
                return result;

            // 부채꼴 기준 방향: 입력 타겟이 있으면 그 방향, 없으면 캐스터 정면.
            Vector3 forward = caster.Forward;
            if (inputTarget != null)
            {
                Vector3 toTarget = inputTarget.Position - caster.Position;
                if (toTarget.sqrMagnitude > 0.0001f)
                    forward = toTarget.normalized;
            }

            float halfAngle = angle * 0.5f;

            _scratch.Clear();
            _registry.Collect(caster.Position, range, _scratch);

            for (int i = 0; i < _scratch.Count; i++)
            {
                IBattleUnit unit = _scratch[i];
                if (unit == null || ReferenceEquals(unit, caster) || !unit.IsAlive)
                    continue;
                if (!caster.IsEnemyOf(unit))
                    continue;

                Vector3 dir = unit.Position - caster.Position;
                if (dir.sqrMagnitude < 0.0001f)
                {
                    result.Add(unit);
                }
                else if (Vector3.Angle(forward, dir.normalized) <= halfAngle)
                {
                    result.Add(unit);
                }
                else
                {
                    continue;
                }

                if (maxTargets > 0 && result.Count >= maxTargets)
                    break;
            }

            return result;
        }
    }
}
