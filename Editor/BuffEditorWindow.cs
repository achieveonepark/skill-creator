using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SkillForge.EditorTools
{
    /// <summary>
    /// 버프 데이터를 JSON 으로 편집하는 EditorWindow.
    /// 목록 / 추가 / 삭제 / 복제 / 스탯·주기 효과 편집 / 저장 / 검증을 제공한다.
    /// </summary>
    public sealed class BuffEditorWindow : EditorWindow
    {
        private string _path = SkillForgeEditorUtil.DefaultBuffsPath;

        private List<BuffDefinition> _buffs = new List<BuffDefinition>();
        private int _selected = -1;
        private Vector2 _listScroll;
        private Vector2 _detailScroll;
        private ValidationReport _report;

        [MenuItem("Tools/SkillForge/Buff Editor")]
        public static void Open()
        {
            GetWindow<BuffEditorWindow>("SkillForge - Buffs");
        }

        private void OnEnable()
        {
            Reload();
        }

        private void Reload()
        {
            _buffs = JsonExporter.LoadBuffs(_path);
            _selected = _buffs.Count > 0 ? 0 : -1;
            _report = null;
        }

        private void OnGUI()
        {
            DrawToolbar();
            EditorGUILayout.BeginHorizontal();
            DrawList();
            DrawDetail();
            EditorGUILayout.EndHorizontal();

            if (_report != null)
            {
                MessageType type = _report.HasErrors ? MessageType.Error
                    : _report.WarningCount > 0 ? MessageType.Warning : MessageType.Info;
                EditorGUILayout.HelpBox(_report.ToString(), type);
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            _path = EditorGUILayout.TextField("Buffs JSON", _path);

            if (GUILayout.Button("Reload", EditorStyles.toolbarButton, GUILayout.Width(60)))
                Reload();
            if (GUILayout.Button("Save", EditorStyles.toolbarButton, GUILayout.Width(60)))
                JsonExporter.SaveBuffs(_buffs, _path);
            if (GUILayout.Button("Validate", EditorStyles.toolbarButton, GUILayout.Width(70)))
                _report = DataValidator.Validate(null, _buffs);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawList()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(220));
            SkillForgeEditorUtil.HeaderRow($"Buffs ({_buffs.Count})");

            _listScroll = EditorGUILayout.BeginScrollView(_listScroll);
            for (int i = 0; i < _buffs.Count; i++)
            {
                bool isSel = i == _selected;
                string label = string.IsNullOrEmpty(_buffs[i].id) ? "(no id)" : _buffs[i].id;
                if (GUILayout.Toggle(isSel, label, EditorStyles.miniButton) && !isSel)
                    _selected = i;
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Add"))
                AddBuff();
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

            if (_selected < 0 || _selected >= _buffs.Count)
            {
                EditorGUILayout.HelpBox("왼쪽에서 버프를 선택하거나 추가하세요.", MessageType.Info);
                EditorGUILayout.EndScrollView();
                EditorGUILayout.EndVertical();
                return;
            }

            BuffDefinition buff = _buffs[_selected];

            SkillForgeEditorUtil.HeaderRow("기본 정보");
            buff.id = EditorGUILayout.TextField("Id", buff.id);
            buff.name = EditorGUILayout.TextField("Name", buff.name);
            buff.duration = EditorGUILayout.FloatField("Duration", buff.duration);
            buff.maxStack = EditorGUILayout.IntField("Max Stack", buff.maxStack);
            buff.stackPolicy = SkillForgeEditorUtil.PopupString("Stack Policy", buff.stackPolicy, StackPolicy.Supported);
            buff.refreshPolicy = SkillForgeEditorUtil.PopupString("Refresh Policy", buff.refreshPolicy, RefreshPolicy.Supported);

            DrawStatModifiers(buff);
            DrawEffectList("주기 효과 (periodic)", buff.periodicEffects, true);
            DrawEffectList("적용 시 효과 (onApply)", buff.onApplyEffects, false);
            DrawEffectList("해제 시 효과 (onRemove)", buff.onRemoveEffects, false);

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawStatModifiers(BuffDefinition buff)
        {
            SkillForgeEditorUtil.HeaderRow($"스탯 수정자 ({buff.statModifiers.Count})");

            for (int i = 0; i < buff.statModifiers.Count; i++)
            {
                StatModifierDefinition m = buff.statModifiers[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                m.statKey = EditorGUILayout.TextField("Stat Key", m.statKey);
                if (SkillForgeEditorUtil.DangerButton("X", 24))
                {
                    buff.statModifiers.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();
                m.modifierType = SkillForgeEditorUtil.PopupString("Modifier Type", m.modifierType, StatModifierType.Supported);
                m.value = EditorGUILayout.FloatField("Value", m.value);
                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button("+ Add Stat Modifier"))
                buff.statModifiers.Add(new StatModifierDefinition());
        }

        private void DrawEffectList(string header, List<EffectDefinition> effects, bool showInterval)
        {
            SkillForgeEditorUtil.HeaderRow($"{header} ({effects.Count})");

            for (int i = 0; i < effects.Count; i++)
            {
                EffectDefinition e = effects[i];
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                e.type = SkillForgeEditorUtil.PopupString("Type", e.type, EffectType.Supported);
                if (SkillForgeEditorUtil.DangerButton("X", 24))
                {
                    effects.RemoveAt(i);
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    break;
                }
                EditorGUILayout.EndHorizontal();

                e.power = EditorGUILayout.FloatField("Power", e.power);
                e.value = EditorGUILayout.FloatField("Flat Value", e.value);
                if (showInterval)
                    e.interval = EditorGUILayout.FloatField("Interval", e.interval);
                if (e.type == EffectType.Damage)
                    e.damageType = EditorGUILayout.TextField("Damage Type", e.damageType);
                if (e.type == EffectType.AddBuff || e.type == EffectType.RemoveBuff)
                    e.buffId = EditorGUILayout.TextField("Buff Id", e.buffId);
                if (e.type == EffectType.PlayVfx)
                    e.vfxId = EditorGUILayout.TextField("Vfx Id", e.vfxId);

                EditorGUILayout.EndVertical();
            }

            if (GUILayout.Button($"+ Add to {header}"))
                effects.Add(new EffectDefinition());
        }

        private void AddBuff()
        {
            _buffs.Add(new BuffDefinition { id = "new_buff", name = "New Buff", duration = 5f, maxStack = 1 });
            _selected = _buffs.Count - 1;
        }

        private void DuplicateSelected()
        {
            if (_selected < 0 || _selected >= _buffs.Count)
                return;

            string json = JsonUtility.ToJson(_buffs[_selected]);
            BuffDefinition copy = JsonUtility.FromJson<BuffDefinition>(json);
            copy.id += "_copy";
            _buffs.Insert(_selected + 1, copy);
            _selected++;
        }

        private void RemoveSelected()
        {
            if (_selected < 0 || _selected >= _buffs.Count)
                return;

            _buffs.RemoveAt(_selected);
            _selected = Mathf.Clamp(_selected, -1, _buffs.Count - 1);
        }
    }
}
