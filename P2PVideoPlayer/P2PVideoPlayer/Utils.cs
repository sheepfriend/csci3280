using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace WpfApplication1
{
    static class Utils
    {
        public static List<video_info> search_list(string key, media_info list)
        {
            List<video_info> result = new List<video_info>();

            //while result 
            if (list == null || list.name_to_list==null)
            {
                return result;
            }
            foreach (video_info tmp in list.name_to_list.Values)
            {
                if (search(key, tmp))
                {
                    result.Add(tmp);
                }
            }
            return result;
        }

        /// <summary>
        /// return the truth value
        /// search music title and singer name and album
        /// </summary>
        /// <param name="key">the string user type in search window</param>
        /// <param name="item">a video_info. You may use iteration to traverse all video_infos</param>
        /// <returns></returns>
        public static bool search(string key, video_info item)
        {
            //split keys into multiple key, since we support multikeyword search
            string[] keys = key.Split(null);
            bool result = false;

            //while result 
            foreach (string mykey in keys)
            {
                Regex regex = new Regex(mykey, RegexOptions.IgnoreCase);

                result = result || regex.IsMatch(item.album) || regex.IsMatch(item.author) || regex.IsMatch(item.title) || regex.IsMatch(item.fileName);
                //return true when result becomes true
                if (result)
                    return result;
            }
            return result;
        }

        /// <summary>
        /// add result to list
        /// </summary>
        /// <param name="item">single item. May iterate through the whole result set</param>
        /// <param name="mediaInfo">mediaInfo object</param>
        /// <param name="mybox">the ListBox component. In our program, pass in "selector in MainWindow class"</param>
        static void update_search_result(video_info item, media_info mediaInfo, ListBox mybox)
        {
            mediaInfo.name_to_list.Add(item.fileName, item);
            mybox.Items.Clear();
            HashSet<String> play_list = mediaInfo.print();
            general_add(play_list, mybox);

        }

        public static void general_add(HashSet<String> play_list, ListBox selector)
        {
            selector.Items.Clear();
            foreach (String fn in play_list)
            {
                selector.Items.Add(fn);
            }
        }

    }
}