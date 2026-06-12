using UnityEngine;

namespace AchReactive.Samples
{
    /// <summary>
    /// IEntity 의 최소 구현 예시. 패키지가 제공하는 StatSheet 을 그대로 노출하고,
    /// hp 스탯으로 생사를 판정한다. 게임의 캐릭터/몬스터가 이런 식으로 구현하면 된다.
    /// </summary>
    public sealed class SampleEntity : MonoBehaviour, IEntity
    {
        [SerializeField] private string id;
        [SerializeField] private float hp = 100f;
        [SerializeField] private float hpMax = 100f;
        [SerializeField] private float atk = 10f;
        [SerializeField] private float def = 2f;

        public StatSheet Stats { get; } = new StatSheet();

        public string Id => id;

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public bool IsAlive => Stats.GetBase(StatKeys.Hp) > 0f;

        private void Awake()
        {
            Stats.SetBase(StatKeys.Hp, hp);
            Stats.SetBase(StatKeys.HpMax, hpMax);
            Stats.SetBase(StatKeys.Atk, atk);
            Stats.SetBase(StatKeys.Def, def);
        }
    }
}
