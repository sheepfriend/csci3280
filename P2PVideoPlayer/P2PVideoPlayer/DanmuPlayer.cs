using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace WpfApplication1
{
    class DanmuPlayer
    {
        public DanmuList danmuList;
        public Client client;
        public Thread play;
        public static int loadedDanmu;
        public DanmakuCurtain dmkCurt;
        public Grid curtain;

        public DanmuPlayer(ref DanmakuCurtain a, ref Client client_,ref Grid curtain_)
        {
            dmkCurt = a;
            curtain = curtain_;
            client = client_;
            danmuList = null;
            loadedDanmu = 0;
        }

        public void loadDanmu() {
            //address就是文件名。。。
            //这里client会有一个延迟
            danmuList = client.askDanmuList(BitmapPlayer.address);
            loadedDanmu = 1;
        }

        public void playDanmu()
        {
            if (BitmapPlayer.isLocal) { return; }
            loadDanmu();
            play = new Thread(playing);
            //等待bitmapplayer载入完成
            while (BitmapPlayer.start == 0) { Thread.Sleep(200); }
            play.Start();
        }

        //这个线程在视频被停止之前就不听了，用BitmapPlayer的countFrame掐时间就好。。。
        public void playing()
        {
            loadedDanmu = 0;
            int current_count = 0;
            while (true)
            {
                if (BitmapPlayer.finish == 1)
                {
                    return;
                }
                while (current_count >= BitmapPlayer.countFrame) {}
 
                while (danmuList.danmu.Count < current_count+1)
                {
                    danmuList.danmu.Add(new List<string>());
                }

                if (danmuList.danmu[current_count].Count > 0)
                {
                    foreach(String a in danmuList.danmu[current_count])
                    {
                        System.Windows.Application.Current.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background, new Action(() =>
                        {
                            
                            dmkCurt.Shoot(curtain, a);
                        }));
                    }
                }

                current_count++;
            }
        }

        public void addDanmu(String content)
        {
            if (BitmapPlayer.isLocal) { return; }
            client.addDamnu(BitmapPlayer.countFrame, content);
        }

        public void stop()
        {
            danmuList = null;
            play.Abort();
        }
    }
}
