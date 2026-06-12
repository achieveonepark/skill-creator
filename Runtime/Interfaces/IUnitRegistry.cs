using System.Collections.Generic;
using UnityEngine;

namespace SkillForge
{
    /// <summary>
    /// 광역 타겟팅(circle / cone)을 위해 전투 중인 유닛들을 조회하는 통합 경계.
    /// 사용자의 전투 시스템이 구현한다. (예: Physics.OverlapSphere 또는 자체 유닛 목록)
    /// </summary>
    public interface IUnitRegistry
    {
        /// <summary>현재 전투에 등록된 모든 유닛.</summary>
        IReadOnlyList<IBattleUnit> AllUnits { get; }

        /// <summary>
        /// center 를 중심으로 radius 안에 있는 유닛을 buffer 에 채운다.
        /// 호출자가 buffer 를 초기화한 뒤 전달한다.
        /// </summary>
        void Collect(Vector3 center, float radius, List<IBattleUnit> buffer);
    }
}
