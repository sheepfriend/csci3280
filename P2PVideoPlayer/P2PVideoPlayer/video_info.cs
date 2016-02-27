using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using static Microsoft.VisualBasic.Interaction;

namespace WpfApplication1
{
    class video_info
    {
        public String fileName {get; set; }
        public String title { get; set; }
        public String author { get; set; }
        public String album { get; set; }
        public String path { get; set; }
        public String id { get; set; }
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
            id = video.Attributes["id"].Value;
            foreach (XmlNode entry in video.ChildNodes)
            {   
                string cont = entry.InnerText;
                switch (entry.Name)
                {
                    case "path":
                        path = cont;
                        char[] separatingChars1 = { '\\', '/' };
                        string[] Names = path.Split(separatingChars1, StringSplitOptions.RemoveEmptyEntries);
                        fileName = Names[Names.Length - 1];
                        break;
                    case "title":
                        title = cont;
                        break;
                    case "author":
                        author = cont;
                        break;
                    case "album":
                        album = cont;
                        break;
                    default:
                        break;
                }
            }
        }

        public void readFromAddr(String text)
        {
            XmlDocument doc = media_info.doc;

            char[] separatingChars1 = { '\\', '/' };
            string[] name = text.Split(separatingChars1, System.StringSplitOptions.RemoveEmptyEntries);
            fileName = name[name.Length - 1];
            //ask for title, author, album
            title = InputBox("What is the title of the music?",fileName+": Title","N/A");
            author = InputBox("Who is the author of the music?", fileName + ": Author", "N/A");
            album = InputBox("What is the album of the music?", fileName + ": Album", "N/A");
            path = text;
            XmlNodeList l = doc.SelectNodes("/Karaoke/video", media_info.man);
            XmlNode last = l[l.Count - 1];
            XmlElement newdata = doc.CreateElement("video");
            int newid;
            if (last != null)
            {
                int lastid = Int32.Parse(last.Attributes["id"].Value);
                newid = lastid + 1;
                newdata.SetAttribute("id", newid.ToString());
                doc.DocumentElement.InsertAfter(newdata, last);
            }
            else
            {
                newid = 1;
                newdata.SetAttribute("id", newid.ToString());
                doc.DocumentElement.SelectSingleNode("/Karaoke").AppendChild(newdata);
            }
            id = newid.ToString(); 
            //path
            XmlElement elem = doc.CreateElement("path");
            elem.InnerText = text;
            newdata.AppendChild(elem);
            //title
            if (title!="N/A")
            {
                elem = doc.CreateElement("title");
                elem.InnerText = title;
                newdata.AppendChild(elem);
            }
            if (author != "N/A")
            {
                elem = doc.CreateElement("author");
                elem.InnerText = author;
                newdata.AppendChild(elem);
            }
            if (album != "N/A")
            {
                elem = doc.CreateElement("album");
                elem.InnerText = album;
                newdata.AppendChild(elem);
            }
        }

        public String print()
        {
            StringBuilder result = new StringBuilder(60);
            result.Append("Path: ").Append(path)
                  .Append("File Name: ").Append(fileName).Append("\n")
                  .Append("Title: ").Append(title).Append("\n")
                  .Append("Author: ").Append(author).Append("\n")
                  .Append("Album: ").Append(album).Append("\n");
            
            return result.ToString(); 
        }
    }
}
