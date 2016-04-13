using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WpfApplication1
{
    class PMM
    {
        public int height;
        public int width;
        public byte[] data;
        public String method;

        public PMM()
        {
            height = 0;
            width = 0;
            data = new byte[0];
            method = "P";
        }

        public PMM(int w,int h,String m){
            width = w;
            height = h;
            method = m;
        }

        public static PMM loadFile(String file)
        {
            PMM a = new PMM();
            int position = 3;
            var reader = new BinaryReader(new FileStream(file, FileMode.Open));
            if (reader.ReadChar() != 'P')
                return null;
            String method = "P"+reader.ReadChar(); //Eat number
            reader.ReadChar(); //Eat newline
            string widths = "", heights = "";
            char temp = reader.ReadChar();
            while (temp!=' ' && temp!='\n')
            {
                widths += temp;
                position++;
                temp = reader.ReadChar();
            }
            position++;
            while ((temp = reader.ReadChar()) >= '0' && temp <= '9')
            {
                heights += temp;
                position++;
            }
            position++;
            if (reader.ReadChar() != '2' || reader.ReadChar() != '5' || reader.ReadChar() != '5')
                return null;
            reader.ReadChar(); //Eat the last newline
            position += 4;
            int width = int.Parse(widths),
                height = int.Parse(heights);

            reader.Close();

            //set PMM object
            a.width = width;
            a.height = height;
            a.method = method;

            byte[] data = File.ReadAllBytes(file);

            a.data = new byte[data.Length - position];

            Buffer.BlockCopy(data, position, a.data, 0, a.data.Length);

            return a;
        }

        public void writeToFile(String filename){
            String output = method + '\n' + width + '\n' + height + "\n255\n";
            byte[] byteArray = System.Text.Encoding.Default.GetBytes(output);
            using (var stream = new FileStream(filename, FileMode.Append))
            {
                stream.Write(byteArray, 0, byteArray.Length);
                stream.Write(data, 0, data.Length);
            }
        }

    }
}
