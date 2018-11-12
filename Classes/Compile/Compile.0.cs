using System.Collections.Generic;
using Compiler.Help;

namespace Compiler.Classes.Compile
{
    using Compiler.Classes.Assembly;

    public partial class Compile
    {
        internal void Bloc(IEnumerable<Tree> trees)
        {
            Insert(Const.nop, null);
            foreach (var tree in trees)
                Compiler(tree);
            Insert(Const.rop, null);
        }

        internal void If(IList<Tree> trees)
        {
            string lb2 = Labels.GetNew(), lb1 = Labels.GetNew();

            Insert(Const.ne, null, Compiler(trees[0]));
            Insert(Const.jmp, null, new Tree(lb2, Kind.Label));

            Compiler(trees[1]);
            if (trees.Count == 3)
                Insert(Const.jmp, lb1, new Tree(lb1, Kind.Label));

            Insert(Const.label, lb2, new Tree(lb2, Kind.Label));

            if (trees.Count != 3) return;

            Compiler(trees[2]);
            Insert(Const.label, lb1, new Tree(lb1, Kind.Label));

        }

        internal void While(IList<Tree> trees)
        {
            string lb2 = Labels.GetNew(), lb1 = Labels.GetNew();

            Insert(Const.label, lb2, new Tree(lb2, Kind.Label));
            Insert(Const.ne, null, Compiler(trees[0]));

            Insert(Const.jmp, null, new Tree(lb1, Kind.Label));
            Compiler(trees[1]);
            
            Insert(Const.mov, null, EAX, True);
            Insert(Const.jmp, null, new Tree(lb2, Kind.Label));
            Insert(Const.label, lb1, new Tree(lb1, Kind.Label));
        }

        internal void Do(IList<Tree> trees)
        {
            var lb1 = Labels.GetNew();            
            Insert(Const.label, lb1, new Tree(lb1, Kind.Label));
            Compiler(trees[0]);
            var T = Compiler(trees[1]);
            Insert(Const.mov, null, EAX, T);
            Insert(Const.jmp, null, new Tree(lb1, Kind.Label));
        }

        private static Tree True = new Tree("0xFFFFFFFF", Kind.Const);
        internal void For(IList<Tree> trees)
        {
            
            string lb1 = Labels.GetNew(), lb2 = Labels.GetNew();
            
            Compiler(trees[0]);
            Insert(Const.label, lb1, new Tree(lb1, Kind.Label));
            Insert(Const.not, null, Compiler(trees[1]));
            Insert(Const.jmp, null, new Tree(lb2, Kind.Label));
            Compiler(trees[3]);
            Compiler(trees[2]);
            Insert(Const.mov, null, True);
            Insert(Const.jmp, null, new Tree(lb1, Kind.Label));
            Insert(Const.label, lb2, new Tree(lb2, Kind.Label));
        }

        internal Tree EqAssigne(IList<Tree> trees)
        {
            var nc = trees[1].Children.Count;
            var T = new Assembler(trees[0].Content, System.Lexical)
            {
                LeftParam = Compiler(trees[1], trees[0].Content)
            };
            if (nc < 2)
                Insert(Const.mov, null, new Tree(T.Name, Kind.Variable), T.LeftParam);
            return trees[1];
        }

        internal void TypeAssigne(IList<Tree> trees)
        {
            System.Lexical.SetVariable(trees[0].Content, trees[1].Content);
            if (trees.Count == 2) return;
            var right = trees[2];
            var left = trees[1];
            Tree tree = new Tree(left.Pile, left.Parent, Kind.EqAssign);
            tree.Children.Add(left); tree.Children.Add(right);
            tree.Start = left.Start; tree.End = right.End;
            tree.Type = left.Parent.Type;
            Compiler(tree);
        }

        internal void Goto(IList<Tree> trees)
        {
            var T = new Assembler(System.Lexical);
            T.Set("goto");
            T.LeftParam = trees[1];
            Insert(Const.mov, null, new Tree("eax", Kind.Register), new Tree("1", Kind.Numbre));
            Insert("jmp", null, new Tree(trees[1].Content, Kind.Label));
        }

