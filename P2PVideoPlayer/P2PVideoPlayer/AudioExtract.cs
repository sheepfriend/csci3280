using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using NAudio.Wave;

namespace WpfApplication1
{
    class AudioExtract
    {
        public static String extract(String filepath)
        {

            String[] token = filepath.Split('\\');
            String filename = token[token.Length - 1];
            if (Local.exist("src/audio/" + filename + ".wav"))
            {
                return "src/audio/" + filename + ".wav";
            }
            long length = new System.IO.FileInfo(filepath).Length;
            if (length >= 536870912)
            {
                return large(filepath, filename);
            }
            else
            {
                return large(filepath, filename);
            }
        }



        public static String large(String src, String filename)
        {
            /*
             * two steps:
             * [1] use ffmpeg.exe to export the audio
             * [2] convert mp3 to wav
             */
            String ffmpegPath = "ffmpeg.exe";

            /*
             Here’s a short explanation on what every parameter does:
 
            -i “input file”
            -vn “skip the video part”
            -ac “audio channels”
            -ar “audio rate”
            -ab “audio bit-rate“
            -f “file format to use”
            (the end if the string) “output file”
             */

            String ffmpegArg = " -i \"" + src + "\" -vn -ar 44100 -ac 1 -ab 320k " + "-f mp3 ";
            Process psi = new Process();
            psi.StartInfo.FileName = ffmpegPath;
            psi.StartInfo.Arguments = ffmpegArg + " src/audio/" + filename + ".mp3";
            psi.StartInfo.CreateNoWindow = true;

            psi.Start();
            psi.WaitForExit();
            try
            {
                using (Mp3FileReader mp3 = new Mp3FileReader("src/audio/" + filename + ".mp3"))
                {
                    using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3))
                    {
                        WaveFileWriter.CreateWaveFile("src/audio/" + filename + ".wav", pcm);
                    }
                }
                return "src/audio/" + filename + ".wav";
            }


            catch
            {
                return "";
            }
        }
    }
    }

