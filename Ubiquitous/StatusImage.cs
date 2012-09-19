using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace Ubiquitous
{
    /// <summary>
    /// Switching status images from "on" to "off" on PictureBox control.
    /// </summary>
    class StatusImage
    {
        private Image _green;
        private Image _red;
        delegate void SetImageCallback(PictureBox pb);
        public StatusImage(Image green, Image red )
        {
            _green = green;
            _red = red;
        }
        public void SetOn(PictureBox pb)
        {
            if (pb.InvokeRequired)
            {
                SetImageCallback d = new SetImageCallback(SetOn);
                pb.Parent.Invoke(d, new object[] { pb });
            }
            else
            {
                if (_green != null)
                    pb.Image = _green;                
            }
        }
        public void SetOff(PictureBox pb)
        {
            if (pb.InvokeRequired)
            {
                SetImageCallback d = new SetImageCallback(SetOff);
                pb.Parent.Invoke(d, new object[] { pb });
            }
            else
            {
                if (_green != null)
                    pb.Image = _red;
            }
        }
    }
}
