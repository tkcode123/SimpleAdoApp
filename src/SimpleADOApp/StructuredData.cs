using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using ADOExtensions;

namespace SimpleADOApp
{
    public class StructuredData : ICustomTypeDescriptor
    {     
        public static IEnumerable<StructuredData> ReadMapped(ReliableReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            var desc = new StructuredDataDescription(reader);
            while (reader.Read())
            {
                yield return desc.Read(reader);
            }
            reader.Close();
            reader.Dispose();
            yield break;
        }

        private readonly StructuredDataDescription description;
        private readonly object[] data;

        public StructuredData(StructuredDataDescription description, object[] data)
        {
            this.description = description;
            this.data = data;
        }

        public StructuredDataDescription GetDescription() { return this.description; }
        public object GetValue(int i) { return this.data[i]; }

        #region ICustomTypeDescriptor Members

        public AttributeCollection GetAttributes()
        {
            throw new NotImplementedException();
        }

        public string GetClassName()
        {
            throw new NotImplementedException();
        }

        public string GetComponentName()
        {
            throw new NotImplementedException();
        }

        public TypeConverter GetConverter()
        {
            throw new NotImplementedException();
        }

        public EventDescriptor GetDefaultEvent()
        {
            throw new NotImplementedException();
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            throw new NotImplementedException();
        }

        public object GetEditor(Type editorBaseType)
        {
            throw new NotImplementedException();
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            throw new NotImplementedException();
        }

        public EventDescriptorCollection GetEvents()
        {
            throw new NotImplementedException();
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return description.GetProperties(attributes);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return description.GetProperties(null);
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
