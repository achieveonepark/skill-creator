using System.Collections.Generic;

namespace SkillForge
{
    /// <summary>
    /// JSON 등에서 로드한 스킬/버프 정의를 Dictionary 로 보관하는 인메모리 데이터베이스.
    /// 런타임은 이 데이터베이스를 그대로 조회하여 실행한다.
    /// </summary>
    public sealed class SkillDatabase
    {
        private readonly Dictionary<string, SkillDefinition> _skills = new Dictionary<string, SkillDefinition>();
        private readonly Dictionary<string, BuffDefinition> _buffs = new Dictionary<string, BuffDefinition>();

        public IReadOnlyDictionary<string, SkillDefinition> Skills => _skills;
        public IReadOnlyDictionary<string, BuffDefinition> Buffs => _buffs;

        public SkillDefinition GetSkill(string id)
        {
            _skills.TryGetValue(id, out SkillDefinition skill);
            return skill;
        }

        public BuffDefinition GetBuff(string id)
        {
            _buffs.TryGetValue(id, out BuffDefinition buff);
            return buff;
        }

        public bool HasSkill(string id) => id != null && _skills.ContainsKey(id);

        public bool HasBuff(string id) => id != null && _buffs.ContainsKey(id);

        /// <summary>기존 스킬을 모두 교체한다. 런타임 패치 시 사용.</summary>
        public void LoadSkills(IEnumerable<SkillDefinition> skills)
        {
            _skills.Clear();
            AddSkills(skills);
        }

        /// <summary>기존 스킬을 유지한 채 병합한다(같은 id 는 덮어씀).</summary>
        public void AddSkills(IEnumerable<SkillDefinition> skills)
        {
            if (skills == null)
                return;

            foreach (SkillDefinition skill in skills)
            {
                if (skill != null && !string.IsNullOrEmpty(skill.id))
                    _skills[skill.id] = skill;
            }
        }

        public void LoadBuffs(IEnumerable<BuffDefinition> buffs)
        {
            _buffs.Clear();
            AddBuffs(buffs);
        }

        public void AddBuffs(IEnumerable<BuffDefinition> buffs)
        {
            if (buffs == null)
                return;

            foreach (BuffDefinition buff in buffs)
            {
                if (buff != null && !string.IsNullOrEmpty(buff.id))
                    _buffs[buff.id] = buff;
            }
        }

        public void Clear()
        {
            _skills.Clear();
            _buffs.Clear();
        }
    }
}
