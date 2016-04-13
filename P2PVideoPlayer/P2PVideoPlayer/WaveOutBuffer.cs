using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WpfApplication1
{
    [Serializable()]
    class WaveOutBuffer : ISerializable
    {
        private AutoResetEvent playEvent = new AutoResetEvent(false);
        public IntPtr waveOut;

        private WaveHdr header;
        private byte[] headerData;
        private GCHandle headerHandle;
        private GCHandle headerDataHandle;

        private bool playing;

        public WaveOutBuffer(SerializationInfo info, StreamingContext ctxt)
        {
            
            headerData = (byte[])info.GetValue("headerData", typeof(byte[]));

            playEvent = new AutoResetEvent(false);

            header.dwBufferLength = headerData.Length;

            headerHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
            headerDataHandle = GCHandle.Alloc(headerData, GCHandleType.Pinned);

            header.lpData = headerDataHandle.AddrOfPinnedObject();
        }

        
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("headerData", headerData);
        }


        public WaveOutBuffer(IntPtr waveOutHandle, int size)
        {
            waveOut = waveOutHandle;
            
            headerHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
            headerData = new byte[size];
            headerDataHandle = GCHandle.Alloc(headerData, GCHandleType.Pinned);

            header.dwUser = (IntPtr)GCHandle.Alloc(this);
            header.lpData = headerDataHandle.AddrOfPinnedObject();

            header.dwBufferLength = size;
            Native.waveOutPrepareHeader(waveOut, ref header, Marshal.SizeOf(header));
        }

        public static void WaveOutProc(IntPtr hdrvr, Messages uMsg, int dwUser, ref WaveHdr wavhdr, int dwParam2)
        {
            if (uMsg == Messages.MM_WOM_DONE)
            {
                try
                {
                    GCHandle h = (GCHandle)wavhdr.dwUser;
                    WaveOutBuffer buf = (WaveOutBuffer)h.Target;
                    buf.OnCompleted();
                }
                catch
                {
                }
            }
        }

        ~WaveOutBuffer()
        {
            Dispose();
        }
        public void Dispose()
        {
            if (header.lpData != IntPtr.Zero)
            {
                Native.waveOutUnprepareHeader(waveOut, ref header, Marshal.SizeOf(header));
                headerHandle.Free();
                header.lpData = IntPtr.Zero;
            }
            playEvent.Close();
            if (headerDataHandle.IsAllocated)
                headerDataHandle.Free();
            GC.SuppressFinalize(this);
        }

        public int Size
        {
            get { return header.dwBufferLength; }
        }

        public IntPtr Data
        {
            get { return header.lpData; }
        }

        public bool Play()
        {
            lock (this)
            {
                playEvent.Reset(); 

                headerHandle = GCHandle.Alloc(header, GCHandleType.Pinned);
                headerDataHandle = GCHandle.Alloc(headerData, GCHandleType.Pinned);
                header.dwUser = (IntPtr)GCHandle.Alloc(this);
                header.lpData = headerDataHandle.AddrOfPinnedObject();
                int b = Native.waveOutPrepareHeader(waveOut, ref header, Marshal.SizeOf(header));
                int a = Native.waveOutWrite(waveOut, ref header, Marshal.SizeOf(header));
                playing = a == Native.MMSYSERR_NOERROR;
                //Thread.Sleep(500);
                return playing;
            }
        }
        public void WaitFor()
        {
            if (playing)
            {
                playing = playEvent.WaitOne();
            }
            else
            {
                Thread.Sleep(0);
            }
        }
        public void OnCompleted()
        {
            playEvent.Set();
            playing = false;
        }
    }

}
