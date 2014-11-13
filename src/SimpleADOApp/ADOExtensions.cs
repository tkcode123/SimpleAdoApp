using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ADOExtensions
{
    public static class Extensions
    {
        [System.Diagnostics.DebuggerStepThrough]
        public static string Format(this DbConnection conn, string sql, params string[] sqlArgs)
        {
            return Delimit(conn, sql, sqlArgs);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static void AddParameter<T>(this DbCommand command, string name, string column, T value, DbType? typ = null)
        {
            var p = command.NewParameter<T>(name, value, typ);
            p.SourceColumn = column;
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static int ExecuteNonQuery(this DbConnection conn, string sql, Func<DbCommand, DbCommand> parameters = null, params string[] sqlArgs)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Option("BindByName", true);
                cmd.CommandText = conn.Format(sql, sqlArgs);
                if (parameters != null)
                    parameters(cmd);
                return cmd.ExecuteNonQuery();
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static IEnumerable<T> ExecuteReader<T>(this DbConnection conn, string sql, Func<DbDataReader, T> read, Func<DbCommand, DbCommand> parameters = null, params string[] sqlArgs)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Option("BindByName", true);
                cmd.CommandText = conn.Format(sql, sqlArgs);
                if (parameters != null)
                    parameters(cmd);
                return ReadMapped(read, new ReliableReader(cmd.ExecuteReader())).ToList();
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static ReliableReader ExecuteReliableReader(this DbConnection conn, string sql, Func<DbCommand, DbCommand> parameters = null, params string[] sqlArgs)
        {
            var cmd = conn.CreateCommand();
            cmd.Option("BindByName", true);
            cmd.CommandText = conn.Format(sql, sqlArgs);
            if (parameters != null)
                parameters(cmd);
            return new ReliableReader(cmd.ExecuteReader(), cmd);
        }

        [System.Diagnostics.DebuggerStepThrough]
        public static T ExecuteReaderSchema<T>(this DbConnection conn, string sql, Func<DbDataReader, T> read, Func<DbCommand, DbCommand> parameters = null, params string[] sqlArgs)
        {
            using (var cmd = conn.CreateCommand())
            {
                cmd.Option("BindByName", true);
                cmd.CommandText = conn.Format(sql, sqlArgs);
                if (parameters != null)
                    parameters(cmd);
                using (var rr = new ReliableReader(cmd.ExecuteReader(CommandBehavior.SchemaOnly)))
                {
                    return read(rr);
                }
            }
        }

        public static DbParameter NewParameter<T>(this DbCommand command, string name, T value, DbType? typ = null)
        {
            var p = command.CreateParameter();
            var delim = GetDelimiters(command.Connection.Backend());
            p.ParameterName = delim[3]+name;
            if (typeof(T) == typeof(string))
                p.Size = value != null ? value.ToString().Length : 50;
            if (typeof(T) == typeof(Guid) && command.NoGuids())
            {
                string s = value.ToString();
                p.Size = s.Length;
                p.Value = s;
                typ = DbType.String;
            }
            else
                p.Value = value;
            if (typ.HasValue) p.DbType = typ.Value;
            command.Parameters.Add(p);
            return p;
        }

        public static DbCommand Parameter<T>(this DbCommand command, string name, T value, DbType? typ = null)
        {
            var p = command.NewParameter<T>(name, value, typ);
            return command;
        }

        public static DbCommand ArrayParameter<T>(this DbCommand command, string name, T[] value, DbType? typ = null)
        {
            var p = command.NewParameter<T>(name, default(T), typ);
            p.Value = value;
            command.Option("ArrayBindCount", value.Length);
            return command;
        }

        public static DbCommand Option<T>(this DbCommand command, string name, T value)
        {
            var pi = command.GetType().GetProperty(name);
            if (pi != null)
                pi.SetValue(command, value, null);
            return command;
        }

        public static DbCommand Transaction(this DbCommand command, DbTransaction txn)
        {
            command.Transaction = txn;
            return command;
        }

        public static DbCommand Seconds(this DbCommand command, int milliseconds)
        {
            command.CommandTimeout = milliseconds;
            return command;
        }

        public static DbCommand Prepared(this DbCommand command)
        {
            command.Prepare();
            return command;
        }

        public static DbCommand Dump(this DbCommand command)
        {
            Console.WriteLine(command.CommandText);
            foreach(DbParameter p in command.Parameters)
                Console.WriteLine(string.Format("      [{0}:{1}] = {2}",p.ParameterName, p.DbType, (p.Value ?? "<NULL>")));
            return command;
        }

        static IEnumerable<T> ReadMapped<T>(Func<DbDataReader, T> mapping, DbDataReader rdr)
        {
            while (rdr.Read())
            {
                yield return mapping(rdr);
            }
            rdr.Close();
            rdr.Dispose();
            yield break;
        }

        public static T Get<T>(this DbDataReader reader, string name, int ordinal)
        {
            if (reader.IsDBNull(ordinal))
                return default(T);
            ReliableReader rr = reader as ReliableReader;
            if (rr != null)
                return rr.Get<T>(name, ordinal);
            return ReliableReader.GetTypedValue<T>(reader, name, ordinal);
        }

        private static string[] GetDelimiters(string backend)
        {
            if (backend == "MSSQL" || backend == "AZURE" || backend == "SQLCE")
                return new string[] { "[", "]", "@", "@" };
            else if (backend == "MYSQL")
                return new string[] { "`", "`", "?", "?" };
            else if (backend == "FIREBIRD" || backend == "VISTADB")
                return new string[] { "\"", "\"", "@", "" };
            else
                return new string[] { "\"", "\"", ":", "" };
        }

        private static string Delimit(DbConnection connection, string fmt, params string[] sqlArgs)
        {
            if (sqlArgs != null && sqlArgs.Length > 0)
            {
                string[] delim = GetDelimiters(connection.Backend());
                if (delim[2] != "@")
                    fmt = fmt.Replace("@", delim[2]);
                return string.Format(System.Globalization.CultureInfo.InvariantCulture, fmt,
                                            sqlArgs.Select(a => string.Concat(delim[0], a, delim[1])).ToArray());
            }
            return fmt;
        }

        public static string Backend(this DbConnection connection)
        {
            var n = connection.GetType().FullName;
            if (n.Equals("System.Data.SqlClient.SqlConnection"))
                return (connection.ConnectionString.Contains(".database.windows.net") ? "AZURE" : "MSSQL");
            else if (n.Contains("MySql."))
                return "MYSQL";
            else if (n.Contains("Firebird"))
                return "FIREBIRD";
            else if (n.Contains("VistaDB"))
                return "VISTADB";
            else if (n.Contains("Oracle"))
                return "ORACLE";
            else if (n.Contains("Npgsql"))
                return "POSTGRES";
            else if (n.Contains("Anywhere"))
                return "SQLANYWHERE";
            else if (n.Contains("SqlServerCe"))
                return "SQLCE";
            else if (n.Contains("Advantage"))
                return "ADS";
            else if (n.Contains("SQLite"))
                return "SQLITE";
            else if (n.Contains("Vista"))
                return "VISTADB";
            throw new Exception("Unknown backend for " + n);
        }

        public static bool NoGuids(this DbCommand command)
        {
            var n = command.GetType().Name;
            return n.Contains("Oracle") || n == "AdsCommand";
        }
    }
}
