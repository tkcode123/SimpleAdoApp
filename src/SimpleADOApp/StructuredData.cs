using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using ADOExtensions;

namespace SimpleADOApp
{
    [System.ComponentModel.TypeDescriptionProvider(typeof(StructuredDataTypeDescriptionProvider))]
    public class StructuredData
    {     
        public static IEnumerable<StructuredData> ReadMapped(ReliableReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException("reader");
            var desc = StructuredDataTypeDescriptionProvider.GetDescription(reader);
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
    }
}
