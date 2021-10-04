using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Halmid_Client.Functions
{
    class NotifyBallon
    {
        public static void Show(string text, string title)
        {
            var icon = new System.Windows.Forms.NotifyIcon();
            icon.Icon = new Icon("logo.ico");
            icon.Visible = true;
            icon.BalloonTipText = text;
            icon.BalloonTipTitle = title;
            icon.ShowBalloonTip(3000);
        }
    }
}
