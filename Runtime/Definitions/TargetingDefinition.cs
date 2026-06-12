using System;

namespace SkillCreator
{
    /// <summary>
    /// 타겟을 찾는 방식에 대한 정적 데이터.
    /// type 값은 <see cref="TargetingType"/> 상수를 사용한다.
    /// </summary>
    [Serializable]
    public class TargetingDefinition
    {
        public string type = TargetingType.SingleEnemy;

        /// <summary>cone 등에서 사용하는 사거리.</summary>
        public float range;

        /// <summary>circle 에서 사용하는 반경.</summary>
        public float radius;

        /// <summary>cone 에서 사용하는 부채꼴 각도(도 단위).</summary>
        public float angle;

        /// <summary>최대 타겟 수. 0 이하이면 제한 없음으로 취급한다.</summary>
        public int maxTargets;
    }
}
