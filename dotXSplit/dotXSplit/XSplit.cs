using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace dotXSplit
{

    public class XSplit
    {
        private const string XSplitProcessName = "XSplit.Core";
        private const string rePattern = @"^Streaming Live - (.*) Viewers:(\d+) .*Bitrate:([\d|,]+)Kbps .*Frames Encoded: (\d+) Dropped: (\d+)$";
        private Stats _stats;
        private Timer _queryTimer, _dropCheckTimer;
        private WindowsAPI _wapi;
        private UInt32 _dropsPrevious;
        private Process _xsplitprocess;

        #region Events

        public event EventHandler<EventArgs> OnFrameDrops;
        public event EventHandler<EventArgs> OnStatusRefresh;
        #endregion
        
        public XSplit()
        {
            _wapi = new WindowsAPI();
            _stats = new Stats() { bitrate = "0", drops = "0" };
            _dropsPrevious = 0;
            _xsplitprocess = null;
            ThreadPool.QueueUserWorkItem(arg => RefreshStatus());
            _dropCheckTimer = new Timer(new TimerCallback(dropCheckTimerTick), null, 0, 5000);
        }
        private void WaitXSplitProcess()
        {
            _xsplitprocess = Process.GetProcessesByName(XSplitProcessName).FirstOrDefault();
            if (_xsplitprocess == null)
            {
                Thread.Sleep(500);
                WaitXSplitProcess();
            }
        }
        private void dropCheckTimerTick(object o)
        {
            if (_dropsPrevious < FrameDrops)
            {
                if (_dropsPrevious == 0)
                {
                    _dropsPrevious = FrameDrops;
                    return;
                }
                OnFrameDrops(this,EventArgs.Empty);
                _dropsPrevious = FrameDrops;
            }
        }
        public string GetJson()
        {
            _stats.bitrate = Bitrate;
            _stats.drops = FrameDrops.ToString();
            return ParseJson<Stats>.WriteObject(_stats);
        }

        /// <summary>
        /// Interval of frame drops check. Default is 5000ms. Minimum value is 1000ms
        /// </summary>
        public UInt32 DropCheckInterval
        {
            set
            {
                if (value >= 1000)
                    _dropCheckTimer.Change(0, value);
            }
        }
        private void RefreshStatus()
        {
            while (true)
            {
                bool resetStats = false;
                while (_xsplitprocess == null)
                {
                    if (resetStats == false)
                    {
                        Bitrate = "0";
                        FrameDrops = 0;
                        OnStatusRefresh(this, EventArgs.Empty);
                        resetStats = true;
                    }
                    WaitXSplitProcess();
                }

                var reTester = new Regex(rePattern);

                ResetStatus();

                var stats =
                (
                    from win in _wapi.GetOpenWindowsFromPID(_xsplitprocess.Id)
                    let matches = reTester.Matches(win.Value)
                    where matches.Count > 0
                    select matches
                ).FirstOrDefault();

                if (stats != null)
                {
                    UInt32 outVal;

                    Stream = stats[0].Groups[1].Value;
                    if (UInt32.TryParse(stats[0].Groups[2].Value, out outVal))
                        Viewers = outVal;
                    Bitrate = stats[0].Groups[3].Value;
                    if (UInt32.TryParse(stats[0].Groups[4].Value, out outVal))
                        Encoded = outVal;
                    if (UInt32.TryParse(stats[0].Groups[5].Value, out outVal))
                        FrameDrops = outVal;

                    OnStatusRefresh(this, EventArgs.Empty);
                }
                else
                {
                    _xsplitprocess = null;
                }
                Thread.Sleep(1000);
            }
        }
        private void ResetStatus()
        {
            Stream = "-";
            Viewers = 0;
            Bitrate = "-";
            Encoded = 0;
            FrameDrops = 0;
        }
        public UInt32 Encoded
        {
            get;
            set;
        }
        public String Bitrate
        {
            get;
            set;
        }
        public UInt32 Viewers
        {
            get;
            set;
        }
        public String Stream
        {
            get;
            set;
        }
        public UInt32 FrameDrops
        {
            get; 
            set;
        }

    }
}
