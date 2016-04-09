using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WpfApplication1
{
    public delegate void BufferFillEventHandler(IntPtr data, int size);
    class WaveOutPlayer
    {

         public WaveOutPlayer(ref Client client_)
        {
            reader = new WaveOutReader();
            client = client_;
            finished = false;
            finish = 0;
            isLocal = true;
            start = 0;
            zero = 0;
            countWaveOut = 0;
            countWaveOutP = 0;
            isPaused = 0;
        }

        public List<WaveOutStream> stream; 
        public WaveOutBuffer currentBuffer;
        public bool finished;
        public byte zero;
        public int countWaveOut;
        public int countWaveOutP;
        public WaveOutReader reader;
        public int finish;
        public Client client;
        public Thread load_audio;
        public Thread load_waveoutstream;
        public int isPaused;
        public String address;
        public bool isLocal;
        public static int start;
        public WavFormat format;

        public Native.WaveDelegate bufferProc = new Native.WaveDelegate(WaveOutBuffer.WaveOutProc);

        public static int DeviceCount
        {
            get { return Native.waveOutGetNumDevs(); }
        }

        public void setLocalInfo(String addr)
        {
            address = addr+".wav";
            if (Local.exist(@"audio\"+address)) { 
                isLocal = true;
                reader.loadFile(Local.ref_addr + @"audio\" + address);
            }
            else { isLocal = false; }
        }

        public void play()
        {
            if (isPaused == 0)
            {
                //重新开始
                countWaveOut = 0;
                countWaveOutP = 0;
                start = 0;
                stream = new List<WaveOutStream>();
                if (isLocal)
                {
                    //本地
                    load_waveoutstream = new Thread(loadWaveOutStream);
                    load_waveoutstream.Start();

                    load_audio = new Thread(loadAudio);

                    while ( stream.Count < 1) { }
                    start = 1;

                    load_audio.Start();
                }
                else
                {
                    //别人的
                    client.askAudio(address);
                    reader.loadFile(Local.ref_addr + @"audio\" + address);
                    isLocal = true;
                    play();
                    /*
                    reader.addRef(format);

                    load_waveoutstream = new Thread(loadWaveOutStreamP2P);
                    load_waveoutstream.Start();

                    load_audio = new Thread(loadAudio);

                    start = 0;
                    while (stream.Count < 20) { }
                    start = 1;

                    load_audio.Start();
                     */
                }
            }
            else
            {
                //暂停的
                isPaused = 0;
                load_audio = new Thread(loadAudio);
                load_audio.Start();

            }
        }

        public void pause()
        {
            if (isPaused == 1) { return; }
            isPaused = 1;
            load_audio.Abort();
        }

        public void stop()
        {
            load_audio.Abort();
            load_waveoutstream.Abort();
            reader.flush();
            countWaveOut = 0;
            countWaveOutP = 0;
            start = 0;
            stream.Clear();
        }

        public void loadAudio()
        {
            while (true)
            {
                while (stream.Count-1 < countWaveOut)
                {
                    if (finish == 1) { return; }
                }
                currentBuffer = stream[countWaveOut].read();
                Console.Out.WriteLine(stream[countWaveOut].position);
                if(currentBuffer != null){
                    //视频滞后等视频
                    //双方都是等待滞后的那个所以不用管超前的
                    //while(BitmapPlayer.countBitmap-1<countWaveOut ){}
                    currentBuffer.waveOut = reader.waveOut;
                    currentBuffer.Play();
                    Thread.Sleep(100);
                }
                else{
                    //播放完了
                    while(stream.Count==0){
                        if (finish == 1) { return; }
                    }
                    countWaveOut++;
                }
            }
        }

        public void loadWaveOutStream()
        {
            finish = 0;
            while (true)
            {
                WaveOutStream s = reader.loadWaveOutStream();
                if (s == null)
                {
                    finish = 1;
                    return;
                }
                stream.Add(s);
            }
        }
        /*
        public void loadWaveOutStreamP2P()
        {
            while (true)
            {
                WaveOutStream s = client.askWaveOutStream(countWaveOutP);
                if (s == null)
                {
                    finish = 1;
                    return;
                }
                stream.Add(s);
                countWaveOutP++;
            }
        }*/

    }
}
