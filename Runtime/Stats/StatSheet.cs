using System.Collections.Generic;

namespace SkillForge
{
    /// <summary>
    /// 기본 스탯 + 수정자(modifier)를 관리한다.
    /// 최종 값 = (base + flat합) * (1 + percent_add합) * Π(1 + percent_mul).
    /// 수정자는 source(출처) 토큰별로 추가/제거할 수 있어 버프 해제 시 정확히 되돌릴 수 있다.
    /// </summary>
    public sealed class StatSheet
    {
        private struct Entry
        {
            public string StatKey;
            public string ModifierType;
            public float Value;
            public object Source;
        }

        private readonly Dictionary<string, float> _base = new Dictionary<string, float>();
        private readonly List<Entry> _modifiers = new List<Entry>();

        public void SetBase(string statKey, float value)
        {
            _base[statKey] = value;
        }

        public float GetBase(string statKey)
        {
            return _base.TryGetValue(statKey, out float v) ? v : 0f;
        }

        public void AddModifier(string statKey, string modifierType, float value, object source)
        {
            _modifiers.Add(new Entry
            {
                StatKey = statKey,
                ModifierType = modifierType,
                Value = value,
                Source = source
            });
        }

        public void AddModifiers(IEnumerable<StatModifierDefinition> mods, object source, float scale = 1f)
        {
            if (mods == null)
                return;

            foreach (StatModifierDefinition mod in mods)
            {
                if (mod == null || string.IsNullOrEmpty(mod.statKey))
                    continue;

                AddModifier(mod.statKey, mod.modifierType, mod.value * scale, source);
            }
        }

        /// <summary>지정한 source 로 추가된 모든 수정자를 제거한다.</summary>
        public void RemoveSource(object source)
        {
            for (int i = _modifiers.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_modifiers[i].Source, source))
                    _modifiers.RemoveAt(i);
            }
        }

        /// <summary>수정자가 적용된 최종 스탯 값을 계산한다.</summary>
        public float Get(string statKey)
        {
            float flat = 0f;
            float percentAdd = 0f;
            float percentMul = 1f;

            for (int i = 0; i < _modifiers.Count; i++)
            {
                Entry e = _modifiers[i];
                if (e.StatKey != statKey)
                    continue;

                switch (e.ModifierType)
                {
                    case StatModifierType.Flat:
                        flat += e.Value;
                        break;
                    case StatModifierType.PercentAdd:
                        percentAdd += e.Value;
                        break;
                    case StatModifierType.PercentMul:
                        percentMul *= 1f + e.Value;
                        break;
                }
            }

            return (GetBase(statKey) + flat) * (1f + percentAdd) * percentMul;
        }
    }
}
