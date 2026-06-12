using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace SkillForge.EditorTools
{
    /// <summary>
    /// 정의 리스트를 JSON 파일로 저장/로드한다. 프로젝트 내 경로면 AssetDatabase 를 갱신한다.
    /// </summary>
    public static class JsonExporter
    {
        public static void SaveSkills(IList<SkillDefinition> skills, string path)
        {
            string json = SkillDataLoader.SkillsToJson(skills, true);
            WriteAndRefresh(path, json);
        }

        public static void SaveBuffs(IList<BuffDefinition> buffs, string path)
        {
            string json = SkillDataLoader.BuffsToJson(buffs, true);
            WriteAndRefresh(path, json);
        }

        public static List<SkillDefinition> LoadSkills(string path)
        {
            if (!File.Exists(path))
                return new List<SkillDefinition>();

            return SkillDataLoader.LoadSkillsFromJson(File.ReadAllText(path));
        }

        public static List<BuffDefinition> LoadBuffs(string path)
        {
            if (!File.Exists(path))
                return new List<BuffDefinition>();

            return SkillDataLoader.LoadBuffsFromJson(File.ReadAllText(path));
        }

        private static void WriteAndRefresh(string path, string json)
        {
            string dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(path, json);

            if (path.Replace('\\', '/').StartsWith("Assets/"))
                AssetDatabase.ImportAsset(path);

            Debug.Log($"[SkillForge] 저장 완료: {path}");
        }
    }
}
