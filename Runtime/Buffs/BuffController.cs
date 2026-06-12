using System.Collections.Generic;

namespace SkillCreator
{
    /// <summary>
    /// 버프의 적용/스택/갱신/주기효과/해제 및 스탯 수정자 적용을 총괄한다.
    /// 활성 버프 인스턴스를 중앙에서 보관하고 <see cref="Tick"/> 으로 시간을 진행한다.
    /// 주기/적용/해제 효과 실행을 위해 <see cref="EffectExecutor"/> 를 사용하므로,
    /// 상호 의존을 끊기 위해 생성 후 <see cref="Initialize"/> 로 주입한다.
    /// </summary>
    public sealed class BuffController
    {
        private readonly SkillDatabase _database;
        private readonly ICombatLogger _logger;

        private readonly List<BuffInstance> _active = new List<BuffInstance>();
        private readonly List<BuffInstance> _removeBuffer = new List<BuffInstance>();
        private readonly List<int> _firedBuffer = new List<int>();
        private readonly List<IBattleUnit> _singleTargetBuffer = new List<IBattleUnit>(1);

        private EffectExecutor _effectExecutor;

        public BuffController(SkillDatabase database, ICombatLogger logger = null)
        {
            _database = database;
            _logger = logger ?? NullCombatLogger.Instance;
        }

        /// <summary>EffectExecutor 주입(생성 순환 의존 해소용).</summary>
        public void Initialize(EffectExecutor effectExecutor)
        {
            _effectExecutor = effectExecutor;
        }

        public IReadOnlyList<BuffInstance> ActiveBuffs => _active;

        public void AddBuff(IBattleUnit target, string buffId, IBattleUnit source, float overrideDuration = 0f)
        {
            if (target == null || string.IsNullOrEmpty(buffId))
                return;

            BuffDefinition def = _database.GetBuff(buffId);
            if (def == null)
            {
                _logger.Log($"버프 정의를 찾을 수 없음: {buffId}");
                return;
            }

            float duration = overrideDuration > 0f ? overrideDuration : def.duration;
            BuffInstance existing = target.Buffs.Get(buffId);

            if (existing != null)
            {
                ApplyReapplyPolicy(existing, def, duration);
                return;
            }

            var instance = new BuffInstance(def, target, source, duration);
            target.Buffs.Add(instance);
            _active.Add(instance);

            ApplyStatModifiers(instance);
            RunEffects(def.onApplyEffects, instance);

            _logger.Log($"Buff Applied: {buffId} to {target.Id}");
        }

        private void ApplyReapplyPolicy(BuffInstance existing, BuffDefinition def, float duration)
        {
            switch (def.stackPolicy)
            {
                case StackPolicy.Ignore:
                    // 갱신 정책만 반영.
                    break;

                case StackPolicy.Replace:
                    existing.RefreshDuration(duration);
                    return;

                case StackPolicy.Stack:
                default:
                    existing.AddStack(def.maxStack);
                    // 스택 변화에 맞춰 스탯 수정자 재적용.
                    existing.Owner.Stats.RemoveSource(existing);
                    ApplyStatModifiers(existing);
                    break;
            }

            if (def.refreshPolicy == RefreshPolicy.RefreshDuration)
                existing.RefreshDuration(duration);
        }

        public void RemoveBuff(IBattleUnit target, string buffId)
        {
            if (target == null)
                return;

            BuffInstance instance = target.Buffs.Get(buffId);
            if (instance != null)
                RemoveInstance(instance);
        }

        private void RemoveInstance(BuffInstance instance)
        {
            instance.Owner.Stats.RemoveSource(instance);
            RunEffects(instance.Definition.onRemoveEffects, instance);
            instance.Owner.Buffs.Remove(instance);
            _active.Remove(instance);

            _logger.Log($"Buff Removed: {instance.Definition.id} from {instance.Owner.Id}");
        }

        /// <summary>모든 활성 버프의 시간을 진행한다. 매 프레임 deltaTime 으로 호출한다.</summary>
        public void Tick(float deltaTime)
        {
            if (_active.Count == 0)
                return;

            _removeBuffer.Clear();

            // 주기 효과 처리 (반복 중 컬렉션 변경을 피하려 인덱스 순회).
            for (int i = 0; i < _active.Count; i++)
            {
                BuffInstance instance = _active[i];

                if (!instance.Owner.IsAlive)
                {
                    _removeBuffer.Add(instance);
                    continue;
                }

                instance.TickPeriodic(deltaTime, _firedBuffer);
                for (int f = 0; f < _firedBuffer.Count; f++)
                {
                    EffectDefinition effect = instance.Definition.periodicEffects[_firedBuffer[f]];
                    RunEffect(effect, instance);
                }

                instance.Advance(deltaTime);
                if (instance.IsExpired)
                    _removeBuffer.Add(instance);
            }

            for (int i = 0; i < _removeBuffer.Count; i++)
                RemoveInstance(_removeBuffer[i]);
        }

        private void ApplyStatModifiers(BuffInstance instance)
        {
            instance.Owner.Stats.AddModifiers(instance.Definition.statModifiers, instance, instance.Stacks);
        }

        private void RunEffects(List<EffectDefinition> effects, BuffInstance instance)
        {
            if (effects == null)
                return;

            for (int i = 0; i < effects.Count; i++)
                RunEffect(effects[i], instance);
        }

        private void RunEffect(EffectDefinition effect, BuffInstance instance)
        {
            if (_effectExecutor == null)
                return;

            _singleTargetBuffer.Clear();
            _singleTargetBuffer.Add(instance.Owner);
            _effectExecutor.Execute(effect, instance.Source, _singleTargetBuffer);
        }
    }
}
