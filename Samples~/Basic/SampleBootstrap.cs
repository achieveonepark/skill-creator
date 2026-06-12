using UnityEngine;

namespace AchReactive.Samples
{
    /// <summary>
    /// AchReactive 를 조립하는 최소 예시. Space 키를 누르면 caster 가 target 에게
    /// fire_slash 스킬을 사용한다(on_use 리액션 실행). 콘솔에서 데미지 로그를 확인할 수 있다.
    /// </summary>
    public sealed class SampleBootstrap : MonoBehaviour
    {
        [SerializeField] private TextAsset skillsJson;   // skills.json (최상위 배열)
        [SerializeField] private SampleEntity caster;
        [SerializeField] private SampleEntity target;
        [SerializeField] private string skillId = "fire_slash";

        private readonly DataBase<SkillData> _skills = new DataBase<SkillData>();
        private readonly ReactionEngine _engine = new ReactionEngine();
        private SampleWorld _world;

        private void Awake()
        {
            // 1) 동작 풀 자동수집 (패키지 기본 effect/condition/trigger 포함)
            EffectRegistry.AutoRegister();
            ConditionRegistry.AutoRegister();
            TriggerRegistry.AutoRegister();

            // 2) 데이터 적재 — 소스는 자유(여기선 TextAsset). CSV/Remote 로 교체 가능.
            if (skillsJson != null)
                _skills.Load(new JsonDataLoader<SkillData>(() => skillsJson.text));

            // 3) 월드 구성
            _world = new SampleWorld();
            _world.Register(caster);
            _world.Register(target);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Space))
                return;

            SkillData skill = _skills.Get(skillId);
            if (skill == null)
            {
                Debug.LogWarning($"[AchReactive] 스킬 '{skillId}' 을 찾을 수 없습니다.");
                return;
            }

            var ctx = new ReactionContext
            {
                Source = caster,
                Target = target,
                World = _world
            };

            _engine.Run("on_use", skill, ctx);
            Debug.Log($"[AchReactive] {target.Id} 의 남은 HP: {target.Stats.GetBase(StatKeys.Hp)}");
        }
    }
}
