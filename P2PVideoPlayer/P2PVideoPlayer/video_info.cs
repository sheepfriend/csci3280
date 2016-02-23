using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace WpfApplication1
{
    class video_info
    {
        public String fileName {get; set; }
        public String title { get; set; }
        public String author { get; set; }
        public String album { get; set; }
        public String path { get; set; }

        //initialize a video object
        public video_info()
        {
            fileName = "N/A";
            title = "N/A";
            author = "N/A";
            album = "N/A";
            path = "N/A";
        }
                    
        
        //load a line of input
        //Filename:'1.wav' 'title' 'author' 'album'
        public void load(XmlNode video) {
            if (video["path"] != null)
            {
                path = video["path"].InnerText;
                char[] separatingChars1 = { '\\', '/' };
                string[] Names = path.Split(separatingChars1, System.StringSplitOptions.RemoveEmptyEntries);
                fileName = Names[Names.Length - 1];
            }
            if (video["title"] != null)
            {
                path = video["title"].InnerText;
            }
            if (video["author"] != null)
            {
                path = video["author"].InnerText;
            }
            if (video["album"] != null)
            {
                path = video["album"].InnerText;
            }
            //char[] separatingChars = {'\''};
            //char[] separatingChars1 = { '\\','/' };
            //string[] words = text.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
            //if (words.Length == 7)
            //{
            //    path = words[0];
            //    string[] name = words[0].Split(separatingChars1, System.StringSplitOptions.RemoveEmptyEntries);
            //    fileName = name[name.Length-1];
            //    title = words[2];
            //    author = words[4];
            //    album = words[6];
            //}
            //else {
            //    return;
            //}

        }

        public void readFromAddr(String text)
        {
            path = text;
            char[] separatingChars1 = { '\\', '/' };
            string[] name = text.Split(separatingChars1, System.StringSplitOptions.RemoveEmptyEntries);
            fileName = name[name.Length - 1];
        }

    }
}
