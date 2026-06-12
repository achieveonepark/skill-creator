using System;
using System.Collections.Generic;

namespace SkillCreator
{
    /// <summary>
    /// skills.json 의 루트 컨테이너. JsonUtility 는 최상위 배열을 직접 역직렬화하지 못하므로
    /// 객체 래퍼를 사용한다. (로더는 최상위 배열 형태도 자동 래핑하여 지원한다.)
    /// </summary>
    [Serializable]
    public class SkillDataFile
    {
        public List<SkillDefinition> skills = new List<SkillDefinition>();
    }

    /// <summary>
    /// buffs.json 의 루트 컨테이너.
    /// </summary>
    [Serializable]
    public class BuffDataFile
    {
        public List<BuffDefinition> buffs = new List<BuffDefinition>();
    }
}
