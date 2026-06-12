namespace GameData
{
    /// <summary>
    /// 리액션 실행 엔진. 특정 트리거에 대해 엔티티의 reactions 를 순회하며
    /// 트리거 가드 → 조건(AND) → 효과 실행 순으로 처리한다.
    /// 도메인(스킬/몬스터/아이템 등)에 무관하게 동일하게 동작한다.
    /// </summary>
    public sealed class ReactionEngine
    {
        /// <summary>
        /// data 의 reactions 중 trigger 채널과 일치하는 것을 실행한다.
        /// </summary>
        /// <returns>실제로 효과까지 실행된 리액션 수.</returns>
        public int Run(string trigger, EntityData data, ReactionContext ctx)
        {
            if (data?.reactions == null || ctx == null || trigger == null)
                return 0;

            int fired = 0;

            foreach (Reaction r in data.reactions)
            {
                if (r == null || r.trigger != trigger)
                    continue;

                if (!TriggerRegistry.Match(r.trigger, ctx, r.triggerParams))
                    continue;

                if (!ConditionsPass(r.conditions, ctx))
                    continue;

                RunEffects(r.effects, ctx);
                fired++;
            }

            return fired;
        }

        private static bool ConditionsPass(ConditionDef[] conditions, ReactionContext ctx)
        {
            if (conditions == null)
                return true;

            foreach (ConditionDef c in conditions)
            {
                if (c == null || string.IsNullOrEmpty(c.type))
                    continue;

                ctx.Params = c.@params ?? ParamBag.Empty;
                if (!ConditionRegistry.Evaluate(c.type, ctx))
                    return false;
            }

            return true;
        }

        private static void RunEffects(EffectDef[] effects, ReactionContext ctx)
        {
            if (effects == null)
                return;

            foreach (EffectDef e in effects)
            {
                if (e == null || string.IsNullOrEmpty(e.type))
                    continue;

                ctx.Params = e.@params ?? ParamBag.Empty;
                if (EffectRegistry.TryGet(e.type, out EffectFn fn))
                    fn(ctx);
            }
        }
    }
}
