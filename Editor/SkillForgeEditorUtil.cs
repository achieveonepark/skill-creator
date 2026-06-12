using UnityEditor;
using UnityEngine;

namespace SkillForge.EditorTools
{
    /// <summary>에디터 창들이 공유하는 IMGUI 보조 함수.</summary>
    internal static class SkillForgeEditorUtil
    {
        public const string DefaultSkillsPath = "Assets/GameData/Skills/skills.json";
        public const string DefaultBuffsPath = "Assets/GameData/Buffs/buffs.json";

        /// <summary>문자열 배열 중에서 선택하는 팝업. 현재 값이 목록에 없으면 끝에 추가해 보존한다.</summary>
        public static string PopupString(string label, string current, string[] options)
        {
            int index = System.Array.IndexOf(options, current);

            string[] display = options;
            if (index < 0)
            {
                display = new string[options.Length + 1];
                System.Array.Copy(options, display, options.Length);
                display[options.Length] = $"{current} (미지원)";
                index = options.Length;
            }

            int selected = EditorGUILayout.Popup(label, index, display);
            return selected < options.Length ? options[selected] : current;
        }

        public static void HeaderRow(string text)
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField(text, EditorStyles.boldLabel);
        }

        public static bool DangerButton(string text, float width = 60f)
        {
            Color prev = GUI.backgroundColor;
            GUI.backgroundColor = new Color(1f, 0.6f, 0.6f);
            bool clicked = GUILayout.Button(text, GUILayout.Width(width));
            GUI.backgroundColor = prev;
            return clicked;
        }
    }
}
