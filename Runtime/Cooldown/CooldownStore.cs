using System;
using System.Collections.Generic;
using UnityEngine;

namespace SkillCreator
{
    /// <summary>
    /// 스킬 id 별 쿨타임을 절대 시간 기준으로 관리한다.
    /// 매 프레임 Tick 이 필요 없도록 시간 제공자(timeProvider)와 비교한다.
    /// 기본 시간 제공자는 UnityEngine.Time.time.
    /// </summary>
    public sealed class CooldownStore
    {
        private readonly Dictionary<string, float> _readyAt = new Dictionary<string, float>();
        private readonly Func<float> _now;

        public CooldownStore(Func<float> timeProvider = null)
        {
            _now = timeProvider ?? (() => Time.time);
        }

        public bool IsReady(string skillId)
        {
            return !_readyAt.TryGetValue(skillId, out float t) || _now() >= t;
        }

        public void Start(string skillId, float cooldown)
        {
            if (cooldown <= 0f)
            {
                _readyAt.Remove(skillId);
                return;
            }

            _readyAt[skillId] = _now() + cooldown;
        }

        /// <summary>남은 쿨타임(초). 준비 완료면 0.</summary>
        public float Remaining(string skillId)
        {
            return _readyAt.TryGetValue(skillId, out float t) ? Mathf.Max(0f, t - _now()) : 0f;
        }

        public void Clear()
        {
            _readyAt.Clear();
        }
    }
}
