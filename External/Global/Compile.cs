// ReSharper disable ParameterTypeCanBeEnumerable.Local

using System.Collections.Generic;
using System.Data.SqlTypes;
using System.IO;

namespace Compiler
{
	public delegate void PropertyChanged<P, T>(P @class,ref T value);
	public class Arbre<T>:List<Arbre<T>>
	{
		public event PropertyChanged<Arbre<T>, T> ValueChanged;
		private void onValueChanged(ref T value)
		{
			if (ValueChanged != null) 
				ValueChanged(this, ref value);
		}
		public Arbre(Arbre<T> parent,T value)
		{
			Parent = parent;
			Value = value;
		}


		public Arbre<T> Parent;
		public string NodeName;

		public T Value
		{
			get { return _value; }
			set
			{
				onValueChanged(ref value);
				_value = value;
			}
		}

		public Arbre<T> _current;
		private T _value;

		public Arbre<T> Current
		{
			get { return _current ?? this; }
			set { _current = value; }
		}

		public void Open(T value)
		{
			var v = new Arbre<T>(Current, value);
			Current.Add(v);
			Current = v;
		}
		public void Close()
		{
			Current = Current.Parent;
		}
		public override string ToString()
		{
			return Value.ToString();
		}
	}
	internal struct Structs:INullable 
	{
		public string Name;
		public string Type;
		public int pointer;
		public bool IsNull { get; private set; }
		public override string ToString()
		{
			return string.Format("{0} {1} : {2}", Type, Name, pointer);
		}
	}
	internal struct Instruction
	{
		internal string Inst;
		internal string Param1;
		internal string Param2;

		public override string ToString()
		{
			return (Inst + " " + Param1) + (!string.IsNullOrEmpty(Param2) ? " ," + Param2 : "");
		}

		public Instruction(string inst, string param1 = null, string param2 = null)
		{
			this.Inst = inst;
			this.Param1 = param1;
			this.Param2 = param2;
		}
	}
	internal class Assemble:List<Instruction>
	{
		public readonly Arbre<Structs> Head = new Arbre<Structs>(null, new Structs());

		public byte[] GetBytes()
		{
			return null;
		}
		public void Index(Instruction s)
		{
			switch (s.Inst)
			{
				case "proc":
				case "space":
				case "class":
					var v = new Structs() {Type = s.Inst, Name = s.Param1, pointer = Count};
					Head.Open(v);
					break;
				case "end space":
				case "end class":
				case "end proc":
					Head.Close();
					break;
			}
		}
		public static Assemble operator +(Assemble assemble, Instruction s)
		{
			assemble.Index(s);
			assemble.Add(s);
			return assemble;
		}
		public static Assemble operator +(Assemble assemble, Assemble s)
		{
			assemble.Capacity += s.Count;
			foreach (Instruction ins in s)
			{
				assemble.Index(ins);
				assemble.Add(ins);
			}
			return assemble;
		}
	}
    internal partial class Compile
    {
        internal void Bloc(IList<Tree> trees)
        {
            Add("nop", "");
            foreach (var tree in trees)
                Compiler(tree);
            Add("rop", "");
        }

        internal void If(IList<Tree> trees)
        {
            var T = Compiler(trees[0]);
            int nl = ++_lb, ml = ++_lb;
            Add("ne", "", T);
            Add("jmp", "", new Tree("B" + nl, Kind.Variable));
            Compiler(trees[1]);
            if (trees.Count == 3)
                Add("jmp", "", new Tree("B" + ml, Kind.Variable));
            Add("label", "B" + nl, new Tree("B" + nl, Kind.Variable));
            if (trees.Count != 3) return;
            Compiler(trees[2]);
            Add("label", "B" + nl, new Tree("B" + ml, Kind.Variable));
        }

        internal void While(IList<Tree> trees)
        {
            int nl = ++_lb, ml = ++_lb;
            Add("label", "B" + nl, new Tree("B" + nl, Kind.Variable));
            Add("ne", "", Compiler(trees[0]));
            Add("jmp", "", new Tree("B" + ml, Kind.Variable));
            Compiler(trees[1]);
            Add("jmp", "", new Tree("B" + nl, Kind.Variable));
            Add("label", "B" + nl, new Tree("B" + ml, Kind.Variable));
        }

        internal void Do(IList<Tree> trees)
        {
            int nl = ++_lb;
            Add("label", "B" + nl, new Tree("B" + nl, Kind.Variable));
            Compiler(trees[0]);
            Tree T = Compiler(trees[1]);
            Add("and", "", T, new Tree("true", Kind.Const));
            Add("jmp", "", new Tree("B" + nl, Kind.Variable));
        }

