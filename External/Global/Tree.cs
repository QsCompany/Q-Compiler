using System;
using System.Collections.Generic;
using Compiler.Decompiler.Bases;

namespace Compiler.Global
{
    public class Tree : List<Tree>
    {
        public Tree(string content, Kind kind, Tree parent = null)
        {
            Builder = new Builder(content);
            End = content.Length - 1;
            Kind = kind;
            Parent = parent;
        }

        public Tree(Builder builder, Tree parent, Kind kind)
        {
            Parent = parent;
            Kind = kind;
            Builder = builder;
        }

        public Builder Builder { get; set; }

        public string Content
        {
            get
            {
                var arr = new char[End - Start + 1];
                Array.Copy(Builder.Stream, Start, arr, 0, End - Start + 1);
                var result = "";
                for (var i = 0; i < arr.Length; i++)
                    result = result + arr[i];

                return result;
            }
        }

        public int End { get; set; }

        public Kind Kind { get; set; }

        public Tree Parent { get; set; }

        public int Start { get; set; }

        public string Join()
        {
            var s = "";
            if (Count > 0)
                s = base[0].Content;
            for (var i = 1; i < Count; i++)
            {
                var item = base[i];
                s += "," + item.Content;
            }
            return s;
        }

        public bool Set(bool save = true)
        {
            if (save & Parent != null)
            {
                Start = Builder.PilePos[Builder.PilePos.Count - 1];
                End = Builder.CurrentPos - 1;
                Parent.Add(this);
            }
            return Builder.Leave(save);
        }

        public override string ToString()
        {
            return Kind + ": " + Content;
        }
    }
}