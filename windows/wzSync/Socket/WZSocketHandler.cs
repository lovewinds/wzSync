using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

using wzSync.winz.customEvent;
using System.Diagnostics;

namespace wzSync.wzSocket
{
    class WZSocketHandler
    {
        private Thread sockThread; 
        private Socket socket;
        private bool isTerminated = false;

        public WZSocketHandler(Socket sock)
        {
            this.socket = sock;
            //sockThread = new Thread(new ThreadStart(SockHandle));
            //sockThread.Start();
        }
        ~WZSocketHandler()
        {
            //isTerminated = true;
            //sockThread.Abort();
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

        public void SendMessage(string Msg)     //  메세지 보내기.
        {
            if (socket != null)
            {
                int bufferCount = 0;
                byte[] buffer = new byte[4096];
                buffer = ASCIIEncoding.UTF8.GetBytes(Msg);
                bufferCount = ASCIIEncoding.UTF8.GetByteCount(Msg);

                socket.Send(buffer, 0, bufferCount, SocketFlags.None);
                WZConnectionHandler.customEvent.SendCustomEvent(new CustomEventArgs("데이터 전송함."));
            }
        }
    }
}
