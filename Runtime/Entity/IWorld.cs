using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    /// <summary>
    /// 엔티티 스폰/조회 등 게임 월드와의 통합 경계. 사용자 게임이 구현한다.
    /// (기존 IUnitRegistry 의 도메인 무관 일반화 버전.)
    /// effect 가 월드에 작용해야 할 때 ReactionContext 를 통해 접근한다.
    /// </summary>
    public interface IWorld
    {
        /// <summary>dataId 정의로 엔티티를 생성해 위치 at 에 배치하고 반환한다.</summary>
        IEntity Spawn(string dataId, Vector3 at);

        /// <summary>center 반경 radius 안의 엔티티를 buffer 에 채운다(호출자가 비워서 전달).</summary>
        void Collect(Vector3 center, float radius, List<IEntity> buffer);
    }
}
