namespace AchReactive.EditorTools
{
    /// <summary>Type Designer 에서 선택 가능한 필드 타입.</summary>
    public enum DataFieldType
    {
        String,
        Int,
        Float,
        Bool,
        StringArray,
        IntArray,
        FloatArray
    }

    /// <summary>Type Designer 의 필드 한 줄 정의.</summary>
    [System.Serializable]
    public sealed class DataFieldDef
    {
        public string name = "newField";
        public DataFieldType type = DataFieldType.Float;
    }

    /// <summary>DataFieldType 과 C# 표현/파싱 사이의 매핑 헬퍼.</summary>
    public static class DataFieldTypeMap
    {
        /// <summary>C# 필드 타입 문자열.</summary>
        public static string CSharp(DataFieldType t)
        {
            switch (t)
            {
                case DataFieldType.String: return "string";
                case DataFieldType.Int: return "int";
                case DataFieldType.Float: return "float";
                case DataFieldType.Bool: return "bool";
                case DataFieldType.StringArray: return "string[]";
                case DataFieldType.IntArray: return "int[]";
                case DataFieldType.FloatArray: return "float[]";
                default: return "string";
            }
        }

        public static bool IsArray(DataFieldType t) =>
            t == DataFieldType.StringArray || t == DataFieldType.IntArray || t == DataFieldType.FloatArray;
    }
}
