using System.Collections.Generic;

namespace SkillForge
{
    /// <summary>
    /// 효과 정의를 실제 런타임 동작으로 변환한다.
    /// damage / heal / add_buff / remove_buff / play_vfx / play_sfx 를 지원한다.
    /// </summary>
    public sealed class EffectExecutor
    {
        private readonly BuffController _buffController;
        private readonly IVfxPlayer _vfxPlayer;
        private readonly ISfxPlayer _sfxPlayer;
        private readonly ICombatLogger _logger;

        public EffectExecutor(
            BuffController buffController,
            IVfxPlayer vfxPlayer = null,
            ISfxPlayer sfxPlayer = null,
            ICombatLogger logger = null)
        {
            _buffController = buffController;
            _vfxPlayer = vfxPlayer ?? NullPresentation.Instance;
            _sfxPlayer = sfxPlayer ?? NullPresentation.Instance;
            _logger = logger ?? NullCombatLogger.Instance;
        }

        public void Execute(EffectDefinition effect, IBattleUnit caster, List<IBattleUnit> targets)
        {
            if (effect == null || targets == null)
                return;

            switch (effect.type)
            {
                case EffectType.Damage:
                    ExecuteDamage(effect, caster, targets);
                    break;
                case EffectType.Heal:
                    ExecuteHeal(effect, caster, targets);
                    break;
                case EffectType.AddBuff:
                    ExecuteAddBuff(effect, caster, targets);
                    break;
                case EffectType.RemoveBuff:
                    ExecuteRemoveBuff(effect, targets);
                    break;
                case EffectType.PlayVfx:
                    ExecutePlayVfx(effect, targets);
                    break;
                case EffectType.PlaySfx:
                    ExecutePlaySfx(effect, targets);
                    break;
                default:
                    _logger.Log($"지원하지 않는 효과 타입: {effect.type}");
                    break;
            }
        }

        /// <summary>데미지 = value + 공격력 * power.</summary>
        public static float ComputeDamage(EffectDefinition effect, IBattleUnit caster)
        {
            float attack = caster != null ? caster.Stats.Get("attack") : 0f;
            return effect.value + attack * effect.power;
        }

        /// <summary>힐량 = value + 공격력 * power.</summary>
        public static float ComputeHeal(EffectDefinition effect, IBattleUnit caster)
        {
            float attack = caster != null ? caster.Stats.Get("attack") : 0f;
            return effect.value + attack * effect.power;
        }

        private void ExecuteDamage(EffectDefinition effect, IBattleUnit caster, List<IBattleUnit> targets)
        {
            float damage = ComputeDamage(effect, caster);
            string damageType = string.IsNullOrEmpty(effect.damageType) ? "physical" : effect.damageType;

            foreach (IBattleUnit target in targets)
            {
                if (target == null)
                    continue;

                target.TakeDamage(damage, damageType);
                _logger.Log($"Damage: {target.Id} - {damage:0.##} ({damageType})");
            }
        }

        private void ExecuteHeal(EffectDefinition effect, IBattleUnit caster, List<IBattleUnit> targets)
        {
            float heal = ComputeHeal(effect, caster);

            foreach (IBattleUnit target in targets)
            {
                if (target == null)
                    continue;

                target.Heal(heal);
                _logger.Log($"Heal: {target.Id} + {heal:0.##}");
            }
        }

        private void ExecuteAddBuff(EffectDefinition effect, IBattleUnit caster, List<IBattleUnit> targets)
        {
            if (_buffController == null)
                return;

            foreach (IBattleUnit target in targets)
            {
                if (target == null)
                    continue;

                _buffController.AddBuff(target, effect.buffId, caster, effect.duration);
            }
        }

        private void ExecuteRemoveBuff(EffectDefinition effect, List<IBattleUnit> targets)
        {
            if (_buffController == null)
                return;

            foreach (IBattleUnit target in targets)
            {
                if (target == null)
                    continue;

                _buffController.RemoveBuff(target, effect.buffId);
            }
        }

        private void ExecutePlayVfx(EffectDefinition effect, List<IBattleUnit> targets)
        {
            foreach (IBattleUnit target in targets)
            {
                if (target == null)
                    continue;

                _vfxPlayer.Play(effect.vfxId, target.Position);
            }
        }

        private void ExecutePlaySfx(EffectDefinition effect, List<IBattleUnit> targets)
        {
            foreach (IBattleUnit target in targets)
            {
                if (target == null)
                    continue;

                _sfxPlayer.Play(effect.sfxId, target.Position);
            }
        }
    }
}
