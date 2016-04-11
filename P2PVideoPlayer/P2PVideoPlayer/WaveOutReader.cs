using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class WaveOutReader
    {
        public int finish;
        public IntPtr waveOut;
        public int device = -1;
        public WavFormat format;
        private Native.WaveDelegate bufferProc = new Native.WaveDelegate(WaveOutBuffer.WaveOutProc);
        public WavStream audioStream;
        public int BUF_SIZE = 705*16;
        public int STREAM_COUNT = 10;
        public int count_stream = 0;
        public WaveOutReader() {}

        public void flush()
        {
            audioStream = null;
            count_stream = 0;
            finish = 0;
        }

        public void loadFile(String filename){
            WavStream S = new WavStream(filename);
            if (S.Length <= 0)
                throw new Exception("Invalid WAV file");
            format = S.Format;
            BUF_SIZE = (int)(format.nAvgBytesPerSec/10.2);
            audioStream = S;
            Native.waveOutOpen(out waveOut, device, ref format, null, 0, Native.CALLBACK_FUNCTION);
            count_stream = 0;
            finish = 0;
        }

        public void addRef(WavFormat format_)
        {
            format = format_;
            BUF_SIZE = format.nAvgBytesPerSec/8;
            int a=Native.waveOutOpen(out waveOut, device, ref format, null, 0, Native.CALLBACK_FUNCTION);
        }

        public WaveOutBuffer loadWaveOutBuffer()
        {
            WaveOutBuffer buf = new WaveOutBuffer(waveOut, BUF_SIZE);
            int pos = 0;
            byte[] b = new byte[BUF_SIZE];
            while (pos < BUF_SIZE)
            {
                int toget = BUF_SIZE - pos;
                int got = audioStream.Read(b, pos, toget);
                pos += got;
            }
            System.Runtime.InteropServices.Marshal.Copy(b, 0, buf.Data, BUF_SIZE);
            return buf;
        }

        //播放完了的情况没有解决

        public WaveOutStream loadWaveOutStream()
        {
            WaveOutStream stream = new WaveOutStream();
            for (int i = 0; i < STREAM_COUNT; i++)
            {
                WaveOutBuffer a = loadWaveOutBuffer();
                stream.addBuffer(a);
            }
            count_stream++;
            return stream;
        }

        public WaveOutStream loadWaveOutStream(int count)
        {
            WaveOutStream a;
            while (count_stream <= count)
            {
                 a = loadWaveOutStream();
                 if (count_stream == count + 1)
                 {
                     return a;
                 }
            }
            return null;
        }

    }
}
