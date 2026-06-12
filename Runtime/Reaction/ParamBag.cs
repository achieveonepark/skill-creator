using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GameData
{
    /// <summary>
    /// effect / condition / trigger 가 받는 임의 키-값 인자 묶음.
    /// JSON 의 임의 객체(<c>{ "power": 1.8, "buffId": "burn" }</c>)를 그대로 담고,
    /// 꺼낼 때 원하는 타입으로 변환한다. Newtonsoft 의 JToken 을 백킹으로 사용한다.
    /// </summary>
    [JsonConverter(typeof(ParamBagConverter))]
    public sealed class ParamBag
    {
        /// <summary>키가 하나도 없는 공유 빈 인스턴스(읽기 전용 용도).</summary>
        public static readonly ParamBag Empty = new ParamBag();

        private readonly Dictionary<string, JToken> _values;

        public ParamBag()
        {
            _values = new Dictionary<string, JToken>(StringComparer.Ordinal);
        }

        public ParamBag(Dictionary<string, JToken> values)
        {
            _values = values ?? new Dictionary<string, JToken>(StringComparer.Ordinal);
        }

        /// <summary>해당 키가 존재하는지.</summary>
        public bool Has(string key) => _values.ContainsKey(key);

        /// <summary>키 값을 T 로 변환해 반환한다. 없거나 변환 실패 시 fallback.</summary>
        public T Get<T>(string key, T fallback = default)
        {
            if (_values.TryGetValue(key, out JToken token) && token != null && token.Type != JTokenType.Null)
            {
                try { return token.ToObject<T>(); }
                catch { return fallback; }
            }
            return fallback;
        }

        /// <summary>변환을 시도한다.</summary>
        public bool TryGet<T>(string key, out T value)
        {
            if (_values.TryGetValue(key, out JToken token) && token != null && token.Type != JTokenType.Null)
            {
                try { value = token.ToObject<T>(); return true; }
                catch { value = default; return false; }
            }
            value = default;
            return false;
        }

        /// <summary>값을 설정한다(코드/에디터에서 구성할 때).</summary>
        public void Set<T>(string key, T value) => _values[key] = value == null ? JValue.CreateNull() : JToken.FromObject(value);

        /// <summary>내부 토큰 맵(읽기 전용). 직렬화 변환기가 사용한다.</summary>
        internal IReadOnlyDictionary<string, JToken> Values => _values;
    }

    /// <summary>ParamBag 을 JSON 객체와 1:1 로 직렬화/역직렬화한다.</summary>
    internal sealed class ParamBagConverter : JsonConverter<ParamBag>
    {
        public override ParamBag ReadJson(JsonReader reader, Type objectType, ParamBag existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return new ParamBag();

            JObject obj = JObject.Load(reader);
            var dict = new Dictionary<string, JToken>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, JToken> pair in obj)
                dict[pair.Key] = pair.Value;
            return new ParamBag(dict);
        }

        public override void WriteJson(JsonWriter writer, ParamBag value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            foreach (KeyValuePair<string, JToken> pair in value.Values)
            {
                writer.WritePropertyName(pair.Key);
                pair.Value.WriteTo(writer);
            }
            writer.WriteEndObject();
        }
    }
}
