using System;
using System.Collections.Generic;
using System.Text;

namespace wzSync.winz.customEvent
{
    public class CustomEventHandler
    {
        public event CustomEventDelegate EventDelegate;

        /// <summary>
        /// 이벤트 핸들러에 연결된 함수로 이벤트를 전달합니다.
        /// </summary>
        /// <param name="arg"></param>
        public void SendCustomEvent(CustomEventArgs arg)
        {
            // 연결된 모든 함수에 이벤트 전송
            CustomEventDelegate handler = EventDelegate;
            if (handler != null) handler(this, arg);
        }
    }
}
