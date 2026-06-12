using System.Collections.Generic;

namespace SkillForge
{
    /// <summary>
    /// 한 유닛이 보유한 버프 인스턴스 목록. 유닛마다 하나씩 가진다.
    /// 추가/제거/주기 진행은 <see cref="BuffController"/> 가 담당하고,
    /// 이 컨테이너는 상태 보관과 조회를 담당한다.
    /// </summary>
    public sealed class BuffContainer
    {
        private readonly List<BuffInstance> _instances = new List<BuffInstance>();

        public IReadOnlyList<BuffInstance> Instances => _instances;

        public bool Has(string buffId)
        {
            return Get(buffId) != null;
        }

        public BuffInstance Get(string buffId)
        {
            for (int i = 0; i < _instances.Count; i++)
            {
                if (_instances[i].Definition.id == buffId)
                    return _instances[i];
            }

            return null;
        }

        public int StackCount(string buffId)
        {
            BuffInstance instance = Get(buffId);
            return instance != null ? instance.Stacks : 0;
        }

        public void Add(BuffInstance instance)
        {
            _instances.Add(instance);
        }

        public void Remove(BuffInstance instance)
        {
            _instances.Remove(instance);
        }
    }
}
