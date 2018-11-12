using Compiler.Classes.Assembly;
using Compiler.Help;

namespace Compiler.Classes.Compile
{
    public partial class Compile
    {
        public System System;

        private Tree CompileExpressoin(ref Tree trees, string name)
        {
            var left = trees.Children[0];
            var midle = trees.Children[1];
            var d = trees.Children.Count == 2;
            return trees = Operation(left, midle, d ? null : trees.Children[2], name, d ? Kind.Unair : Kind.Operator);
        }
        internal Tree Operation(Tree left, Tree midle, Tree right, string name = null, Kind kind = Kind.Operator)
        {
            Tree w;
            var T = new Assembler(System.Lexical);
            var fn = kind == Kind.Operator ? CalcOperation(left, midle, right, ref T, out w,name) : CalcUnair(left, midle, ref T, out w,name);
            T.Set(fn);
            System.Lexical.SetInstruction(T);
            Insert(Const.mov, null, w, EAX);
            return w;
        }

        public readonly static Tree EAX = new Tree(Const.eax, Kind.Register);

        private string CalcUnair (Tree left, Tree right, ref Assembler T, out Tree Parent, string name)
        {
            T.Set(left.Content);
            T.LeftParam = Compiler(right);
            var d = string.IsNullOrWhiteSpace(name);
            FieldInfo z = New_Get_Variable(left.Parent.Type, name);
            T.Name = z.Name;
            var fn = Parser.GetUnair(T.Fn[0], T.Fn.Length > 1 && T.Fn[1] == '=');
            Parent = new Tree(T.Name, Kind.Variable, left.Parent);
            Parent.Children.Add(left);
            Parent.Children.Add(right);
            Parent.Type = T.Type;
            if ( d ) System.Lexical.DisactiveVariable(z);
            return fn;
        }

        private string CalcOperation(Tree left, Tree midle, Tree right, ref Assembler T, out Tree Parent,string name)
        {
            T.Set(midle.Content);
            T.LeftParam = Compiler(left);
            T.RightParam = Compiler(right);
            var d = string.IsNullOrWhiteSpace(name);
            FieldInfo z = New_Get_Variable(left.Parent.Type, name);
            T.Name = z.Name;
            //var fn = Parser.GetOperant(T.Fn[0], T.Fn.Length > 1 && T.Fn[1] == '=');
            Parent = new Tree(T.Name ?? "", Kind.Variable, left.Parent);
            Parent.Children.Add(left);
            Parent.Children.Add(midle);
            Parent.Children.Add(right);
            Parent.Type = T.Type;
            if ( d ) System.Lexical.DisactiveVariable(z);
            return T.Fn;
        }
    }
}
