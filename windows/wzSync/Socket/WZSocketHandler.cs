using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

using wzSync.winz.customEvent;
using System.Diagnostics;
using System.IO;

namespace wzSync.wzSocket
{
    class WZSocketHandler
    {
        private Thread sockThread; 
        private Socket socket;
        private bool isTerminated = false;
        private SocketAsyncEventArgs m_ReceiveAsyncEventArg = null;

        public WZSocketHandler(Socket sock)
        {
            this.socket = sock;
            m_ReceiveAsyncEventArg = new SocketAsyncEventArgs();
            //sockThread = new Thread(new ThreadStart(SockHandle));
            //sockThread.Start();
        }
        ~WZSocketHandler()
        {
            //isTerminated = true;
            //sockThread.Abort();
        }

        public SocketAsyncEventArgs ReceiveAsyncEventArg
        {
            get { return m_ReceiveAsyncEventArg; }
            set { m_ReceiveAsyncEventArg = value; }
        }

        // 소켓 데이터 처리 Thread
        public void SockHandle()
        {
            byte[] buffer = new byte[1024];
            int bufferCount = 0;
            try
            {
                while (!isTerminated)
                {
                    WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("수신 대기중.."));
                    buffer.Initialize();
                    bufferCount = socket.Receive(buffer);
                    WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("수신 감지. 데이터 수신중"));
                    if (bufferCount == 0) break;

                    //String msg = ASCIIEncoding.UTF8.GetString(buffer);
                    byte[] bTemp = new byte[bufferCount];
                    for (int i = 0; i < bufferCount; i++)
                    {
                        bTemp[i] = buffer[i];
                    }
                    String msg = ASCIIEncoding.UTF8.GetString(bTemp);
                    WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("수신 완료"));
                    WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs(msg));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.StackTrace);
            }
            finally
            {
                socket.Close();
                socket = null;
            }
        }

        public void InitReceiveAsync()
        {
            if (m_ReceiveAsyncEventArg != null )
            {
                m_ReceiveAsyncEventArg.UserToken = socket;
                m_ReceiveAsyncEventArg.Completed += new EventHandler<SocketAsyncEventArgs>(wzAsyncReceive);
                m_ReceiveAsyncEventArg.SetBuffer(new byte[1024], 0, 1024);
            }
        }

        public void wzAsyncReceive(object sender, SocketAsyncEventArgs e)
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

                socket.ReceiveAsync(m_ReceiveAsyncEventArg);
            }
            else
            {
                msg = string.Format("Receive Close : ");
                Debug.WriteLine(msg);

                socket.Close();

                /*
                m_AcceptAsyncEventArg = new SocketAsyncEventArgs();
                m_AcceptAsyncEventArg.Completed += myAsyncAccept;
                listener.AcceptAsync(m_AcceptAsyncEventArg);
                WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs(msg));
                 */
            }
        }

        /// <summary>
        /// Send a byte[] buffer
        /// </summary>
        /// <param name="buffer"></param>
        public void wzAsyncSend(byte[] buffer)
        {
            if (socket != null && buffer != null)
            {
                try
                {
                    int bufferCount = buffer.Length;
                    socket.Send(buffer, 0, bufferCount, SocketFlags.None);
                    socket.SendAsync(new SocketAsyncEventArgs());
                    WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("Byte[] sended."));
                }
                catch(Exception e)
                {
                    WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("[wzAsyncSend] Error occured!"));
                }
            }
        }

        public void wzAsyncSend(string msg)
        {
            if (socket != null)
            {
                try
                {
                    int bufferCount = 0;
                    byte[] buffer = new byte[1024];
                    buffer = Encoding.UTF8.GetBytes(msg);
                    bufferCount = Encoding.UTF8.GetByteCount(msg);
                    //clientSock.SendBufferSize = 4096;
                    //clientSock.Send(buffer, bufferCount, SocketFlags.None);

                    //  스트림에 텍스트 메시지를 읽고 쓰는 작업
                    using (NetworkStream ns = new NetworkStream(socket))
                    using (StreamReader sr = new StreamReader(ns))
                    using (StreamWriter sw = new StreamWriter(ns))
                    {
                        //string welcome = "Welcome to my test server";
                        //지정된 데이터에 라인 피드값을 추가하여 해당 스트림에 전송
                        sw.WriteLine(msg);
                        sw.Flush();//사용한 스트림을 비워줍니다.
                    }
                    string tmp = string.Format("String sended. : {0}", ASCIIEncoding.UTF8.GetString(buffer));
                    WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs(tmp));

                    Debug.WriteLine(tmp);
                }
                catch (Exception ex)
                {
                    WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("[wzAsyncSend] Error occured!"));
                    Debug.WriteLine("[wzAsyncSend] Error occured!");
                }
            }
        }
    }
}
