using System;
using System.Collections.Generic;

namespace GameData
{
    /// <summary>
    /// 도메인 무관 기본 엔티티 데이터. 그대로 사용해도 되고, 도메인별 필드를 더하려면 상속한다.
    /// (예: <c>class SkillData : EntityData { public float cooldown; }</c>)
    /// reactions 만으로 동작을 표현하므로, 도메인이 늘어도 실행 코드는 늘지 않는다.
    /// </summary>
    [Serializable]
    public class EntityData : IData
    {
        /// <summary>고유 식별자.</summary>
        public string id;

        /// <summary>수치 스탯(선택적). 키 → 값.</summary>
        public Dictionary<string, float> stats = new Dictionary<string, float>();

        /// <summary>이 엔티티의 반응 규칙 목록.</summary>
        public Reaction[] reactions = Array.Empty<Reaction>();

        /// <inheritdoc/>
        public string Id => id;
    }
}
