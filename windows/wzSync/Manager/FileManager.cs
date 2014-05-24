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
        private DBManager db = DBManager.Instance;

        private FileSystemWatcher fs = new FileSystemWatcher(); //개체 생성

        // Singleton
        private static FileManager _instance = null;

        public static CustomEventHandler customEvent = new CustomEventHandler();

        private FileManager()
        {
            bw_fileLoader.WorkerReportsProgress = true;
            bw_fileLoader.DoWork += new DoWorkEventHandler(Do_FileListLoad);
            bw_fileLoader.ProgressChanged += new ProgressChangedEventHandler(bw_fileLoader_ProgressChanged);
            bw_fileLoader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_fileLoader_WorkerCompleted);
        }

        public static FileManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FileManager();
                }
                return _instance;
            }
        }

        public void Load( string path )
        {
            if( bw_fileLoader.IsBusy == false )
            {
                bw_fileLoader.RunWorkerAsync(path);
                SetWatch(path);
            }
        }

        public List<FileItem> FileList
        {
            get { return file_list; }
        }

        public void SetWatch(string folderName)
        {
            fs.Path = folderName; //Test 폴더 감시 

            fs.NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fs.Filter = ""; // *.*

            fs.Created += new FileSystemEventHandler(fs_Created);
            fs.Deleted += new FileSystemEventHandler(fs_Deleted);
            fs.Renamed += new RenamedEventHandler(fs_Renamed);
            fs.EnableRaisingEvents = true; //이벤트 활성화
        }

        #region Event Handler
        private void fs_Created(object sender, FileSystemEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("{0} / {1}", e.FullPath, e.ChangeType.ToString()) );
            //AddLog(e.FullPath, e.ChangeType.ToString());
            //UpdateFileList(e.Name, e.ChangeType);
            FileInfo fi = new FileInfo(e.FullPath);
            db.InsertFile(fi.FullName, fi.Name, fi.Extension, fi.Length);
        }
        private void fs_Renamed(object sender, RenamedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("{0} / {1}", e.FullPath, e.ChangeType.ToString()));
            //AddLog(e.FullPath, e.ChangeType.ToString());
            //ModFileList(e.OldName, e.Name);
        }
        private void fs_Deleted(object sender, FileSystemEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("{0} / {1}", e.FullPath, e.ChangeType.ToString()));
            //AddLog(e.FullPath, e.ChangeType.ToString());
            //UpdateFileList(e.Name, WatcherChangeTypes.Deleted);
        }
        #endregion

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

            XMLGenerator xml = new XMLGenerator();

            xml.XML_StartCreatedFileList(fileEntries.Length);
            foreach (string fileName in fileEntries)
            {
                FileInfo fi = new FileInfo(fileName);
                file_list.Add(new FileItem(fi.Name, path, fi.Length));

                xml.XML_AppendCreatedFile(fi);
                worker.ReportProgress(current / max);
            }
            xml.XML_EndCreatedFileList();
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
