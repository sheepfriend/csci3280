using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace WpfApplication1
{

    [Serializable()]
    class DanmuList: ISerializable
    {
        public static String path = Local.ref_addr+"danmu\\";
        public List<List<String>> danmu;
        public String name;
        public DanmuList()
        {
            danmu = new List<List<string>>();
            name = "";
        }

        public DanmuList(SerializationInfo info, StreamingContext ctxt)
        {
            danmu = (List<List<String>>)info.GetValue("stream", typeof(List<List<String>>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("stream",danmu);
        }

        public List<String> getDamnu(int num)
        {
            if (danmu.Count < num) { return new List<string>(); }
            else
            {
                return danmu[num];
            }
        }

        public void addDanmu(int num, String content)
        {
            if (danmu.Count < num)
            {
                while (danmu.Count < num)
                {
                    danmu.Add(new List<String>());
                }
            }
            danmu[num].Add(content.Replace('\n','\0').Replace('\r','\0').Replace('\t','\0'));
        }

        public void writeToFile()
        {
            if (name == "") { return; }
            writeToFile(name);
        }

        public void writeToFile(String filename)
        {
                using (StreamWriter sw = File.CreateText(path+filename))
                {
                    for (int i = 0; i < danmu.Count; i++)
                    {
                        for (int j = 0; j < danmu[i].Count; j++)
                        {
                            sw.WriteLine("{0}\t{1}",i,danmu[i][j]);
                        }
                        sw.WriteLine();
                    }
                }
        }

        public void appendDanmu(int num, String content)
        {
            if (name == "") { return; }
            appendDanmu(num, content, name);
        }

        public static void appendDanmu(int num, String content,String filename)
        {
            String[] tmp = filename.Split('\\');
            if (!File.Exists(path + tmp[tmp.Length-1]))
            {
                using (StreamWriter sw = File.CreateText(path + tmp[tmp.Length - 1]))
                {
                    sw.WriteLine("{0}\t{1}", num, content);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(path + tmp[tmp.Length - 1]))
                {
                    sw.WriteLine("{0}\t{1}", num, content);
                }
            }
        }

        public void readFromFile(String filename)
        {
            name = filename;
            danmu = new List<List<string>>();
            if (!File.Exists(path + filename)) { return; }
            using (StreamReader sr = File.OpenText(path+filename))
            {
                String s = "";
                while ((s = sr.ReadLine()) != null)
                {
                    String[] content = s.Split('\t');
                    if (content.Length == 1) { continue; }
                    int num = Int32.Parse(content[0]);
                    while (danmu.Count < num+1)
                    {
                        danmu.Add(new List<string>());
                    }
                    danmu[num].Add(content[1]);
                }
            }
        }
    }
}
