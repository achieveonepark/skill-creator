using UnityEngine;

namespace SkillForge
{
    /// <summary>
    /// 스킬 실행 결과를 기록하는 통합 경계. Combat Log / Preview Simulator 에서 활용한다.
    /// </summary>
    public interface ICombatLogger
    {
        void Log(string message);
    }

    /// <summary>UnityEngine.Debug 로 출력하는 기본 구현.</summary>
    public sealed class UnityCombatLogger : ICombatLogger
    {
        public static readonly UnityCombatLogger Instance = new UnityCombatLogger();

        public void Log(string message)
        {
            Debug.Log($"[SkillForge] {message}");
        }
    }

    /// <summary>아무 것도 하지 않는 구현.</summary>
    public sealed class NullCombatLogger : ICombatLogger
    {
        public static readonly NullCombatLogger Instance = new NullCombatLogger();

        public void Log(string message) { }
    }
}
