using System;
using System.Collections.Generic;
using Compiler.Help;

namespace Compiler.Classes
{
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class Trace:List<Trace>
    {
        public int End;
        public string Identifier;
        private Kind _kind;
        public Trace Parent;
        public bool Error ;
        public int Start;

        public Trace Add(int start, int end, Kind kind)
        {
            var trace = new Trace(start, end, kind);
            Add(trace);
            return trace;
        }

        public new Trace Add(Trace item)
        {
            base.Add(item);
            item.Parent = this;
            return item;
        }
        public Trace(int start, int end, Kind kind)
        {
            End = end;
            Start = start;
            _kind = kind;
        }

        public Trace(Kind kind)
        {
            _kind = kind;
        }

        public Kind Kind
        {
            get { return _kind; }
            set { _kind = value; }
        }
    }
}