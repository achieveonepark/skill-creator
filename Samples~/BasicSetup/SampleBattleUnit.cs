using UnityEngine;

namespace SkillCreator.Samples
{
    /// <summary>
    /// IBattleUnit 의 최소 구현 예제. 실제 프로젝트에서는 자신의 캐릭터 클래스가 IBattleUnit 을 구현하면 된다.
    /// </summary>
    public sealed class SampleBattleUnit : MonoBehaviour, IBattleUnit
    {
        [SerializeField] private string _id = "unit";
        [SerializeField] private int _team = 0;
        [SerializeField] private float _maxHp = 1000f;
        [SerializeField] private float _attack = 100f;
        [SerializeField] private float _defense = 10f;

        private float _hp;

        public string Id => _id;
        public int Team => _team;

        public StatSheet Stats { get; private set; }
        public CooldownStore Cooldowns { get; private set; }
        public BuffContainer Buffs { get; private set; }

        public bool IsAlive => _hp > 0f;
        public float HpRatio => _maxHp > 0f ? Mathf.Clamp01(_hp / _maxHp) : 0f;
        public Vector3 Position => transform.position;
        public Vector3 Forward => transform.forward;

        private void Awake()
        {
            Stats = new StatSheet();
            Stats.SetBase("attack", _attack);
            Stats.SetBase("defense", _defense);

            Cooldowns = new CooldownStore();
            Buffs = new BuffContainer();
            _hp = _maxHp;
        }

        public bool IsEnemyOf(IBattleUnit other)
        {
            return other is SampleBattleUnit unit && unit.Team != Team;
        }

        public void TakeDamage(float amount, string damageType)
        {
            // 방어력 반영 예시: 데미지에서 방어력의 일부를 차감.
            float mitigated = Mathf.Max(1f, amount - Stats.Get("defense") * 0.5f);
            _hp = Mathf.Max(0f, _hp - mitigated);
        }

        public void Heal(float amount)
        {
            _hp = Mathf.Min(_maxHp, _hp + amount);
        }
    }
}
