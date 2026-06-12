using System.Collections.Generic;
using UnityEngine;

namespace SkillForge.Samples
{
    /// <summary>
    /// 씬의 SampleBattleUnit 들을 모아 광역 타겟팅을 지원하는 IUnitRegistry 예제.
    /// </summary>
    public sealed class SampleUnitRegistry : MonoBehaviour, IUnitRegistry
    {
        private readonly List<IBattleUnit> _units = new List<IBattleUnit>();

        public IReadOnlyList<IBattleUnit> AllUnits => _units;

        public void Register(IBattleUnit unit)
        {
            if (!_units.Contains(unit))
                _units.Add(unit);
        }

        public void Unregister(IBattleUnit unit)
        {
            _units.Remove(unit);
        }

        public void RebuildFromScene()
        {
            _units.Clear();
            foreach (SampleBattleUnit unit in FindObjectsOfType<SampleBattleUnit>())
                _units.Add(unit);
        }

        public void Collect(Vector3 center, float radius, List<IBattleUnit> buffer)
        {
            float sqr = radius * radius;
            for (int i = 0; i < _units.Count; i++)
            {
                IBattleUnit unit = _units[i];
                if ((unit.Position - center).sqrMagnitude <= sqr)
                    buffer.Add(unit);
            }
        }
    }
}
