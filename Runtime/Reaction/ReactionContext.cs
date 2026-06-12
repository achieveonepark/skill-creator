using System.Collections.Generic;

namespace GameData
{
    /// <summary>
    /// 한 번의 리액션 실행에 필요한 모든 주변 정보. 엔진이 effect/condition 마다
    /// <see cref="Params"/> 를 현재 항목의 인자로 바꿔가며 전달한다.
    /// </summary>
    public sealed class ReactionContext
    {
        /// <summary>리액션의 주체(시전자/소유자).</summary>
        public IEntity Source;

        /// <summary>단일 대상(없을 수 있음).</summary>
        public IEntity Target;

        /// <summary>광역 대상 목록(없을 수 있음).</summary>
        public IReadOnlyList<IEntity> Targets;

        /// <summary>스폰/조회 등 게임 월드 경계.</summary>
        public IWorld World;

        /// <summary>현재 실행 중인 effect/condition 의 인자. 엔진이 주입한다.</summary>
        public ParamBag Params = ParamBag.Empty;

        // 다른 도메인 데이터가 필요한 effect 는 수동등록 시 해당 DataBase<T> 를 클로저로 캡처한다.
    }
}
