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
        SQLiteConnection SQL = null;
        SQLiteCommand CMD = null;
        private static DBManager _instance = null;
        private static Queue<string> q_QueryBuffer;
        private static SQLiteDataReader reader;
        private Thread th_ExecuteBufferThread;
        private string _startupPath = System.Windows.Forms.Application.StartupPath;

        public SQLiteDataReader Reader
        {
            get
            {
                if (reader.IsClosed)
                {
                    return null;
                }
                else
                {
                    return reader;
                }
            }
        }
        private DBManager()
        {
            q_QueryBuffer = new Queue<string>();
            reader = null;
        }

        ~DBManager()
        {
            if (_instance.SQL.State == System.Data.ConnectionState.Open)
            {
                _instance.SQL.Close();
            }
            th_ExecuteBufferThread.Abort();
            th_ExecuteBufferThread = null;
        }

        private bool CreateDBFile()
        {
            /* DB File 생성 */
            if (File.Exists(".\\sync.db") == false)
            {
                SQLiteFactory sqf = SQLiteFactory.Instance;
                SQLiteConnection.CreateFile(".\\sync.db");

                SQLiteConnectionStringBuilder sqlcsb = new SQLiteConnectionStringBuilder();
                sqlcsb.DataSource = _startupPath + "./sync.db";
                //sqlcsb.Password = "sync";

                try
                {
                    _instance.SQL = new SQLiteConnection(sqlcsb.ConnectionString);
                    _instance.CMD = new SQLiteCommand(_instance.SQL);
                    if (_instance.SQL.State == System.Data.ConnectionState.Closed)
                    {
                        _instance.SQL.Open();
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

        public static DBManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DBManager();
                }
                if (!_instance.CreateDBFile())
                {
                    SQLiteConnectionStringBuilder sqlcsb = new SQLiteConnectionStringBuilder();
                    sqlcsb.DataSource = "./sync.db";
                    //sqlcsb.Password = "sync";

                    try
                    {
                        _instance.SQL = new SQLiteConnection(sqlcsb.ConnectionString);
                        _instance.CMD = new SQLiteCommand(_instance.SQL);
                        if (_instance.SQL.State == System.Data.ConnectionState.Closed)
                        {
                            _instance.SQL.Open();
                        }
                        ThreadPool.QueueUserWorkItem(new WaitCallback(_instance.ExecuteBuffer), null);
                    }
                    catch (SQLiteException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e.ToString());
                    }
                }

                return _instance;
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
            if (_instance.SQL.State == System.Data.ConnectionState.Open)
            {
                _instance.SQL.Close();
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
                    _instance.CMD.CommandText = query[i];
                    _instance.CMD.ExecuteNonQuery();
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
                _instance.CMD.CommandText = query;
                _instance.CMD.ExecuteNonQuery();
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
                _instance.CMD.CommandText = query;
                reader = _instance.CMD.ExecuteReader();
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
                _instance.CMD.CommandText = "BEGIN";
                _instance.CMD.ExecuteNonQuery();
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
                _instance.CMD.CommandText = "COMMIT";
                _instance.CMD.ExecuteNonQuery();
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
