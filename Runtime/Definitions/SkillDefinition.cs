using System;
using System.Collections.Generic;

namespace SkillForge
{
    /// <summary>
    /// 스킬의 정적 데이터. JSON 으로 직렬화/역직렬화된다.
    /// </summary>
    [Serializable]
    public class SkillDefinition
    {
        public string id;
        public string name;

        /// <summary>쿨타임(초).</summary>
        public float cooldown;

        /// <summary>캐스팅 시간(초).</summary>
        public float castTime;

        public TargetingDefinition targeting = new TargetingDefinition();

        public List<ConditionDefinition> conditions = new List<ConditionDefinition>();
        public List<EffectDefinition> effects = new List<EffectDefinition>();
    }
}
