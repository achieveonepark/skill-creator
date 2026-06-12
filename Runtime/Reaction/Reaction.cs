using System;

namespace GameData
{
    /// <summary>
    /// 하나의 반응 규칙: 특정 트리거가 발생했을 때, 조건을 모두 만족하면 효과들을 실행한다.
    /// 모든 도메인(스킬/몬스터/아이템/맵/퀘스트)이 공유하는 공통 표현.
    /// </summary>
    [Serializable]
    public class Reaction
    {
        /// <summary>발동 채널 이름. 예: "on_use", "on_hit", "on_equip", "on_hp_below".</summary>
        public string trigger;

        /// <summary>트리거 가드에 전달되는 인자(임계값 등). 가드가 없으면 무시된다.</summary>
        public ParamBag triggerParams;

        /// <summary>모두(AND) 만족해야 효과가 실행된다.</summary>
        public ConditionDef[] conditions;

        /// <summary>순서대로 실행되는 효과들.</summary>
        public EffectDef[] effects;
    }

    /// <summary>실행할 효과 하나. type 은 EffectRegistry 에 등록된 이름.</summary>
    [Serializable]
    public class EffectDef
    {
        public string type;
        public ParamBag @params;
    }

    /// <summary>평가할 조건 하나. type 은 ConditionRegistry 에 등록된 이름.</summary>
    [Serializable]
    public class ConditionDef
    {
        public string type;
        public ParamBag @params;
    }
}
