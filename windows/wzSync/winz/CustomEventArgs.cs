using System;
using System.Collections.Generic;
using System.Text;

namespace wzSync.winz.customEvent
{
    public class CustomEventArgs : EventArgs
    {
        public enum eventtype { NORMAL, MESSAGE, COMPLETED };
        private String message;
        private eventtype event_type;

        public CustomEventArgs(eventtype type)
        {
            this.event_type = type;
            message = "";
        }
        public CustomEventArgs(String msg)
        {
            this.event_type = eventtype.MESSAGE;
            message = msg;
        }

        public String Message
        {
            get { return message; }
            set { message = value; }
        }
        public eventtype EventType
        {
            get { return event_type; }
            set { event_type = value; }
        }
    }

    public delegate void CustomEventDelegate(object sender, CustomEventArgs e);
}