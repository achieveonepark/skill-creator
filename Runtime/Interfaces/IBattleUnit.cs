using UnityEngine;

namespace SkillCreator
{
    /// <summary>
    /// 스킬/버프 시스템이 다루는 전투 유닛의 통합 경계.
    /// 사용자의 전투 시스템이 이 인터페이스를 구현하면 Skill Creator 와 연결된다.
    /// Stats / Cooldowns / Buffs 는 Skill Creator 가 제공하는 구현체를 그대로 사용할 수 있다.
    /// </summary>
    public interface IBattleUnit
    {
        string Id { get; }

        bool IsAlive { get; }

        /// <summary>현재 HP 비율(0~1).</summary>
        float HpRatio { get; }

        Vector3 Position { get; }

        /// <summary>cone 타겟팅 등에서 기준이 되는 정면 방향.</summary>
        Vector3 Forward { get; }

        StatSheet Stats { get; }

        CooldownStore Cooldowns { get; }

        BuffContainer Buffs { get; }

        /// <summary>해당 유닛이 적대 관계인지 판정한다.</summary>
        bool IsEnemyOf(IBattleUnit other);

        void TakeDamage(float amount, string damageType);

        void Heal(float amount);
    }
}
