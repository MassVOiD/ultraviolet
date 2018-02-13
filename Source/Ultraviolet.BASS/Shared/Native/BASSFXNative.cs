﻿using System;
using System.Runtime.InteropServices;
using System.Security;
using Ultraviolet.Core;
using Ultraviolet.Core.Native;

namespace Ultraviolet.BASS.Native
{
#pragma warning disable 1591
    [SuppressUnmanagedCodeSecurity]
    public static class BASSFXNative
    {
        // NOTE: The #ifdefs everywhere are necessary because I haven't yet found a way to make
        // the new dynamic loader work on mobile platforms, particularly Android, where dlopen()
        // sometimes maps the same library to multiple address spaces for reasons that I haven't
        // yet been able to discern. My hope is that if the proposed .NET Standard API for dynamic
        // library loading ever makes it to Xamarin Android/iOS, we can standardize all supported
        // platforms on a single declaration type. For now, though, this nonsense seems necessary.

#if ANDROID
        const String LIBRARY = "bass_fx";
#elif IOS
        const String LIBRARY = "__Internal";
#else
        private static readonly NativeLibrary lib = new NativeLibrary(
            UltravioletPlatformInfo.CurrentPlatform == UltravioletPlatform.Windows ? "bass_fx" : "libbass_fx");
#endif

        public const UInt32 BASS_FX_FREESOURCE = 0x10000;

#if ANDROID || IOS
        [DllImport(LIBRARY, EntryPoint="BASS_FX_TempoCreate", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 BASS_FX_TempoCreate(UInt32 chan, UInt32 flags);
#else
        [MonoNativeFunctionWrapper]
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate UInt32 BASS_FX_TempoCreateDelegate(UInt32 chan, UInt32 flags);
        private static readonly BASS_FX_TempoCreateDelegate pBASS_FX_TempoCreate = lib.LoadFunction<BASS_FX_TempoCreateDelegate>("BASS_FX_TempoCreate");
        public static UInt32 BASS_FX_TempoCreate(UInt32 chan, UInt32 flags) => pBASS_FX_TempoCreate(chan, flags);
#endif
    }
#pragma warning restore 1591
}
