using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AchReactive.EditorTools
{
    /// <summary>
    /// UIToolkit 기반 타입 디자이너. 타입명/베이스/필드를 구성하고 Generate 를 누르면
    /// 도메인 데이터 클래스와 CSV 매퍼 C# 파일을 생성한다. JSON 스키마를 손으로 쓰지 않는다.
    /// </summary>
    public sealed class TypeDesignerWindow : EditorWindow
    {
        private readonly List<DataFieldDef> _fields = new List<DataFieldDef>();

        private TextField _typeName;
        private PopupField<string> _baseType;
        private TextField _outputFolder;
        private VisualElement _fieldList;
        private Label _status;

        private static readonly List<string> BaseTypes = new List<string> { "EntityData", "SkillData" };

        [MenuItem("Tools/AchReactive/Type Designer")]
        public static void Open()
        {
            var w = GetWindow<TypeDesignerWindow>();
            w.titleContent = new GUIContent("AchReactive - Type Designer");
            w.minSize = new Vector2(440, 420);
        }

        private void CreateGUI()
        {
            VisualElement root = rootVisualElement;
            root.style.paddingLeft = root.style.paddingRight = 10;
            root.style.paddingTop = root.style.paddingBottom = 10;

            root.Add(Header("Type Designer"));

            _typeName = new TextField("Type Name") { value = "Monster" };
            root.Add(_typeName);
            root.Add(new Label("→ 생성: <TypeName>Data, <TypeName>CsvMapper") { style = { opacity = 0.6f, marginBottom = 6 } });

            _baseType = new PopupField<string>("Base Type", BaseTypes, 0);
            root.Add(_baseType);

            _outputFolder = new TextField("Output Folder") { value = "Assets/Generated" };
            root.Add(_outputFolder);

            root.Add(Header("Fields"));
            _fieldList = new VisualElement();
            root.Add(_fieldList);

            var addBtn = new Button(() => AddField(new DataFieldDef())) { text = "+ Add Field" };
            root.Add(addBtn);

            var genBtn = new Button(Generate) { text = "Generate" };
            genBtn.style.marginTop = 12;
            genBtn.style.height = 28;
            root.Add(genBtn);

            _status = new Label { style = { marginTop = 8, whiteSpace = WhiteSpace.Normal } };
            root.Add(_status);

            // 기본 예시 필드 하나
            AddField(new DataFieldDef { name = "hp", type = DataFieldType.Float });
        }

        private static Label Header(string text)
        {
            return new Label(text)
            {
                style =
                {
                    unityFontStyleAndWeight = FontStyle.Bold,
                    marginTop = 8,
                    marginBottom = 4,
                    fontSize = 13
                }
            };
        }

        private void AddField(DataFieldDef def)
        {
            _fields.Add(def);

            var row = new VisualElement { style = { flexDirection = FlexDirection.Row, marginBottom = 2 } };

            var nameField = new TextField { value = def.name, style = { flexGrow = 1, marginRight = 4 } };
            nameField.RegisterValueChangedCallback(e => def.name = e.newValue);
            row.Add(nameField);

            var typeField = new EnumField(def.type) { style = { width = 110, marginRight = 4 } };
            typeField.RegisterValueChangedCallback(e => def.type = (DataFieldType)e.newValue);
            row.Add(typeField);

            var removeBtn = new Button { text = "−", style = { width = 24 } };
            removeBtn.clicked += () =>
            {
                _fields.Remove(def);
                _fieldList.Remove(row);
            };
            row.Add(removeBtn);

            _fieldList.Add(row);
        }

        private void Generate()
        {
            string typeName = (_typeName.value ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(typeName))
            {
                SetStatus("Type Name 을 입력하세요.", true);
                return;
            }

            string folder = (_outputFolder.value ?? "Assets/Generated").Trim();
            if (!folder.StartsWith("Assets"))
            {
                SetStatus("Output Folder 는 Assets 하위여야 합니다.", true);
                return;
            }

            string abs = Path.GetFullPath(folder);
            Directory.CreateDirectory(abs);

            string dataPath = Path.Combine(folder, typeName + "Data.cs");
            string mapperPath = Path.Combine(folder, typeName + "CsvMapper.cs");

            File.WriteAllText(Path.GetFullPath(dataPath),
                DataTypeCodeWriter.WriteData(typeName, _baseType.value, _fields));
            File.WriteAllText(Path.GetFullPath(mapperPath),
                DataTypeCodeWriter.WriteCsvMapper(typeName, _fields));

            AssetDatabase.Refresh();
            SetStatus($"생성 완료: {dataPath}, {mapperPath}", false);
        }

        private void SetStatus(string message, bool error)
        {
            _status.text = message;
            _status.style.color = error ? new Color(0.9f, 0.4f, 0.4f) : new Color(0.4f, 0.8f, 0.4f);
        }
    }
}
