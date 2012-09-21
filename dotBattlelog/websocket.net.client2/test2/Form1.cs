using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WebSocket4Net;
using System.Diagnostics;



namespace test2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        ///spheres/planet/users/2832663157699333695?authToken=1348275118;4505ca05a88e2d0e6d3975b771e7740081d4c645
        WebSocket websocket;

        private void button1_Click(object sender, EventArgs e)
        {

            List<KeyValuePair<string,string>> headers = new List<KeyValuePair<string,string>>();
            //headers.Add( new KeyValuePair<string,string>("Sec-WebSocket-Key", "T1ondlcFAARbT1hQRRghFw=="));
            //headers.Add( new KeyValuePair<string,string>("Sec-WebSocket-Origin", "http://battlelog.battlefield.com"));
            //headers.Add( new KeyValuePair<string,string>("Sec-WebSocket-Version", "8"));

            websocket = new WebSocket(@"ws://89.234.38.223:80/spheres/planet/users/2832663157699333695?authToken=1348281393;5caea42829471ce60a3da407fec918d33ddcd37a", "", null, headers, "", "http://battlelog.battlefield.com", WebSocketVersion.DraftHybi10);

            websocket.Opened += new EventHandler(websocket_Opened);
            websocket.Error += websocket_Error;
            //websocket.Closed += new EventHandler(websocket_Closed);
            websocket.MessageReceived += websocket_MessageReceived;
            websocket.DataReceived += websocket_DataReceived;
            websocket.Open();


        }
        private void websocket_DataReceived(object sender, WebSocket4Net.DataReceivedEventArgs e)
        {
            Debug.Print("Received data:" + e.Data);
        }
        private void websocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            MessageBox.Show(e.Exception.Message);
        }
        private void websocket_Opened(object sender, EventArgs e)
        {
            //MessageBox.Show("connected");
            websocket.Send("__PING__");
            //websocket.Send("_SUB " + "1348280584;78b63ddfae2ca194d786ccb5d8925d94e6f508d6@2832663157699333695");
            websocket.Send("_SUB 1348283134;0201ac9b20127dd47b01733cf9be8a4c93730f6d@2832663157699333695");
            //websocket.Send("_SUB " + "1348281393;5caea42829471ce60a3da407fec918d33ddcd37a@2832663157699333695");
            //websocket.Send("_SUB " + "1348280584;88f2e8604d1bc48ad226b374809d5c34f591098d#friendfeed_2832663157699333695");
            //websocket.Send("_SUB bp:internal");
            //websocket.Send("_SUB " + "@2832663157699333695");
            timer1.Start();
            //websocket.Send("Hello World!");
        }
        private void websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            //MessageBox.Show(e.Message);
            //m_MessageReceiveEvent.Set();
            Debug.Print("Received: " + e.Message);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            websocket.Send("__PING__");
            Debug.Print("Ping sent");
        }
    }
}
