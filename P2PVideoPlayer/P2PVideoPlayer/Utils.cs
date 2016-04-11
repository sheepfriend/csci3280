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

        /// <summary>
        /// return the filename
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

                result = result||regex.IsMatch(item.album) || regex.IsMatch(item.author) || regex.IsMatch(item.title);
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
            List<String> plat_list = mediaInfo.print();
            for (int i = 0; i < plat_list.Count; i++)
            {
                mybox.Items.Add(plat_list[i]);
            }

        }

    }
}
