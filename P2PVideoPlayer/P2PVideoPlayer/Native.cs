using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WpfApplication1
{
    class Native
    {
        public const int MMSYSERR_NOERROR = 0;
        public const int CALLBACK_FUNCTION = 0x00030000;

        // callbacks
        public delegate void WaveDelegate(IntPtr hdrvr, Messages uMsg, int dwUser, ref WaveHdr wavhdr, int dwParam2);

        private const string mmdll = "winmm.dll";

        [DllImport(mmdll)]
        public static extern int waveOutGetNumDevs();
        [DllImport(mmdll)]
        public static extern int waveOutPrepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);
        [DllImport(mmdll)]
        public static extern int waveOutUnprepareHeader(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);
        [DllImport(mmdll)]
        public static extern int waveOutWrite(IntPtr hWaveOut, ref WaveHdr lpWaveOutHdr, int uSize);
        [DllImport(mmdll)]
        public static extern int waveOutOpen(out IntPtr hWaveOut, int uDeviceID, ref WavFormat lpFormat, WaveDelegate dwCallback, int dwInstance, int dwFlags);
        [DllImport(mmdll)]
        public static extern int waveOutReset(IntPtr hWaveOut);
        [DllImport(mmdll)]
        public static extern int waveOutClose(IntPtr hWaveOut);
        [DllImport(mmdll)]
        public static extern int waveOutSetVolume(IntPtr hWaveOut, int dwVolume);
        [DllImport(mmdll)]
        public static extern int waveOutGetVolume(IntPtr hWaveOut, out int dwVolume);
        [DllImport(mmdll)]
        public static extern int waveOutSetPlaybackRate(IntPtr hWaveOut, int dwRate);
        [DllImport(mmdll)]
        public static extern int waveOutSetPitch(IntPtr hWaveOut, int dwPitch);
    }
}
