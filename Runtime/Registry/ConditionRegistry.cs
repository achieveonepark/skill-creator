using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    /// <summary>condition 이름 → 평가 함수. 현재 항목 인자는 ctx.Params 로 전달된다.</summary>
    public delegate bool ConditionFn(ReactionContext ctx);

    /// <summary>
    /// 모든 도메인이 공유하는 condition 평가 풀. 어트리뷰트 자동수집과 수동등록을 지원한다.
    /// </summary>
    public static class ConditionRegistry
    {
        private static readonly Dictionary<string, ConditionFn> _map =
            new Dictionary<string, ConditionFn>(StringComparer.Ordinal);

        /// <summary>등록된 condition 이름 목록(에디터 드롭다운 등에 사용).</summary>
        public static IEnumerable<string> Names => _map.Keys;

        /// <summary>모든 어셈블리의 [Condition] 정적 메서드를 수집해 등록한다.</summary>
        public static void AutoRegister()
        {
            foreach (var (attr, method) in ReflectionScan.Find<ConditionAttribute>())
            {
                try
                {
                    var fn = (ConditionFn)Delegate.CreateDelegate(typeof(ConditionFn), method);
                    _map[attr.Name] = fn;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[GameData] '{attr.Name}' condition 등록 실패: {method.DeclaringType?.Name}.{method.Name} — " +
                                   $"시그니처는 'static bool {method.Name}(ReactionContext)' 여야 합니다. {e.Message}");
                }
            }
        }

        /// <summary>수동 등록. 같은 이름은 덮어쓴다.</summary>
        public static void Register(string name, ConditionFn fn)
        {
            if (string.IsNullOrEmpty(name) || fn == null)
                return;
            _map[name] = fn;
        }

        /// <summary>
        /// 조건을 평가한다. 등록되지 않은 조건은 false 로 처리(미정의 조건이 통과하면 위험).
        /// </summary>
        public static bool Evaluate(string name, ReactionContext ctx)
        {
            if (name != null && _map.TryGetValue(name, out ConditionFn fn))
                return fn(ctx);

            Debug.LogWarning($"[GameData] 미등록 condition '{name}' — false 로 처리합니다.");
            return false;
        }

        /// <summary>등록 여부.</summary>
        public static bool Has(string name) => name != null && _map.ContainsKey(name);

        /// <summary>모든 등록을 비운다(테스트/재초기화).</summary>
        public static void Clear() => _map.Clear();
    }
}
