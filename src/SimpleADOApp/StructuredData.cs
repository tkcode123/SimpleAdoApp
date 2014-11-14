using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;

namespace SimpleADOApp
{
    public class StructuredData : ICustomTypeDescriptor, IEquatable<StructuredData>
    {
        public static IEnumerable<T> ReadMapped<T>(DbDataReader reader, string source = null) where T : StructuredData, new()
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            if (reader.IsClosed)
                yield break;
            var desc = new StructuredDataDescription(reader, source ?? reader.ToString(), () => new T());
            while (reader.Read())
            {
                yield return desc.Interpret(reader) as T;
            }
            reader.Close();
            reader.Dispose();
        }

        private StructuredDataDescription description;
        private object[] data;
        private int hash;

        public StructuredData()
        {
        }

        internal StructuredData Init(StructuredDataDescription s, object[] d)
        {
            this.description = s;
            this.data = d;
            return this;
        }

        internal StructuredDataDescription GetDescription() { return this.description; }
        
        public object GetValue(int i) { return this.data[i]; }

        public void SetValue(int i, object val)
        {
            if (val == null)
                this.data[i] = null;
            else if (val.GetType() == this.description.targetType[i])
                this.data[i] = val;
            else
                throw new InvalidOperationException(string.Format("Expected instance of type '{0}', but got '{1}'.", this.description.targetType[i].FullName, val.GetType().FullName));
        }

        public override int GetHashCode()
        {
            if (this.hash == 0)
            {
                int h = description.GetHashCode();
                for (int i = 0; i < data.Length; i++)
                    h ^= (h * 17 + (data[i] != null ? data[i].GetHashCode() : 13));
                this.hash = h;
            }
            return this.hash;
        }

        public override bool Equals(object obj)
        {
            var other = obj as StructuredData;
            if (other != null)
                return Equals(other);
            return base.Equals(obj);
        }

        public override string ToString()
        {
            return description.ToString();
        }

        #region ICustomTypeDescriptor Members

        public virtual AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        public virtual string GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        public virtual string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        public virtual TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        public virtual EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        public virtual PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        public virtual object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        public virtual EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        public virtual EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        public virtual PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return description.GetProperties(attributes);
        }

        public virtual PropertyDescriptorCollection GetProperties()
        {
            return description.GetProperties(null);
        }

        public virtual object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this;
        }

        #endregion

        #region IEquatable<StructuredData> Members

        public bool Equals(StructuredData other)
        {
            if (this == other)
                return true;

            if (other != null && other.description == this.description && this.data.Length == other.data.Length)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    if (object.Equals(this.data[i], other.data[i]) == false)
                        return false;
                }
                return true;
            }
            return false;
        }

        #endregion
    }
}
