using UnityEngine;

namespace SkillForge
{
    /// <summary>VFX 재생 통합 경계. play_vfx 효과가 사용한다.</summary>
    public interface IVfxPlayer
    {
        void Play(string vfxId, Vector3 position);
    }

    /// <summary>SFX 재생 통합 경계. play_sfx 효과가 사용한다.</summary>
    public interface ISfxPlayer
    {
        void Play(string sfxId, Vector3 position);
    }

    /// <summary>VFX/SFX 가 필요 없을 때 사용하는 무동작 구현.</summary>
    public sealed class NullPresentation : IVfxPlayer, ISfxPlayer
    {
        public static readonly NullPresentation Instance = new NullPresentation();

        void IVfxPlayer.Play(string vfxId, Vector3 position) { }

        void ISfxPlayer.Play(string sfxId, Vector3 position) { }
    }
}
