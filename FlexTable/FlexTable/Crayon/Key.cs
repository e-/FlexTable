using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlexTable.Crayon
{
    class Key
    {
        public Object Key1;
        public Object Key2;

        public Key(Object key1, Object key2)
        {
            Key1 = key1;
            Key2 = key2;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Key)) return false;
            Key o = obj as Key;

            return Key1 == o.Key1 && Key2 == o.Key2;
        }

        public override int GetHashCode()
        {
            return Key1.GetHashCode() + Key2.GetHashCode();
        }
    }
}
