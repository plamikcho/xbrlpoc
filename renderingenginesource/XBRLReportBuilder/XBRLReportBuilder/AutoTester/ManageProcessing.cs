using System;
using System.Collections.Generic;

using System.Text;
using System.Timers;
using System.IO;

namespace XBRLReportBuilder
{
    public class ManageProcessing
    {
        private Timer timer;
        private FileSystemWatcher watcher;
        public delegate void statusDone(object sender, EventArgs e);
        
        public ManageProcessing(string directoryPath, double timerDuration)
        {   
            watcher = new FileSystemWatcher(directoryPath);
            timer = new Timer(timerDuration);
        }

        public void StartTimer()
        {
            watcher.NotifyFilter = NotifyFilters.CreationTime;
            watcher.Filter = "*.zip";
            watcher.Changed += new FileSystemEventHandler(watcher_Changed);
            watcher.EnableRaisingEvents = true;

            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            timer.Enabled = true;
            timer.Start();
        }

        private void watcher_Changed(object sender, FileSystemEventArgs e)
        {
            timer.Stop();
            timer.Start();
        }

        private void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            done(this, e);
        }

        public event statusDone done;
        
        protected void status_Done(object sender, EventArgs e)
        {
            if (done != null)
            {
                done(this, e);
            }
        }
       
    }
}
