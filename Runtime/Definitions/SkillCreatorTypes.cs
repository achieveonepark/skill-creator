namespace SkillCreator
{
    /// <summary>타겟팅 타입 상수. MVP 4종.</summary>
    public static class TargetingType
    {
        public const string Self = "self";
        public const string SingleEnemy = "single_enemy";
        public const string Circle = "circle";
        public const string Cone = "cone";

        public static readonly string[] Supported =
        {
            Self, SingleEnemy, Circle, Cone
        };
    }

    /// <summary>조건 타입 상수.</summary>
    public static class ConditionType
    {
        public const string Always = "always";
        public const string Alive = "alive";
        public const string Chance = "chance";
        public const string HpBelow = "hp_below";
        public const string HpAbove = "hp_above";
        public const string HasBuff = "has_buff";
        public const string NotHasBuff = "not_has_buff";
        public const string TargetCountAbove = "target_count_above";
        public const string TargetCountBelow = "target_count_below";

        public static readonly string[] Supported =
        {
            Always, Alive, Chance, HpBelow, HpAbove,
            HasBuff, NotHasBuff, TargetCountAbove, TargetCountBelow
        };
    }

    /// <summary>효과 타입 상수.</summary>
    public static class EffectType
    {
        public const string Damage = "damage";
        public const string Heal = "heal";
        public const string AddBuff = "add_buff";
        public const string RemoveBuff = "remove_buff";
        public const string PlayVfx = "play_vfx";
        public const string PlaySfx = "play_sfx";

        public static readonly string[] Supported =
        {
            Damage, Heal, AddBuff, RemoveBuff, PlayVfx, PlaySfx
        };
    }

    /// <summary>버프 스택 정책.</summary>
    public static class StackPolicy
    {
        /// <summary>maxStack 까지 스택을 쌓는다.</summary>
        public const string Stack = "stack";

        /// <summary>이미 존재하면 새 적용을 무시한다.</summary>
        public const string Ignore = "ignore";

        /// <summary>이미 존재하면 기존 인스턴스를 대체한다(스택 1 유지).</summary>
        public const string Replace = "replace";

        public static readonly string[] Supported = { Stack, Ignore, Replace };
    }

    /// <summary>버프 갱신 정책.</summary>
    public static class RefreshPolicy
    {
        /// <summary>재적용 시 지속시간을 초기화한다.</summary>
        public const string RefreshDuration = "refresh_duration";

        /// <summary>재적용 시 남은 지속시간을 유지한다.</summary>
        public const string Keep = "keep";

        public static readonly string[] Supported = { RefreshDuration, Keep };
    }

    /// <summary>스탯 변경 연산 타입.</summary>
    public static class StatModifierType
    {
        public const string Flat = "flat";
        public const string PercentAdd = "percent_add";
        public const string PercentMul = "percent_mul";

        public static readonly string[] Supported = { Flat, PercentAdd, PercentMul };
    }
}