        internal void For(IList<Tree> trees)
        {
            Compiler(trees[0]);
            int nl = ++_lb, ml = ++_lb;
            Add("label", "B" + nl, new Tree("B" + nl, Kind.Variable));
            Add("ne", "", Compiler(trees[1]));
            Add("jmp", "", new Tree("B" + ml, Kind.Variable));
            Compiler(trees[3]);
            Compiler(trees[2]);
            Add("ne", "", new Tree("0", Kind.Numbre));
            Add("jmp", "", new Tree("B" + nl, Kind.Variable));
            Add("label", "", new Tree("B" + ml, Kind.Variable));
        }

        internal Tree EqAssigne(IList<Tree> trees)
        {
            var nc = trees[1].Count;
            var T = new Assembler(trees[0].Content)
                {
                    Param1 = Compiler(trees[1], trees[0].Content)
                };
            if (nc < 2)
                Add("mov", "", new Tree(T.Name, Kind.Variable), T.Param1);
            trees.RemoveAt(1);
            return trees[0];
        }

        internal void TypeAssigne(IList<Tree> trees)
        {
            Add("push", "", trees[1]);
            Add("mem", "", trees[0]);
            trees.RemoveAt(0);
            if (trees.Count <= 1) return;
            trees[0].Parent.Kind = Kind.EqAssign;
            Compiler(trees[0].Parent);
        }

        internal void Goto(IList<Tree> trees)
        {
            var T = new Assembler();
            T.Set("goto");
            T.Param1 = trees[1];
            Add("jmp", "", new Tree(trees[1].Content, Kind.Label));
        }

        internal Tree Caller(IList<Tree> trees, Kind kind = Kind.Caller, string name = "")
        {
            var dname = name == "";
            var T = new Assembler(dname ? "L" + ++_count : name);
            T.Set(trees[0].Content);

            for (int j = 0; j < trees[1].Count; j++)
                trees[1][j] =
                    Compiler(trees[1][j]);
            T.Param1 = trees[1];
            var h = new Tree(new Builder(T.Name), trees[0].Parent,
                             Kind.Variable) {Start = 0, End = T.Name.Length - 1};

            foreach (var item in T.Param1)
                Add("push", "", new Tree(item.Content, Kind.Variable));
            Add("push", "", new Tree("IP", Kind.Register));
            if (kind == Kind.Caller)
                Add("call", "", new Tree(T.Fn, Kind.Variable));
            else
            {
                Add("push", "", new Tree(T.Fn, Kind.Variable));
                Add("call", "", new Tree("Get_Array", Kind.Variable));
            }
            Add("pop", "", new Tree(T.Name, Kind.Variable));

            if (dname) _count -= 1;
            return h;
        }
    }

    internal partial class Compile
    {
        internal readonly List<Assembler> _structs = new List<Assembler>(15);
	    internal Assemble String = new Assemble();
        internal int _count;
        internal int _lb;
        
        internal void Add(string fn, string name, Tree p1 = null, Tree p2 = null)
        {
            var T = new Assembler(name);
            T.Set(fn);
            T.Param1 = Compiler(p1);
            T.Param2 = Compiler(p2);
            switch (fn)
            {
				case "pow":
                case "mul":
                case "div":
                case "add":
                case "sub":
                case "or":
                case "and":
                case "xor":
                case "inf":
                case "sup":
                case "jmp":
                case "ne":
                case "pop":
                case "push":
                case "ret":
                case "mem":
                case "call":
                case "upl":
                case "usu":
                case "mov":
					String += new Instruction(fn, T.Param1.Content, T.Param2 != null ? T.Param2.Content : "");
                    break;
                case "proc":
                case "class":
                case "space":
				case "end proc":
                case "end class":
                case "end space":
					String += new Instruction(fn, name);
                    break;
                case "label":
					String += new Instruction(fn, name);
                    break;
				case "nop":
		            String += new Instruction(fn);
					break;
                default:
					String += new Instruction(fn, T.Param1 != null ? T.Param1.Content : name, T.Param2 != null ? T.Param2.Content : "");
                    break;
            }
            _structs.Add(T);
        }

