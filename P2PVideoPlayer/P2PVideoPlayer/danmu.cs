using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace WpfApplication1
{
    public class OutlinedDanmaku : OutlinedTextBlock
    {
        public OutlinedDanmaku(string text)
        {
            FontSize = 36;
            Text = text;
            Fill = Brushes.White;
            Stroke = Brushes.Black;

            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
        }
    }

    public class ShadowDanmaku : TextBlock
    {
        //TODO: Optimizing the performance of danmaku animation with shadow.
        public ShadowDanmaku(string text)
        {
            FontSize = 36;
            Text = text;
            Foreground = Brushes.White;

            Effect = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 2,
                ShadowDepth = 1,
                Opacity = 1,
                RenderingBias = RenderingBias.Performance
            };

            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(0, 0, DesiredSize.Width, DesiredSize.Height));
        }
    }


    public class DanmakuManager
    {
        private Grid container;

        private int lineHeight = 48;
        private int paddingTop = 8;

        private Boolean[] isOccupy;
        private Boolean enableShadowEffect;
        private int lines;

        public int usableLine()
        {
            for (int line = 0; line < lines; line += 1)
            {
                if (!isOccupy[line])
                {
                    isOccupy[line] = true;
                    return line;
                }
            }
            return -1;
        }

        public void clearLine()
        {
            for (int line = 0; line < lines; line += 1)
            {
                isOccupy[line] = false;
            }
        }

        public int lineLocationY(int line)
        {
            return (line * lineHeight) + paddingTop;
        }

        public DanmakuManager(Grid grid, bool enableShadow)
        {
            container = grid;

            lines = (int)(container.RenderSize.Height / lineHeight) - 1;
            isOccupy = new Boolean[lines];

            enableShadowEffect = enableShadow;
        }

        public void Shoot(string text)
        {
            var line = usableLine();

            if (line == -1)
            {
                clearLine();
                line = usableLine();
            }

            FrameworkElement danmaku = new OutlinedDanmaku(text);
            // Danmaku initilization and display
            if (enableShadowEffect)
            {
                danmaku = new ShadowDanmaku(text);
            }

            danmaku.Margin = new Thickness(0, lineLocationY(line), 0, 0);
            container.Children.Add(danmaku);

            // Initilizing animation
            var anim = new DoubleAnimation();
            anim.From = this.container.RenderSize.Width;
            anim.To = -danmaku.DesiredSize.Width - 1600;
            anim.SpeedRatio = danmaku.DesiredSize.Width > 80 ?
                (.05 * (danmaku.DesiredSize.Width / 1500 + 1)) :
                (.1 * ((100 - danmaku.DesiredSize.Width) / 100 + 1));
            TranslateTransform trans = new TranslateTransform();
            danmaku.RenderTransform = trans;

            // Handling the end of danmaku
            anim.Completed += new EventHandler(delegate(Object o, EventArgs a)
            {
                container.Children.Remove(danmaku);
            });

            // Managing the danmaku lines
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += new EventHandler(delegate(Object o, EventArgs a)
            {
                Point relativePoint = danmaku.TransformToAncestor(container)
                          .Transform(new Point(0, 0));
                if (relativePoint.X < container.ActualWidth - danmaku.DesiredSize.Width - 50)
                {
                    timer.Stop();
                    isOccupy[line] = false;
                }
            });
            timer.Start();

            // Play animation
            trans.BeginAnimation(TranslateTransform.XProperty, anim);
        }
    }
         public static class WindowsServices {
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetWindowExTransparent(IntPtr hwnd) {
            var extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
        }
    }
         public partial class DanmakuCurtain : Window
         {
             public double screenHeight = SystemParameters.PrimaryScreenHeight;
             public double screenWidth = SystemParameters.PrimaryScreenWidth;

             private bool enableShadowEffect = false;

             private DanmakuManager dm = null;

             private void Window_Loaded(object sender, RoutedEventArgs e)
             {

             }

             public DanmakuCurtain()
             {

                 Top = 0;
                 Left = 0;
                 Width = screenWidth;
                 Height = screenHeight;

                 enableShadowEffect = true;
             }

             public void Shoot(Grid curtain, string text)
             {
                 if (dm == null)
                 {
                     dm = new DanmakuManager(curtain, enableShadowEffect);
                 }
                 dm.Shoot(text);
             }
         }
}