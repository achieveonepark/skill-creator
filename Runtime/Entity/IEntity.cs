using UnityEngine;

namespace GameData
{
    /// <summary>
    /// 리액션의 주체/대상이 되는 게임 객체의 통합 경계. 사용자 게임이 구현한다.
    /// (기존 IBattleUnit 의 도메인 무관 일반화 버전.)
    /// 스탯/버프 등 도메인 상태는 M4 이식 단계에서 확장된다.
    /// </summary>
    public interface IEntity
    {
        /// <summary>이 엔티티가 참조하는 데이터 정의의 id (선택적).</summary>
        string Id { get; }

        /// <summary>월드 좌표. 일부 effect(스폰/VFX 등)가 사용한다.</summary>
        Vector3 Position { get; set; }
    }
}
