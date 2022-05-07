using Dalamud.Utility.Signatures;
using System;

namespace EurekaTrackerAutoPopper
{
    internal unsafe class SoundImplementation
    {
        [Signature("E8 ?? ?? ?? ?? 4D 39 BE ?? ?? ?? ??")]
        public readonly delegate* unmanaged<uint, IntPtr, IntPtr, byte, void> PlaySoundEffect = null;

        internal SoundImplementation()
        {
            SignatureHelper.Initialise(this);
        }
    }

    public class Sound
    {
        private static readonly SoundImplementation _impl = new();

        public static void PlayEffect(uint soundEffectId)
        {
            unsafe
            {
                _impl.PlaySoundEffect(soundEffectId, IntPtr.Zero, IntPtr.Zero, 0);
            }
        }
    }
}
