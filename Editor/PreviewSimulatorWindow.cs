using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SkillForge.EditorTools
{
    /// <summary>
    /// 실제 전투 없이 스킬을 시뮬레이션해 예상 데미지/힐/버프 결과를 미리 본다.
    /// 더미 캐스터/타겟을 만들어 SkillSystem 을 그대로 구동한다.
    /// </summary>
    public sealed class PreviewSimulatorWindow : EditorWindow
    {
        private string _skillsPath = SkillForgeEditorUtil.DefaultSkillsPath;
        private string _buffsPath = SkillForgeEditorUtil.DefaultBuffsPath;

        private float _casterAttack = 100f;
        private float _targetDefense = 0f;
        private int _targetCount = 3;
        private float _targetHpRatio = 1f;
        private float _simSeconds = 5f;

        private string _skillId = "";
        private string _log = "";
        private Vector2 _scroll;

        [MenuItem("Tools/SkillForge/Preview Simulator")]
        public static void Open()
        {
            GetWindow<PreviewSimulatorWindow>("SkillForge - Preview");
        }

        private void OnGUI()
        {
            SkillForgeEditorUtil.HeaderRow("데이터 경로");
            _skillsPath = EditorGUILayout.TextField("Skills JSON", _skillsPath);
            _buffsPath = EditorGUILayout.TextField("Buffs JSON", _buffsPath);

            SkillForgeEditorUtil.HeaderRow("시뮬레이션 입력");
            _skillId = EditorGUILayout.TextField("Skill Id", _skillId);
            _casterAttack = EditorGUILayout.FloatField("Caster Attack", _casterAttack);
            _targetDefense = EditorGUILayout.FloatField("Target Defense", _targetDefense);
            _targetCount = Mathf.Max(1, EditorGUILayout.IntField("Target Count", _targetCount));
            _targetHpRatio = EditorGUILayout.Slider("Target HP Ratio", _targetHpRatio, 0f, 1f);
            _simSeconds = EditorGUILayout.FloatField("Sim Seconds (버프 진행)", _simSeconds);

            if (GUILayout.Button("Run Simulation", GUILayout.Height(28)))
                RunSimulation();

            SkillForgeEditorUtil.HeaderRow("결과 로그");
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            EditorGUILayout.TextArea(_log, GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
        }

        private void RunSimulation()
        {
            var skills = JsonExporter.LoadSkills(_skillsPath);
            var buffs = JsonExporter.LoadBuffs(_buffsPath);

            var logger = new StringCombatLogger();
            var registry = new SimRegistry();
            var system = new SkillSystem(registry, logger: logger);
            system.LoadSkills(skills);
            system.LoadBuffs(buffs);

            var caster = new SimUnit("Caster", _casterAttack, 0f, isEnemyOfOthers: true);
            registry.Add(caster);

            for (int i = 0; i < _targetCount; i++)
            {
                var target = new SimUnit($"Target_{i + 1}", 0f, _targetDefense, isEnemyOfOthers: true)
                {
                    HpRatioValue = _targetHpRatio
                };
                registry.Add(target);
            }

            IBattleUnit firstTarget = registry.AllUnits.Count > 1 ? registry.AllUnits[1] : null;

            SkillUseResult result = system.Use(_skillId, caster, firstTarget);
            logger.Line($"Result: {result.Status}, Targets: {(result.Targets != null ? result.Targets.Count : 0)}");

            // 버프 주기 효과 진행(1초 간격으로 틱).
            float t = 0f;
            while (t < _simSeconds)
            {
                system.Tick(1f);
                t += 1f;
            }

            _log = logger.Build();
        }

        /// <summary>로그를 문자열로 모으는 로거.</summary>
        private sealed class StringCombatLogger : ICombatLogger
        {
            private readonly StringBuilder _sb = new StringBuilder();

            public void Log(string message) => _sb.AppendLine(message);

            public void Line(string message) => _sb.AppendLine(message);

            public string Build() => _sb.ToString();
        }

        /// <summary>시뮬레이션용 인메모리 유닛.</summary>
        private sealed class SimUnit : IBattleUnit
        {
            private readonly bool _isEnemyOfOthers;

            public SimUnit(string id, float attack, float defense, bool isEnemyOfOthers)
            {
                Id = id;
                _isEnemyOfOthers = isEnemyOfOthers;
                Stats = new StatSheet();
                Stats.SetBase("attack", attack);
                Stats.SetBase("defense", defense);
                Cooldowns = new CooldownStore(() => 0f);
                Buffs = new BuffContainer();
            }

            public string Id { get; }
            public bool IsAlive => HpRatioValue > 0f;
            public float HpRatioValue { get; set; } = 1f;
            public float HpRatio => HpRatioValue;
            public Vector3 Position => Vector3.zero;
            public Vector3 Forward => Vector3.forward;
            public StatSheet Stats { get; }
            public CooldownStore Cooldowns { get; }
            public BuffContainer Buffs { get; }

            public bool IsEnemyOf(IBattleUnit other) => _isEnemyOfOthers && !ReferenceEquals(other, this);
            public void TakeDamage(float amount, string damageType) { }
            public void Heal(float amount) { }
        }

        /// <summary>모든 유닛을 반경 무시하고 반환하는 시뮬 레지스트리.</summary>
        private sealed class SimRegistry : IUnitRegistry
        {
            private readonly List<IBattleUnit> _units = new List<IBattleUnit>();

            public IReadOnlyList<IBattleUnit> AllUnits => _units;

            public void Add(IBattleUnit unit) => _units.Add(unit);

            public void Collect(Vector3 center, float radius, List<IBattleUnit> buffer)
            {
                buffer.AddRange(_units);
            }
        }
    }
}
