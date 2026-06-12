using System;

namespace SkillForge
{
    /// <summary>
    /// 스킬 또는 버프가 실제로 수행하는 효과의 정적 데이터.
    /// type 값은 <see cref="EffectType"/> 상수를 사용한다.
    /// </summary>
    [Serializable]
    public class EffectDefinition
    {
        public string type = EffectType.Damage;

        /// <summary>고정 수치(예: 고정 데미지/힐).</summary>
        public float value;

        /// <summary>계수(예: 공격력 * power).</summary>
        public float power;

        /// <summary>add_buff 시 버프 지속시간 오버라이드. 0 이하이면 버프 정의 값을 사용한다.</summary>
        public float duration;

        /// <summary>주기 효과(periodic) 간격(초).</summary>
        public float interval;

        /// <summary>예약 필드. 효과 대상 재지정용(self/caster 등).</summary>
        public string target;

        /// <summary>add_buff / remove_buff 대상 버프 id.</summary>
        public string buffId;

        /// <summary>play_vfx 대상 이펙트 id.</summary>
        public string vfxId;

        /// <summary>play_sfx 대상 사운드 id.</summary>
        public string sfxId;

        /// <summary>예약 필드. Formula 시스템 연동용.</summary>
        public string formulaId;

        /// <summary>데미지 타입(physical / magical / fire 등). 자유 문자열.</summary>
        public string damageType;
    }
}
