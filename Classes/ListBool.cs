using System;

namespace Compiler.Classes
{
    [Serializable]
    public class ListBool
    {
        public ListBool Parent;
        public bool Value;

        public ListBool(ListBool parent, bool val)
        {
            Parent = parent;
            Value = val;
        }

        public new ListBool Add(bool val)
        {
            return new ListBool(this, val);
        }
        public new ListBool Remove()
        {
            return Parent;

        }
    }
}