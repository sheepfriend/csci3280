using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace WpfApplication1
{
    class Compress
    {
        public static byte[] compress(byte[] input)
        {
            using (MemoryStream inFile = new MemoryStream(input))
            {
                using (MemoryStream outFile = new MemoryStream())
                {
                    using (GZipStream Compress = new GZipStream(outFile,
                            CompressionMode.Compress))
                    {
                        // Copy the source file into the compression stream.
                        byte[] buffer = new byte[4096];
                        int numRead;
                        while ((numRead = inFile.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            Compress.Write(buffer, 0, numRead);
                        }
                        inFile.Close();
                        Compress.Close();
                        byte[] result = outFile.ToArray();
                        Console.Out.WriteLine("before:{0}\tafter:{1}", input.Length, result.Length);
                        return result;

                    }
                }
            }

        }
        public static byte[] decompress(byte[] buffer1)
        {
            // Get the stream of the source file. 

            MemoryStream inFile1 = new MemoryStream(buffer1, false);

            using (MemoryStream outFile = new MemoryStream())
            {
                using (GZipStream Decompress = new GZipStream(inFile1,
                        CompressionMode.Decompress))
                {
                    //Copy the decompression stream into the output file.
                    byte[] buffer = new byte[4096];
                    int numRead;
                    while ((numRead = Decompress.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        outFile.Write(buffer, 0, numRead);
                    }
                    buffer = new byte[outFile.Length];
                    outFile.Position = 0;
                    outFile.Read(buffer, 0, buffer.Length);
                    return outFile.ToArray();
                }
            }
        }

    }
}
