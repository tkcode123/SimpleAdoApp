using System;
using System.ComponentModel;
using System.Linq;

namespace SimpleADOApp
{
    public class StructuredDataPropertyDescriptor : PropertyDescriptor
    {
        private readonly StructuredDataDescription description;
        private readonly int index;
        private TypeConverter converter;

        internal StructuredDataPropertyDescriptor(StructuredDataDescription descr, int idx, string desc)
            : base(descr.columnName[idx], new Attribute[] 
                                        { new BrowsableAttribute(true), 
                                          new ReadOnlyAttribute(true) })
        {
            this.description = descr;
            this.index = idx;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return typeof(StructuredData); }
        }

        public override object GetValue(object component)
        {
            var sd = component as StructuredData;
            if (sd != null && this.description == sd.GetDescription())
                return sd.GetValue(this.index);
            throw new NotImplementedException();
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override Type PropertyType
        {
            get { return this.description.targetType[this.index]; }
        }

        public override void ResetValue(object component)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object component, object value)
        {
            throw new NotImplementedException();
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                var a = base.Attributes;
                return a;
            }
        }

        public override bool DesignTimeOnly
        {
            get
            {
                return base.DesignTimeOnly;
            }
        }

        public override string DisplayName
        {
            get
            {
                return base.Name;
            }
        }

        public override string Name
        {
            get
            {
                return base.Name;
            }
        }

        public override string Category
        {
            get
            {
                return "Data";
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                if (converter == null)
                    converter = base.Converter;
                return converter;
            }
        }

        public override string Description
        {
            get
            {
                var d = base.Description;
                return d;
            }
        }

        public override bool IsBrowsable
        {
            get
            {
                return true;
            }
        }

        public override bool IsLocalizable
        {
            get
            {
                return base.IsLocalizable;
            }
        }

        public override bool SupportsChangeEvents
        {
            get
            {
                return false;
            }
        }

        public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
        {
            return base.GetChildProperties(instance, filter);
        }

        public override object GetEditor(Type editorBaseType)
        {
            return base.GetEditor(editorBaseType);
        }

        public override string ToString()
        {
            return string.Format("{0} : {1} ({2} : {3})", this.Name, this.PropertyType.Name, this.index, this.Description);
        }
    }
}
