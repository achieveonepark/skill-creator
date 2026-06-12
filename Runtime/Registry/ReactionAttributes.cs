using System;

namespace GameData
{
    /// <summary>
    /// 정적 메서드를 effect 로 등록한다. 시그니처는 <c>static void M(ReactionContext)</c>.
    /// EffectRegistry.AutoRegister() 가 모든 어셈블리에서 수집한다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class EffectAttribute : Attribute
    {
        public string Name { get; }
        public EffectAttribute(string name) => Name = name;
    }

    /// <summary>
    /// 정적 메서드를 condition 으로 등록한다. 시그니처는 <c>static bool M(ReactionContext)</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class ConditionAttribute : Attribute
    {
        public string Name { get; }
        public ConditionAttribute(string name) => Name = name;
    }

    /// <summary>
    /// 정적 메서드를 트리거 가드로 등록한다. 시그니처는
    /// <c>static bool M(ReactionContext ctx, ParamBag triggerParams)</c>.
    /// 가드가 없는 트리거는 채널 이름만 일치하면 통과한다.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class TriggerAttribute : Attribute
    {
        public string Name { get; }
        public TriggerAttribute(string name) => Name = name;
    }
}
