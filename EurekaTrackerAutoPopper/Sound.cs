﻿using Dalamud.Utility.Signatures;
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
        public static readonly int[] SoundEffects = { 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, 52 };
        private static readonly SoundImplementation _impl = new();

        public static unsafe void PlayEffect(int soundEffectId)
        {
            _impl.PlaySoundEffect((uint) soundEffectId, IntPtr.Zero, IntPtr.Zero, 0);
        }
    }
}
