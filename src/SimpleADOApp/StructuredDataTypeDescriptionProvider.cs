using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using ADOExtensions;

namespace SimpleADOApp
{
    public class StructuredDataTypeDescriptionProvider : TypeDescriptionProvider
    {
        internal static StructuredDataDescription GetDescription(ReliableReader reader)
        {
            return new StructuredDataDescription(reader);
        }

        public StructuredDataTypeDescriptionProvider()
        {

        }

        public override bool IsSupportedType(Type type)
        {
            return type == typeof(StructuredData);
        }

        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            return base.GetExtendedTypeDescriptor(instance);
        }

        public override string GetFullComponentName(object component)
        {
            return base.GetFullComponentName(component);
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return new StructuredDataTypeDescriptor();
        }

        protected override IExtenderProvider[] GetExtenderProviders(object instance)
        {
            return base.GetExtenderProviders(instance);
        }

        public override object CreateInstance(IServiceProvider provider, Type objectType, Type[] argTypes, object[] args)
        {
            return base.CreateInstance(provider, objectType, argTypes, args);
        }

        public override System.Collections.IDictionary GetCache(object instance)
        {
            return base.GetCache(instance);
        }

        public override Type GetReflectionType(Type objectType, object instance)
        {
            return base.GetReflectionType(objectType, instance);
        }

        public override Type GetRuntimeType(Type reflectionType)
        {
            return base.GetRuntimeType(reflectionType);
        }
    }
}
