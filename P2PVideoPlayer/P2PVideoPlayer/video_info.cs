﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Microsoft.VisualBasic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;



namespace WpfApplication1
{
    [Serializable()]
    class video_info : ISerializable
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
        
        public video_info(SerializationInfo info, StreamingContext ctxt)
        {
            fileName = (String)info.GetValue("fileName", typeof(String));
            title = (String)info.GetValue("title", typeof(String));
            author = (String)info.GetValue("author", typeof(String));
            album = (String)info.GetValue("album", typeof(String));
            path = (String)info.GetValue("path", typeof(String));
            id = (String)info.GetValue("id", typeof(String));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("fileName", fileName);
            info.AddValue("title",title);
            info.AddValue("author",author);
            info.AddValue("album",album);
            info.AddValue("path",path);
            info.AddValue("id",id);
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
            title = Interaction.InputBox("What is the title of the music?",fileName+": Title","N/A");
            author = Interaction.InputBox("Who is the author of the music?", fileName + ": Author", "N/A");
            album = Interaction.InputBox("What is the album of the music?", fileName + ": Album", "N/A");
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
