using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using wzSync.wzSocket;
using wzSync.winz.customEvent;
using wzSync.Manager;

namespace wzSync
{
    public partial class Form1 : Form
    {
        private WZConnectionHandler connHandler = null;
        private FileManager fileManager = null;
        public Form1()
        {
            InitializeComponent();
            connHandler = WZConnectionHandler.getInstance();
            fileManager = FileManager.Instance;

            WZConnectionHandler.customEvent.EventDelegate += new CustomEventDelegate(customEvent_EventDelegate);
            FileManager.customEvent.EventDelegate += new CustomEventDelegate(customEvent_EventDelegate);
        }

        void customEvent_EventDelegate(object sender, CustomEventArgs e)
        {
            switch (e.EventType)
            {
                case CustomEventArgs.eventtype.MESSAGE:
                    listBox1.Items.Add(e.Message);
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                    break;

                case CustomEventArgs.eventtype.COMPLETED:
                    List<FileItem> flist = fileManager.FileList;
                    int i=0;
                    int max_count = flist.Count;
                    listBox1.Items.Clear();
                    for( i=0 ; i<max_count ; i++ )
                    {
                        //string msg = string.Format("{0} - {1} [{2}]", flist[i].Path, flist[i].Name, flist[i].Size);
                        //listBox1.Items.Add(msg);

                        ListViewItem item = new ListViewItem(flist[i].Name);
                        item.SubItems.Add(flist[i].Path);
                        item.SubItems.Add(flist[i].Size.ToString());

                        listView_file.Items.Add(item);
                    }
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length > 0)
            {
                connHandler.SendMessage(textBox1.Text);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listView_file.Items.Clear();

            fileManager.Load("D:\\music");
        }
    }
}
