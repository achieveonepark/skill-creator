using System.Collections.Generic;
using UnityEngine;

namespace SkillForge
{
    /// <summary>
    /// JSON 문자열을 정의 객체로 변환한다.
    /// UnityEngine.JsonUtility 기반이라 외부 의존성이 없다.
    /// 입력은 두 형태 모두 지원한다.
    ///   1) { "skills": [ ... ] }  (래핑된 객체)
    ///   2) [ ... ]                (최상위 배열 — 자동으로 래핑)
    /// </summary>
    public static class SkillDataLoader
    {
        public static List<SkillDefinition> LoadSkillsFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<SkillDefinition>();

            SkillDataFile file = JsonUtility.FromJson<SkillDataFile>(Wrap(json, "skills"));
            return file != null && file.skills != null ? file.skills : new List<SkillDefinition>();
        }

        public static List<BuffDefinition> LoadBuffsFromJson(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new List<BuffDefinition>();

            BuffDataFile file = JsonUtility.FromJson<BuffDataFile>(Wrap(json, "buffs"));
            return file != null && file.buffs != null ? file.buffs : new List<BuffDefinition>();
        }

        public static string SkillsToJson(IEnumerable<SkillDefinition> skills, bool pretty = true)
        {
            var file = new SkillDataFile { skills = new List<SkillDefinition>(skills) };
            return JsonUtility.ToJson(file, pretty);
        }

        public static string BuffsToJson(IEnumerable<BuffDefinition> buffs, bool pretty = true)
        {
            var file = new BuffDataFile { buffs = new List<BuffDefinition>(buffs) };
            return JsonUtility.ToJson(file, pretty);
        }

        /// <summary>최상위 배열이면 지정한 필드명으로 감싸 JsonUtility 가 읽을 수 있게 만든다.</summary>
        private static string Wrap(string json, string field)
        {
            string trimmed = json.TrimStart();
            if (trimmed.StartsWith("["))
                return "{\"" + field + "\":" + json + "}";

            return json;
        }
    }
}
