using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SC2TV.RTFControl;
using System.Drawing;

namespace Ubiquitous
{
    public enum ChatIcon
    {
        Default,
        Sc2Tv,
        TwitchTv,
        Skype,
        Console,
        Steam,
        Admin,
        Goodgame
    }
    /// <summary>
    /// Thread safe writing to textbox
    /// </summary>
    class Log
    {
        private ExRichTextBox tb;
        delegate void SetTextCallback(string text, ChatIcon icon);


        /// <summary>
        /// Provied Textbox object to the constructor
        /// </summary>
        /// <param name="logTb"></param>
        /// 
        public Log(ExRichTextBox logTb)
        {
            tb = logTb;
        }
        /// <summary>
        /// Writes a line to the textbox. Automaticall adds newline character
        /// </summary>
        /// <param name="text"></param>
        public void WriteLine(string text, ChatIcon icon = ChatIcon.Default)
        {
            if (tb.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(WriteLine);
                try
                {
                    tb.Parent.Invoke(d, new object[] { text, icon });
                }
                catch { }
            }
            else
            {
                Bitmap chatIcon = null;
                switch (icon)
                {
                    case ChatIcon.Sc2Tv:
                        chatIcon = Properties.Resources.sc2icon;
                        break;
                    case ChatIcon.TwitchTv:
                        chatIcon = Properties.Resources.twitchicon;
                        break;
                    case ChatIcon.Steam:
                        chatIcon = Properties.Resources.steamicon;
                        break;
                    case ChatIcon.Skype:
                        chatIcon = Properties.Resources.skypeicon;
                        break;
                    case ChatIcon.Admin:
                        chatIcon = Properties.Resources.adminicon;
                        break;
                    case ChatIcon.Goodgame:
                        chatIcon = Properties.Resources.goodgameicon;
                        break;
                    default:
                        chatIcon = null;
                        break;
                }
                if( tb.Text.Length > 0 )
                    tb.AppendText(Environment.NewLine);

                if (text != null)
                {
                    tb.AppendText(DateTime.Now.GetDateTimeFormats('T')[0] + " ");
                    if(chatIcon != null)
                        tb.InsertImage( chatIcon );
                    tb.AppendText(" " + text);
                }
                tb.ScrollToEnd();
            }
        }
    }
}
