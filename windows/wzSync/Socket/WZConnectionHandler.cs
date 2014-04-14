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
                
                m_ReceiveAsyncEventArg = new SocketAsyncEventArgs();
                m_ReceiveAsyncEventArg.UserToken = clientSock;
                m_ReceiveAsyncEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(myAsyncReceive);
                m_ReceiveAsyncEventArg.SetBuffer(new byte[2048], 0, 2048);
                clientSock.ReceiveAsync(m_ReceiveAsyncEventArg);

                // 클라이언트 IP 얻기
                String clientIP = ((IPEndPoint)clientSock.RemoteEndPoint).Address.ToString();
                string msg = string.Format(" client {0} 접속됨.", clientIP);
                WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs(msg));
                
                //client = new WZSocketHandler(clientSock);
            }
            catch (Exception ex)
            {
                string msg = string.Format("Accept Error : {0}", ex.Message);
                Debug.WriteLine(msg);
                //WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs(ex.Message));
            }
        }

        private void myAsyncReceive(object sender, SocketAsyncEventArgs e)
        {
            String msg;
            if (e.BytesTransferred > 0)
            {
                //WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("수신 감지. 데이터 수신중"));
                msg = string.Format("Receive : {0}", ASCIIEncoding.UTF8.GetString(e.Buffer));
                WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs(msg));
                //WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("수신 완료"));

                msg = string.Format("Receive Data : {0}", e.SocketError);
                Debug.WriteLine(msg);



                clientSock.ReceiveAsync(m_ReceiveAsyncEventArg);
            }
            else
            {
                msg = string.Format("Receive Close : ");
                Debug.WriteLine(msg);

                clientSock.Close();

                m_AcceptAsyncEventArg = new SocketAsyncEventArgs();
                m_AcceptAsyncEventArg.Completed += myAsyncAccept;
                listener.AcceptAsync(m_AcceptAsyncEventArg);
            }
            
        }


        public void SendMessage(string msg)
        {
            if (clientSock != null)
            {
                try
                {
                    int bufferCount = 0;
                    byte[] buffer = new byte[4096];
                    buffer = Encoding.UTF8.GetBytes(msg);
                    bufferCount = Encoding.UTF8.GetByteCount(msg);
                    //clientSock.SendBufferSize = 4096;
                    //clientSock.Send(buffer, bufferCount, SocketFlags.None);

                    //  스트림에 텍스트 메시지를 읽고 쓰는 작업
                    using ( NetworkStream ns = new NetworkStream(clientSock) )
                    using ( StreamReader sr = new StreamReader(ns) )
                    using ( StreamWriter sw = new StreamWriter(ns) )
                    {
                        //string welcome = "Welcome to my test server";
                        //지정된 데이터에 라인 피드값을 추가하여 해당 스트림에 전송
                        sw.WriteLine(msg);
                        sw.Flush();//사용한 스트림을 비워줍니다.
                    }

                    string tmp = string.Format("Send : {0}", ASCIIEncoding.UTF8.GetString(buffer));
                    //WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("데이터 전송함."));
                    WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs(tmp));

                    Debug.WriteLine(tmp);
                }
                catch (Exception ex)
                {
                    WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("전송 중 오류발생!"));
                    Debug.WriteLine("전송 중 오류발생!");
                }
            }
        }
    }
}
