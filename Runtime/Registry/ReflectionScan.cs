using System;
using System.Collections.Generic;
using System.Reflection;

namespace GameData
{
    /// <summary>
    /// 로드된 모든 어셈블리에서 특정 어트리뷰트가 붙은 정적 메서드를 찾는다.
    /// 레지스트리의 AutoRegister 가 사용한다. (에디터/Mono 환경 기준. IL2CPP 는 link.xml 필요.)
    /// </summary>
    public static class ReflectionScan
    {
        /// <summary>
        /// TAttr 가 붙은 public/non-public 정적 메서드를 (어트리뷰트, 메서드) 쌍으로 열거한다.
        /// 같은 메서드에 어트리뷰트가 여러 개면 각각 산출한다.
        /// </summary>
        public static IEnumerable<(TAttr attr, MethodInfo method)> Find<TAttr>() where TAttr : Attribute
        {
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (Skip(asm))
                    continue;

                Type[] types;
                try { types = asm.GetTypes(); }
                catch (ReflectionTypeLoadException ex) { types = ex.Types; }
                catch { continue; }

                if (types == null)
                    continue;

                foreach (Type type in types)
                {
                    if (type == null)
                        continue;

                    MethodInfo[] methods;
                    try { methods = type.GetMethods(flags); }
                    catch { continue; }

                    foreach (MethodInfo method in methods)
                    {
                        object[] attrs;
                        try { attrs = method.GetCustomAttributes(typeof(TAttr), false); }
                        catch { continue; }

                        foreach (object a in attrs)
                            yield return ((TAttr)a, method);
                    }
                }
            }
        }

        /// <summary>스캔에서 제외할 명백한 시스템/3rd-party 어셈블리.</summary>
        private static bool Skip(Assembly asm)
        {
            string name = asm.GetName().Name;
            if (string.IsNullOrEmpty(name))
                return true;

            return name.StartsWith("System", StringComparison.Ordinal)
                || name.StartsWith("Unity", StringComparison.Ordinal)
                || name.StartsWith("UnityEngine", StringComparison.Ordinal)
                || name.StartsWith("UnityEditor", StringComparison.Ordinal)
                || name.StartsWith("mscorlib", StringComparison.Ordinal)
                || name.StartsWith("netstandard", StringComparison.Ordinal)
                || name.StartsWith("Mono.", StringComparison.Ordinal)
                || name.StartsWith("Newtonsoft", StringComparison.Ordinal);
        }
    }
}
