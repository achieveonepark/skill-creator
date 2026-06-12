using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    /// <summary>트리거 가드: 채널 이름이 일치한 뒤, 추가로 발동 여부를 판정한다.</summary>
    public delegate bool TriggerFn(ReactionContext ctx, ParamBag triggerParams);

    /// <summary>
    /// 트리거 가드 풀. 대부분의 트리거(on_use, on_hit 등)는 채널 이름만으로 충분해 가드가 없다.
    /// 임계값 비교가 필요한 트리거(on_hp_below 등)만 가드를 등록한다.
    /// </summary>
    public static class TriggerRegistry
    {
        private static readonly Dictionary<string, TriggerFn> _map =
            new Dictionary<string, TriggerFn>(StringComparer.Ordinal);

        /// <summary>가드가 등록된 트리거 이름 목록.</summary>
        public static IEnumerable<string> Names => _map.Keys;

        /// <summary>모든 어셈블리의 [Trigger] 정적 메서드를 수집해 등록한다.</summary>
        public static void AutoRegister()
        {
            foreach (var (attr, method) in ReflectionScan.Find<TriggerAttribute>())
            {
                try
                {
                    var fn = (TriggerFn)Delegate.CreateDelegate(typeof(TriggerFn), method);
                    _map[attr.Name] = fn;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[GameData] '{attr.Name}' trigger 등록 실패: {method.DeclaringType?.Name}.{method.Name} — " +
                                   $"시그니처는 'static bool {method.Name}(ReactionContext, ParamBag)' 여야 합니다. {e.Message}");
                }
            }
        }

        /// <summary>수동 등록. 같은 이름은 덮어쓴다.</summary>
        public static void Register(string name, TriggerFn fn)
        {
            if (string.IsNullOrEmpty(name) || fn == null)
                return;
            _map[name] = fn;
        }

        /// <summary>
        /// 트리거 가드를 평가한다. 가드가 없는 트리거는 항상 통과(true).
        /// </summary>
        public static bool Match(string name, ReactionContext ctx, ParamBag triggerParams)
        {
            if (name != null && _map.TryGetValue(name, out TriggerFn fn))
                return fn(ctx, triggerParams ?? ParamBag.Empty);
            return true;
        }

        /// <summary>가드 등록 여부.</summary>
        public static bool Has(string name) => name != null && _map.ContainsKey(name);

        /// <summary>모든 등록을 비운다(테스트/재초기화).</summary>
        public static void Clear() => _map.Clear();
    }
}
