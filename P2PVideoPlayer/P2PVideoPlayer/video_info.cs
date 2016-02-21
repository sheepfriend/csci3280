using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1
{
    class video_info
    {
        public String fileName;
        public String title;
        public String author;
        public String what;
        public String path;
        
        //initialize a video object
        public video_info() {
            fileName = "";
            title = "";
            author = "";
            what = "";
        }

        //load a line of input
        //'1.wav' 'title' 'author' 'what'
        public void load(String text) {
            char[] separatingChars = {'\''};
            char[] separatingChars1 = { '\\','/' };
            string[] words = text.Split(separatingChars, System.StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 7)
            {
                path = words[0];
                string[] name = words[0].Split(separatingChars1, System.StringSplitOptions.RemoveEmptyEntries);
                fileName = name[name.Length-1];
                title = words[2];
                author = words[4];
                what = words[6];
            }
            else {
                return;
            }
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
