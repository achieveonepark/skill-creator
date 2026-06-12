using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchReactive.EditorTools
{
    /// <summary>
    /// UIToolkit 기반 데이터 에디터. EntityData 파생 타입의 인스턴스와 reactions 를 구성해
    /// JSON 배열로 내보낸다. trigger/condition/effect 타입은 레지스트리에서 채운 드롭다운으로
    /// 선택하므로 오타·미등록 타입을 원천 차단한다(params 값만 직접 입력).
    /// </summary>
    public sealed class DataEditorWindow : EditorWindow
    {
        private readonly List<EntityData> _instances = new List<EntityData>();
        private EntityData _selected;

        private VisualElement _instanceList;
        private VisualElement _detail;
        private Label _status;

        private List<string> _triggerOptions;
        private List<string> _conditionOptions;
        private List<string> _effectOptions;

        [MenuItem("Tools/AchReactive/Data Editor")]
        public static void Open()
        {
            var w = GetWindow<DataEditorWindow>();
            w.titleContent = new GUIContent("AchReactive - Data Editor");
            w.minSize = new Vector2(620, 480);
        }

        private void CreateGUI()
        {
            // 드롭다운을 채우기 위해 레지스트리 자동수집(에디터 전용)
            EffectRegistry.AutoRegister();
            ConditionRegistry.AutoRegister();
            TriggerRegistry.AutoRegister();

            _effectOptions = Sorted(EffectRegistry.Names);
            _conditionOptions = Sorted(ConditionRegistry.Names);
            _triggerOptions = CommonChannels.Concat(TriggerRegistry.Names).Distinct().OrderBy(s => s).ToList();
            if (_effectOptions.Count == 0) _effectOptions.Add("damage");
            if (_conditionOptions.Count == 0) _conditionOptions.Add("always");
            if (_triggerOptions.Count == 0) _triggerOptions.Add("on_use");

            VisualElement root = rootVisualElement;
            root.style.flexDirection = FlexDirection.Column;

            // 상단 툴바
            var toolbar = new VisualElement { style = { flexDirection = FlexDirection.Row, paddingLeft = 8, paddingRight = 8, paddingTop = 6, paddingBottom = 6 } };
            var addBtn = new Button(AddInstance) { text = "+ Instance" };
            var exportBtn = new Button(Export) { text = "Export JSON…" };
            toolbar.Add(addBtn);
            toolbar.Add(exportBtn);
            root.Add(toolbar);

            // 본문: 좌측 목록 + 우측 디테일
            var body = new VisualElement { style = { flexDirection = FlexDirection.Row, flexGrow = 1 } };
            root.Add(body);

            _instanceList = new VisualElement { style = { width = 180, borderRightWidth = 1, borderRightColor = new Color(0, 0, 0, 0.2f), paddingLeft = 4, paddingTop = 4 } };
            body.Add(_instanceList);

            var rightScroll = new ScrollView { style = { flexGrow = 1, paddingLeft = 8, paddingRight = 8, paddingTop = 4 } };
            _detail = new VisualElement();
            rightScroll.Add(_detail);
            body.Add(rightScroll);

            _status = new Label { style = { paddingLeft = 8, paddingBottom = 6, whiteSpace = WhiteSpace.Normal } };
            root.Add(_status);

            RefreshInstanceList();
        }

        // ---- 인스턴스 ----

        private void AddInstance()
        {
            var e = new EntityData { id = "new_id", reactions = Array.Empty<Reaction>() };
            _instances.Add(e);
            _selected = e;
            RefreshInstanceList();
            RefreshDetail();
        }

        private void RefreshInstanceList()
        {
            _instanceList.Clear();
            foreach (EntityData inst in _instances)
            {
                EntityData captured = inst;
                var btn = new Button(() => { _selected = captured; RefreshDetail(); })
                {
                    text = string.IsNullOrEmpty(inst.id) ? "(no id)" : inst.id,
                    style = { unityTextAlign = TextAnchor.MiddleLeft }
                };
                if (ReferenceEquals(inst, _selected))
                    btn.style.backgroundColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
                _instanceList.Add(btn);
            }
        }

        private void RefreshDetail()
        {
            _detail.Clear();
            if (_selected == null)
            {
                _detail.Add(new Label("좌측에서 인스턴스를 선택하거나 + Instance 로 추가하세요.") { style = { opacity = 0.6f } });
                return;
            }

            var idField = new TextField("Id") { value = _selected.id };
            idField.RegisterValueChangedCallback(e => { _selected.id = e.newValue; RefreshInstanceList(); });
            _detail.Add(idField);

            var delBtn = new Button(() =>
            {
                _instances.Remove(_selected);
                _selected = _instances.Count > 0 ? _instances[0] : null;
                RefreshInstanceList();
                RefreshDetail();
            }) { text = "Delete Instance", style = { marginBottom = 8 } };
            _detail.Add(delBtn);

            _detail.Add(SectionHeader("Reactions"));
            var reactions = new List<Reaction>(_selected.reactions ?? Array.Empty<Reaction>());

            var reactionContainer = new VisualElement();
            _detail.Add(reactionContainer);

            void Rebuild()
            {
                _selected.reactions = reactions.ToArray();
                reactionContainer.Clear();
                foreach (Reaction r in reactions)
                    reactionContainer.Add(BuildReaction(r, reactions, Rebuild));
            }

            var addReaction = new Button(() =>
            {
                reactions.Add(new Reaction { trigger = _triggerOptions[0], conditions = Array.Empty<ConditionDef>(), effects = Array.Empty<EffectDef>() });
                Rebuild();
            }) { text = "+ Reaction" };
            _detail.Add(addReaction);

            Rebuild();
        }

        // ---- 리액션 UI ----

        private VisualElement BuildReaction(Reaction r, List<Reaction> owner, Action rebuildOwner)
        {
            var box = Card();

            var head = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 4 } };
            var trigger = new PopupField<string>("Trigger", _triggerOptions, IndexOf(_triggerOptions, r.trigger));
            trigger.style.flexGrow = 1;
            trigger.RegisterValueChangedCallback(e => r.trigger = e.newValue);
            head.Add(trigger);
            var rmReaction = new Button(() => { owner.Remove(r); rebuildOwner(); }) { text = "−", style = { width = 24, marginLeft = 4 } };
            head.Add(rmReaction);
            box.Add(head);

            // conditions
            box.Add(SubLabel("Conditions (AND)"));
            var condList = new List<ConditionDef>(r.conditions ?? Array.Empty<ConditionDef>());
            var condContainer = new VisualElement();
            box.Add(condContainer);
            void RebuildConds()
            {
                r.conditions = condList.ToArray();
                condContainer.Clear();
                foreach (ConditionDef c in condList)
                    condContainer.Add(TypeParamRow(_conditionOptions, c.type, s => c.type = s, c.@params, p => c.@params = p, () => { condList.Remove(c); RebuildConds(); }));
            }
            box.Add(new Button(() => { condList.Add(new ConditionDef { type = _conditionOptions[0] }); RebuildConds(); }) { text = "+ Condition" });
            RebuildConds();

            // effects
            box.Add(SubLabel("Effects"));
            var fxList = new List<EffectDef>(r.effects ?? Array.Empty<EffectDef>());
            var fxContainer = new VisualElement();
            box.Add(fxContainer);
            void RebuildFx()
            {
                r.effects = fxList.ToArray();
                fxContainer.Clear();
                foreach (EffectDef fx in fxList)
                    fxContainer.Add(TypeParamRow(_effectOptions, fx.type, s => fx.type = s, fx.@params, p => fx.@params = p, () => { fxList.Remove(fx); RebuildFx(); }));
            }
            box.Add(new Button(() => { fxList.Add(new EffectDef { type = _effectOptions[0] }); RebuildFx(); }) { text = "+ Effect" });
            RebuildFx();

            return box;
        }

        /// <summary>[type 드롭다운][params JSON][remove] 한 줄.</summary>
        private VisualElement TypeParamRow(List<string> options, string current, Action<string> onType,
            ParamBag currentParams, Action<ParamBag> onParams, Action onRemove)
        {
            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };

            var type = new PopupField<string>(options, IndexOf(options, current)) { style = { width = 150, marginRight = 4 } };
            type.RegisterValueChangedCallback(e => onType(e.newValue));
            onType(type.value);
            row.Add(type);

            var json = new TextField { value = ParamsToJson(currentParams), style = { flexGrow = 1, marginRight = 4 } };
            json.tooltip = "params (JSON object). 예: { \"power\": 1.8 }";
            json.RegisterValueChangedCallback(e =>
            {
                if (TryParseParams(e.newValue, out ParamBag bag))
                {
                    onParams(bag);
                    json.style.color = StyleKeyword.Null;
                }
                else
                {
                    json.style.color = new Color(0.9f, 0.4f, 0.4f);
                }
            });
            row.Add(json);

            var rm = new Button(onRemove) { text = "−", style = { width = 24 } };
            row.Add(rm);
            return row;
        }

        // ---- Export ----

        private void Export()
        {
            if (_instances.Count == 0)
            {
                SetStatus("내보낼 인스턴스가 없습니다.", true);
                return;
            }

            string path = EditorUtility.SaveFilePanel("Export JSON", Application.dataPath, "data", "json");
            if (string.IsNullOrEmpty(path))
                return;

            try
            {
                string json = JsonConvert.SerializeObject(_instances, Formatting.Indented);
                File.WriteAllText(path, json);
                AssetDatabase.Refresh();
                SetStatus($"내보내기 완료: {path}", false);
            }
            catch (Exception e)
            {
                SetStatus($"내보내기 실패: {e.Message}", true);
            }
        }

        // ---- helpers ----

        private static readonly List<string> CommonChannels = new List<string>
        {
            "on_use", "on_hit", "on_equip", "on_unequip", "on_hp_below", "on_enter_zone", "on_quest_complete"
        };

        private static List<string> Sorted(IEnumerable<string> src) => src.OrderBy(s => s).ToList();

        private static int IndexOf(List<string> options, string value)
        {
            int i = options.IndexOf(value);
            return i < 0 ? 0 : i;
        }

        private static string ParamsToJson(ParamBag bag)
        {
            if (bag == null)
                return "{}";
            try { return JsonConvert.SerializeObject(bag); }
            catch { return "{}"; }
        }

        private static bool TryParseParams(string text, out ParamBag bag)
        {
            try
            {
                bag = string.IsNullOrWhiteSpace(text)
                    ? new ParamBag()
                    : JsonConvert.DeserializeObject<ParamBag>(text);
                return bag != null;
            }
            catch
            {
                bag = null;
                return false;
            }
        }

        private static VisualElement Card()
        {
            return new VisualElement
            {
                style =
                {
                    borderLeftWidth = 1, borderRightWidth = 1, borderTopWidth = 1, borderBottomWidth = 1,
                    borderLeftColor = new Color(0, 0, 0, 0.2f), borderRightColor = new Color(0, 0, 0, 0.2f),
                    borderTopColor = new Color(0, 0, 0, 0.2f), borderBottomColor = new Color(0, 0, 0, 0.2f),
                    paddingLeft = 6, paddingRight = 6, paddingTop = 4, paddingBottom = 6,
                    marginBottom = 6
                }
            };
        }

        private static Label SectionHeader(string text) => new Label(text)
        {
            style = { unityFontStyleAndWeight = FontStyle.Bold, marginTop = 6, marginBottom = 4, fontSize = 13 }
        };

        private static Label SubLabel(string text) => new Label(text)
        {
            style = { opacity = 0.7f, marginTop = 4, marginBottom = 2 }
        };

        private void SetStatus(string message, bool error)
        {
            _status.text = message;
            _status.style.color = error ? new Color(0.9f, 0.4f, 0.4f) : new Color(0.4f, 0.8f, 0.4f);
        }
    }
}
