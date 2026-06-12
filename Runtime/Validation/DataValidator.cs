using System;
using System.Collections.Generic;

namespace SkillCreator
{
    /// <summary>
    /// 스킬/버프 데이터의 정합성을 검사한다.
    /// "잘못된 데이터를 런타임 전에 막아주는 것"이 이 도구의 핵심 가치다.
    /// vfxId 존재 여부는 알 수 없는 자산이므로 known 집합을 선택적으로 받는다.
    /// </summary>
    public static class DataValidator
    {
        public static ValidationReport Validate(
            IList<SkillDefinition> skills,
            IList<BuffDefinition> buffs,
            ISet<string> knownVfxIds = null)
        {
            var report = new ValidationReport();

            var buffIds = new HashSet<string>();
            if (buffs != null)
            {
                foreach (BuffDefinition buff in buffs)
                {
                    if (buff != null && !string.IsNullOrEmpty(buff.id))
                        buffIds.Add(buff.id);
                }
            }

            ValidateSkills(skills, buffIds, knownVfxIds, report);
            ValidateBuffs(buffs, report);
            return report;
        }

        private static void ValidateSkills(
            IList<SkillDefinition> skills,
            HashSet<string> buffIds,
            ISet<string> knownVfxIds,
            ValidationReport report)
        {
            if (skills == null)
                return;

            var seenIds = new HashSet<string>();

            foreach (SkillDefinition skill in skills)
            {
                string ctx = $"Skill '{skill?.id ?? "(null)"}'";

                if (skill == null)
                {
                    report.Error("Skill", "null 스킬 항목이 있습니다.");
                    continue;
                }

                if (string.IsNullOrEmpty(skill.id))
                    report.Error(ctx, "id 가 비어 있습니다.");
                else if (!seenIds.Add(skill.id))
                    report.Error(ctx, "id 가 중복됩니다.");

                if (skill.cooldown < 0f)
                    report.Error(ctx, "cooldown 이 음수입니다.");
                if (skill.castTime < 0f)
                    report.Error(ctx, "castTime 이 음수입니다.");

                if (skill.targeting == null)
                    report.Error(ctx, "targeting 이 없습니다.");
                else if (!Contains(TargetingType.Supported, skill.targeting.type))
                    report.Error(ctx, $"지원하지 않는 targeting.type: '{skill.targeting.type}'");

                if (skill.effects == null || skill.effects.Count == 0)
                {
                    report.Error(ctx, "effects 가 최소 1개 이상 필요합니다.");
                }
                else
                {
                    for (int i = 0; i < skill.effects.Count; i++)
                        ValidateEffect(skill.effects[i], $"{ctx} effect[{i}]", buffIds, knownVfxIds, report);
                }

                if (skill.conditions != null)
                {
                    for (int i = 0; i < skill.conditions.Count; i++)
                    {
                        string c = skill.conditions[i]?.type;
                        if (!Contains(ConditionType.Supported, c))
                            report.Error($"{ctx} condition[{i}]", $"지원하지 않는 condition.type: '{c}'");
                    }
                }
            }
        }

        private static void ValidateEffect(
            EffectDefinition effect,
            string ctx,
            HashSet<string> buffIds,
            ISet<string> knownVfxIds,
            ValidationReport report)
        {
            if (effect == null)
            {
                report.Error(ctx, "null 효과 항목입니다.");
                return;
            }

            if (!Contains(EffectType.Supported, effect.type))
            {
                report.Error(ctx, $"지원하지 않는 effect.type: '{effect.type}'");
                return;
            }

            switch (effect.type)
            {
                case EffectType.AddBuff:
                case EffectType.RemoveBuff:
                    if (string.IsNullOrEmpty(effect.buffId))
                        report.Error(ctx, $"{effect.type} 효과에 buffId 가 없습니다.");
                    else if (effect.type == EffectType.AddBuff && !buffIds.Contains(effect.buffId))
                        report.Error(ctx, $"add_buff 의 buffId '{effect.buffId}' 가 존재하지 않습니다.");
                    break;

                case EffectType.PlayVfx:
                    if (string.IsNullOrEmpty(effect.vfxId))
                        report.Error(ctx, "play_vfx 효과에 vfxId 가 없습니다.");
                    else if (knownVfxIds != null && !knownVfxIds.Contains(effect.vfxId))
                        report.Warning(ctx, $"vfxId '{effect.vfxId}' 가 등록된 VFX 목록에 없습니다.");
                    break;

                case EffectType.PlaySfx:
                    if (string.IsNullOrEmpty(effect.sfxId))
                        report.Error(ctx, "play_sfx 효과에 sfxId 가 없습니다.");
                    break;
            }
        }

        private static void ValidateBuffs(IList<BuffDefinition> buffs, ValidationReport report)
        {
            if (buffs == null)
                return;

            var seenIds = new HashSet<string>();

            foreach (BuffDefinition buff in buffs)
            {
                string ctx = $"Buff '{buff?.id ?? "(null)"}'";

                if (buff == null)
                {
                    report.Error("Buff", "null 버프 항목이 있습니다.");
                    continue;
                }

                if (string.IsNullOrEmpty(buff.id))
                    report.Error(ctx, "id 가 비어 있습니다.");
                else if (!seenIds.Add(buff.id))
                    report.Error(ctx, "id 가 중복됩니다.");

                if (buff.duration <= 0f)
                    report.Error(ctx, "duration 은 0 보다 커야 합니다.");
                if (buff.maxStack < 1)
                    report.Error(ctx, "maxStack 은 1 이상이어야 합니다.");

                if (!Contains(StackPolicy.Supported, buff.stackPolicy))
                    report.Error(ctx, $"지원하지 않는 stackPolicy: '{buff.stackPolicy}'");
                if (!Contains(RefreshPolicy.Supported, buff.refreshPolicy))
                    report.Error(ctx, $"지원하지 않는 refreshPolicy: '{buff.refreshPolicy}'");

                if (buff.periodicEffects != null)
                {
                    for (int i = 0; i < buff.periodicEffects.Count; i++)
                    {
                        EffectDefinition pe = buff.periodicEffects[i];
                        if (pe != null && pe.interval <= 0f)
                            report.Error($"{ctx} periodicEffect[{i}]", "interval 은 0 보다 커야 합니다.");
                    }
                }

                if (buff.statModifiers != null)
                {
                    for (int i = 0; i < buff.statModifiers.Count; i++)
                    {
                        StatModifierDefinition mod = buff.statModifiers[i];
                        if (mod == null)
                            continue;
                        if (string.IsNullOrEmpty(mod.statKey))
                            report.Error($"{ctx} statModifier[{i}]", "statKey 가 비어 있습니다.");
                        if (!Contains(StatModifierType.Supported, mod.modifierType))
                            report.Error($"{ctx} statModifier[{i}]", $"지원하지 않는 modifierType: '{mod.modifierType}'");
                    }
                }
            }
        }

        private static bool Contains(string[] set, string value)
        {
            if (value == null)
                return false;

            return Array.IndexOf(set, value) >= 0;
        }
    }
}
