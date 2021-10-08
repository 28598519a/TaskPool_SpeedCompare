using System.Collections.Generic;
using System.Windows;

namespace TaskPool_SpeedCompare
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        public static List<int> delayList = new List<int>();
        public static int max = 1000;
        public static int pool = 10;
        public static int count = 0;
    }
}
