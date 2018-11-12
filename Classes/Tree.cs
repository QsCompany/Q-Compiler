using System;
using System.Collections.Generic;
using Compiler.Help;
using Type = Compiler.Classes.Assembly.Type;
namespace Compiler.Classes
{
    using Compiler.Classes.Assembly;
    using VM.Parser;

    [Serializable]
    public class Tree
    {
        public void Add(Tree tree){Children.Add(tree);}
        public int Count{get { return Children.Count; }}
        public Tree this[int i]
        {
            get
            {
                return Children[i]; 
            }
            set { Children[i] = value; }
        }

        public Tree(ref Pile pile, Tree parent, Kind kind)
        {
            Kind = kind;
            Children = new List<Tree>(0);
            Parent = parent;
            Pile = pile;
        }

        public Tree(string content, Kind kind, Tree parent = null)
        {
            Pile = new Pile(content);
            End = content.Length - 1;
            Kind = kind;
            Children = new List<Tree>(0);
            Parent = parent;
        }

        public Tree(Pile pile, Tree parent, Kind kind)
        {
            Parent = parent;
            Kind = kind;
            Children = new List<Tree>(0);
            Pile = pile;
        }

        internal string Compile { get; set; }
        public Kind Kind { get; set; }
        public int Start { get; set; }
        public int End { get; set; }
        public Tree Parent { get; set; }
        public readonly List<Tree> Children;
        public Pile Pile { get; set; }
        public Type Type { get; set; }
        public MembreInfo Membre { get; set; }
        public Type This;
        /// <summary>
        ///  If the Value==Null ==> there are an Error else 
        ///  If the Value==false ==> the analyse Symentique is not started
        ///  If the Value==Null ==> the analyse Symentique is started yet
        /// </summary>
        public bool? Compiled = false;
        public bool? IsVariabe_Method = false;
        public string Content
        {
            get
            {
                var arr = new char[End - Start + 1];
                Array.Copy(Pile.Stream, Start, arr, 0, End - Start + 1);
                var result = "";
                foreach (var c in arr)
                    result = result + c;
                return result;
            }
        }

        public bool Set(bool save = true,bool Mark=true)
        {
            if (save)
            {
                Start = Pile.PilePos[Pile.PilePos.Count - 1];
                End = Pile.CurrentPos - 1;
                Parent.Children.Add(this);
            }
            return Pile.Leave(save,Mark);
        }

        public override string ToString()
        {
            return Kind + ": " + Content;
        }

        public string Join()
        {
            var s = "";
            if (Children.Count > 0)
                s = Children[0].Content;
            for (var i = 1; i < Children.Count; i++)
            {
                var item = Children[i];
                s += "," + item.Content;
            }
            return s;
        }
    }
}