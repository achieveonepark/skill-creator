using System.IO;
using UnityEngine;

namespace SkillForge.Samples
{
    /// <summary>
    /// SkillForge 를 씬에서 구동하는 부트스트랩 예제.
    /// StreamingAssets/GameData 의 JSON 을 로드해 SkillSystem 을 만들고, 매 프레임 버프를 Tick 한다.
    /// 스페이스바로 첫 적에게 fire_slash 를 사용한다.
    /// </summary>
    [RequireComponent(typeof(SampleUnitRegistry))]
    public sealed class SampleCombatBootstrap : MonoBehaviour
    {
        [SerializeField] private SampleBattleUnit _caster;
        [SerializeField] private SampleBattleUnit _target;
        [SerializeField] private string _skillId = "fire_slash";

        private SkillSystem _system;
        private SampleUnitRegistry _registry;

        private void Start()
        {
            _registry = GetComponent<SampleUnitRegistry>();
            _registry.RebuildFromScene();

            _system = new SkillSystem(_registry, logger: UnityCombatLogger.Instance);

            string skillsJson = ReadStreaming("GameData/skills.json");
            string buffsJson = ReadStreaming("GameData/buffs.json");
            _system.LoadFromJson(skillsJson, buffsJson);

            // 데이터 검증 결과를 콘솔에 남긴다.
            var skills = new System.Collections.Generic.List<SkillDefinition>(_system.Database.Skills.Values);
            var buffs = new System.Collections.Generic.List<BuffDefinition>(_system.Database.Buffs.Values);
            Debug.Log(DataValidator.Validate(skills, buffs));
        }

        private void Update()
        {
            _system?.Tick(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Space) && _caster != null)
            {
                SkillUseResult result = _system.Use(_skillId, _caster, _target);
                Debug.Log($"[SkillForge] {_skillId} -> {result.Status}");
            }
        }

        private static string ReadStreaming(string relative)
        {
            string path = Path.Combine(Application.streamingAssetsPath, relative);
            return File.Exists(path) ? File.ReadAllText(path) : null;
        }
    }
}
