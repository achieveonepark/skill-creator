using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SkillCreator
{
    public enum SkillUseStatus
    {
        Success,
        SkillNotFound,
        OnCooldown,
        NoTargets,
        ConditionFailed
    }

    public readonly struct SkillUseResult
    {
        public readonly SkillUseStatus Status;
        public readonly List<IBattleUnit> Targets;

        public SkillUseResult(SkillUseStatus status, List<IBattleUnit> targets)
        {
            Status = status;
            Targets = targets;
        }

        public bool Success => Status == SkillUseStatus.Success;
    }

    /// <summary>
    /// 스킬 사용의 전체 흐름을 담당한다.
    ///   조회 -> 쿨타임 -> 타겟 계산 -> 조건 검사 -> (캐스팅) -> 효과 실행 -> 쿨타임 적용
    /// 즉시 실행 <see cref="Use"/> 와, 캐스팅 시간을 기다리는 코루틴 <see cref="UseRoutine"/> 을 제공한다.
    /// </summary>
    public sealed class SkillRunner
    {
        private readonly SkillDatabase _database;
        private readonly TargetResolver _targetResolver;
        private readonly ConditionEvaluator _conditionEvaluator;
        private readonly EffectExecutor _effectExecutor;
        private readonly ICombatLogger _logger;

        public SkillRunner(
            SkillDatabase database,
            TargetResolver targetResolver,
            ConditionEvaluator conditionEvaluator,
            EffectExecutor effectExecutor,
            ICombatLogger logger = null)
        {
            _database = database;
            _targetResolver = targetResolver;
            _conditionEvaluator = conditionEvaluator;
            _effectExecutor = effectExecutor;
            _logger = logger ?? NullCombatLogger.Instance;
        }

        /// <summary>
        /// 스킬을 즉시 실행한다. 캐스팅 시간은 무시한다.
        /// (캐스팅 연출이 필요하면 <see cref="UseRoutine"/> 사용.)
        /// </summary>
        public SkillUseResult Use(string skillId, IBattleUnit caster, IBattleUnit inputTarget = null)
        {
            if (!TryBegin(skillId, caster, inputTarget, out SkillDefinition skill, out List<IBattleUnit> targets, out SkillUseResult failure))
                return failure;

            ExecuteEffects(skill, caster, targets);
            return new SkillUseResult(SkillUseStatus.Success, targets);
        }

        /// <summary>
        /// 캐스팅 시간을 기다린 뒤 스킬을 실행하는 코루틴.
        /// MonoBehaviour.StartCoroutine 으로 구동한다.
        /// </summary>
        public IEnumerator UseRoutine(string skillId, IBattleUnit caster, IBattleUnit inputTarget = null, Action<SkillUseResult> onComplete = null)
        {
            if (!TryBegin(skillId, caster, inputTarget, out SkillDefinition skill, out List<IBattleUnit> targets, out SkillUseResult failure))
            {
                onComplete?.Invoke(failure);
                yield break;
            }

            if (skill.castTime > 0f)
                yield return new WaitForSeconds(skill.castTime);

            ExecuteEffects(skill, caster, targets);
            onComplete?.Invoke(new SkillUseResult(SkillUseStatus.Success, targets));
        }

        /// <summary>조회/쿨타임/타겟/조건까지 검사. 성공 시 skill, targets 를 채운다.</summary>
        private bool TryBegin(
            string skillId,
            IBattleUnit caster,
            IBattleUnit inputTarget,
            out SkillDefinition skill,
            out List<IBattleUnit> targets,
            out SkillUseResult failure)
        {
            targets = null;
            failure = default;

            skill = _database.GetSkill(skillId);
            if (skill == null)
            {
                _logger.Log($"스킬 정의를 찾을 수 없음: {skillId}");
                failure = new SkillUseResult(SkillUseStatus.SkillNotFound, null);
                return false;
            }

            if (caster == null || !caster.Cooldowns.IsReady(skill.id))
            {
                failure = new SkillUseResult(SkillUseStatus.OnCooldown, null);
                return false;
            }

            targets = _targetResolver.Resolve(skill.targeting, caster, inputTarget);
            if (targets.Count == 0)
            {
                failure = new SkillUseResult(SkillUseStatus.NoTargets, targets);
                return false;
            }

            if (!_conditionEvaluator.Evaluate(skill.conditions, caster, targets))
            {
                failure = new SkillUseResult(SkillUseStatus.ConditionFailed, targets);
                return false;
            }

            _logger.Log($"{skill.id} used by {caster.Id}");
            return true;
        }

        private void ExecuteEffects(SkillDefinition skill, IBattleUnit caster, List<IBattleUnit> targets)
        {
            for (int i = 0; i < skill.effects.Count; i++)
                _effectExecutor.Execute(skill.effects[i], caster, targets);

            caster.Cooldowns.Start(skill.id, skill.cooldown);
        }
    }
}
