using System.Collections.Generic;
using UnityEngine;

namespace AchReactive.Samples
{
    /// <summary>
    /// IWorld 의 최소 구현 예시. 등록된 엔티티 목록을 들고, 반경 조회/스폰을 제공한다.
    /// 실제 게임에서는 Physics.OverlapSphere 나 자체 공간 분할을 써도 된다.
    /// </summary>
    public sealed class SampleWorld : IWorld
    {
        private readonly List<IEntity> _entities = new List<IEntity>();

        public void Register(IEntity entity)
        {
            if (entity != null && !_entities.Contains(entity))
                _entities.Add(entity);
        }

        public IEntity Spawn(string dataId, Vector3 at)
        {
            // 예시에서는 실제 인스턴스화 대신 로그만 남긴다. 게임이 자기 프리팹 생성으로 대체한다.
            Debug.Log($"[AchReactive] Spawn '{dataId}' at {at}");
            return null;
        }

        public void Collect(Vector3 center, float radius, List<IEntity> buffer)
        {
            float sqr = radius * radius;
            foreach (IEntity e in _entities)
            {
                if (e == null)
                    continue;
                if ((e.Position - center).sqrMagnitude <= sqr)
                    buffer.Add(e);
            }
        }
    }
}
