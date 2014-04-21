using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using wzSync.winz.customEvent;
using System.Diagnostics;
using System.IO;

namespace wzSync.wzSocket
{
    class WZConnectionHandler
    {
        public const uint SYNC_PORT = 3750;      // Default port
        public static CustomEventHandler customEvent = new CustomEventHandler();

        // Singleton
        private static WZConnectionHandler instance = null;
        private Thread waitingSocketThread;
        private Socket listener;
        private uint port = SYNC_PORT;
        private Socket clientSock;

        private SocketAsyncEventArgs m_AcceptAsyncEventArg;
        private SocketAsyncEventArgs m_ReceiveAsyncEventArg;
        private SocketAsyncEventArgs m_SendAsyncEventArg;

        private bool isTerminated = false;
        public WZSocketHandler client;

        #region 초기화
        private WZConnectionHandler()
        {
            this.port = WZConnectionHandler.SYNC_PORT;
            // 비동기방식으로 Accept
            WaitingConnection();

            m_AcceptAsyncEventArg = null;
            m_ReceiveAsyncEventArg = null;
        }
        ~WZConnectionHandler()
        {
            ReleaseInstance();
        }
        #endregion

        public static WZConnectionHandler getInstance()
        {
            if (instance == null)
            {
                instance = new WZConnectionHandler();
            }
            return instance;
        }
        private void ReleaseInstance()
        {
            //isTerminated = true;
            //waitingSocketThread.Abort();
            listener.Close();
        }

        private void WaitingConnection()
        {
            object state = new object();
            IPAddress ipaddr = IPAddress.Any;
            IPEndPoint ipEndPoint = new IPEndPoint(ipaddr, (int)port);
            listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(ipEndPoint);
            listener.Blocking = true;
            listener.Listen(2); // 최대 연결 대기 수
            WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("소켓 초기화"));

            m_AcceptAsyncEventArg = new SocketAsyncEventArgs();
            m_AcceptAsyncEventArg.Completed += myAsyncAccept;

            listener.AcceptAsync(m_AcceptAsyncEventArg);
            /*
            while (!isTerminated)
            {
                clientSock = listener.Accept();
                listener.AcceptAsync(new SocketAsyncEventArgs());
                
                // 클라이언트 IP 얻기
                String clientIP = ((IPEndPoint)clientSock.RemoteEndPoint).Address.ToString();

                string msg = string.Format(" client {0} 접속됨.", clientIP);
                WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs(msg));
                client = new WZSocketHandler(clientSock);
            }
             */
        }

        private void myAsyncAccept(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success)
            {
                proceedAsyncAccept(e);
            }
            else if (e.SocketError == SocketError.OperationAborted)
                listener.AcceptAsync(m_AcceptAsyncEventArg);

            string msg = string.Format("Accept state : {0}", e.SocketError);
            Debug.WriteLine(msg);
        }

        private void proceedAsyncAccept(SocketAsyncEventArgs e)
        {
            try
            {
                clientSock = e.AcceptSocket;
                client = new WZSocketHandler(clientSock);
                // Handle receive event
                client.InitReceiveAsync();
                clientSock.ReceiveAsync(client.ReceiveAsyncEventArg);

                client.wzAsyncSend("Welcome to wzSync server !");

                // 클라이언트 IP 얻기
                String clientIP = ((IPEndPoint)clientSock.RemoteEndPoint).Address.ToString();
                string msg = string.Format(" client {0} 접속됨.", clientIP);
                WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs(msg));
            }
            catch (Exception ex)
            {
                string msg = string.Format("Accept Error : {0}", ex.Message);
                Debug.WriteLine(msg);
                //WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs(ex.Message));
            }
        }

        public void SendMessage(string msg)
        {
            client.wzAsyncSend(msg);
        }
    }
}
