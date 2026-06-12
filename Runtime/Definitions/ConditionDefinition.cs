using System;

namespace SkillCreator
{
    /// <summary>
    /// 효과 실행 전 검사하는 조건의 정적 데이터.
    /// type 값은 <see cref="ConditionType"/> 상수를 사용한다.
    /// </summary>
    [Serializable]
    public class ConditionDefinition
    {
        public string type = ConditionType.Always;

        /// <summary>chance(확률), hp_below/hp_above(비율), target_count_* 등에서 사용하는 수치.</summary>
        public float value;

        /// <summary>has_buff / not_has_buff 에서 사용하는 버프 id.</summary>
        public string key;

        /// <summary>예약 필드. 비교 연산자 확장용.</summary>
        public string compare;
    }
}
