using System;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;

namespace SimpleADOApp
{
    public class StructuredDataDescription
    {
        private readonly Func<DbDataReader, int, object>[] readFunc;
        internal readonly string[] columnName;
        internal readonly Type[] targetType;
        private readonly PropertyDescriptorCollection allProps;
        private readonly string source;
        private readonly Func<StructuredData> creator;

        private static readonly object zeroInt32 = 0;
        private static readonly object oneInt32 = 1;
        private static readonly object falseBool = false;
        private static readonly object trueBool = true;
        private static readonly object zeroInt64 = 0L;

        internal StructuredDataDescription(DbDataReader reader, string source, Func<StructuredData> create)
        {
            int cnt = reader.VisibleFieldCount;
            this.readFunc = new Func<DbDataReader, int, object>[cnt];
            this.columnName = new string[cnt];
            this.targetType = new Type[cnt];
            this.creator = create;

            var prop = new PropertyDescriptor[cnt];
            for (int i = 0; i < cnt; i++)
            {
                this.columnName[i] = reader.GetName(i);
                this.targetType[i] = reader.GetFieldType(i);
                this.readFunc[i] = FindFunc(targetType[i]);

                prop[i] = new StructuredDataPropertyDescriptor(this, i, reader.GetDataTypeName(i));
            }
            this.allProps = new PropertyDescriptorCollection(prop, true);
            this.source = source ?? "StructuredData";
        }

        private static Func<DbDataReader, int, object> FindFunc(Type type)
        {
            if (type == typeof(byte) || type == typeof(byte?))
                return ReadByte;
            if (type == typeof(short) || type == typeof(short?))
                return ReadInt16;
            if (type == typeof(int) || type == typeof(int?))
                return ReadInt32;
            if (type == typeof(long) || type == typeof(long?))
                return ReadInt64;
            if (type == typeof(bool) || type == typeof(bool?))
                return ReadBoolean;
            if (type == typeof(string))
                return ReadString;
            if (type == typeof(float) || type == typeof(float?))
                return ReadFloat;
            if (type == typeof(double) || type == typeof(double?))
                return ReadDouble;
            if (type == typeof(decimal) || type == typeof(decimal?))
                return ReadDecimal;
            if (type == typeof(Guid) || type == typeof(Guid?))
                return ReadGuid;
            if (type == typeof(byte[]))
                return ReadBytes;
            return ReadObject;
        }

        private static object ReadByte(DbDataReader reader, int ordinal)
        {           
            return reader.GetByte(ordinal);
        }

        private static object ReadInt16(DbDataReader reader, int ordinal)
        {
            return reader.GetInt16(ordinal);
        }

        private static object ReadInt32(DbDataReader reader, int ordinal)
        {
            var v = reader.GetInt32(ordinal);
            if (v == 0)
                return zeroInt32;
            if (v == 1)
                return oneInt32;
            return v;
        }

        private static object ReadInt64(DbDataReader reader, int ordinal)
        {
            var v = reader.GetInt64(ordinal);
            if (v == 0)
                return zeroInt64;          
            return v;
        }

        private static object ReadBoolean(DbDataReader reader, int ordinal)
        {
            var v = reader.GetBoolean(ordinal);
            return v ? trueBool : falseBool;
        }

        private static object ReadString(DbDataReader reader, int ordinal)
        {
            return reader.GetString(ordinal);
        }

        private static object ReadFloat(DbDataReader reader, int ordinal)
        {
            return reader.GetFloat(ordinal);
        }

        private static object ReadDouble(DbDataReader reader, int ordinal)
        {
            return reader.GetDouble(ordinal);
        }

        private static object ReadGuid(DbDataReader reader, int ordinal)
        {
            return reader.GetGuid(ordinal);
        }

        private static object ReadDecimal(DbDataReader reader, int ordinal)
        {
            return reader.GetDecimal(ordinal);
        }

        private static object ReadBytes(DbDataReader reader, int ordinal)
        {
            var len = reader.GetBytes(ordinal, 0L, null, 0, 0);
            var ret = new byte[len];
            reader.GetBytes(ordinal, 0L, ret, 0, ret.Length);
            return ret;
        }

        private static object ReadObject(DbDataReader reader, int ordinal)
        {
            return reader.GetValue(ordinal);
        }

        internal StructuredData Interpret(DbDataReader reader)
        {
            var func = this.readFunc;
            var data = new object[func.Length];
            for (int i = 0; i < data.Length; i++)
            {
                if (reader.IsDBNull(i) == false)
                    data[i] = func[i](reader, i);
            }
            var sd = creator();
            return sd.Init(this, data);
        }

        internal PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return allProps;
        }

        public override int GetHashCode()
        {
            return source.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return false;
        }

        public override string ToString()
        {
            return source;
        }
    }
}
