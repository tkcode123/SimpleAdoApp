using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using ADOExtensions;

namespace SimpleADOApp
{
    partial class Program
    { 
        static void Main(string[] args)
        {
            Console.WriteLine("CLR={0}, {1}bit", typeof(int).Assembly.ImageRuntimeVersion, IntPtr.Size * 8);
            //Console.WriteLine("PATH={0}", System.Environment.GetEnvironmentVariable("PATH"));

            string connectionId = args.Length > 0 ? args[0] : "help";
            if (connectionId == "help")
            {
                Console.WriteLine("Defined connection string names:");
                foreach (ConnectionStringSettings e in ConfigurationManager.ConnectionStrings)
                    Console.WriteLine("'{0}' \t => {1}", e.Name, e.ConnectionString);
                return;
            }
            var css = ConfigurationManager.ConnectionStrings[connectionId];
            if (css == null)
            {
                Console.WriteLine("Unknown connection string '{0}'. Use help to see list of defined connection strings.", connectionId);
                return;
            }
            var fact = Show(() => DbProviderFactories.GetFactory(css.ProviderName), "GetFactory");
            if (fact == null)
            {
                Console.WriteLine("Unable to load database driver '{0}'.", css.ProviderName);
                return;
            }
            using (var conn = fact.CreateConnection())
            {
                Console.WriteLine("Connection");
                conn.ConnectionString = css.ConnectionString;
                conn.Open();
                string backend = conn.Backend();
                
                Console.WriteLine("{4} {0} : {3} {1} => {2}", css.Name, fact.GetType().Assembly.GetName().Version, conn.ServerVersion, css.ProviderName, backend);
                string tableName = "TESTME";
                DropTables(conn, tableName);

                CreateTables(conn, tableName);
                FillTables(conn, tableName, 100);
                Count(conn, tableName);
                ReadAllDyn(conn, tableName);
                ReadAll(conn, tableName);
                CleanTables(conn, tableName);
                DropTables(conn, tableName);
                
                conn.Close();
            }
        }

        static void ReadAll(DbConnection conn, string name)
        {
            Show(() => conn.ExecuteReader("SELECT NAME, VAL FROM {0}",
                r => new { N = r.GetString(0), V = r.GetInt64(1) },
                c => c.Dump(),
                name));
        }

        static void ReadAllDyn(DbConnection conn, string name)
        {
            var list = Show(() =>
                {
                    using (var r = conn.ExecuteReliableReader("SELECT NAME, VAL FROM {0}",
                                    c => c.Dump(),
                                    name))
                    {
                        return StructuredData.ReadMapped<StructuredData>(r).ToList();
                    }
                });

            var props = System.ComponentModel.TypeDescriptor.GetProperties(list.First());

            using (var grid = new GridForm())
            {
                grid.Text = "DATA";
                grid.dataGridView1.AutoGenerateColumns = true;
                grid.dataGridView1.AutoSize = true;
                grid.dataGridView1.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.DisplayedCells;
                grid.structuredDataBindingSource.DataSource = list;
                grid.ShowDialog();
            }
        }

        private static int Count(DbConnection conn, string name)
        {
            return Show(() => conn.ExecuteReader("SELECT COUNT(1) FROM {0}", x => x.GetInt32(0), x => x.Dump(), name).Single());
        }

        [System.Diagnostics.DebuggerStepThrough]
        private static X Show<X>(Func<X> action, string name = null, params object[] args)
        {
            X ret = default(X);
            Exception ex = null;
            DateTime start = DateTime.Now;
            try
            {
                ret = action();
            }
            catch (Exception e)
            {
                ex = e;
            }
            DateTime stop = DateTime.Now;
            Console.WriteLine("{3}  => {0,8}   duration={1} {2}", ret, (stop - start), ex != null ? ex.Message : "", string.Format(name??"",args));
            return ret;
        }

        [System.Diagnostics.DebuggerStepThrough]
        private static int Show<X>(Func<IEnumerable<X>> action, string name = null, params object[] args)
        {
            int ret = 0;
            DateTime start = DateTime.Now;
            foreach (var x in action())
            {
                Console.WriteLine("  #{0,4} => {1}", ret, x);
                ret++;
            }
            DateTime stop = DateTime.Now;
            Console.WriteLine("{2}  => {0,8}   duration={1}", ret, (stop - start), string.Format(name??"", args));
            return ret;
        }

        #region CREATE+FILL+DELETE TABLES
        
        private static int CreateTables(DbConnection conn, string name)
        {
            string sql = "";
            switch (conn.Backend())
            {
                case "AZURE":
                case "SQLCE":
                case "MSSQL":
                    sql =
                        @"CREATE TABLE {0} (
    ID int NOT NULL,
    NAME nvarchar(50) NULL,
    VAL bigint NULL,
    CONSTRAINT {1} PRIMARY KEY CLUSTERED (ID ASC ))";                 
                    break;
                case "ORACLE":
                    sql =
                        @"CREATE TABLE {0} (
    ID number(19) NOT NULL,
    NAME varchar2(50),
    VAL number(26),
    CONSTRAINT {1} PRIMARY KEY (ID))";
                    break;
                case "POSTGRES":
                case "SQLITE":
                case "VISTADB":
                    sql =
                        @"CREATE TABLE {0} (
    ID int NOT NULL,
    NAME varchar(50),
    VAL bigint,
    CONSTRAINT {1} PRIMARY KEY (ID))";
                    break;
                default:
                    throw new Exception("Please fill in the needed SQL with the same names and types than the other databases.");
            }
            return Show(() => conn.ExecuteNonQuery(sql, x => x.Dump(), name, "PK_" + name));
        }
        
        private static int FillTables(DbConnection conn, string name, int cnt)
        {
            int ret = 0;
            var txn = conn.BeginTransaction();
            for (int i = 0; i < cnt; i++)
            {
                ret += Show(() => conn.ExecuteNonQuery("INSERT INTO {0}({1},{2},{3}) VALUES (@ID,@NAME,0)",
                    c => c.Parameter("ID", i, DbType.Int32)
                          .Parameter("NAME", "Name" + i, DbType.String)
                          .Transaction(txn),
                    name, "ID", "NAME", "VAL"));
            }
            return Show(() => { txn.Commit(); return ret; }, "COMMIT");
        }
        
        static void CleanTables(DbConnection conn, string name)
        {
            var txn = conn.BeginTransaction();
            Show(() => conn.ExecuteNonQuery(@"DELETE FROM {0}", x => x.Transaction(txn).Dump(), name));
            txn.Commit();
        }
        
        static void DropTables(DbConnection conn, string name)
        {
            Show(() => conn.ExecuteNonQuery(@"DROP TABLE {0}", x => x.Dump(), name));
        }
    
        #endregion
    }
}