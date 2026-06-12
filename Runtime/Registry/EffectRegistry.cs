using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameData
{
    /// <summary>effect 이름 → 실행 함수.</summary>
    public delegate void EffectFn(ReactionContext ctx);

    /// <summary>
    /// 모든 도메인이 공유하는 effect 동작 풀. 어트리뷰트 자동수집과 수동등록을 모두 지원한다.
    /// </summary>
    public static class EffectRegistry
    {
        private static readonly Dictionary<string, EffectFn> _map =
            new Dictionary<string, EffectFn>(StringComparer.Ordinal);

        /// <summary>등록된 effect 이름 목록(에디터 드롭다운 등에 사용).</summary>
        public static IEnumerable<string> Names => _map.Keys;

        /// <summary>모든 어셈블리의 [Effect] 정적 메서드를 수집해 등록한다.</summary>
        public static void AutoRegister()
        {
            foreach (var (attr, method) in ReflectionScan.Find<EffectAttribute>())
            {
                try
                {
                    var fn = (EffectFn)Delegate.CreateDelegate(typeof(EffectFn), method);
                    _map[attr.Name] = fn;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[GameData] '{attr.Name}' effect 등록 실패: {method.DeclaringType?.Name}.{method.Name} — " +
                                   $"시그니처는 'static void {method.Name}(ReactionContext)' 여야 합니다. {e.Message}");
                }
            }
        }

        /// <summary>수동 등록(DI/클로저 캡처가 필요할 때). 같은 이름은 덮어쓴다.</summary>
        public static void Register(string name, EffectFn fn)
        {
            if (string.IsNullOrEmpty(name) || fn == null)
                return;
            _map[name] = fn;
        }

        /// <summary>이름으로 effect 함수를 조회한다.</summary>
        public static bool TryGet(string name, out EffectFn fn)
        {
            if (name != null)
                return _map.TryGetValue(name, out fn);
            fn = null;
            return false;
        }

        /// <summary>등록 여부.</summary>
        public static bool Has(string name) => name != null && _map.ContainsKey(name);

        /// <summary>모든 등록을 비운다(테스트/재초기화).</summary>
        public static void Clear() => _map.Clear();
    }
}
