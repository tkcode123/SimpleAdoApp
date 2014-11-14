using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace ADOExtensions
{
    public sealed class ReliableReader : DbDataReader
    {
        private DbDataReader r;
        private TypeCode[] codes;
        private DbCommand owner;

        public ReliableReader(DbDataReader rdr, DbCommand own = null)
        {
            r = rdr;
            owner = own;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (r != null)
                {
                    r.Dispose();
                    r = null;
                }
                if (owner != null)
                {
                    owner.Dispose();
                    owner = null;
                }
            }
        }

        [System.Diagnostics.DebuggerStepThrough]
        private TypeCode GetTypeCode(int ordinal)
        {
            return Type.GetTypeCode(r.GetFieldType(ordinal));
        }

        public override void Close()
        {
            if (r.IsClosed == false)
                r.Close();
        }

        public override int Depth
        {
            get { return r.Depth; }
        }

        public override int FieldCount
        {
            get { return r.FieldCount; }
        }

        public override bool GetBoolean(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return false;
            return r.GetBoolean(ordinal);
        }

        public override byte GetByte(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return 0;
            return r.GetByte(ordinal);
        }

        public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
        {
            if (r.IsDBNull(ordinal))
                return 0;
            return r.GetBytes(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override char GetChar(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return '\0';
            return r.GetChar(ordinal);
        }

        public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
        {
            return r.GetChars(ordinal, dataOffset, buffer, bufferOffset, length);
        }

        public override string GetDataTypeName(int ordinal)
        {
            return r.GetDataTypeName(ordinal);
        }

        public override DateTime GetDateTime(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return new DateTime();
            return r.GetDateTime(ordinal);
        }

        public override decimal GetDecimal(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return 0m;
            return r.GetDecimal(ordinal);
        }

        public override double GetDouble(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return 0.0;
            return r.GetDouble(ordinal);
        }

        public override System.Collections.IEnumerator GetEnumerator()
        {
            return r.GetEnumerator();
        }

        public override Type GetFieldType(int ordinal)
        {
            return r.GetFieldType(ordinal);
        }

        public override float GetFloat(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return 0.0f;
            return r.GetFloat(ordinal);
        }

        public override Guid GetGuid(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return Guid.Empty;
            if (codes[ordinal] == TypeCode.String)
                return Guid.Parse(r.GetString(ordinal));
            return r.GetGuid(ordinal);
        }

        public override short GetInt16(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return 0;
            switch (codes[ordinal])
            {
                case TypeCode.Int32: return (short)r.GetInt32(ordinal);
                case TypeCode.Int64: return (short)r.GetInt64(ordinal);
                case TypeCode.Decimal: return (short)r.GetDecimal(ordinal);
            }
            return r.GetInt16(ordinal);
        }

        public override int GetInt32(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return 0;
            switch (codes[ordinal])
            {
                case TypeCode.Int64: return (int)r.GetInt64(ordinal);
                case TypeCode.Decimal: return (int)r.GetDecimal(ordinal);
            }
            return r.GetInt32(ordinal);
        }

        public override long GetInt64(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return 0;
            switch (codes[ordinal])
            {
                case TypeCode.Int32: return r.GetInt32(ordinal);
                case TypeCode.Decimal: return (long)r.GetDecimal(ordinal);
            }
            return r.GetInt64(ordinal);
        }

        public override string GetName(int ordinal)
        {
            return r.GetName(ordinal);
        }

        public override int GetOrdinal(string name)
        {
            return r.GetOrdinal(name);
        }

        public override DataTable GetSchemaTable()
        {
            return r.GetSchemaTable();
        }

        public override string GetString(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return null;
            return r.GetString(ordinal);
        }

        public override object GetValue(int ordinal)
        {
            if (r.IsDBNull(ordinal))
                return null;
            return r.GetValue(ordinal);
        }

        public override int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public override int GetProviderSpecificValues(object[] values)
        {
            return r.GetProviderSpecificValues(values);
        }

        protected override DbDataReader GetDbDataReader(int ordinal)
        {
            throw new NotImplementedException();
        }

        public override object GetProviderSpecificValue(int ordinal)
        {
            return r.GetProviderSpecificValue(ordinal);
        }

        public override Type GetProviderSpecificFieldType(int ordinal)
        {
            return r.GetProviderSpecificFieldType(ordinal);
        }

        public override bool HasRows
        {
            get { return r.HasRows; }
        }

        public override bool IsClosed
        {
            get { return r.IsClosed; }
        }

        public override bool IsDBNull(int ordinal)
        {
            return r.IsDBNull(ordinal);
        }

        public override bool NextResult()
        {
            codes = null;
            return r.NextResult();
        }

        public override bool Read()
        {
            bool ret = r.Read();
            if (ret && codes == null)
            {
                codes = new TypeCode[r.VisibleFieldCount];
                for (int i = 0; i < codes.Length; i++)
                    codes[i] = Type.GetTypeCode(r.GetFieldType(i));
            }
            return ret;
        }

        public override int RecordsAffected
        {
            get { return r.RecordsAffected; }
        }

        public override int VisibleFieldCount
        {
            get { return r.VisibleFieldCount; }
        }

        public override object this[string name]
        {
            get { return r[name]; }
        }

        public override object this[int ordinal]
        {
            get { return r[ordinal]; }
        }

        public T Get<T>(string methodName, int ordinal)
        {
            return GetTypedValue<T>(r, methodName, ordinal);
        }

        public static T GetTypedValue<T>(object instance, string methodName, int ordinal)
        {
            var mi = instance.GetType().GetMethod(methodName);
            var inst = mi.Invoke(instance, new object[] { ordinal });
            return (T)inst;
        }

        public override string ToString()
        {
            return (owner != null ? owner.CommandText : (r != null ? r.ToString() : "<closed>"));
        }
    }
}
