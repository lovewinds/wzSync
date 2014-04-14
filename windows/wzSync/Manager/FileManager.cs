using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using wzSync.winz.customEvent;

namespace wzSync.Manager
{
    public class FileManager
    {
        private List<FileItem> file_list = null;
        private BackgroundWorker bw_fileLoader = new BackgroundWorker();
        private int progress = 0;

        // Singleton
        private static FileManager instance = null;

        public static CustomEventHandler customEvent = new CustomEventHandler();

        private FileManager()
        {
            bw_fileLoader.WorkerReportsProgress = true;
            bw_fileLoader.DoWork += new DoWorkEventHandler(Do_FileListLoad);
            bw_fileLoader.ProgressChanged += new ProgressChangedEventHandler(bw_fileLoader_ProgressChanged);
            bw_fileLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_fileLoader_WorkerCompleted);
        }

        public static FileManager getInstance()
        {
            if (instance == null)
            {
                instance = new FileManager();
            }
            return instance;
        }

        public void Load( string path )
        {
            if( bw_fileLoader.IsBusy == false )
            {
                bw_fileLoader.RunWorkerAsync(path);
            }
        }

        public List<FileItem> FileList
        {
            get { return file_list; }
        }

        #region Background Works
        private void Do_FileListLoad(object sender, DoWorkEventArgs wa)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            if (file_list == null)
                file_list = new List<FileItem>();
            else
                file_list.Clear();

            string path = (string)wa.Argument;
            string[] fileEntries = Directory.GetFiles(path);
            int current = 0;
            int max = fileEntries.Length;
            foreach (string fileName in fileEntries)
            {
                FileInfo fi = new FileInfo(fileName);
                file_list.Add(new FileItem(fi.Name, path, fi.Length));

                worker.ReportProgress(current / max);
            }
        }
        private void bw_fileLoader_ProgressChanged( object sender, ProgressChangedEventArgs pe )
        {
            this.progress = pe.ProgressPercentage;
        }
        private void bw_fileLoader_WorkerCompleted( object sender, RunWorkerCompletedEventArgs ce )
        {
            FileManager.customEvent.SendCustomEvent(new CustomEventArgs(CustomEventArgs.eventtype.COMPLETED));
        }
        #endregion
    }

    public class FileItem
    {
        private string file_name;
        private string file_path;
        private long file_size;

        public FileItem(string name, string path, long size)
        {
            file_name = name;
            file_path = path;
            file_size = size;
        }
        
        public string Name
        {
            get { return file_name; }
            set { file_name = value; }
        }
        public string Path
        {
            get { return file_path; }
            set { file_path = value; }
        }
        public long Size
        {
            get { return file_size; }
        }
    }
}
