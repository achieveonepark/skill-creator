using System;

namespace SkillCreator
{
    /// <summary>
    /// 스탯 변경 효과의 정적 데이터.
    /// modifierType 값은 <see cref="StatModifierType"/> 상수를 사용한다.
    /// </summary>
    [Serializable]
    public class StatModifierDefinition
    {
        /// <summary>변경할 스탯 키(예: attack, defense, speed).</summary>
        public string statKey;

        /// <summary>flat / percent_add / percent_mul.</summary>
        public string modifierType = StatModifierType.Flat;

        /// <summary>변경 값. percent 계열은 0.2 == +20% 를 의미한다.</summary>
        public float value;
    }
}
