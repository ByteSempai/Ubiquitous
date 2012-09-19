using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Ubiquitous
{
    /// <summary>
    /// Handy wrapper for Background worker. Executes function at start of work and on the end
    /// </summary>
    public class BGWorker
    {
        private BackgroundWorker bw;
        public delegate void StartFunc();
        public delegate void CompleteFunc();

        private StartFunc _startFunc;
        private CompleteFunc _completeFunc;

        public BGWorker(StartFunc startFunc, CompleteFunc completeFunc)
        {
            _startFunc = startFunc;
            _completeFunc = completeFunc;

            bw = new BackgroundWorker();
            bw.DoWork += DoWork;
            bw.RunWorkerCompleted += Complete;
            bw.RunWorkerAsync();
        }
        public void Stop()
        {
            bw.DoWork -= DoWork;
            bw.RunWorkerCompleted -= Complete;
            if (bw.IsBusy)
                bw.CancelAsync();
            bw.Dispose();
        }
        private void DoWork(object sender, DoWorkEventArgs e)
        {
            if(_startFunc != null)
                _startFunc();
        }
        private void Complete(object sender, RunWorkerCompletedEventArgs e)
        {
            if(_completeFunc != null)
                _completeFunc();
        }
    }
}