        internal Tree Compiler(Tree trees, string name = "")
        {

			var e = System.Diagnostics.Process.GetCurrentProcess().Threads;
            if (trees == null) return null;
            if (trees.Count == 0) return trees;
            switch (trees.Kind)
            {
                case Kind.Const:
                case Kind.Numbre:
                case Kind.String:
                case Kind.Variable:
                    return trees;
                case Kind.Bloc:
                    Bloc(trees);
                    break;
                case Kind.Caller:
                    trees = Caller(trees, name: name);
                    break;
                case Kind.Array:
                    trees = Caller(trees, Kind.Array, name);
                    break;
                case Kind.Do:
                    Do(trees);
                    break;
                case Kind.While:
                    While(trees);
                    break;
                case Kind.For:
                    For(trees);
                    break;
                case Kind.EqAssign:
                    trees = EqAssigne(trees);
                    break;
                case Kind.TypeAssigne:
                    TypeAssigne(trees);
                    break;
                case Kind.Hyratachy:
                case Kind.Expression:
                case Kind.Parent:
                case Kind.Facteur:
                case Kind.Word:
                case Kind.Term:
                case Kind.Power:
                case Kind.Logic:
                    if (trees.Count > 2)
                        for (int j = 0;
                             j < trees.Count
                             & trees.Count > 1;
                             j++)
                        {
                            if (trees[0].Kind == Kind.Unair)
                                trees[j] =
                                    Unair(trees, j, name);
                            else
                                trees[j] =
                                    Operator(trees, j, name);
                            j--;
                        }
                    else if (trees.Count == 2 &
                             trees[0].Kind == Kind.Unair)
                        goto case Kind.Unair;
                    break;
                case Kind.Unair:
                    trees[0] = Unair(trees, 0, name);
                    break;
                case Kind.If:
                    If(trees);
                    break;
                case Kind.Goto:
                    Goto(trees);
                    break;
                case Kind.Function:
                    Function(trees);
                    break;
                case Kind.Class:
                    Class(trees);
                    break;
                case Kind.Space:
                    NameSpace(trees);
                    break;
                case Kind.Return:
                    Add("mov", "", new Tree("return", Kind.Variable), trees[1]);
                    trees.RemoveAt(0);
                    break;
                case Kind.Label:
                    Add("label", trees[0].Content);
                    break;
                case Kind.Param:
                    Compiler(trees[0]);
                    break;

                default:
		            String += new Instruction("ERROR", trees.Kind.ToString(), null);
                    break;
            }

            if (trees.Count == 1) trees = trees[0];
            return trees;
        }

        internal void NameSpace(IList<Tree> trees)
        {
            Add("space", trees[0].Content);
            for (int i = 1; i < trees.Count; i++)
            {
                if (trees[i].Kind == Kind.Class)
                    Class(trees[i]);
                else NameSpace(trees[i]);
            }
            Add("end space", trees[0].Content);
        }

        internal void Class(IList<Tree> trees)
        {
            Add("class", trees[0].Content);
            foreach (var tree in trees)
            {
                if (tree.Kind == Kind.Function)
                {
                    var s = String;
	                String = new Assemble();
                    Function(tree);
                    String = s + String;
                }
            }
            Add(("end class"), trees[0].Content);
        }

        internal void Function(IList<Tree> trees)
        {
            //String += "*******************" + trees[1].Content + "****************************";
            Add("proc", trees[1].Content);
            Add("nop", "");
            Add("pop", "", new Tree("_IP", Kind.Variable));
            for (int j = trees[2].Count - 1; j >= 0; j--)
                Add("pop", "", trees[2][j][1]);
            Add("pop", "", new Tree("return", Kind.Variable));
            Compiler(trees[3]);
            Add("rop", "");
            Add("ret", "", new Tree("_IP", Kind.Variable));
            Add("end proc", trees[1].Content);
        }

        internal Tree Operator(List<Tree> trees, int i, string name = "")
        {
            bool dname = name == "";
            var T = new Assembler(dname ? "L" + ++_count : name);
            T.Set(trees[i + 1].Content);
            T.Param1 = Compiler(trees[i]);
            _count++;
            T.Param2 = Compiler(trees[i + 2]);
            var h = new Tree(new Builder(T.Name), trees[0].Parent, Kind.Variable) {Start = 0, End = T.Name.Length - 1};
            trees.RemoveRange(1, 2);
            int y = Parser.Operats.IndexOf(T.Fn[0]);
            Add(Parser.FOperats[y], "", T.Param1, T.Param2);
            Add("mov", "", new Tree(T.Name, Kind.Variable), new Tree("ax", Kind.Register));
            _count -= 1;
            if (dname) _count -= 1;
            return h;
        }

        internal Tree Unair(IList<Tree> trees, int i,string name="")
        {
            bool dname = name == "";
            var T = new Assembler(dname ? "L" + ++_count : name);
            T.Set(trees[i].Content);
            T.Param1 = Compiler(trees[1]);

            var h = new Tree(new Builder(T.Name), trees[0].Parent, Kind.Variable) {Start = 0, End = T.Name.Length - 1};
            trees.RemoveAt(1);

            int y = Parser.Unair.IndexOf(T.Fn[0]);
            Add(Parser.FUnair[y], "", T.Param1);
            Add("mov", "", new Tree(T.Name, Kind.Variable), new Tree("ax", Kind.Register));
            if(dname)
                _count -= 1;
            return h;
        }

        public bool Build(string fileIn,string fileOut)
        {
            var em = File.ReadAllText(fileIn);
            var s = new FileStream(fileOut, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);
            var b = new Builder(em);
            var g = new Parser(b);
            var parent = new Tree(b, null, Kind.Null);
            var d = g.NameSpace(parent);
            if (d)
            {
                var r = new Compile();
                var e = r.Compiler(parent[0]);
                s.Flush();
	            var bytes = r.String.GetBytes();
                s.Write(bytes, 0, bytes.Length);
            }
            s.Close();
            return d;
        }
    }

}