        private void Params (IReadOnlyList <Tree> list)
        {
            int j = -1;
            var jm = new List <FieldInfo>(5);
            for (int i = 0; i < list.Count; i++) {
                FieldInfo d = null;
                bool e;
                var isNotConst = ((list[i].Kind & Const.ConstKind)) == 0;
                if ( isNotConst ) {
                    d = System.Lexical.GetNewVaiable(list[i].Type);
                    jm.Add(d);
                    j++;
                }
                else if ( list[i].Kind == Kind.Variable ) 
                    if ( !this.System.Lexical.GetVariable(list[i].Content, out d, out e) ) throw new global::System.Exception("Element Not Found");
                
                Compiler(list[i], d == null ? null : d.Name);
                Insert(Const.push, null, new Tree(d == null ? list[i].Content : d.Name, isNotConst ? Kind.Variable : (list[i].Kind == Kind.Variable ? Kind.Variable : Kind.Numbre)));
            }
            for (; j >= 0; j--) {
                System.Lexical.DisactiveVariable(jm[j]);
                jm.RemoveAt(0);
            }
        }

        private FieldInfo New_Get_Variable (Type type, string name = null)
        {
            if ( string.IsNullOrWhiteSpace(name) ) return System.Lexical.GetNewVaiable(type);
            bool b;
            FieldInfo field;
            if ( !this.System.Lexical.GetVariable(name, out field, out b) ) 
                return this.System.Lexical.SetVariable(type, name);
            return field;
        }

        internal Tree Caller(IList<Tree> trees, Kind kind = Kind.Caller, string name = null)
        {
            var dname = string.IsNullOrEmpty(name);
            bool isvoid = trees[0].Type.Equals("System.void");
            FieldInfo d = isvoid ? null : New_Get_Variable(trees[0].Type, name);
            string _dname = isvoid ? null : d.Name;
            var T = new Assembler(isvoid ? null : d.Name, System.Lexical);
            Params(trees[1].Children);
            var FO = System.Lexical.GetMethod(trees[0], trees[1].Children);
            if (FO == null) throw new global::System.Exception("Function Not Found");

            if (kind == Kind.Caller)
            {
                System.Lexical.CurrentMethod.Calls.Add(new Label<MethodInfo>() { index = System.Lexical.StreamWriter.Offset, Value = FO });
                System.Lexical.Call(FO);
            }
            else
            {
                Insert(Const.push, null, new Tree(T.Fn, Kind.Variable));
                System.Lexical.CurrentMethod.Calls.Add(new Label<MethodInfo>() { index = System.Lexical.StreamWriter.Offset, Value = FO });
                Insert(Const.call, null, new Tree("this", Kind.Variable));
            }
            if (!isvoid)
                Insert(Const.pop, null, new Tree(_dname, Kind.Variable));
            if (dname & !isvoid) System.Lexical.DisactiveVariable(d);
            return new Tree(_dname, Kind.Variable, trees[0].Parent);
        }
    }
    
    public partial class Compile
    {
        internal Tree New(IList<Tree> trees, Kind kind = Kind.Caller, string name = null)
        {
            var dname = string.IsNullOrEmpty(name);
            FieldInfo d = New_Get_Variable(trees[0].Type, name);
            var T = new Assembler(d.Name, System.Lexical);
            Params(trees[1].Children);
            var FO = System.Lexical.GetMethod(trees[0], trees[1].Children);
            if (FO == null) throw new global::System.Exception("Function Not Found");

            if (kind == Kind.Caller)
            {
                System.Lexical.CurrentMethod.Calls.Add(new Label<MethodInfo>() { index = System.Lexical.StreamWriter.Offset, Value = FO });
                System.Lexical.Call(FO);
            }
            else
            {
                Insert(Const.push, null, new Tree(T.Fn, Kind.Variable));
                System.Lexical.CurrentMethod.Calls.Add(new Label<MethodInfo>() { index = System.Lexical.StreamWriter.Offset, Value = FO });
                Insert(Const.call, null, new Tree("this", Kind.Variable));
            }
            Insert(Const.pop, null, new Tree(d.Name, Kind.Variable));
            if (dname) System.Lexical.DisactiveVariable(d);
            return new Tree(d.Name, Kind.Variable, trees[0].Parent);
        }
    }
}
