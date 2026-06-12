using System;
using System.Collections.Generic;

namespace SkillCreator
{
    /// <summary>
    /// 버프의 정적 데이터. JSON 으로 직렬화/역직렬화된다.
    /// </summary>
    [Serializable]
    public class BuffDefinition
    {
        public string id;
        public string name;

        /// <summary>지속시간(초).</summary>
        public float duration;

        /// <summary>최대 스택 수. 1 이상.</summary>
        public int maxStack = 1;

        /// <summary>스택 정책. <see cref="StackPolicy"/> 상수.</summary>
        public string stackPolicy = StackPolicy.Stack;

        /// <summary>갱신 정책. <see cref="RefreshPolicy"/> 상수.</summary>
        public string refreshPolicy = RefreshPolicy.RefreshDuration;

        public List<StatModifierDefinition> statModifiers = new List<StatModifierDefinition>();
        public List<EffectDefinition> onApplyEffects = new List<EffectDefinition>();
        public List<EffectDefinition> periodicEffects = new List<EffectDefinition>();
        public List<EffectDefinition> onRemoveEffects = new List<EffectDefinition>();
    }
}
