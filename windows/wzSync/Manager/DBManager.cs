using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SQLite;
using System.Threading;
using System.IO;

namespace wzSync.Manager
{
    class DBManager
    {
        private static DBManager _instance = null;

        private SQLiteConnection sqlConn = null;
        private SQLiteCommand sqlCommand = null;
        
        private static Queue<string> q_QueryBuffer;
        private static SQLiteDataReader reader;
        private Thread th_ExecuteBufferThread;
        private const string _startupPath = System.Windows.Forms.Application.StartupPath;

        #region Initialize
        private DBManager()
        {
            q_QueryBuffer = new Queue<string>();
            reader = null;
        }

        ~DBManager()
        {
            if (_instance.sqlConn != null &&
                _instance.sqlConn.State == System.Data.ConnectionState.Open)
            {
                _instance.sqlConn.Close();
            }
            th_ExecuteBufferThread.Abort();
            th_ExecuteBufferThread = null;
        }

        public DBManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DBManager();
                }
                return _instance;
            }
        }
        #endregion

        public SQLiteDataReader Reader
        {
            get
            {
                if (reader.IsClosed) return null;
                return reader;
            }
        }
        private bool CreateDBFile()
        {
            /* DB File 생성 */
            if (File.Exists(".\\sync.db") == false)
            {
                //SQLiteFactory sqf = SQLiteFactory.Instance;
                SQLiteConnection.CreateFile(".\\sync.db");

                SQLiteConnectionStringBuilder sqlcsb = new SQLiteConnectionStringBuilder();
                sqlcsb.DataSource = _startupPath + "./sync.db";
                //sqlcsb.Password = "sync";

                try
                {
                    _instance.sqlConn = new SQLiteConnection(sqlcsb.ConnectionString);
                    _instance.sqlCommand = new SQLiteCommand(_instance.sqlConn);
                    if (_instance.sqlConn.State == System.Data.ConnectionState.Closed)
                    {
                        _instance.sqlConn.Open();
                    }
                    _instance.ExecuteNonQuery(
                        "Create Table Files ("+
                        "   name nvarchar(25),"+
                        "   directory nvarchar(50),"+
                        "   size int,"+
                        "   created nvarchar(25),"+
                        "   accessed nvarchar(25),"+
                        "   modified nvarchar(25),"+
                        "   extension nvarchar(10),"+
                        "   accessedCount int,"+
                        "   Primary Key(name, directory)"+
                        ")"
                    );
                }
                catch (SQLiteException e)
                {
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private Queue<string> QueryBuffer
        {
            get
            {
                return q_QueryBuffer;
            }
        }

        public int BufferCount
        {
            get
            {
                return q_QueryBuffer.Count;
            }
        }

        public void BufferIn(string query)
        {
            q_QueryBuffer.Enqueue(query);
            //Debug.WriteLine("" + QueryBuffer.Count);
        }

        public bool DBClose()
        {
            if (_instance.sqlConn != null &&
                _instance.sqlConn.State == System.Data.ConnectionState.Open)
            {
                _instance.sqlConn.Close();
                return true;
            }
            return false;
        }

        public bool ExecuteNonQuery(string[] query, int querySize)
        {
            try
            {
                for (int i = 0; i < querySize; i++)
                {
                    _instance.sqlCommand.CommandText = query[i];
                    _instance.sqlCommand.ExecuteNonQuery();
                }
                return true;
            }
            catch (SQLiteException e)
            {
                //Debug.WriteLine("NonQuery Error : " + query + " - " + e.Message);
                return false;
            }
        }

        public bool ExecuteNonQuery(string query)
        {
            try
            {
                _instance.sqlCommand.CommandText = query;
                _instance.sqlCommand.ExecuteNonQuery();
            }
            catch (SQLiteException e)
            {
                //Debug.WriteLine("NonQuery Error : " + query + " - " + e.Message);
                return false;
            }
            catch (InvalidOperationException e2)
            {
                //Debug.WriteLine(e2.ToString() + " in ExecuteNonQuery");
            }
            return true;
        }

        public SQLiteDataReader ExecuteQuery(string query)
        {
            try
            {
                _instance.sqlCommand.CommandText = query;
                reader = _instance.sqlCommand.ExecuteReader();
                return reader;
            }
            catch (SQLiteException e)
            {
                //Debug.WriteLine("Query Error : " + query + " - " + e.Message);
                return null;
            }
        }

        public SQLiteDataReader ReadNext()
        {
            if (reader == null)
            {
                return null;
            }
            else
            {
                if (!reader.Read())
                {
                    reader.Close();
                    //Debug.WriteLine("Reader Close : " + reader.IsClosed.ToString());
                    reader = null;
                }
                return reader;
            }
        }

        public bool nonQueryStart()
        {
            try
            {
                _instance.sqlCommand.CommandText = "BEGIN";
                _instance.sqlCommand.ExecuteNonQuery();
                //Debug.WriteLine("NonQueryStart");
                return true;
            }
            catch
            {
                //Debug.WriteLine("NonQueryStart Error");
                return false;
            }
        }

        public bool nonQueryEnd()
        {
            try
            {
                _instance.sqlCommand.CommandText = "COMMIT";
                _instance.sqlCommand.ExecuteNonQuery();
                //Debug.WriteLine("NonQueryEnd");
                return true;
            }
            catch
            {
                //Debug.WriteLine("NonQueryEnd Error");
                return false;
            }
        }

        private void ExecuteBuffer(object _object)
        {
            bool transaction = false;
            try
            {
                while (true)
                {
                    if (reader != null)
                    {
                        if (!reader.IsClosed)
                        {
                            //Debug.WriteLine("Reader is open");
                            Thread.Sleep(500);
                            continue;
                        }
                    }
                    lock (DBManager.q_QueryBuffer)
                    {

                        if (DBManager.q_QueryBuffer.Count > 2 && !transaction)
                        {
                            nonQueryStart();
                            transaction = true;
                        }

                        if (DBManager.q_QueryBuffer.Count == 0)
                        {
                            if (transaction)
                            {
                                nonQueryEnd();
                                transaction = false;
                            }
                            else
                            {
                                Thread.Sleep(500);
                            }
                        }
                        if (DBManager.q_QueryBuffer.Count > 0)
                        {
                            ExecuteNonQuery(DBManager.q_QueryBuffer.Dequeue());
                            /*
                            if (ExecuteNonQuery(DB.q_QueryBuffer.Dequeue()))
                            {
                                //Debug.WriteLine("DB : " + DB.q_QueryBuffer.Count);
                            }
                            */
                        }
                    }
                }
            }
            catch (Exception E)
            {
                //Debug.WriteLine(E.ToString());
            }
        }

        public void InsertFile(string path, string name, string ext, long size)
        {
            string query = string.Format(@"
                INSERT INTO Files (
	                {0},
	                {1},
	                {2},
	                'TEST',
	                'TEST',
	                'TEST',
	                {3},
	                0
                );
            ", path, name, size, ext);
            ExecuteQuery(query);
        }
    }

    class DBQuery
    {
        private string myQuery;
        public string QueryString
        {
            get
            {
                return myQuery;
            }
            set
            {
                myQuery = value;
            }
        }

        public DBQuery(string query)
        {
            myQuery = query;
        }
    }
}
