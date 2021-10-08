using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace TaskPool_SpeedCompare
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            App.delayList = delayList_Initial(App.max);
        }

        public List<int> delayList_Initial(int max)
        {
            List<int> l = new List<int>();
            for (int i = 0; i < max; i++)
            {
                var rand = new Random();
                l.Add(Convert.ToInt32(rand.NextDouble() * 500));
            }
            return l;
        }

        public Task D(int s)
        {
            Task.Delay(s);
            App.count++;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Method 1
        /// SemaphoreSlim
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<Task> tasks = new List<Task>();
            DateTime TimeNow = DateTime.Now;

            foreach (int r in App.delayList)
            {
                tasks.Add(D(r));
            }
            
            using (SemaphoreSlim throttler = new SemaphoreSlim(App.pool))
            {
                List<Task> postTaskTasks = new List<Task>();

                // Have each task notify the throttler when it completes so that it decrements the number of tasks currently running.
                tasks.ForEach(x => postTaskTasks.Add(x.ContinueWith(tsk => throttler.Release())));

                Task t = Task.WhenAll(postTaskTasks);
                tasks.Clear();
                try { t.Wait(); }
                catch { }
            }
            
            lb1.Content = ((TimeSpan)(DateTime.Now - TimeNow)).TotalMilliseconds.ToString();
            lb_count.Content = $"Task call: {App.count}";
        }

        /// <summary>
        /// Method 2
        /// If
        /// </summary>
        private void Button2_Click(object sender, RoutedEventArgs e)
        {
            List<Task> tasks = new List<Task>();

            DateTime TimeNow = DateTime.Now;

            int count = 0;
            foreach (int r in App.delayList)
            {
                tasks.Add(D(r));
                count++;

                if ((count % App.pool).Equals(0) || App.max == count)
                {
                    Task t = Task.WhenAll(tasks);
                    tasks.Clear();
                    try
                    {
                        t.Wait();
                    }
                    catch { }
                }
            }

            lb2.Content = ((TimeSpan)(DateTime.Now - TimeNow)).TotalMilliseconds.ToString();
            lb_count.Content = $"Task call: {App.count}";
        }

        /// <summary>
        /// Method 3
        /// Parallel.ForEach
        /// </summary>
        private void Button3_Click(object sender, RoutedEventArgs e)
        {
            List<int> ls = new List<int>();
            List<Task> tasks = new List<Task>();

            DateTime TimeNow = DateTime.Now;

            // Don't use this sometimes it won't process all
            Parallel.ForEach(App.delayList, new ParallelOptions { MaxDegreeOfParallelism = App.pool }, t => {
                D(t);
            });

            lb3.Content = ((TimeSpan)(DateTime.Now - TimeNow)).TotalMilliseconds.ToString();
            lb_count.Content = $"Task call: {App.count}";
        }

        /// <summary>
        /// Method 4
        /// SemaphoreSlim + Task.Factory
        /// </summary>
        private void Button4_Click(object sender, RoutedEventArgs e)
        {
            DateTime TimeNow = DateTime.Now;

            using (SemaphoreSlim throttler = new SemaphoreSlim(App.pool))
            {
                List<Task> tasks = new List<Task>();
                foreach (int r in App.delayList)
                {
                    throttler.Wait();

                    Task t = Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            D(r);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    });

                    tasks.Add(t);
                }

                Task.WhenAll(tasks.ToArray());
            }

            lb4.Content = ((TimeSpan)(DateTime.Now - TimeNow)).TotalMilliseconds.ToString();
            lb_count.Content = $"Task call: {App.count}";
        }
    }
}
