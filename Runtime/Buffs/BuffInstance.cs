using System.Collections.Generic;

namespace SkillCreator
{
    /// <summary>
    /// 유닛에 적용된 버프의 런타임 상태.
    /// 스택, 남은 지속시간, 주기 효과 타이머를 보관한다.
    /// 인스턴스 자신이 StatSheet 수정자의 source 토큰으로 사용된다.
    /// </summary>
    public sealed class BuffInstance
    {
        public BuffDefinition Definition { get; }
        public IBattleUnit Owner { get; }
        public IBattleUnit Source { get; }

        public int Stacks { get; private set; }
        public float Remaining { get; private set; }

        /// <summary>periodicEffects 와 인덱스가 1:1 대응하는 누적 타이머.</summary>
        private readonly List<float> _periodicTimers = new List<float>();

        public BuffInstance(BuffDefinition definition, IBattleUnit owner, IBattleUnit source, float duration)
        {
            Definition = definition;
            Owner = owner;
            Source = source;
            Stacks = 1;
            Remaining = duration;

            int count = definition.periodicEffects != null ? definition.periodicEffects.Count : 0;
            for (int i = 0; i < count; i++)
                _periodicTimers.Add(0f);
        }

        public bool IsExpired => Remaining <= 0f;

        public void RefreshDuration(float duration)
        {
            Remaining = duration;
        }

        public void AddStack(int maxStack)
        {
            if (Stacks < maxStack)
                Stacks++;
        }

        /// <summary>남은 시간을 감소시킨다.</summary>
        public void Advance(float deltaTime)
        {
            Remaining -= deltaTime;
        }

        /// <summary>
        /// 주기 효과 타이머를 진행시키고, interval 을 넘긴 효과 인덱스들을 발생시킨다.
        /// 누적 방식이라 한 프레임에 여러 번 발동할 수도 있다.
        /// </summary>
        public void TickPeriodic(float deltaTime, List<int> firedIndices)
        {
            firedIndices.Clear();

            if (Definition.periodicEffects == null)
                return;

            for (int i = 0; i < _periodicTimers.Count; i++)
            {
                EffectDefinition effect = Definition.periodicEffects[i];
                float interval = effect.interval > 0f ? effect.interval : 1f;

                _periodicTimers[i] += deltaTime;

                while (_periodicTimers[i] >= interval)
                {
                    _periodicTimers[i] -= interval;
                    firedIndices.Add(i);
                }
            }
        }
    }
}
