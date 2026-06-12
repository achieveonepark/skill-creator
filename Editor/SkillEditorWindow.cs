using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SkillForge.EditorTools
{
    /// <summary>
    /// 스킬 데이터를 JSON 으로 편집하는 EditorWindow.
    /// 목록 보기 / 추가 / 삭제 / 복제 / 필드 편집 / 저장 / 검증을 제공한다.
    /// </summary>
    public sealed class SkillEditorWindow : EditorWindow
    {
        private string _path = SkillForgeEditorUtil.DefaultSkillsPath;
        private string _buffsPath = SkillForgeEditorUtil.DefaultBuffsPath;

        private List<SkillDefinition> _skills = new List<SkillDefinition>();
        private List<BuffDefinition> _buffs = new List<BuffDefinition>();

        private int _selected = -1;
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private ValidationReport _report;

        [MenuItem("Tools/SkillForge/Skill Editor")]
        public static void Open()
        {
            GetWindow<SkillEditorWindow>("SkillForge - Skills");
        }

        private void OnEnable()
        {
            Reload();
        }

        private void Reload()
        {
            _skills = JsonExporter.LoadSkills(_path);
            _buffs = JsonExporter.LoadBuffs(_buffsPath);
            _selected = _skills.Count > 0 ? 0 : -1;
            _report = null;
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.BeginHorizontal();
            DrawList();
            DrawDetail();
            EditorGUILayout.EndHorizontal();
            DrawReport();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            _path = EditorGUILayout.TextField("Skills JSON", _path);

            if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60)))
                Reload();

            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
                JsonExporter.SaveSkills(_skills, _path);

            if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(70)))
                _report = DataValidator.Validate(_skills, _buffs);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(220));
            SkillForgeEditorUtil.HeaderRow($"Skills ({_skills.Count})");

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            for (int i = 0; i < _skills.Count; i++)
            {
                bool isSel = i == _selected;
                string label = string.IsNullOrEmpty(_skills[i].id) ? "(no id)" : _skills[i].id;
                if (GUILayout.Toggle(isSel, label, EditorStyles.miniButton) && !isSel)
                    _selected = i;
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add"))
                AddSkill();
            if (GUILayout.Button("Duplicate"))
                DuplicateSelected();
            EditorGUILayout.EndHorizontal();

            if (SkillForgeEditorUtil.DangerButton("- Remove", 220))
                RemoveSelected();

            EditorGUILayout.EndVertical();
        }

        private void DrawDetail()
        {
            EditorGUILayout.BeginVertical();
            _detailScroll = EditorGUILayout.BeginScrollView(_detailScroll);

            if (_selected < 0 || _selected >= _skills.Count)
            {
                EditorGUILayout.HelpBox("왼쪽에서 스킬을 선택하거나 추가하세요.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                return;
            }

            SkillDefinition skill = _skills[_selected];

            SkillForgeEditorUtil.HeaderRow("기본 정보");
            skill.id = EditorGUILayout.TextField("Id", skill.id);
            skill.name = EditorGUILayout.TextField("Name", skill.name);
            skill.cooldown = EditorGUILayout.FloatField("Cooldown", skill.cooldown);
            skill.castTime = EditorGUILayout.FloatField("Cast Time", skill.castTime);

            SkillForgeEditorUtil.HeaderRow("타겟팅");
            skill.targeting ??= new TargetingDefinition();
            skill.targeting.type = SkillForgeEditorUtil.PopupString("Type", skill.targeting.type, TargetingType.Supported);
            skill.targeting.range = EditorGUILayout.FloatField("Range", skill.targeting.range);
            skill.targeting.radius = EditorGUILayout.FloatField("Radius", skill.targeting.radius);
            skill.targeting.angle = EditorGUILayout.FloatField("Angle", skill.targeting.angle);
            skill.targeting.maxTargets = EditorGUILayout.IntField("Max Targets", skill.targeting.maxTargets);

            DrawConditions(skill);
            DrawEffects(skill);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawConditions(SkillDefinition skill)
        {
            SkillForgeEditorUtil.HeaderRow($"조건 ({skill.conditions.Count})");

            for (int i = 0; i < skill.conditions.Count; i++)
            {
                ConditionDefinition c = skill.conditions[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                c.type = SkillForgeEditorUtil.PopupString("Type", c.type, ConditionType.Supported);
                if (SkillForgeEditorUtil.DangerButton("X", 24))
                {
                    skill.conditions.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                c.value = EditorGUILayout.FloatField("Value", c.value);
                c.key = EditorGUILayout.TextField("Key (buffId 등)", c.key);
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("+ Add Condition"))
                skill.conditions.Add(new ConditionDefinition());
        }

        private void DrawEffects(SkillDefinition skill)
        {
            SkillForgeEditorUtil.HeaderRow($"효과 ({skill.effects.Count})");

            for (int i = 0; i < skill.effects.Count; i++)
            {
                EffectDefinition e = skill.effects[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                e.type = SkillForgeEditorUtil.PopupString("Type", e.type, EffectType.Supported);
                if (SkillForgeEditorUtil.DangerButton("X", 24))
                {
                    skill.effects.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                DrawEffectFields(e);
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("+ Add Effect"))
                skill.effects.Add(new EffectDefinition());
        }

        private static void DrawEffectFields(EffectDefinition e)
        {
            switch (e.type)
            {
                case EffectType.Damage:
                case EffectType.Heal:
                    e.power = EditorGUILayout.FloatField("Power (x공격력)", e.power);
                    e.value = EditorGUILayout.FloatField("Flat Value", e.value);
                    if (e.type == EffectType.Damage)
                        e.damageType = EditorGUILayout.TextField("Damage Type", e.damageType);
                    break;

                case EffectType.AddBuff:
                    e.buffId = EditorGUILayout.TextField("Buff Id", e.buffId);
                    e.duration = EditorGUILayout.FloatField("Duration (0=기본)", e.duration);
                    break;

                case EffectType.RemoveBuff:
                    e.buffId = EditorGUILayout.TextField("Buff Id", e.buffId);
                    break;

                case EffectType.PlayVfx:
                    e.vfxId = EditorGUILayout.TextField("Vfx Id", e.vfxId);
                    break;

                case EffectType.PlaySfx:
                    e.sfxId = EditorGUILayout.TextField("Sfx Id", e.sfxId);
                    break;
            }
        }

        private void DrawReport()
        {
            if (_report == null)
                return;

            MessageType type = _report.HasErrors ? MessageType.Error
                : _report.WarningCount > 0 ? MessageType.Warning : MessageType.Info;
            EditorGUILayout.HelpBox(_report.ToString(), type);
        }

        private void AddSkill()
        {
            _skills.Add(new SkillDefinition { id = "new_skill", name = "New Skill" });
            _selected = _skills.Count - 1;
        }

        private void DuplicateSelected()
        {
            if (_selected < 0 || _selected >= _skills.Count)
                return;

            string json = JsonUtility.ToJson(_skills[_selected]);
            SkillDefinition copy = JsonUtility.FromJson<SkillDefinition>(json);
            copy.id += "_copy";
            _skills.Insert(_selected + 1, copy);
            _selected++;
        }

        private void RemoveSelected()
        {
            if (_selected < 0 || _selected >= _skills.Count)
                return;

            _skills.RemoveAt(_selected);
            _selected = Mathf.Clamp(_selected, -1, _skills.Count - 1);
        }
    }
}
