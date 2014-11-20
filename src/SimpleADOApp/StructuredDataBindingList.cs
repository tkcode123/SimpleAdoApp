using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection;

namespace SimpleADOApp
{
    public class StructuredDataBindingList : BindingList<StructuredData>, ITypedList
    {
        private ListSortDirection direction;
        private PropertyDescriptor property;
        private PropertyDescriptorCollection allprops;
        private PropertyDescriptorCollection addprops;

        public StructuredDataBindingList() { }
        public StructuredDataBindingList(IList<StructuredData> l, bool position) : base(l) 
        {
            if (position)
                AddVirtualProperty("#", "#", typeof(int), (x, n) => x.GetPosition(), null);
        }

        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        protected override void ApplySortCore(
          PropertyDescriptor _property, ListSortDirection _direction)
        {
            var items = this.Items as List<StructuredData>;

            if (items != null)
            {
                items.Sort(new PropertyDescriptorComparer(_property, _direction));
                property = _property;
                direction = _direction;
            }
            else
            {
                property = null;
            }

            this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
        }

        protected override void RemoveSortCore()
        {
            property = null;
        }

        protected override bool IsSortedCore
        {
            get { return (property != null); }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get { return direction; }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get { return property; }
        }

        #region ITypedList Members

        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            if (allprops == null && this.Items.Count > 0)
            {
                allprops = TypeDescriptor.GetProperties(this.Items[0]);
                if (addprops != null && addprops.Count > 0)
                {
                    PropertyDescriptor[] combined = new PropertyDescriptor[addprops.Count + allprops.Count];
                    for (int i = 0; i < addprops.Count; i++)
                        combined[i] = addprops[i];
                    for (int i = 0, o = addprops.Count; i < allprops.Count; i++)
                        combined[i + o] = allprops[i];
                    allprops = new PropertyDescriptorCollection(combined);
                }
            }
            return allprops;           
        }

        public string GetListName(PropertyDescriptor[] listAccessors)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        public override string ToString()
        {
            return this.Items.Count > 0 ? this.Items[0].ToString() : "DATA";
        }

        public delegate object GetVirtualValue(StructuredData from, string name);
        public delegate void SetVirtualValue(StructuredData from, string name, object o);

        public void AddVirtualProperty(string name, string header, Type t,
                                       GetVirtualValue funcG, SetVirtualValue funcS)
        {
            Attribute[] attr = null;
            if (header != null)
            {
                attr = new Attribute[1];
                attr[0] = new DisplayNameAttribute(header);
            }
            PropertyDescriptor vp = new VirtualProperty(name, attr, funcG, funcS, t);
            if (addprops == null)
                addprops = new PropertyDescriptorCollection(null);
            addprops.Add(vp);
        }

        private class VirtualProperty : PropertyDescriptor
        {
            private string pname;
            private Type ptype;
            private GetVirtualValue pget;
            private SetVirtualValue pset;

            internal VirtualProperty(string name, Attribute[] attr, GetVirtualValue func,
                                     SetVirtualValue funcS, Type t)
                : base(name, attr)
            {
                pname = name; pget = func; ptype = t; pset = funcS;
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
                return pget((StructuredData)component, pname);
            }

            public override bool IsBrowsable
            {
                get { return true; }
            }

            public override bool IsReadOnly
            {
                get { return (pset == null); }
            }

            public override Type PropertyType
            {
                get { return ptype; }
            }

            public override void ResetValue(object component)
            {
                throw new Exception("The method or operation is not implemented.");
            }

            public override void SetValue(object component, object value)
            {
                if (pset != null)
                {
                    pset((StructuredData)component, pname, value);
                    return;
                }
                throw new Exception("Cannot set value.");
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }
    }

    public sealed class PropertyDescriptorComparer : System.Collections.Generic.IComparer<StructuredData>
    {
        private PropertyDescriptor _property;
        private bool _ascending;

        public PropertyDescriptorComparer(PropertyDescriptor property, ListSortDirection direction)
        {
            _property = property;
            _ascending = (direction == ListSortDirection.Ascending);
        }

        #region IComparer<T>

        public int Compare(StructuredData xWord, StructuredData yWord)
        {
            // Get property values
            object xValue = _property.GetValue(xWord);
            object yValue = _property.GetValue(yWord);

            int val = Compare(xValue, yValue);
            // Determine sort order
            if (_ascending)
            {
                return val;
            }
            return 0 - val;
        }

        public bool Equals(StructuredData xWord, StructuredData yWord)
        {
            if (xWord != null)
                return xWord.Equals(yWord);
            return (yWord == null);
        }

        public int GetHashCode(StructuredData obj)
        {
            return (obj != null ? obj.GetHashCode() : 0);
        }

        #endregion

        // Compare two property values of any type
        private int Compare(object xValue, object yValue)
        {
            if (xValue == null)
                return (yValue == null ? 0 : -1);
            else if (yValue == null)
                return 1;

            // If values implement IComparer
            IComparable z = xValue as IComparable;
            if (z != null)
            {
                return z.CompareTo(yValue);
            }
            // If values don't implement IComparer but are equivalent
            else if (xValue.Equals(yValue))
            {
                return 0;
            }
            // Values don't implement IComparer and are not equivalent, so compare as string values
            return xValue.ToString().CompareTo(yValue.ToString());
        }
    }
}
