using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Diagnostics;

namespace Ubiquitous
{
    class SocketData
    {
        public Client _client;
        public String _data;
        public SocketData(Client client, String data)
        {
            _client = client;
            _data = data;
        }
        public Client Client
        {
            get { return _client; }
        }
        public String Data
        {
            get { return _data; }
        }
    }
    class Client
        {
            public event EventHandler<EventArgs> OnData;
            public event EventHandler<EventArgs> OnDisconnect;
            public static ManualResetEvent dataFlag = new ManualResetEvent(true);
            private TcpClient tcpClient;
            private byte[] dataRcvBuf;
            private bool isShuttingDown;
            private Encoding encoding = Encoding.UTF8;
            private Object socketLock = new Object();
            public bool Connected
            {
                get {
                    if (tcpClient == null)
                        return false;
                    if (tcpClient.Client == null)
                        return false;

                        bool part1 = tcpClient.Client.Poll(5000, SelectMode.SelectRead);
                        bool part2 = (tcpClient.Client.Available == 0);
                        if (part1 & part2)
                            return false;
                        else
                            return true; 
                }
            }

            public void Send(String message)
            {
                if (tcpClient == null)
                    return;

                if (string.IsNullOrEmpty(message))
                    return;

                try
                {
                    if (Connected)
                    {
                        var stream = tcpClient.GetStream();
                        var bytes = Encoding.UTF8.GetBytes(message);
                        stream.Write(bytes, 0, bytes.Length);
                        stream.Flush();
                    }
                }
                catch
                {
                    Debug.Print("Send exception"); }
                finally
                {
                    dataFlag.Set();
                }

                if (isShuttingDown)
                {
                    Close();
                    return;
                }
            }

            public Client(TcpClient c)
            {
                isShuttingDown = false;
                dataRcvBuf = new byte[4096];
                tcpClient = c;
                ThreadPool.QueueUserWorkItem(arg => BeginReceive());
                
            }

            private void BeginReceive()
            {                
                if( tcpClient == null )
                    return;

                dataFlag.Reset();
                try
                {
                    if (Connected)
                    {
                        tcpClient.Client.BeginReceive(
                                dataRcvBuf, 0,
                                dataRcvBuf.Length,
                                SocketFlags.None,
                                new AsyncCallback(OnBytesReceived),
                                this
                        );
                    }
                }
                catch
                {
                    Debug.Print("Receive exception");
                }
                finally
                {
                    dataFlag.WaitOne();
                    Thread.Sleep(10);
                }
            }
            public void Close()
            {
                try
                {
                    if (tcpClient != null)
                    {
                        dataFlag.WaitOne();
                        tcpClient.Close();
                        OnDisconnect(this, EventArgs.Empty);
                    }
                }
                catch { 
                    Debug.Print("Disconnect exception"); 
                }

            }
            public void Disconnect()
            {
                isShuttingDown = true;
            }
            protected void OnBytesReceived(IAsyncResult result)
            {
                int nBytesRec = 0;

                if (tcpClient == null)
                    return;

                try
                {
                    if (!Connected)
                    {
                        dataFlag.Set();
                        return;
                    }
                        nBytesRec = tcpClient.Client.EndReceive(result);
                    }
                catch
                { Debug.Print("OnBYtesReceived exception"); }

                if (nBytesRec <= 0)
                {
                    dataFlag.Set();
                    return;
                }

                string strReceived = encoding.GetString(
                    dataRcvBuf, 0, nBytesRec);

                OnData(new SocketData(this, strReceived), EventArgs.Empty);


                dataFlag.Set();

                if (isShuttingDown)
                {                    
                    Close();
                    return;
                }

                ThreadPool.QueueUserWorkItem(arg => BeginReceive());

            }
    }
    class StatusServer
    {

        private const int port = 3003;
        private const String policyRequest = "<policy-file-request/>";
        private TcpListener serverSocket;
        private List<Client> clientsList;

        private String lastMessage;

        public static ManualResetEvent tcpClientConnected = new ManualResetEvent(false);

        public StatusServer()
        {
            lastMessage = String.Empty;
            clientsList = new List<Client>();
        }
        public bool Start()
        {

            try
            {
                serverSocket = new TcpListener(new IPEndPoint(IPAddress.Any, port));
                serverSocket.Start();
                ThreadPool.QueueUserWorkItem(arg => CheckPendingClients());

            }
            catch {
                return false;
            }
            return true;
        }
        private void CheckPendingClients()
        {
            while (true)
            {
                if (serverSocket.Pending())
                {
                    tcpClientConnected.Reset();
                    BeginAcceptClient();
                    tcpClientConnected.WaitOne();
                }
                Thread.Sleep(10);
            }

        }
        private void BeginAcceptClient()
        {
            serverSocket.BeginAcceptTcpClient( new AsyncCallback(AcceptClient), serverSocket);
        }
        private void AcceptClient( IAsyncResult asRes)
        {
            var clientSocket = ((TcpListener)asRes.AsyncState).EndAcceptTcpClient(asRes);
            var client = new Client(clientSocket);
            client.OnData += DataReceived;
            client.OnDisconnect += ClientDisconnected;
            clientsList.Add(client);

            tcpClientConnected.Set();
        }
        public void ClientDisconnected(object o, EventArgs e)
        {
            var clnt = (Client)o;
            clientsList.Remove(clnt);
        }
        public void DataReceived(object o, EventArgs e)
        {
            SocketData data = (SocketData)o;
            String str = data.Data;
            if (str.Contains(policyRequest))
            {
                try
                {
                    if (data.Client != null)
                    {
                        data.Client.Send(FlashPolicy());
                        data.Client.Disconnect();
                        //clientsList.Remove(data.Client);
                    }
                }
                catch {
                    Debug.Print("Receive exception");
                }
                
            }
            
        }
        public void Broadcast(String m)
        {
            if (String.IsNullOrEmpty(m))
                return;
            
            clientsList.ForEach( c => {if( c!= null) c.Send(String.Format("{0}",m));});
        }


        private string FlashPolicy()
        {
            var xml = "<?xml version=\"1.0\"?>" + Environment.NewLine;
            xml += "<!DOCTYPE cross-domain-policy SYSTEM \"http://www.macromedia.com/xml/dtds/cross-domain-policy.dtd\">" + Environment.NewLine;
            xml += "<cross-domain-policy>" + Environment.NewLine;
            xml += "<allow-access-from domain=\"*\" to-ports=\"*\"/>" + Environment.NewLine;
            xml += "</cross-domain-policy>" + Environment.NewLine;
            return xml;

        }
    
    }

}
